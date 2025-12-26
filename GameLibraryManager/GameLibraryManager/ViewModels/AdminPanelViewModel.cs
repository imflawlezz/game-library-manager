using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class AdminPanelViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;
        public bool IsAdmin => _sessionManager.IsAdmin;

        private ObservableCollection<Game> _games = new();
        private ObservableCollection<CatalogGame> _filteredGames = new();
        private Game? _selectedGame;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private CancellationTokenSource? _filterCancellation;
        
        public NotificationService NotificationService => NotificationService.Instance;
        private HashSet<int> _userLibraryGameIds = new();

        public ObservableCollection<Game> Games
        {
            get => _games;
            set => this.RaiseAndSetIfChanged(ref _games, value);
        }

        public ObservableCollection<CatalogGame> FilteredGames
        {
            get => _filteredGames;
            private set => this.RaiseAndSetIfChanged(ref _filteredGames, value);
        }

        public Game? SelectedGame
        {
            get => _selectedGame;
            set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }



        public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }
        public ReactiveCommand<CatalogGame, Unit> ViewGameCommand { get; }
        public ReactiveCommand<CatalogGame, Unit> AddToLibraryCommand { get; }
        public ReactiveCommand<CatalogGame, Unit> DeleteGameCommand { get; }

        private readonly Action<int> _openGameDetails;

        public AdminPanelViewModel(SessionManager sessionManager, DatabaseService dbService, Action<int> openGameDetails)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;
            _openGameDetails = openGameDetails;

            LoadGamesCommand = ReactiveCommand.CreateFromTask(LoadGamesAsync);
            ViewGameCommand = ReactiveCommand.Create<CatalogGame>(ViewGame);
            AddToLibraryCommand = ReactiveCommand.CreateFromTask<CatalogGame>(AddToLibraryAsync);
            DeleteGameCommand = ReactiveCommand.CreateFromTask<CatalogGame>(DeleteGameAsync);

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Task.Run(() => ApplyFilterAsync()));

            IsLoading = true;
            
            Task.Run(async () => await LoadGamesAsync().ConfigureAwait(false));
        }

        private async Task LoadGamesAsync()
        {
            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var gamesTask = Task.Run(() => _dbService.GetAllGamesAsync());
                var userLibraryTask = _sessionManager.CurrentUser != null 
                    ? Task.Run(() => _dbService.GetUserLibraryAsync(_sessionManager.CurrentUser.UserID))
                    : Task.FromResult(new List<UserGame>());
                
                await Task.WhenAll(gamesTask, userLibraryTask).ConfigureAwait(false);
                
                var games = await gamesTask.ConfigureAwait(false);
                var userLibrary = await userLibraryTask.ConfigureAwait(false);
                
                _userLibraryGameIds = new HashSet<int>(userLibrary.Select(ug => ug.GameID));
                
                var catalogGames = games.Select(g => new CatalogGame(g, _userLibraryGameIds.Contains(g.GameID))).ToList();
                
                Dispatcher.UIThread.Post(() =>
                {
                    Games = new ObservableCollection<Game>(games);
                    FilteredGames = new ObservableCollection<CatalogGame>(catalogGames);
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Failed to load games: {ex.Message}");
                    IsLoading = false;
                });
            }
        }

        private async Task ApplyFilterAsync()
        {
            _filterCancellation?.Cancel();
            _filterCancellation = new CancellationTokenSource();
            var token = _filterCancellation.Token;
            
            var searchText = SearchText;
            var allGames = Games?.ToList() ?? new List<Game>();
            var libraryIds = _userLibraryGameIds;
            
            var filtered = await Task.Run(() =>
            {
                if (token.IsCancellationRequested) return null;
                
                IEnumerable<Game> result;
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    result = allGames;
                }
                else
                {
                    result = allGames.Where(g =>
                        g.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (g.Developer?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (g.Publisher?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                
                return result.Select(g => new CatalogGame(g, libraryIds.Contains(g.GameID))).ToList();
            }, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested || filtered == null) return;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    FilteredGames = new ObservableCollection<CatalogGame>(filtered);
                }
            });
        }

        private void ViewGame(CatalogGame catalogGame)
        {
            SelectedGame = catalogGame.Game;
            _openGameDetails?.Invoke(catalogGame.Game.GameID);
        }

        private async Task AddToLibraryAsync(CatalogGame catalogGame)
        {
            if (_sessionManager.CurrentUser == null) return;
            if (catalogGame.IsInLibrary) return; // Already in library

            IsLoading = true;

            try
            {
                var success = await _dbService.AddGameToLibraryAsync(
                    _sessionManager.CurrentUser.UserID,
                    catalogGame.Game.GameID,
                    "Backlog"
                );

                if (success)
                {
                    _userLibraryGameIds.Add(catalogGame.Game.GameID);
                    catalogGame.IsInLibrary = true;
                    NotificationService.ShowSuccess("Added to your library");
                }
                else
                {
                    NotificationService.ShowError("Failed to add game to library");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteGameAsync(CatalogGame catalogGame)
        {
            IsLoading = true;

            try
            {
                var success = await _dbService.DeleteGameAsync(catalogGame.Game.GameID);
                if (success)
                {
                    await LoadGamesAsync();
                    NotificationService.ShowSuccess("Game deleted successfully");
                }
                else
                {
                    NotificationService.ShowError("Failed to delete game");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    public class CatalogGame : ReactiveObject
    {
        public Game Game { get; }
        
        private bool _isInLibrary;
        public bool IsInLibrary
        {
            get => _isInLibrary;
            set => this.RaiseAndSetIfChanged(ref _isInLibrary, value);
        }
        
        public int GameID => Game.GameID;
        public string Title => Game.Title;
        public string? Developer => Game.Developer;
        public string? Publisher => Game.Publisher;
        public int? ReleaseYear => Game.ReleaseYear;
        public string? CoverImageURL => Game.CoverImageURL;
        public string? GenresString => Game.GenresString;
        
        public CatalogGame(Game game, bool isInLibrary)
        {
            Game = game;
            _isInLibrary = isInLibrary;
        }
    }
}


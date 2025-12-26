using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;
using GameLibraryManager.Views;

namespace GameLibraryManager.ViewModels
{
    public class GameManagementViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        private ObservableCollection<Game> _games = new();
        private ObservableCollection<Game> _filteredGames = new();
        private string _searchText = string.Empty;
        private bool _isLoading;
        
        public NotificationService NotificationService => NotificationService.Instance;

        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                this.RaiseAndSetIfChanged(ref _games, value);
                _ = ApplyFilterAsync();
                this.RaisePropertyChanged(nameof(TotalGames));
            }
        }

        public ObservableCollection<Game> FilteredGames
        {
            get => _filteredGames;
            private set => this.RaiseAndSetIfChanged(ref _filteredGames, value);
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



        public int TotalGames => Games?.Count ?? 0;
        public bool IsEmpty => !IsLoading && (FilteredGames?.Count ?? 0) == 0;

        public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }
        public ReactiveCommand<Unit, Unit> AddGameCommand { get; }
        public ReactiveCommand<Game, Unit> EditGameCommand { get; }
        public ReactiveCommand<Game, Unit> DeleteGameCommand { get; }

        private CancellationTokenSource? _filterCancellation;

        public GameManagementViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            LoadGamesCommand = ReactiveCommand.CreateFromTask(LoadGamesAsync);
            AddGameCommand = ReactiveCommand.CreateFromTask(AddGameAsync);
            EditGameCommand = ReactiveCommand.CreateFromTask<Game>(EditGameAsync);
            DeleteGameCommand = ReactiveCommand.CreateFromTask<Game>(DeleteGameAsync);

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
                var games = await Task.Run(() => _dbService.GetAllGamesAsync()).ConfigureAwait(false);
                
                Dispatcher.UIThread.Post(() =>
                {
                    Games = new ObservableCollection<Game>(games);
                    FilteredGames = new ObservableCollection<Game>(games);
                    this.RaisePropertyChanged(nameof(IsEmpty));
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
            
            var filtered = await Task.Run(() =>
            {
                if (token.IsCancellationRequested) return null;
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return allGames;
                }
                else
                {
                    return allGames.Where(g =>
                        g.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (g.Developer?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (g.Publisher?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }
            }, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested || filtered == null) return;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    FilteredGames = new ObservableCollection<Game>(filtered);
                    this.RaisePropertyChanged(nameof(IsEmpty));
                }
            });
        }

        private async Task AddGameAsync()
        {
            var dialog = new AddEditGameDialog();
            var viewModel = new AddEditGameDialogViewModel(_dbService, _sessionManager, (saved) =>
            {
                dialog.Close(saved);
            });
            dialog.DataContext = viewModel;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                var result = await dialog.ShowDialog<bool?>(parentWindow);
                if (result == true)
                {
                    await LoadGamesAsync();
                    NotificationService.ShowSuccess("Game added successfully");
                }
            }
        }

        private async Task EditGameAsync(Game game)
        {
            if (game == null) return;

            var dialog = new AddEditGameDialog();
            var viewModel = new AddEditGameDialogViewModel(_dbService, _sessionManager, (saved) =>
            {
                dialog.Close(saved);
            }, game);
            dialog.DataContext = viewModel;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                var result = await dialog.ShowDialog<bool?>(parentWindow);
                if (result == true)
                {
                    await LoadGamesAsync();
                    NotificationService.ShowSuccess("Game updated successfully");
                }
            }
        }

        private async Task DeleteGameAsync(Game game)
        {
            if (game == null) return;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                var confirmDialog = new ConfirmDeleteDialog();
                var confirmViewModel = new ConfirmDeleteDialogViewModel(
                    $"Delete Game: {game.Title}?",
                    $"This will permanently delete \"{game.Title}\" from the catalog and remove it from all user libraries. This action cannot be undone.",
                    (confirmed) => confirmDialog.Close(confirmed)
                );
                confirmDialog.DataContext = confirmViewModel;

                var result = await confirmDialog.ShowDialog<bool?>(parentWindow);
                if (result != true) return;
            }

            IsLoading = true;

            try
            {
                var success = await _dbService.DeleteGameAsync(game.GameID);
                if (success)
                {
                    await LoadGamesAsync();
                    NotificationService.ShowSuccess($"Game \"{game.Title}\" deleted successfully");
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



        private Window? GetParentWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }
}

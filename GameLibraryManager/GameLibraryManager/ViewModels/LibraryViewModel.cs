using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;
using ReactiveUI.Fody.Helpers;

namespace GameLibraryManager.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        private ObservableCollection<UserGame> _userGames = new();
        private ObservableCollection<UserGame> _filteredGames = new();
        private UserGame? _selectedGame;
        private string _selectedStatus = "All";
        private int? _selectedGenreId;
        private int? _selectedPlatformId;
        private string _searchText = string.Empty;
        private bool _isLoading;
        
        public NotificationService NotificationService => NotificationService.Instance;
        private UserStatistics? _statistics;

        public ObservableCollection<UserGame> UserGames
        {
            get => _userGames;
            set => this.RaiseAndSetIfChanged(ref _userGames, value);
        }

        public ObservableCollection<UserGame> FilteredGames
        {
            get => _filteredGames;
            set => this.RaiseAndSetIfChanged(ref _filteredGames, value);
        }

        public UserGame? SelectedGame
        {
            get => _selectedGame;
            set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedStatus, value);
                _ = ApplyFiltersAsync();
            }
        }

        public int? SelectedGenreId
        {
            get => _selectedGenreId;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedGenreId, value);
                _ = ApplyFiltersAsync();
            }
        }

        public int? SelectedPlatformId
        {
            get => _selectedPlatformId;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPlatformId, value);
                _ = ApplyFiltersAsync();
            }
        }

        public Genre? SelectedGenre
        {
            get => Genres.FirstOrDefault(g => g.GenreID == _selectedGenreId);
            set
            {
                SelectedGenreId = value?.GenreID;
            }
        }

        public Platform? SelectedPlatform
        {
            get => Platforms.FirstOrDefault(p => p.PlatformID == _selectedPlatformId);
            set
            {
                SelectedPlatformId = value?.PlatformID;
            }
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



        public UserStatistics? Statistics
        {
            get => _statistics;
            set => this.RaiseAndSetIfChanged(ref _statistics, value);
        }

        public ObservableCollection<Genre> Genres { get; } = new();
        public ObservableCollection<Platform> Platforms { get; } = new();
        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            "All", "Playing", "Completed", "Backlog", "Wishlist", "Dropped"
        };

        public ReactiveCommand<Unit, Unit> LoadLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }
        public ReactiveCommand<UserGame, Unit> ViewGameDetailsCommand { get; }
        public ReactiveCommand<UserGame, Unit> OpenGameDetailsCommand { get; }
        public ReactiveCommand<UserGame, Unit> RemoveGameCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportLibraryCommand { get; }

        private readonly Action<int> _openGameDetails;

        private CancellationTokenSource? _filterCancellation;

        public LibraryViewModel(SessionManager sessionManager, DatabaseService dbService, Action<int> openGameDetails)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;
            _openGameDetails = openGameDetails;

            LoadLibraryCommand = ReactiveCommand.CreateFromTask(LoadLibraryAsync);
            LoadGamesCommand = LoadLibraryCommand;
            ViewGameDetailsCommand = ReactiveCommand.Create<UserGame>(ViewGameDetails);
            OpenGameDetailsCommand = ReactiveCommand.Create<UserGame>(OpenGameDetails);
            ExportLibraryCommand = ReactiveCommand.CreateFromTask(ExportLibraryAsync);
            ImportLibraryCommand = ReactiveCommand.CreateFromTask(ImportLibraryAsync);
            RemoveGameCommand = ReactiveCommand.CreateFromTask<UserGame>(RemoveGameAsync);

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Task.Run(() => ApplyFiltersAsync()));

            IsLoading = true;
            
            Task.Run(async () => await LoadLibraryAsync().ConfigureAwait(false));
        }

        private async Task LoadLibraryAsync()
        {
            if (_sessionManager.CurrentUser == null)
            {
                Dispatcher.UIThread.Post(() => IsLoading = false);
                return;
            }

            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var userId = _sessionManager.CurrentUser.UserID;
                var status = SelectedStatus == "All" ? null : SelectedStatus;
                var genreId = SelectedGenreId;
                var platformId = SelectedPlatformId;

                var userGamesTask = Task.Run(() => _dbService.GetUserLibraryAsync(userId, status, genreId, platformId));
                var genresTask = Task.Run(() => _dbService.GetAllGenresAsync());
                var platformsTask = Task.Run(() => _dbService.GetAllPlatformsAsync());
                var statisticsTask = Task.Run(() => _dbService.GetUserStatisticsAsync(userId));

                await Task.WhenAll(userGamesTask, genresTask, platformsTask, statisticsTask).ConfigureAwait(false);

                var userGames = await userGamesTask.ConfigureAwait(false);
                var genres = await genresTask.ConfigureAwait(false);
                var platforms = await platformsTask.ConfigureAwait(false);
                var statistics = await statisticsTask.ConfigureAwait(false);

                Dispatcher.UIThread.Post(() =>
                {
                    UserGames = new ObservableCollection<UserGame>(userGames);
                    FilteredGames = new ObservableCollection<UserGame>(userGames);

                    Genres.Clear();
                    foreach (var genre in genres)
                    {
                        Genres.Add(genre);
                    }

                    Platforms.Clear();
                    foreach (var platform in platforms)
                    {
                        Platforms.Add(platform);
                    }

                    Statistics = statistics;
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Failed to load library: {ex.Message}");
                    IsLoading = false;
                });
            }
        }

        private async Task ApplyFiltersAsync()
        {
            _filterCancellation?.Cancel();
            _filterCancellation = new CancellationTokenSource();
            var token = _filterCancellation.Token;
            
            var searchText = SearchText;
            var allGames = UserGames.ToList();
            
            var filtered = await Task.Run(() =>
            {
                if (token.IsCancellationRequested) return null;
                
                IEnumerable<UserGame> result = allGames;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    result = result.Where(g => 
                        g.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (g.Developer?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }

                return result.ToList();
            }, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested || filtered == null) return;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    FilteredGames = new ObservableCollection<UserGame>(filtered);
                }
            });
        }

        private void ViewGameDetails(UserGame userGame)
        {
            SelectedGame = userGame;
        }

        private void OpenGameDetails(UserGame userGame)
        {
            SelectedGame = userGame;
            _openGameDetails?.Invoke(userGame.GameID);
        }

        private async Task RemoveGameAsync(UserGame userGame)
        {
            if (_sessionManager.CurrentUser == null) return;

            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var userId = _sessionManager.CurrentUser.UserID;
                var gameId = userGame.GameID;
                
                var success = await Task.Run(() => _dbService.RemoveGameFromLibraryAsync(userId, gameId)).ConfigureAwait(false);

                if (success)
                {
                    await LoadLibraryAsync().ConfigureAwait(false);
                    Dispatcher.UIThread.Post(() => NotificationService.ShowSuccess("Game removed from library"));
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        NotificationService.ShowError("Failed to remove game from library");
                        IsLoading = false;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Error: {ex.Message}");
                    IsLoading = false;
                });
            }
        }

        private async Task ExportLibraryAsync()
        {
            if (_sessionManager.CurrentUser == null) return;

            try
            {
                IsLoading = true;

                var xmlData = await Task.Run(() => _dbService.ExportUserLibraryToXMLAsync(_sessionManager.CurrentUser.UserID));

                if (string.IsNullOrEmpty(xmlData))
                {
                    NotificationService.ShowError("No library data to export");
                    return;
                }

                var topLevel = GetTopLevel();
                if (topLevel == null) return;

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Export Library",
                    SuggestedFileName = $"library_export_{DateTime.Now:yyyyMMdd_HHmmss}.xml",
                    DefaultExtension = "xml",
                    FileTypeChoices = new[]
                    {
                        FilePickerFileTypes.All,
                        new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } }
                    }
                });

                if (file != null)
                {
                    var xmlDoc = XDocument.Parse(xmlData);
                    var formattedXml = xmlDoc.ToString();
                    
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(formattedXml);
                    await writer.FlushAsync();

                    NotificationService.ShowSuccess("Library exported successfully!");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Failed to export library: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportLibraryAsync()
        {
            if (_sessionManager.CurrentUser == null) return;

            try
            {
                var topLevel = GetTopLevel();
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Import Library",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        FilePickerFileTypes.All,
                        new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } }
                    }
                });

                if (files.Count == 0) return;

                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var xmlData = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(xmlData))
                {
                    NotificationService.ShowError("The selected file is empty");
                    return;
                }

                IsLoading = true;

                var success = await Task.Run(() => _dbService.ImportUserLibraryFromXMLAsync(_sessionManager.CurrentUser.UserID, xmlData));

                if (success)
                {
                    NotificationService.ShowSuccess("Library imported successfully!");
                    await LoadLibraryAsync();
                }
                else
                {
                    NotificationService.ShowError("Failed to import library");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Failed to import library: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Avalonia.Controls.TopLevel? GetTopLevel()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        }
    }
}


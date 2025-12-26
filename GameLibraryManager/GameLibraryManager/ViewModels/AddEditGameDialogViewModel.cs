using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class AddEditGameDialogViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;
        private readonly Game? _existingGame;
        private readonly Action<bool> _closeDialog;

        private string _title = string.Empty;
        private string _developer = string.Empty;
        private string _publisher = string.Empty;
        private string _releaseYearText = string.Empty;
        private string _description = string.Empty;
        private string _coverImageURL = string.Empty;
        private bool _isLoading;
        
        public NotificationService NotificationService => NotificationService.Instance;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Developer
        {
            get => _developer;
            set => this.RaiseAndSetIfChanged(ref _developer, value);
        }

        public string Publisher
        {
            get => _publisher;
            set => this.RaiseAndSetIfChanged(ref _publisher, value);
        }

        public string ReleaseYearText
        {
            get => _releaseYearText;
            set => this.RaiseAndSetIfChanged(ref _releaseYearText, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public string CoverImageURL
        {
            get => _coverImageURL;
            set => this.RaiseAndSetIfChanged(ref _coverImageURL, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }



        public bool IsEditMode => _existingGame != null;
        public string DialogTitle => IsEditMode ? "Edit Game" : "Add Game";
        public string SaveButtonText => IsEditMode ? "Save Changes" : "Add Game";

        public ObservableCollection<SelectableGenre> Genres { get; } = new();
        public ObservableCollection<SelectablePlatform> Platforms { get; } = new();

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<SelectableGenre, Unit> ToggleGenreCommand { get; }
        public ReactiveCommand<SelectablePlatform, Unit> TogglePlatformCommand { get; }

        public AddEditGameDialogViewModel(DatabaseService dbService, SessionManager sessionManager, 
            Action<bool> closeDialog, Game? existingGame = null)
        {
            _dbService = dbService;
            _sessionManager = sessionManager;
            _closeDialog = closeDialog;
            _existingGame = existingGame;

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(Cancel);
            ToggleGenreCommand = ReactiveCommand.Create<SelectableGenre>(ToggleGenre);
            TogglePlatformCommand = ReactiveCommand.Create<SelectablePlatform>(TogglePlatform);

            IsLoading = true;
            
            ThreadPool.QueueUserWorkItem(async _ => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsLoading = true;
            });

            try
            {
                var existingGameId = _existingGame?.GameID;
                
                var genresTask = _dbService.GetAllGenresAsync();
                var platformsTask = _dbService.GetAllPlatformsAsync();
                Task<Game?>? gameTask = null;
                
                if (existingGameId != null)
                {
                    gameTask = _dbService.GetGameByIdAsync(existingGameId.Value);
                }

                await Task.WhenAll(genresTask, platformsTask, gameTask ?? Task.FromResult<Game?>(null));

                var allGenres = await genresTask;
                var allPlatforms = await platformsTask;
                var gameDetails = gameTask != null ? await gameTask : null;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Genres.Clear();
                    Platforms.Clear();

                    if (gameDetails != null)
                    {
                        Title = gameDetails.Title;
                        Developer = gameDetails.Developer ?? string.Empty;
                        Publisher = gameDetails.Publisher ?? string.Empty;
                        ReleaseYearText = gameDetails.ReleaseYear?.ToString() ?? string.Empty;
                        Description = gameDetails.Description ?? string.Empty;
                        CoverImageURL = gameDetails.CoverImageURL ?? string.Empty;
                    }

                    foreach (var genre in allGenres)
                    {
                        var isSelected = gameDetails?.Genres.Any(g => g.GenreID == genre.GenreID) ?? false;
                        Genres.Add(new SelectableGenre(genre, isSelected));
                    }

                    foreach (var platform in allPlatforms)
                    {
                        var isSelected = gameDetails?.Platforms.Any(p => p.PlatformID == platform.PlatformID) ?? false;
                        Platforms.Add(new SelectablePlatform(platform, isSelected));
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    NotificationService.ShowError($"Failed to load data: {ex.Message}");
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private void ToggleGenre(SelectableGenre genre)
        {
            genre.IsSelected = !genre.IsSelected;
        }

        private void TogglePlatform(SelectablePlatform platform)
        {
            platform.IsSelected = !platform.IsSelected;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                NotificationService.ShowError("Title is required");
                return;
            }

            if (_sessionManager.CurrentUser == null)
            {
                NotificationService.ShowError("User not logged in");
                return;
            }

            int? releaseYear = null;
            if (!string.IsNullOrWhiteSpace(ReleaseYearText))
            {
                if (int.TryParse(ReleaseYearText, out var year))
                {
                    releaseYear = year;
                }
                else
                {
                    NotificationService.ShowError("Invalid release year");
                    return;
                }
            }

            IsLoading = true;

            try
            {
                var game = new Game
                {
                    Title = Title.Trim(),
                    Developer = string.IsNullOrWhiteSpace(Developer) ? null : Developer.Trim(),
                    Publisher = string.IsNullOrWhiteSpace(Publisher) ? null : Publisher.Trim(),
                    ReleaseYear = releaseYear,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    CoverImageURL = string.IsNullOrWhiteSpace(CoverImageURL) ? null : CoverImageURL.Trim(),
                    CreatedBy = _sessionManager.CurrentUser.UserID
                };

                var genreIds = Genres.Where(g => g.IsSelected).Select(g => g.GenreID).ToList();
                var platformIds = Platforms.Where(p => p.IsSelected).Select(p => p.PlatformID).ToList();

                if (_existingGame != null)
                {
                    game.GameID = _existingGame.GameID;
                    var success = await _dbService.UpdateGameAsync(game, genreIds, platformIds);
                    if (!success)
                    {
                        NotificationService.ShowError("Failed to update game");
                        return;
                    }
                }
                else
                {
                    await _dbService.AddGameAsync(game, genreIds, platformIds);
                }

                _closeDialog(true);
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

        private void Cancel()
        {
            _closeDialog(false);
        }
    }
}

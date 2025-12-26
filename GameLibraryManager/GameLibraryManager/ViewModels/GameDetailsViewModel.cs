using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class GameDetailsViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;
        private readonly Action? _onBack;
        private readonly int _gameId;

        private UserGame? _userGame;
        private Game? _game;
        private string _status = "Backlog";
        private string _ratingText = string.Empty;
        private string _hoursPlayedText = string.Empty;
        private string _personalNotes = string.Empty;
        private bool _isLoading;
        private bool _isInLibrary;
        
        public NotificationService NotificationService => NotificationService.Instance;

        public UserGame? UserGame
        {
            get => _userGame;
            set => this.RaiseAndSetIfChanged(ref _userGame, value);
        }

        public Game? Game
        {
            get => _game;
            set => this.RaiseAndSetIfChanged(ref _game, value);
        }

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public string Rating
        {
            get => _ratingText;
            set => this.RaiseAndSetIfChanged(ref _ratingText, value);
        }

        public string HoursPlayed
        {
            get => _hoursPlayedText;
            set => this.RaiseAndSetIfChanged(ref _hoursPlayedText, value);
        }

        public string PersonalNotes
        {
            get => _personalNotes;
            set => this.RaiseAndSetIfChanged(ref _personalNotes, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool IsInLibrary
        {
            get => _isInLibrary;
            set => this.RaiseAndSetIfChanged(ref _isInLibrary, value);
        }

        public bool IsAdmin => _sessionManager.IsAdmin;

        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            "Playing", "Completed", "Backlog", "Wishlist", "Dropped"
        };

        public ReactiveCommand<Unit, Unit> LoadGameDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; }
        public ReactiveCommand<Unit, Unit> AddToLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveFromLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }

        public GameDetailsViewModel(SessionManager sessionManager, DatabaseService dbService, int gameId, Action? onBack = null)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;
            _gameId = gameId;
            _onBack = onBack;

            LoadGameDetailsCommand = ReactiveCommand.CreateFromTask(() => LoadGameDetailsAsync(_gameId));
            SaveChangesCommand = ReactiveCommand.CreateFromTask(SaveChangesAsync);
            AddToLibraryCommand = ReactiveCommand.CreateFromTask(AddToLibraryAsync);
            RemoveFromLibraryCommand = ReactiveCommand.CreateFromTask(RemoveFromLibraryAsync);
            BackCommand = ReactiveCommand.Create(() => _onBack?.Invoke());

            IsLoading = true;
            
            Task.Run(async () => await LoadGameDetailsAsync(_gameId).ConfigureAwait(false));
        }

        private async Task LoadGameDetailsAsync(int gameId)
        {
            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var userId = _sessionManager.CurrentUser?.UserID;
                
                var gameTask = Task.Run(() => _dbService.GetGameByIdAsync(gameId));
                Task<UserGame?> userGameTask = userId != null 
                    ? Task.Run(() => _dbService.GetUserGameDetailsAsync(userId.Value, gameId))
                    : Task.FromResult<UserGame?>(null);

                await Task.WhenAll(gameTask, userGameTask).ConfigureAwait(false);
                
                var game = await gameTask.ConfigureAwait(false);
                var userGame = await userGameTask.ConfigureAwait(false);

                Dispatcher.UIThread.Post(() =>
                {
                    Game = game;
                    
                    if (userGame != null)
                    {
                        UserGame = userGame;
                        IsInLibrary = true;
                        Status = userGame.Status;
                        Rating = userGame.PersonalRating?.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                        HoursPlayed = userGame.HoursPlayed?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                        PersonalNotes = userGame.PersonalNotes ?? string.Empty;
                    }
                    else
                    {
                        IsInLibrary = false;
                    }
                    
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Failed to load game details: {ex.Message}");
                    IsLoading = false;
                });
            }
        }

        private async Task SaveChangesAsync()
        {
            if (_sessionManager.CurrentUser == null || UserGame == null) return;

            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                UserGame.Status = Status;
                
                if (decimal.TryParse(Rating, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var ratingValue))
                {
                    if (ratingValue < 0 || ratingValue > 10)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            NotificationService.ShowError("Rating must be between 0 and 10");
                            IsLoading = false;
                        });
                        return;
                    }
                    UserGame.PersonalRating = Math.Round(ratingValue, 1);
                }
                else if (string.IsNullOrWhiteSpace(Rating))
                {
                    UserGame.PersonalRating = null;
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        NotificationService.ShowError("Invalid rating format");
                        IsLoading = false;
                    });
                    return;
                }
                
                if (decimal.TryParse(HoursPlayed, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var hoursValue))
                {
                    if (hoursValue < 0)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            NotificationService.ShowError("Hours cannot be negative");
                            IsLoading = false;
                        });
                        return;
                    }
                    UserGame.HoursPlayed = Math.Round(hoursValue, 2);
                }
                else if (string.IsNullOrWhiteSpace(HoursPlayed))
                {
                    UserGame.HoursPlayed = null;
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        NotificationService.ShowError("Invalid hours format");
                        IsLoading = false;
                    });
                    return;
                }
                
                UserGame.PersonalNotes = string.IsNullOrWhiteSpace(PersonalNotes) ? null : PersonalNotes;

                var success = await Task.Run(() => _dbService.UpdateUserGameAsync(UserGame)).ConfigureAwait(false);
                
                Dispatcher.UIThread.Post(() =>
                {
                    if (success)
                    {
                        NotificationService.ShowSuccess("Changes saved!");
                    }
                    else
                    {
                        NotificationService.ShowError("Failed to save changes");
                    }
                    IsLoading = false;
                });
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

        private async Task AddToLibraryAsync()
        {
            if (_sessionManager.CurrentUser == null || Game == null) return;

            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var userId = _sessionManager.CurrentUser.UserID;
                var gameId = Game.GameID;
                var status = Status;
                
                var success = await Task.Run(() => _dbService.AddGameToLibraryAsync(userId, gameId, status)).ConfigureAwait(false);

                if (success)
                {
                    Dispatcher.UIThread.Post(() => IsInLibrary = true);
                    await LoadGameDetailsAsync(gameId).ConfigureAwait(false);
                    Dispatcher.UIThread.Post(() => NotificationService.ShowSuccess("Game added to library!"));
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        NotificationService.ShowError("Failed to add game to library");
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

        private async Task RemoveFromLibraryAsync()
        {
            if (_sessionManager.CurrentUser == null || Game == null) return;

            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var userId = _sessionManager.CurrentUser.UserID;
                var gameId = Game.GameID;
                
                var success = await Task.Run(() => _dbService.RemoveGameFromLibraryAsync(userId, gameId)).ConfigureAwait(false);

                Dispatcher.UIThread.Post(() =>
                {
                    if (success)
                    {
                        IsInLibrary = false;
                        UserGame = null;
                        Status = "Backlog";
                        Rating = string.Empty;
                        HoursPlayed = string.Empty;
                        PersonalNotes = string.Empty;
                        NotificationService.ShowSuccess("Game removed from library");
                    }
                    else
                    {
                        NotificationService.ShowError("Failed to remove game from library");
                    }
                    IsLoading = false;
                });
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
    }
}

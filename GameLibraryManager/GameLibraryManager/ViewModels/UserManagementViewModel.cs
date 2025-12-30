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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;
using GameLibraryManager.Views;

namespace GameLibraryManager.ViewModels
{
    public class UserManagementViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        private ObservableCollection<User> _users = new();
        private ObservableCollection<User> _filteredUsers = new();
        private string _searchText = string.Empty;
        private bool _isLoading;
        
        public NotificationService NotificationService => NotificationService.Instance;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                this.RaiseAndSetIfChanged(ref _users, value);
                _ = ApplyFilterAsync();
                this.RaisePropertyChanged(nameof(TotalUsers));
                this.RaisePropertyChanged(nameof(AdminCount));
                this.RaisePropertyChanged(nameof(RegularUserCount));
                this.RaisePropertyChanged(nameof(IsEmpty));
            }
        }

        public ObservableCollection<User> FilteredUsers
        {
            get => _filteredUsers;
            private set => this.RaiseAndSetIfChanged(ref _filteredUsers, value);
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

        public int TotalUsers => Users?.Count ?? 0;
        public int AdminCount => Users?.Count(u => u.Role == "Admin") ?? 0;
        public int RegularUserCount => Users?.Count(u => u.Role == "User") ?? 0;
        public bool IsEmpty => !IsLoading && (FilteredUsers?.Count ?? 0) == 0;

        public ReactiveCommand<Unit, Unit> LoadUsersCommand { get; }
        public ReactiveCommand<Unit, Unit> AddUserCommand { get; }
        public ReactiveCommand<User, Unit> EditUserCommand { get; }
        public ReactiveCommand<User, Unit> DeleteUserCommand { get; }

        private CancellationTokenSource? _filterCancellation;

        public UserManagementViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            LoadUsersCommand = ReactiveCommand.CreateFromTask(LoadUsersAsync);
            AddUserCommand = ReactiveCommand.CreateFromTask(AddUserAsync);
            EditUserCommand = ReactiveCommand.CreateFromTask<User>(EditUserAsync);
            DeleteUserCommand = ReactiveCommand.CreateFromTask<User>(DeleteUserAsync);

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Task.Run(() => ApplyFilterAsync()));

            IsLoading = true;
            
            Task.Run(async () => await LoadUsersAsync().ConfigureAwait(false));
        }

        private async Task LoadUsersAsync()
        {
            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var users = await Task.Run(() => _dbService.GetAllUsersAsync()).ConfigureAwait(false);
                
                Dispatcher.UIThread.Post(() =>
                {
                    Users = new ObservableCollection<User>(users);
                    FilteredUsers = new ObservableCollection<User>(users);
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Failed to load users: {ex.Message}");
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
            var allUsers = Users?.ToList() ?? new List<User>();
            
            var filtered = await Task.Run(() =>
            {
                if (token.IsCancellationRequested) return null;
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return allUsers;
                }
                else
                {
                    var searchLower = searchText.ToLowerInvariant();
                    return allUsers.Where(u =>
                        u.UserID.ToString().Contains(searchLower) ||
                        u.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        u.Role.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested || filtered == null) return;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    FilteredUsers = new ObservableCollection<User>(filtered);
                    this.RaisePropertyChanged(nameof(IsEmpty));
                }
            });
        }

        private async Task AddUserAsync()
        {
            var dialog = new EditUserDialog();
            var viewModel = new EditUserDialogViewModel(_dbService, (saved) =>
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
                    await LoadUsersAsync();
                    NotificationService.ShowSuccess("User created successfully");
                }
            }
        }

        private async Task EditUserAsync(User user)
        {
            if (user == null) return;

            var dialog = new EditUserDialog();
            var viewModel = new EditUserDialogViewModel(user, _dbService, (saved) =>
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
                    await LoadUsersAsync();
                    NotificationService.ShowSuccess("User updated successfully");
                }
            }
        }

        private async Task DeleteUserAsync(User user)
        {
            if (user == null) return;

            if (user.UserID == _sessionManager.CurrentUser?.UserID)
            {
                NotificationService.ShowError("You cannot delete your own account");
                return;
            }

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                var confirmDialog = new ConfirmDeleteDialog();
                var confirmViewModel = new ConfirmDeleteDialogViewModel(
                    $"Delete User: {user.Username}?",
                    $"This will permanently delete the user \"{user.Username}\" ({user.Email}) and all their library data. This action cannot be undone.",
                    (confirmed) => confirmDialog.Close(confirmed)
                );
                confirmDialog.DataContext = confirmViewModel;

                var result = await confirmDialog.ShowDialog<bool?>(parentWindow);
                if (result != true) return;
            }

            IsLoading = true;

            try
            {
                var success = await _dbService.DeleteUserAsync(user.UserID);
                if (success)
                {
                    await LoadUsersAsync();
                    NotificationService.ShowSuccess($"User \"{user.Username}\" deleted successfully");
                }
                else
                {
                    NotificationService.ShowError("Failed to delete user");
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

using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        [Reactive] public ViewModelBase? CurrentViewModel { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public string CurrentTitle { get; set; } = "My Library";
        [Reactive] public string CurrentSubtitle { get; set; } = string.Empty;
        [Reactive] public string SearchWatermark { get; set; } = "Search games...";
        
        public string WindowTitle => $"GLManager - {CurrentTitle} - {CurrentUser?.Username ?? "Guest"}";
        
        [Reactive] public bool IsLibraryActive { get; set; } = true;
        [Reactive] public bool IsCatalogActive { get; set; }
        [Reactive] public bool IsUserManagementActive { get; set; }
        [Reactive] public bool IsGameManagementActive { get; set; }
        [Reactive] public bool IsLogActive { get; set; }
        [Reactive] public bool IsReportsActive { get; set; }
        [Reactive] public bool IsReportsManagementActive { get; set; }
        [Reactive] public int UnreviewedReportsCount { get; set; }
        public bool ShowUnreviewedBadge => UnreviewedReportsCount > 0;
        
        [Reactive] public bool ShowSearch { get; set; } = true;
        [Reactive] public bool ShowBrowseCatalog { get; set; } = true;
        [Reactive] public bool ShowAddGame { get; set; }
        [Reactive] public bool ShowAddUser { get; set; }
        [Reactive] public bool ShowGameDetailsActions { get; set; }
        [Reactive] public bool IsInLibrary { get; set; }
        
        public NotificationService NotificationService => NotificationService.Instance;

        public User? CurrentUser => _sessionManager.CurrentUser;
        public bool IsAdmin => _sessionManager.IsAdmin;

        public ReactiveCommand<Unit, Unit> NavigateToLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToAllGamesCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToLogCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToUserManagementCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToGameManagementCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToReportsCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToReportsManagementCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenProfileSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> AddUserCommand { get; }
        public ReactiveCommand<Unit, Unit> AddGameCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveGameDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddToLibraryFromDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveFromLibraryCommand { get; }

        public MainWindowViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            NavigateToLibraryCommand = ReactiveCommand.Create(NavigateToLibrary);
            NavigateToAllGamesCommand = ReactiveCommand.Create(NavigateToAllGames);
            NavigateToUserManagementCommand = ReactiveCommand.Create(NavigateToUserManagement);
            NavigateToGameManagementCommand = ReactiveCommand.Create(NavigateToGameManagement);
            NavigateToLogCommand = ReactiveCommand.Create(NavigateToLog);
            NavigateToReportsCommand = ReactiveCommand.Create(NavigateToReports);
            NavigateToReportsManagementCommand = ReactiveCommand.Create(NavigateToReportsManagement);
            OpenProfileSettingsCommand = ReactiveCommand.Create(OpenProfileSettings);
            LogoutCommand = ReactiveCommand.Create(Logout);
            RefreshCommand = ReactiveCommand.Create(Refresh);
            AddUserCommand = ReactiveCommand.Create(AddUser);
            AddGameCommand = ReactiveCommand.Create(AddGame);
            ClearSearchCommand = ReactiveCommand.Create(ClearSearch);
            SaveGameDetailsCommand = ReactiveCommand.CreateFromTask(SaveGameDetails);
            AddToLibraryFromDetailsCommand = ReactiveCommand.CreateFromTask(AddToLibraryFromDetails);
            RemoveFromLibraryCommand = ReactiveCommand.CreateFromTask(RemoveFromLibrary);

            this.WhenAnyValue(x => x.SearchText)
                .Subscribe(text => OnSearchTextChanged(text));

            this.WhenAnyValue(x => x.CurrentTitle, x => x.CurrentUser.Username)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(WindowTitle)));

            NavigateToLibrary();
            
            if (IsAdmin)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    var count = await _dbService.GetUnreviewedReportsCountAsync();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UnreviewedReportsCount = count;
                        this.RaisePropertyChanged(nameof(ShowUnreviewedBadge));
                    });
                });
            }
            
            _ = RefreshUnreviewedReportsCount();
        }

        private void ResetNavState()
        {
            IsLibraryActive = false;
            IsCatalogActive = false;
            IsUserManagementActive = false;
            IsGameManagementActive = false;
            IsLogActive = false;
            IsReportsActive = false;
            IsReportsManagementActive = false;
        }

        private void ResetToolbar()
        {
            ShowSearch = false;
            ShowBrowseCatalog = false;
            ShowAddGame = false;
            ShowAddUser = false;
            ShowGameDetailsActions = false;
            IsInLibrary = false;
            SearchText = string.Empty;
        }

        private void NavigateToLibrary()
        {
            var vm = new LibraryViewModel(_sessionManager, _dbService, NavigateToGameDetails);
            
            CurrentViewModel = vm;
            CurrentTitle = "My Library";
            CurrentSubtitle = "Your personal game collection";
            SearchWatermark = "Search games...";
            
            ResetNavState();
            IsLibraryActive = true;
            
            ResetToolbar();
            ShowSearch = true;
            ShowBrowseCatalog = true;
        }

        private void NavigateToAllGames()
        {
            var vm = new AdminPanelViewModel(_sessionManager, _dbService, NavigateToGameDetails);
            
            CurrentViewModel = vm;
            CurrentTitle = "Browse Catalog";
            CurrentSubtitle = "Discover new games to add to your collection";
            SearchWatermark = "Search games...";
            
            ResetNavState();
            IsCatalogActive = true;
            
            ResetToolbar();
            ShowSearch = true;
        }

        private void NavigateToUserManagement()
        {
            CurrentViewModel = new UserManagementViewModel(_sessionManager, _dbService);
            CurrentTitle = "User Management";
            CurrentSubtitle = "Manage user accounts and permissions";
            SearchWatermark = "Search users...";
            
            ResetNavState();
            IsUserManagementActive = true;
            
            ResetToolbar();
            ShowSearch = true;
            ShowAddUser = true;
        }

        private void NavigateToGameManagement()
        {
            CurrentViewModel = new GameManagementViewModel(_sessionManager, _dbService);
            CurrentTitle = "Game Management";
            CurrentSubtitle = "Manage the game catalog";
            SearchWatermark = "Search games...";
            
            ResetNavState();
            IsGameManagementActive = true;
            
            ResetToolbar();
            ShowSearch = true;
            ShowAddGame = true;
        }

        private void NavigateToLog()
        {
            CurrentViewModel = new LogViewModel(_dbService);
            CurrentTitle = "Audit Log";
            CurrentSubtitle = "View system activity and changes";
            SearchWatermark = "Search log entries...";
            
            ResetNavState();
            IsLogActive = true;
            
            ResetToolbar();
            ShowSearch = true;
        }

        private void NavigateToReports()
        {
            CurrentViewModel = new ReportsViewModel(_sessionManager, _dbService);
            CurrentTitle = "Reports & Suggestions";
            CurrentSubtitle = "Submit feedback and report issues";
            SearchWatermark = string.Empty;
            
            ResetNavState();
            IsReportsActive = true;
            
            ResetToolbar();
        }

        private void NavigateToReportsManagement()
        {
            var vm = new ReportsManagementViewModel(_sessionManager, _dbService);
            CurrentViewModel = vm;
            CurrentTitle = "Reports";
            CurrentSubtitle = "Review and manage user reports and suggestions";
            SearchWatermark = string.Empty;
            
            ResetNavState();
            IsReportsManagementActive = true;
            
            ResetToolbar();
            
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ReportsManagementViewModel.UnreviewedCount))
                {
                    UnreviewedReportsCount = vm.UnreviewedCount;
                    this.RaisePropertyChanged(nameof(ShowUnreviewedBadge));
                }
            };
            
            Task.Run(async () =>
            {
                await Task.Delay(500);
                var count = await _dbService.GetUnreviewedReportsCountAsync();
                Dispatcher.UIThread.Post(() => UnreviewedReportsCount = count);
            });
        }

        private void NavigateToGameDetails(int gameId)
        {
            var vm = new GameDetailsViewModel(_sessionManager, _dbService, gameId, NavigateBack);
            CurrentViewModel = vm;
            CurrentTitle = "Game Details";
            CurrentSubtitle = string.Empty;
            
            ResetNavState();
            
            ResetToolbar();
            ShowGameDetailsActions = true;
            
            IsInLibrary = vm.IsInLibrary;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameDetailsViewModel.IsInLibrary))
                {
                    IsInLibrary = vm.IsInLibrary;
                }
            };
        }

        private void OpenProfileSettings()
        {
            CurrentViewModel = new UserProfileViewModel(_sessionManager, _dbService, NavigateToLibrary);
            CurrentTitle = "Profile Settings";
            CurrentSubtitle = "Manage your account settings";
            
            ResetNavState();
            
            ResetToolbar();
        }

        private void NavigateBack()
        {
            NavigateToLibrary();
        }

        public async Task RefreshUnreviewedReportsCount()
        {
            if (IsAdmin)
            {
                var count = await _dbService.GetUnreviewedReportsCountAsync();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UnreviewedReportsCount = count;
                    this.RaisePropertyChanged(nameof(ShowUnreviewedBadge));
                });
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UnreviewedReportsCount = 0;
                    this.RaisePropertyChanged(nameof(ShowUnreviewedBadge));
                });
            }
        }

        private void Refresh()
        {
            if (CurrentViewModel is LibraryViewModel libraryVm)
            {
                libraryVm.LoadGamesCommand.Execute().Subscribe();
            }
            else if (CurrentViewModel is AdminPanelViewModel adminVm)
            {
                adminVm.LoadGamesCommand.Execute().Subscribe();
            }
            else if (CurrentViewModel is UserManagementViewModel userVm)
            {
                userVm.LoadUsersCommand.Execute().Subscribe();
            }
            else if (CurrentViewModel is GameManagementViewModel gameVm)
            {
                gameVm.LoadGamesCommand.Execute().Subscribe();
            }
            else if (CurrentViewModel is ReportsManagementViewModel reportsMgtVm)
            {
                reportsMgtVm.LoadReportsCommand.Execute().Subscribe();
            }
            
            if (IsAdmin)
            {
                _ = RefreshUnreviewedReportsCount();
            }
        }

        private void AddUser()
        {
            if (CurrentViewModel is UserManagementViewModel userVm)
            {
                userVm.AddUserCommand.Execute().Subscribe();
            }
        }

        private void AddGame()
        {
            if (CurrentViewModel is GameManagementViewModel gameVm)
            {
                gameVm.AddGameCommand.Execute().Subscribe();
            }
        }

        private void OnSearchTextChanged(string text)
        {
            if (CurrentViewModel is LibraryViewModel libraryVm)
            {
                libraryVm.SearchText = text;
            }
            else if (CurrentViewModel is AdminPanelViewModel adminVm)
            {
                adminVm.SearchText = text;
            }
            else if (CurrentViewModel is GameManagementViewModel gameVm)
            {
                gameVm.SearchText = text;
            }
            else if (CurrentViewModel is UserManagementViewModel userVm)
            {
                userVm.SearchText = text;
            }
            else if (CurrentViewModel is LogViewModel logVm)
            {
                logVm.SearchText = text;
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        private void Logout()
        {
            try
            {
                _sessionManager.ClearSession();
                
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    App.ShowLoginWindow(desktop);
                }
            }
            catch
            {
            }
        }

        private async Task SaveGameDetails()
        {
            if (CurrentViewModel is GameDetailsViewModel vm)
            {
                vm.SaveChangesCommand.Execute().Subscribe();
            }
            await Task.CompletedTask;
        }

        private async Task AddToLibraryFromDetails()
        {
            if (CurrentViewModel is GameDetailsViewModel vm)
            {
                vm.AddToLibraryCommand.Execute().Subscribe(_ =>
                {
                    IsInLibrary = vm.IsInLibrary;
                });
            }
            await Task.CompletedTask;
        }

        private async Task RemoveFromLibrary()
        {
            if (CurrentViewModel is GameDetailsViewModel vm)
            {
                vm.RemoveFromLibraryCommand.Execute().Subscribe(_ =>
                {
                    IsInLibrary = vm.IsInLibrary;
                });
            }
            await Task.CompletedTask;
        }
    }
}

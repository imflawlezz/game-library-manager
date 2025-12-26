using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GameLibraryManager.Views;
using GameLibraryManager.ViewModels;
using GameLibraryManager.Services;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace GameLibraryManager
{
    public partial class App : Application
    {
        private static DatabaseService? _dbService;
        private static AuthenticationService? _authService;
        private static SessionManager? _sessionManager;

        public static DatabaseService DbService => _dbService ??= new DatabaseService();
        public static AuthenticationService AuthService => _authService ??= new AuthenticationService(DbService);
        public static SessionManager SessionManager => _sessionManager ??= SessionManager.Instance;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            IconProvider.Current
                .Register<FontAwesomeIconProvider>();
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await DbService.TestConnectionAsync();
                }
                catch
                {
                }
            });
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ShowLoginWindow(desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var oldWindow = desktop.MainWindow;
            
            var loginWindow = new LoginView
            {
                DataContext = new LoginViewModel(AuthService, SessionManager, DbService),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            desktop.MainWindow = loginWindow;
            
            loginWindow.Show();
            loginWindow.Activate();
            loginWindow.BringIntoView();
            loginWindow.WindowState = WindowState.Normal;
            loginWindow.Topmost = true;
            loginWindow.Topmost = false;
            
            if (oldWindow != null && oldWindow != loginWindow)
            {
                oldWindow.Close();
            }
        }

        public static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var oldWindow = desktop.MainWindow;
            
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(SessionManager, DbService),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            desktop.MainWindow = mainWindow;
            
            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.BringIntoView();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            
            if (oldWindow != null && oldWindow != mainWindow)
            {
                oldWindow.Close();
            }
        }
    }
}

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;
        private CancellationTokenSource? _connectionCheckCts;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _username = string.Empty;
        private bool _isLoading;
        private bool _isRegisterMode;
        
        [Reactive] public bool IsConnected { get; set; }
        [Reactive] public bool IsCheckingConnection { get; set; }
        
        public NotificationService NotificationService => NotificationService.Instance;

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }



        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set => this.RaiseAndSetIfChanged(ref _isRegisterMode, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleModeCommand { get; }

        public LoginViewModel(AuthenticationService authService, SessionManager sessionManager, DatabaseService dbService)
        {
            _authService = authService;
            _sessionManager = sessionManager;
            _dbService = dbService;

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, outputScheduler: RxApp.MainThreadScheduler);
            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync, outputScheduler: RxApp.MainThreadScheduler);
            ToggleModeCommand = ReactiveCommand.Create(ToggleMode);
            
            LoginCommand.ThrownExceptions.Subscribe(ex => 
            {
                NotificationService.ShowError($"Login error: {ex.Message}");
            });
            
            StartConnectionCheck();
        }
        
        private void StartConnectionCheck()
        {
            _connectionCheckCts = new CancellationTokenSource();
            var token = _connectionCheckCts.Token;
            
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        IsCheckingConnection = true;
                        await _dbService.TestConnectionAsync();
                        
                        await Task.Delay(100, token); // Small delay for UI update
                        IsConnected = true;
                        IsCheckingConnection = false;
                    }
                    catch
                    {
                        await Task.Delay(100, token);
                        IsConnected = false;
                        IsCheckingConnection = false;
                    }
                    
                    await Task.Delay(5000, token);
                }
            }, token);
        }
        
        public void Dispose()
        {
            _connectionCheckCts?.Cancel();
            _connectionCheckCts?.Dispose();
            _connectionCheckCts = null;
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                NotificationService.ShowError("Please enter email and password");
                return;
            }

            IsLoading = true;

            try
            {
                var passwordHash = AuthenticationService.HashPassword(Password);
                var user = await Task.Run(() => _authService.AuthenticateAsync(Email, Password));
                
                if (user != null)
                {
                    _sessionManager.SetCurrentUser(user, Email, passwordHash);
                    NavigateToMainWindow();
                }
                else
                {
                    NotificationService.ShowError("Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                NotificationService.ShowError("Please enter email and password");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                NotificationService.ShowError("Please enter a username");
                return;
            }

            if (Password.Length < 6)
            {
                NotificationService.ShowError("Password must be at least 6 characters");
                return;
            }

            IsLoading = true;

            try
            {
                var (success, message) = await Task.Run(() => _authService.RegisterAsync(Email, Password, Username));
                if (success)
                {
                    NotificationService.ShowSuccess("Registration successful! Please sign in.");
                    IsRegisterMode = false;
                    Password = string.Empty;
                }
                else
                {
                    NotificationService.ShowError(message);
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Registration failed: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            NotificationService.Clear();
            Password = string.Empty;
            Username = string.Empty;
        }

        private void NavigateToMainWindow()
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    App.ShowMainWindow(desktop);
                }
                else
                {
                    NotificationService.ShowError("Failed to navigate: Application lifetime not available");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Navigation error: {ex.Message}");
            }
        }
    }
}

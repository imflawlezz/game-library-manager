using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;
        private readonly Action _onSaved;

        [Reactive] public string Username { get; set; } = string.Empty;
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string? ProfileImageURL { get; set; }
        [Reactive] public string NewPassword { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        
        public NotificationService NotificationService => NotificationService.Instance;

        public User? CurrentUser => _sessionManager.CurrentUser;
        
        public string PasswordLabel => "New Password";
        public string PasswordHint => "(leave blank to keep current)";

        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public UserProfileViewModel(SessionManager sessionManager, DatabaseService dbService, Action onSaved)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;
            _onSaved = onSaved;

            if (CurrentUser != null)
            {
                Username = CurrentUser.Username;
                Email = CurrentUser.Email;
                ProfileImageURL = CurrentUser.ProfileImageURL;
            }

            var canSave = this.WhenAnyValue(
                x => x.Username,
                x => x.IsLoading,
                (username, loading) => !string.IsNullOrWhiteSpace(username) && !loading);

            SaveProfileCommand = ReactiveCommand.CreateFromTask(SaveProfileAsync, canSave);
            CancelCommand = ReactiveCommand.Create(() => _onSaved?.Invoke());
        }

        private async Task SaveProfileAsync()
        {
            if (CurrentUser == null) return;

            try
            {
                IsLoading = true;

                string? newPasswordHash = null;
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (NewPassword.Length < 6)
                    {
                        NotificationService.ShowError("Password must be at least 6 characters");
                        return;
                    }
                    newPasswordHash = AuthenticationService.HashPassword(NewPassword);
                }

                var userId = CurrentUser.UserID;
                var username = Username.Trim();
                var profileImage = ProfileImageURL?.Trim();

                var success = await Task.Run(() => 
                    _dbService.UpdateUserProfileAsync(userId, username, profileImage, newPasswordHash));

                if (success)
                {
                    CurrentUser.Username = username;
                    CurrentUser.ProfileImageURL = profileImage;
                    
                    _sessionManager.RefreshCurrentUser();
                    
                    NotificationService.ShowSuccess("Profile updated successfully!");
                    
                    NewPassword = string.Empty;

                    await Task.Delay(1500);
                    _onSaved?.Invoke();
                }
                else
                {
                    NotificationService.ShowError("Failed to update profile");
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
}


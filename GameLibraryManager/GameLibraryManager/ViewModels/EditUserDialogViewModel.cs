using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class EditUserDialogViewModel : ViewModelBase
    {
        private readonly DatabaseService _dbService;
        private readonly User? _originalUser;
        private readonly Action<bool> _closeDialog;

        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _role = string.Empty;
        private string _newPassword = string.Empty;
        private string? _profileImageURL;
        private string _joinDate = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Role
        {
            get => _role;
            set
            {
                this.RaiseAndSetIfChanged(ref _role, value);
                this.RaisePropertyChanged(nameof(RoleBadgeColor));
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set => this.RaiseAndSetIfChanged(ref _newPassword, value);
        }

        public string? ProfileImageURL
        {
            get => _profileImageURL;
            set => this.RaiseAndSetIfChanged(ref _profileImageURL, value);
        }

        public string JoinDate
        {
            get => _joinDate;
            set => this.RaiseAndSetIfChanged(ref _joinDate, value);
        }

        public bool IsEditMode => _originalUser != null;

        public string DialogTitle => IsEditMode ? "Edit User" : "Add User";

        public string DialogSubtitle => IsEditMode ? "Update user information" : "Create a new user";

        public string PasswordLabel => IsEditMode ? "New Password" : "Password";

        public string PasswordLabelHint => IsEditMode ? "(optional)" : "(required)";

        public string PasswordWatermark => IsEditMode ? "Leave blank to keep current" : "Enter password";

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string RoleBadgeColor => Role == "Admin" ? "#8957E5" : "#238636";

        public ObservableCollection<string> AvailableRoles { get; } = new()
        {
            "User",
            "Admin"
        };

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public EditUserDialogViewModel(User user, DatabaseService dbService, Action<bool> closeDialog)
        {
            _originalUser = user;
            _dbService = dbService;
            _closeDialog = closeDialog;

            Username = user.Username;
            Email = user.Email;
            Role = user.Role;
            ProfileImageURL = user.ProfileImageURL;
            JoinDate = user.CreatedAt.ToString("dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        public EditUserDialogViewModel(DatabaseService dbService, Action<bool> closeDialog)
        {
            _originalUser = null;
            _dbService = dbService;
            _closeDialog = closeDialog;

            Role = "User";
            JoinDate = "";

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required";
                return;
            }

            IsLoading = true;

            try
            {
                if (_originalUser == null)
                {
                    if (string.IsNullOrWhiteSpace(NewPassword))
                    {
                        ErrorMessage = "Password is required for new users";
                        IsLoading = false;
                        return;
                    }

                    var passwordHash = HashPassword(NewPassword);
                    var (success, message) = await _dbService.RegisterUserAsync(
                        Email.Trim(), 
                        passwordHash, 
                        Username.Trim());

                    if (!success)
                    {
                        ErrorMessage = message;
                        return;
                    }

                    var users = await _dbService.GetAllUsersAsync();
                    var newUser = users.FirstOrDefault(u => u.Email == Email.Trim());
                    if (newUser != null)
                    {
                        if (Role != "User")
                        {
                            var roleUpdated = await _dbService.UpdateUserRoleAsync(newUser.UserID, Role);
                            if (!roleUpdated)
                            {
                                ErrorMessage = "User created but failed to set role";
                                return;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(ProfileImageURL))
                        {
                            var profileUpdated = await _dbService.UpdateUserProfileAsync(
                                newUser.UserID,
                                Username.Trim(),
                                ProfileImageURL.Trim(),
                                null);
                            if (!profileUpdated)
                            {
                                ErrorMessage = "User created but failed to set profile image";
                                return;
                            }
                        }
                    }

                    _closeDialog(true);
                }
                else
                {
                    string? passwordHash = null;
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                    {
                        passwordHash = HashPassword(NewPassword);
                    }

                    var profileUpdated = await _dbService.UpdateUserProfileAsync(
                        _originalUser.UserID,
                        Username.Trim(),
                        string.IsNullOrWhiteSpace(ProfileImageURL) ? null : ProfileImageURL.Trim(),
                        passwordHash);

                    if (!profileUpdated)
                    {
                        ErrorMessage = "Failed to update user profile";
                        return;
                    }

                    if (Role != _originalUser.Role)
                    {
                        var roleUpdated = await _dbService.UpdateUserRoleAsync(_originalUser.UserID, Role);
                        if (!roleUpdated)
                        {
                            ErrorMessage = "Profile updated but failed to update role";
                            return;
                        }
                    }

                    _closeDialog(true);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
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

        private static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}


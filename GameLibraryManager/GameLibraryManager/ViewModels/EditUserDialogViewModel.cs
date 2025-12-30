using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
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
        public ReactiveCommand<Unit, Unit> ExportUserLibraryCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportUserLibraryCommand { get; }

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
            ExportUserLibraryCommand = ReactiveCommand.CreateFromTask(ExportUserLibraryAsync);
            ImportUserLibraryCommand = ReactiveCommand.CreateFromTask(ImportUserLibraryAsync);
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
            ExportUserLibraryCommand = ReactiveCommand.CreateFromTask(ExportUserLibraryAsync);
            ImportUserLibraryCommand = ReactiveCommand.CreateFromTask(ImportUserLibraryAsync);
        }

        private async Task ExportUserLibraryAsync()
        {
            if (_originalUser == null) return;

            try
            {
                IsLoading = true;

                var xmlData = await Task.Run(() => _dbService.ExportUserLibraryToXMLAsync(_originalUser.UserID));

                if (string.IsNullOrEmpty(xmlData))
                {
                    ErrorMessage = "No library data to export";
                    return;
                }

                var topLevel = GetTopLevel();
                if (topLevel == null) return;

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = $"Export Library - {_originalUser.Username}",
                    SuggestedFileName = $"library_{_originalUser.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.xml",
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

                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to export library: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportUserLibraryAsync()
        {
            if (_originalUser == null) return;

            try
            {
                var topLevel = GetTopLevel();
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = $"Import Library - {_originalUser.Username}",
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
                    ErrorMessage = "The selected file is empty";
                    return;
                }

                IsLoading = true;

                var success = await Task.Run(() => _dbService.ImportUserLibraryFromXMLAsync(_originalUser.UserID, xmlData));

                if (success)
                {
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Failed to import library";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to import library: {ex.Message}";
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


using System.Threading.Tasks;
using GameLibraryManager.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GameLibraryManager.Services
{
    public class SessionManager : ReactiveObject
    {
        private static SessionManager? _instance;
        public static SessionManager Instance => _instance ??= new SessionManager();

        [Reactive] public User? CurrentUser { get; private set; }
        private string? _passwordHash;
        private string? _email;
        
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        private SessionManager() { }

        public void SetCurrentUser(User user, string email, string passwordHash)
        {
            CurrentUser = user;
            _email = email;
            _passwordHash = passwordHash;
        }

        public void ClearSession()
        {
            CurrentUser = null;
            _email = null;
            _passwordHash = null;
        }
        
        public void RefreshCurrentUser()
        {
            this.RaisePropertyChanged(nameof(CurrentUser));
        }

        public async Task<bool> ValidateSessionAsync(DatabaseService dbService)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_passwordHash))
            {
                return false;
            }

            try
            {
                return await dbService.ValidateUserSessionAsync(_email, _passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}


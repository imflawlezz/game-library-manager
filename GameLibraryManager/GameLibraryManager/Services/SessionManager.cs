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
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        private SessionManager() { }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void ClearSession()
        {
            CurrentUser = null;
        }
        
        public void RefreshCurrentUser()
        {
            this.RaisePropertyChanged(nameof(CurrentUser));
        }
    }
}


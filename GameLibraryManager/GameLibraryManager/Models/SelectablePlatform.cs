using ReactiveUI;

namespace GameLibraryManager.Models
{
    public class SelectablePlatform : ReactiveObject
    {
        private bool _isSelected;

        public Platform Platform { get; }
        
        public int PlatformID => Platform.PlatformID;
        public string PlatformName => Platform.PlatformName;

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public SelectablePlatform(Platform platform, bool isSelected = false)
        {
            Platform = platform;
            _isSelected = isSelected;
        }
    }
}

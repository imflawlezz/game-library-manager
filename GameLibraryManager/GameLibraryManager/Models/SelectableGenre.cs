using ReactiveUI;

namespace GameLibraryManager.Models
{
    public class SelectableGenre : ReactiveObject
    {
        private bool _isSelected;

        public Genre Genre { get; }
        
        public int GenreID => Genre.GenreID;
        public string GenreName => Genre.GenreName;

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public SelectableGenre(Genre genre, bool isSelected = false)
        {
            Genre = genre;
            _isSelected = isSelected;
        }
    }
}

using Avalonia;
using Avalonia.Controls;

namespace GameLibraryManager.Controls
{
    public partial class GameCard : UserControl
    {
        public static readonly StyledProperty<System.Windows.Input.ICommand?> CommandProperty =
            AvaloniaProperty.Register<GameCard, System.Windows.Input.ICommand?>(nameof(Command));

        public System.Windows.Input.ICommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<object?> CommandParameterProperty =
            AvaloniaProperty.Register<GameCard, object?>(nameof(CommandParameter));

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<GameCard, string>(nameof(Title));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<object?> CoverImageProperty =
            AvaloniaProperty.Register<GameCard, object?>(nameof(CoverImage));

        public object? CoverImage
        {
            get => GetValue(CoverImageProperty);
            set => SetValue(CoverImageProperty, value);
        }

        public static readonly StyledProperty<string?> BadgeTextProperty =
            AvaloniaProperty.Register<GameCard, string?>(nameof(BadgeText));

        public string? BadgeText
        {
            get => GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        public static readonly StyledProperty<decimal?> RatingProperty =
            AvaloniaProperty.Register<GameCard, decimal?>(nameof(Rating));

        public decimal? Rating
        {
            get => GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }

        public static readonly StyledProperty<decimal?> HoursProperty =
            AvaloniaProperty.Register<GameCard, decimal?>(nameof(Hours));

        public decimal? Hours
        {
            get => GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public static readonly StyledProperty<string?> DeveloperProperty =
            AvaloniaProperty.Register<GameCard, string?>(nameof(Developer));

        public string? Developer
        {
            get => GetValue(DeveloperProperty);
            set => SetValue(DeveloperProperty, value);
        }

        public static readonly StyledProperty<string?> GenresProperty =
            AvaloniaProperty.Register<GameCard, string?>(nameof(Genres));

        public string? Genres
        {
            get => GetValue(GenresProperty);
            set => SetValue(GenresProperty, value);
        }

        public static readonly StyledProperty<System.Collections.IEnumerable?> PlatformsProperty =
            AvaloniaProperty.Register<GameCard, System.Collections.IEnumerable?>(nameof(Platforms));

        public System.Collections.IEnumerable? Platforms
        {
            get => GetValue(PlatformsProperty);
            set => SetValue(PlatformsProperty, value);
        }

        public static readonly StyledProperty<bool> ShowLibraryStatsProperty =
            AvaloniaProperty.Register<GameCard, bool>(nameof(ShowLibraryStats));

        public bool ShowLibraryStats
        {
            get => GetValue(ShowLibraryStatsProperty);
            set => SetValue(ShowLibraryStatsProperty, value);
        }

        public static readonly StyledProperty<object?> ActionContentProperty =
            AvaloniaProperty.Register<GameCard, object?>(nameof(ActionContent));

        public object? ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        public GameCard()
        {
            InitializeComponent();
        }
    }
}

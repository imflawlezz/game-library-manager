using Avalonia;
using Avalonia.Controls;

namespace GameLibraryManager.Controls
{
    public partial class LoadingSpinner : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<LoadingSpinner, string>(nameof(Text), "Loading...");

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public LoadingSpinner()
        {
            InitializeComponent();
        }
    }
}


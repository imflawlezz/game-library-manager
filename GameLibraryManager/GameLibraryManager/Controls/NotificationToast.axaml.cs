using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameLibraryManager.Services;

namespace GameLibraryManager.Controls
{
    public partial class NotificationToast : UserControl
    {
        public NotificationToast()
        {
            InitializeComponent();
            DataContext = NotificationService.Instance;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}


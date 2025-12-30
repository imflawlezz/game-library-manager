using System.Diagnostics;
using Avalonia.Controls;

namespace GameLibraryManager.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void OpenGitHub(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/imflawlezz",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }
    }
}

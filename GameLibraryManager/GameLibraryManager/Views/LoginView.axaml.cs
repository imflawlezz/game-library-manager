using System.Diagnostics;
using Avalonia.Controls;
using GameLibraryManager.ViewModels;

namespace GameLibraryManager.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            this.Closing += LoginView_Closing;
        }

        private void LoginView_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Dispose();
            }
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

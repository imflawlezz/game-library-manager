using System;
using System.Reactive;
using ReactiveUI;

namespace GameLibraryManager.ViewModels
{
    public class ConfirmDeleteDialogViewModel : ViewModelBase
    {
        private readonly Action<bool> _closeDialog;
        
        private string _title = string.Empty;
        private string _message = string.Empty;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public ConfirmDeleteDialogViewModel(string title, string message, Action<bool> closeDialog)
        {
            _closeDialog = closeDialog;
            Title = title;
            Message = message;

            ConfirmCommand = ReactiveCommand.Create(Confirm);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Confirm()
        {
            _closeDialog(true);
        }

        private void Cancel()
        {
            _closeDialog(false);
        }
    }
}


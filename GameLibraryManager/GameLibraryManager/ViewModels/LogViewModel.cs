using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Threading;
using ReactiveUI;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<AuditLogEntry> _allLogEntries = new();
        private ObservableCollection<AuditLogEntry> _logEntries = new();
        private string _searchText = string.Empty;
        private bool _isLoading;
        private CancellationTokenSource? _filterCancellation;

        public NotificationService NotificationService => NotificationService.Instance;

        public ObservableCollection<AuditLogEntry> LogEntries
        {
            get => _logEntries;
            private set => this.RaiseAndSetIfChanged(ref _logEntries, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool IsEmpty => !IsLoading && (LogEntries?.Count ?? 0) == 0;

        public ReactiveCommand<Unit, Unit> LoadLogsCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; }
        public ReactiveCommand<string, Unit> CopyToClipboardCommand { get; }

        public LogViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            LoadLogsCommand = ReactiveCommand.CreateFromTask(LoadLogsAsync);
            ClearSearchCommand = ReactiveCommand.Create(ClearSearch);
            CopyToClipboardCommand = ReactiveCommand.CreateFromTask<string>(CopyToClipboardAsync);

            IsLoading = true;
            
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => Task.Run(() => ApplyFilterAsync()));
            
            Task.Run(async () => await LoadLogsAsync().ConfigureAwait(false));
        }

        private async Task LoadLogsAsync()
        {
            Dispatcher.UIThread.Post(() => IsLoading = true);

            try
            {
                var entries = await Task.Run(() => _dbService.GetAuditLogAsync(limit: 1000)).ConfigureAwait(false);
                
                Dispatcher.UIThread.Post(() =>
                {
                    _allLogEntries = new ObservableCollection<AuditLogEntry>(entries);
                    LogEntries = new ObservableCollection<AuditLogEntry>(entries);
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    NotificationService.ShowError($"Failed to load audit log: {ex.Message}");
                    IsLoading = false;
                });
            }
        }

        private async Task ApplyFilterAsync()
        {
            _filterCancellation?.Cancel();
            _filterCancellation = new CancellationTokenSource();
            var token = _filterCancellation.Token;
            
            var searchText = SearchText;
            var allEntries = _allLogEntries.ToList();
            
            var filtered = await Task.Run(() =>
            {
                if (token.IsCancellationRequested)
                    return null;
                    
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return allEntries;
                }
                else
                {
                    var searchLower = searchText.ToLowerInvariant();
                    return allEntries.Where(e =>
                        e.LogID.ToString().Contains(searchLower) ||
                        (e.Username?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        e.TableName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        e.Action.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        (e.FieldName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        e.RecordID.ToString().Contains(searchLower)
                    ).ToList();
                }
            }, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested || filtered == null)
                return;
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested)
                {
                    LogEntries = new ObservableCollection<AuditLogEntry>(filtered);
                    this.RaisePropertyChanged(nameof(IsEmpty));
                }
            });
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        public async Task CopyToClipboardAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var window = desktop.MainWindow;
                        if (window != null)
                        {
                            var clipboard = window.Clipboard;
                            if (clipboard != null)
                            {
                                await clipboard.SetTextAsync(text);
                                NotificationService.ShowSuccess("Copied to clipboard");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Failed to copy to clipboard: {ex.Message}");
            }
        }

        public bool ShouldShowCopyButton(string? fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(fieldName))
                return false;

            var copyableFields = new[] 
            {
                "Email", "Username", "ProfileImageURL", "CoverImageURL", 
                "ImageURL", "Description", "Title", "Developer", "Publisher",
                "Bio", "Notes", "PersonalNotes", "URL", "Link"
            };

            var fieldLower = fieldName.ToLowerInvariant();
            return copyableFields.Any(f => fieldLower.Contains(f.ToLowerInvariant()));
        }
    }
}


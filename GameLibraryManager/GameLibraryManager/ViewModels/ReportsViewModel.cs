using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Models;
using GameLibraryManager.Services;
using GameLibraryManager.Views;

namespace GameLibraryManager.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        [Reactive] public string SelectedType { get; set; } = "Suggestion";
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public string Content { get; set; } = string.Empty;
        [Reactive] public bool IsSubmitting { get; set; }
        [Reactive] public string SelectedView { get; set; } = "Submit";
        [Reactive] public bool IsLoading { get; set; }
        
        private ObservableCollection<ReportSuggestion> _userReports = new();
        public ObservableCollection<ReportSuggestion> UserReports
        {
            get => _userReports;
            set => this.RaiseAndSetIfChanged(ref _userReports, value);
        }
        
        public bool IsSubmitSelected => SelectedView == "Submit";
        public bool IsMyReportsSelected => SelectedView == "My Reports";
        public bool ShowSubmitForm => SelectedView == "Submit";
        public bool ShowMyReports => SelectedView == "My Reports";
        public bool IsEmpty => UserReports.Count == 0 && !IsLoading;
        public bool ShowContent => !IsLoading && !IsEmpty;
        public bool ShowEmptyState => !IsLoading && IsEmpty;
        
        public List<string> ReportTypes { get; } = new List<string> { "Suggestion", "Report" };

        public NotificationService NotificationService => NotificationService.Instance;

        public ReactiveCommand<Unit, Unit> SubmitReportCommand { get; }
        public ReactiveCommand<string, Unit> SwitchViewCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadUserReportsCommand { get; }
        public ReactiveCommand<ReportSuggestion, Unit> ViewReportCommand { get; }

        public ReportsViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            SubmitReportCommand = ReactiveCommand.CreateFromTask(SubmitReportAsync, 
                this.WhenAnyValue(x => x.Title, x => x.Content, 
                    (title, content) => !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(content) && !IsSubmitting));
            
            SwitchViewCommand = ReactiveCommand.Create<string>(SwitchView);
            LoadUserReportsCommand = ReactiveCommand.CreateFromTask(LoadUserReportsAsync);
            ViewReportCommand = ReactiveCommand.CreateFromTask<ReportSuggestion>(ViewReportAsync);
            
            this.WhenAnyValue(x => x.SelectedView)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(IsSubmitSelected));
                    this.RaisePropertyChanged(nameof(IsMyReportsSelected));
                    this.RaisePropertyChanged(nameof(ShowSubmitForm));
                    this.RaisePropertyChanged(nameof(ShowMyReports));
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                    
                    if (SelectedView == "My Reports")
                    {
                        LoadUserReportsCommand.Execute().Subscribe();
                    }
                });
        }
        
        private void SwitchView(string view)
        {
            SelectedView = view;
        }
        
        private async Task LoadUserReportsAsync()
        {
            if (_sessionManager.CurrentUser == null)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = true);

            try
            {
                var reports = await Task.Run(() => _dbService.GetReportsAsync(userId: _sessionManager.CurrentUser.UserID)).ConfigureAwait(false);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UserReports = new ObservableCollection<ReportSuggestion>(reports.OrderByDescending(r => r.CreatedAt));
                    IsLoading = false;
                    
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    NotificationService.ShowError($"Failed to load reports: {ex.Message}");
                    IsLoading = false;
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                });
            }
        }

        private async Task SubmitReportAsync()
        {
            if (_sessionManager.CurrentUser == null)
            {
                NotificationService.ShowError("You must be logged in to submit a report");
                return;
            }

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                NotificationService.ShowError("Title and content are required");
                return;
            }

            IsSubmitting = true;

            try
            {
                var reportId = await _dbService.CreateReportAsync(
                    _sessionManager.CurrentUser.UserID,
                    SelectedType,
                    Title.Trim(),
                    Content.Trim()
                );

                if (reportId > 0)
                {
                    NotificationService.ShowSuccess($"{SelectedType} submitted successfully");
                    Title = string.Empty;
                    Content = string.Empty;
                    
                    SelectedView = "My Reports";
                    await LoadUserReportsAsync();
                }
                else
                {
                    NotificationService.ShowError("Failed to submit report");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
        
        private async Task ViewReportAsync(ReportSuggestion report)
        {
            if (report == null)
                return;

            var dialog = new ReviewReportDialog();
            
            var viewModel = new ReviewReportDialogViewModel(report, _sessionManager.CurrentUser?.UserID ?? 0, _dbService, (reviewed) =>
            {
                dialog.Close();
            }, isReadOnly: true);
            dialog.DataContext = viewModel;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                await dialog.ShowDialog(parentWindow);
            }
        }
        
        private Window? GetParentWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }
}


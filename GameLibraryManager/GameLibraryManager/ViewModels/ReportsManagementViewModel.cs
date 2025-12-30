using System;
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
using GameLibraryManager;

namespace GameLibraryManager.ViewModels
{
    public class ReportsManagementViewModel : ViewModelBase
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseService _dbService;

        private ObservableCollection<ReportSuggestion> _allReports = new();
        private ObservableCollection<ReportSuggestion> _unreviewedReports = new();
        private ObservableCollection<ReportSuggestion> _reviewedReports = new();
        private bool _isLoading;
        private int _unreviewedCount;
        private string _selectedView = "Unreviewed";

        public NotificationService NotificationService => NotificationService.Instance;

        public ObservableCollection<ReportSuggestion> UnreviewedReports
        {
            get => _unreviewedReports;
            private set
            {
                this.RaiseAndSetIfChanged(ref _unreviewedReports, value);
                this.RaisePropertyChanged(nameof(CurrentReports));
                this.RaisePropertyChanged(nameof(IsEmpty));
                this.RaisePropertyChanged(nameof(ShowContent));
                this.RaisePropertyChanged(nameof(ShowEmptyState));
            }
        }

        public ObservableCollection<ReportSuggestion> ReviewedReports
        {
            get => _reviewedReports;
            private set
            {
                this.RaiseAndSetIfChanged(ref _reviewedReports, value);
                this.RaisePropertyChanged(nameof(CurrentReports));
                this.RaisePropertyChanged(nameof(IsEmpty));
                this.RaisePropertyChanged(nameof(ShowContent));
                this.RaisePropertyChanged(nameof(ShowEmptyState));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (this.RaiseAndSetIfChanged(ref _isLoading, value))
                {
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                }
            }
        }

        public int UnreviewedCount
        {
            get => _unreviewedCount;
            private set => this.RaiseAndSetIfChanged(ref _unreviewedCount, value);
        }

        public string SelectedView
        {
            get => _selectedView;
            set
            {
                if (_selectedView != value)
                {
                    _selectedView = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsUnreviewedSelected));
                    this.RaisePropertyChanged(nameof(IsReviewedSelected));
                    this.RaisePropertyChanged(nameof(CurrentReports));
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                }
            }
        }

        public bool IsUnreviewedSelected => SelectedView == "Unreviewed";
        public bool IsReviewedSelected => SelectedView == "Reviewed";

        public ObservableCollection<ReportSuggestion> CurrentReports
        {
            get
            {
                return SelectedView == "Unreviewed" ? UnreviewedReports : ReviewedReports;
            }
        }

        public bool IsEmpty => CurrentReports.Count == 0 && !IsLoading;
        public bool ShowContent => !IsLoading && !IsEmpty;
        public bool ShowEmptyState => !IsLoading && IsEmpty;

        public ReactiveCommand<Unit, Unit> LoadReportsCommand { get; }
        public ReactiveCommand<string, Unit> SwitchViewCommand { get; }
        public ReactiveCommand<ReportSuggestion, Unit> ReviewReportCommand { get; }
        public ReactiveCommand<ReportSuggestion, Unit> ViewReportCommand { get; }
        public ReactiveCommand<ReportSuggestion, Unit> DeleteReportCommand { get; }

        public ReportsManagementViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            LoadReportsCommand = ReactiveCommand.CreateFromTask(LoadReportsAsync);
            SwitchViewCommand = ReactiveCommand.Create<string>(view => SelectedView = view);
            ReviewReportCommand = ReactiveCommand.CreateFromTask<ReportSuggestion>(ReviewReportAsync);
            ViewReportCommand = ReactiveCommand.CreateFromTask<ReportSuggestion>(ViewReportAsync);
            DeleteReportCommand = ReactiveCommand.CreateFromTask<ReportSuggestion>(DeleteReportAsync);

            this.WhenAnyValue(x => x.SelectedView)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(CurrentReports));
                    this.RaisePropertyChanged(nameof(IsUnreviewedSelected));
                    this.RaisePropertyChanged(nameof(IsReviewedSelected));
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                });
            
            this.WhenAnyValue(x => x.UnreviewedReports.Count, x => x.ReviewedReports.Count)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(IsEmpty));
                    this.RaisePropertyChanged(nameof(ShowContent));
                    this.RaisePropertyChanged(nameof(ShowEmptyState));
                });

            IsLoading = true;
            LoadReportsCommand.Execute().Subscribe();
        }

        private async Task LoadReportsAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = true);

            try
            {
                var reports = await Task.Run(() => _dbService.GetReportsAsync()).ConfigureAwait(false);
                var count = await Task.Run(() => _dbService.GetUnreviewedReportsCountAsync()).ConfigureAwait(false);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _allReports = new ObservableCollection<ReportSuggestion>(reports);
                    var unreviewed = new ObservableCollection<ReportSuggestion>(
                        reports.Where(r => r.Status == "Unreviewed").OrderByDescending(r => r.CreatedAt));
                    var reviewed = new ObservableCollection<ReportSuggestion>(
                        reports.Where(r => r.Status != "Unreviewed").OrderByDescending(r => r.ReviewedAt ?? r.CreatedAt));
                    
                    UnreviewedReports = unreviewed;
                    ReviewedReports = reviewed;
                    UnreviewedCount = count;
                    IsLoading = false;
                    
                    // Force update CurrentReports after both collections are set
                    this.RaisePropertyChanged(nameof(CurrentReports));
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
                });
            }
        }

        private async Task<bool> ValidateSessionAsync()
        {
            try
            {
                var isValid = await _sessionManager.ValidateSessionAsync(_dbService);
                if (!isValid)
                {
                    NotificationService.ShowError("Your session has expired. Please log in again.");
                    Logout();
                    return false;
                }
                return true;
            }
            catch
            {
                NotificationService.ShowError("Session validation failed. Please log in again.");
                Logout();
                return false;
            }
        }

        private void Logout()
        {
            try
            {
                _sessionManager.ClearSession();
                
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    App.ShowLoginWindow(desktop);
                }
            }
            catch
            {
            }
        }

        private async Task ReviewReportAsync(ReportSuggestion report)
        {
            if (report == null || _sessionManager.CurrentUser == null)
                return;
            if (!await ValidateSessionAsync()) return;

            var dialog = new ReviewReportDialog();
            bool reviewCompleted = false;
            
            var viewModel = new ReviewReportDialogViewModel(report, _sessionManager.CurrentUser.UserID, _dbService, (reviewed) =>
            {
                reviewCompleted = reviewed;
                dialog.Close();
            }, isReadOnly: false);
            dialog.DataContext = viewModel;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                await dialog.ShowDialog(parentWindow);
                
                if (reviewCompleted)
                {
                    await LoadReportsAsync();
                    NotificationService.ShowSuccess("Report reviewed successfully");
                    
                    if (parentWindow.DataContext is MainWindowViewModel mainVm)
                    {
                        var count = await _dbService.GetUnreviewedReportsCountAsync();
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            mainVm.UnreviewedReportsCount = count;
                            mainVm.RaisePropertyChanged(nameof(MainWindowViewModel.ShowUnreviewedBadge));
                        });
                    }
                }
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

        private async Task DeleteReportAsync(ReportSuggestion report)
        {
            if (report == null)
                return;
            if (!await ValidateSessionAsync()) return;

            var parentWindow = GetParentWindow();
            if (parentWindow != null)
            {
                var confirmDialog = new ConfirmDeleteDialog();
                var confirmViewModel = new ConfirmDeleteDialogViewModel(
                    $"Delete {report.Type}?",
                    $"This will permanently delete the {report.Type.ToLower()} \"{report.Title}\". This action cannot be undone.",
                    (confirmed) => confirmDialog.Close(confirmed)
                );
                confirmDialog.DataContext = confirmViewModel;

                var result = await confirmDialog.ShowDialog<bool?>(parentWindow);
                if (result != true) return;
            }

            IsLoading = true;

            try
            {
                var success = await _dbService.DeleteReportAsync(report.ReportID);
                if (success)
                {
                    await LoadReportsAsync();
                    NotificationService.ShowSuccess($"{report.Type} deleted successfully");
                }
                else
                {
                    NotificationService.ShowError("Failed to delete report");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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


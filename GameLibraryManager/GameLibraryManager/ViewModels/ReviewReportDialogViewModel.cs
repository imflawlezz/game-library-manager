using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

namespace GameLibraryManager.ViewModels
{
    public class ReviewReportDialogViewModel : ViewModelBase
    {
        private readonly ReportSuggestion _report;
        private readonly int _reviewedBy;
        private readonly DatabaseService _dbService;
        private readonly Action<bool> _closeCallback;
        private readonly bool _isReadOnly;

        [Reactive] public string SelectedStatus { get; set; } = "Reviewed";
        [Reactive] public string AdminNotes { get; set; } = string.Empty;
        [Reactive] public bool IsSubmitting { get; set; }
        
        public bool IsReadOnly => _isReadOnly;
        public List<string> StatusOptions { get; } = new List<string> { "Reviewed", "Resolved", "Dismissed" };

        public ReportSuggestion Report => _report;
        public NotificationService NotificationService => NotificationService.Instance;

        public ReactiveCommand<Unit, Unit>? SubmitReviewCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public ReviewReportDialogViewModel(ReportSuggestion report, int reviewedBy, DatabaseService dbService, Action<bool> closeCallback, bool isReadOnly = false)
        {
            _report = report;
            _reviewedBy = reviewedBy;
            _dbService = dbService;
            _closeCallback = closeCallback;
            _isReadOnly = isReadOnly;

            if (!isReadOnly)
            {
                SelectedStatus = report.Status != "Unreviewed" ? report.Status : "Reviewed";
                AdminNotes = report.AdminNotes ?? string.Empty;
                SubmitReviewCommand = ReactiveCommand.CreateFromTask(SubmitReviewAsync, 
                    this.WhenAnyValue(x => x.IsSubmitting).Select(x => !x));
            }
            else
            {
                SelectedStatus = report.Status;
                AdminNotes = report.AdminNotes ?? string.Empty;
                SubmitReviewCommand = null;
            }
            
            CancelCommand = ReactiveCommand.Create(() => _closeCallback(false));
        }

        private async Task SubmitReviewAsync()
        {
            IsSubmitting = true;

            try
            {
                var success = await _dbService.ReviewReportAsync(
                    _report.ReportID,
                    _reviewedBy,
                    SelectedStatus,
                    string.IsNullOrWhiteSpace(AdminNotes) ? null : AdminNotes.Trim()
                );

                if (success)
                {
                    _closeCallback(true);
                }
                else
                {
                    NotificationService.ShowError("Failed to review report");
                    IsSubmitting = false;
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Error: {ex.Message}");
                IsSubmitting = false;
            }
        }
    }
}


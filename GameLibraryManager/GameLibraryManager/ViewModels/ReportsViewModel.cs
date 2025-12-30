using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using GameLibraryManager.Models;
using GameLibraryManager.Services;

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
        
        public List<string> ReportTypes { get; } = new List<string> { "Suggestion", "Report" };

        public NotificationService NotificationService => NotificationService.Instance;

        public ReactiveCommand<Unit, Unit> SubmitReportCommand { get; }

        public ReportsViewModel(SessionManager sessionManager, DatabaseService dbService)
        {
            _sessionManager = sessionManager;
            _dbService = dbService;

            SubmitReportCommand = ReactiveCommand.CreateFromTask(SubmitReportAsync, 
                this.WhenAnyValue(x => x.Title, x => x.Content, 
                    (title, content) => !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(content) && !IsSubmitting));
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
    }
}


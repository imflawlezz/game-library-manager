using System;
using System.Timers;
using ReactiveUI;

namespace GameLibraryManager.Services
{
    public enum NotificationType
    {
        Info,
        Success,
        Error
    }

    public class NotificationMessage
    {
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
    }

    public class NotificationService : ReactiveObject
    {
        private static readonly NotificationService _instance = new NotificationService();
        public static NotificationService Instance => _instance;

        private NotificationMessage? _currentNotification;
        private readonly System.Collections.Generic.Queue<NotificationQueueItem> _queue = new();
        private readonly Timer _timer;
        private bool _isProcessing;

        private class NotificationQueueItem
        {
            public string Message { get; set; } = string.Empty;
            public NotificationType Type { get; set; }
            public int DurationMs { get; set; }
        }

        public NotificationMessage? CurrentNotification
        {
            get => _currentNotification;
            private set => this.RaiseAndSetIfChanged(ref _currentNotification, value);
        }

        private NotificationService()
        {
            _timer = new Timer();
            _timer.Elapsed += (s, e) => ProcessNext();
            _timer.AutoReset = false;
        }

        private void ProcessNext()
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                {
                    CurrentNotification = null;
                    _isProcessing = false;
                    return;
                }

                var item = _queue.Dequeue();
                CurrentNotification = new NotificationMessage { Message = item.Message, Type = item.Type };
                
                if (item.DurationMs > 0)
                {
                    _timer.Interval = item.DurationMs;
                    _timer.Start();
                }
            }
        }

        public void Show(string message, NotificationType type = NotificationType.Info, int durationMs = 4000)
        {
            lock (_queue)
            {
                _queue.Enqueue(new NotificationQueueItem 
                { 
                    Message = message, 
                    Type = type, 
                    DurationMs = durationMs 
                });

                if (!_isProcessing)
                {
                    _isProcessing = true;
                    ProcessNext();
                }
            }
        }

        public void ShowSuccess(string message) => Show(message, NotificationType.Success);
        public void ShowError(string message) => Show(message, NotificationType.Error);
        public void ShowInfo(string message) => Show(message, NotificationType.Info);

        public void Clear()
        {
            lock (_queue)
            {
                _timer.Stop();
                _queue.Clear();
                CurrentNotification = null;
                _isProcessing = false;
            }
        }
    }
}

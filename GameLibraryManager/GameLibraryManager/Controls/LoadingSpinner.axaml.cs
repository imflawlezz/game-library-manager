using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace GameLibraryManager.Controls
{
    public partial class LoadingSpinner : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<LoadingSpinner, string>(nameof(Text), "Loading...");

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private DispatcherTimer? _rotationTimer;
        private double _rotationAngle = 0;

        public LoadingSpinner()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            StartRotation();
        }

        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            StopRotation();
        }

        private void StartRotation()
        {
            if (SpinnerIcon?.RenderTransform is not RotateTransform rotateTransform)
                return;

            _rotationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _rotationTimer.Tick += (s, e) =>
            {
                _rotationAngle += 6; // 6 degrees per frame = 360 degrees in 1 second at 60 FPS
                if (_rotationAngle >= 360)
                    _rotationAngle = 0;
                
                rotateTransform.Angle = _rotationAngle;
            };
            _rotationTimer.Start();
        }

        private void StopRotation()
        {
            _rotationTimer?.Stop();
            _rotationTimer = null;
        }
    }
}


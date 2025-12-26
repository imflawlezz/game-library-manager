using System;
using System.IO;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace GameLibraryManager.Controls
{
    public partial class AsyncImage : UserControl
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly ConcurrentDictionary<string, Bitmap?> _cache = new();
        private CancellationTokenSource? _loadCts;

        public static readonly StyledProperty<string?> SourceProperty =
            AvaloniaProperty.Register<AsyncImage, string?>(nameof(Source));

        public string? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public AsyncImage()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SourceProperty)
            {
                LoadImageAsync(change.GetNewValue<string?>());
            }
        }

        private async void LoadImageAsync(string? url)
        {
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;

            ImageControl.Opacity = 0;
            ImageControl.Source = null;
            LoadingPlaceholder.Opacity = 1;

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                Bitmap? bitmap = null;
                
                if (_cache.TryGetValue(url, out var cachedBitmap) && cachedBitmap != null)
                {
                    bitmap = cachedBitmap;
                }
                else
                {
                    bitmap = await Task.Run(async () =>
                    {
                        try
                        {
                            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                var imageBytes = await _httpClient.GetByteArrayAsync(url, token);
                                using var stream = new MemoryStream(imageBytes);
                                var bmp = new Bitmap(stream);
                                _cache[url] = bmp;
                                return bmp;
                            }

                            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                            {
                                var filePath = new Uri(url).LocalPath;
                                if (File.Exists(filePath))
                                {
                                    var bmp = new Bitmap(filePath);
                                    _cache[url] = bmp;
                                    return bmp;
                                }
                            }

                            if (File.Exists(url))
                            {
                                var bmp = new Bitmap(url);
                                _cache[url] = bmp;
                                return bmp;
                            }

                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }, token);
                }

                if (token.IsCancellationRequested)
                    return;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (!token.IsCancellationRequested && bitmap != null)
                    {
                        ImageControl.Source = bitmap;
                        ImageControl.Opacity = 1;
                        LoadingPlaceholder.Opacity = 0;
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }
    }
}

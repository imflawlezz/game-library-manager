using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace GameLibraryManager.Converters
{
    public class ImageUrlConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Bitmap?> _cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string url || string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                if (_cache.TryGetValue(url, out var cachedBitmap))
                {
                    return cachedBitmap;
                }

                Bitmap? bitmap = null;

                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var imageBytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                        using var stream = new MemoryStream(imageBytes);
                        bitmap = new Bitmap(stream);
                        _cache[url] = bitmap;
                        return bitmap;
                    }
                    catch
                    {
                        _cache[url] = null;
                        return null;
                    }
                }

                if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var filePath = new Uri(url).LocalPath;
                        if (File.Exists(filePath))
                        {
                            bitmap = new Bitmap(filePath);
                            _cache[url] = bitmap;
                            return bitmap;
                        }
                    }
                    catch
                    {
                    }
                }

                if (url.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var uri = new Uri(url);
                    }
                    catch
                    {
                    }
                }

                if (File.Exists(url))
                {
                    try
                    {
                        bitmap = new Bitmap(url);
                        _cache[url] = bitmap;
                        return bitmap;
                    }
                    catch
                    {
                    }
                }

                _cache[url] = null;
                return null;
            }
            catch
            {
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


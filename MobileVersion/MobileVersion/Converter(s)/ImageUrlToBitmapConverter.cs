using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Utilities;

namespace MobileVersion.Converters
{
    public class ImageUrlToBitmapConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, Bitmap?> _cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrEmpty(url))
            {
                // Check cache first
                if (_cache.TryGetValue(url, out var cachedBitmap))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] Cache hit for {url}");
                    return cachedBitmap;
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] Loading {url}");
                    
                    // Download synchronously (blocking)
                    byte[] imageBytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    var bitmap = new Bitmap(new MemoryStream(imageBytes));
                    
                    // Cache it
                    _cache[url] = bitmap;
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] Successfully loaded {url}");
                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] Failed to load {url}: {ex.Message}");
                    _cache[url] = null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

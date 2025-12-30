using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GameLibraryManager.Converters
{
    public class TableNameColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string tableName)
            {
                return tableName.ToLower() switch
                {
                    "users" => Color.Parse("#A371F7"), // Purple
                    "games" => Color.Parse("#F85149"), // Red
                    "usergames" => Color.Parse("#238636"), // Green
                    "reports" => Color.Parse("#58A6FF"), // Blue
                    _ => Color.Parse("#8B949E") // Default gray
                };
            }
            return Color.Parse("#8B949E");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


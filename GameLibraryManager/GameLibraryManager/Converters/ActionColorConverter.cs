using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GameLibraryManager.Converters
{
    public class ActionColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string action)
            {
                return action.ToUpper() switch
                {
                    "INSERT" => Color.Parse("#238636"), // Green
                    "UPDATE" => Color.Parse("#58A6FF"), // Blue
                    "DELETE" => Color.Parse("#F85149"), // Red
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


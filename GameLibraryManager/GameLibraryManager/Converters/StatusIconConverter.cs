using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GameLibraryManager.Converters
{
    public class StatusIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Resolved" => "fa-solid fa-check-circle",
                    "Dismissed" => "fa-solid fa-times-circle",
                    "Reviewed" => "fa-solid fa-list-check",
                    _ => "fa-solid fa-circle"
                };
            }
            return "fa-solid fa-circle";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Resolved" => Color.Parse("#238636"),
                    "Dismissed" => Color.Parse("#F85149"),
                    "Reviewed" => Color.Parse("#58A6FF"),
                    _ => Color.Parse("#6E7681")
                };
            }
            return Color.Parse("#6E7681");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameLibraryManager.Converters
{
    public class ActionIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string action)
            {
                return action.ToUpper() switch
                {
                    "INSERT" => "fa-solid fa-plus-circle",
                    "UPDATE" => "fa-solid fa-pencil",
                    "DELETE" => "fa-solid fa-trash",
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
}


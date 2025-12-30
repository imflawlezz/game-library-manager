using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace GameLibraryManager.Converters
{
    public class BoolToSelectedBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                if (targetType == typeof(IBrush) || targetType == typeof(Brush))
                {
                    return new SolidColorBrush(Color.Parse("#58A6FF"));
                }
                return Color.Parse("#58A6FF");
            }
            if (targetType == typeof(IBrush) || targetType == typeof(Brush))
            {
                return new SolidColorBrush(Color.Parse("#00000000"));
            }
            return Color.Parse("#00000000");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToSelectedForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                if (targetType == typeof(IBrush) || targetType == typeof(Brush))
                {
                    return new SolidColorBrush(Color.Parse("#FFFFFF"));
                }
                return Color.Parse("#FFFFFF");
            }
            if (targetType == typeof(IBrush) || targetType == typeof(Brush))
            {
                return new SolidColorBrush(Color.Parse("#8B949E"));
            }
            return Color.Parse("#8B949E");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
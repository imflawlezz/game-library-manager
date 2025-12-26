using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using GameLibraryManager.Models;
using GameLibraryManager.ViewModels;

namespace GameLibraryManager.Converters
{
    public class ShouldShowCopyConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 3)
                return false;

            var value = values[0] as string;
            var fieldName = values[1] as string;
            var viewModel = values[2] as LogViewModel;

            if (viewModel == null || string.IsNullOrEmpty(value) || string.IsNullOrEmpty(fieldName))
                return false;

            return viewModel.ShouldShowCopyButton(fieldName, value);
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


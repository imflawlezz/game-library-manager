using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GameLibraryManager.Services;

namespace GameLibraryManager.Converters
{
    public static class NotificationConverters
    {
        public static readonly IValueConverter TypeToBackground =
            new FuncValueConverter<NotificationType, IBrush>(type =>
            {
                return type switch
                {
                    NotificationType.Success => Brush.Parse("#238636"),
                    NotificationType.Error => Brush.Parse("#F85149"),
                    _ => Brush.Parse("#21262D")
                };
            });

        public static readonly IValueConverter TypeToIcon =
            new FuncValueConverter<NotificationType, string>(type =>
            {
                return type switch
                {
                    NotificationType.Success => "fa-solid fa-circle-check",
                    NotificationType.Error => "fa-solid fa-circle-exclamation",
                    _ => "fa-solid fa-circle-info"
                };
            });
    }
}

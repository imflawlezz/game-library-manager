using Avalonia;
using Avalonia.Controls;
using System;
using Avalonia.ReactiveUI;

namespace GameLibraryManager.Views
{
    public partial class AdminPanelView : UserControl
    {
        public AdminPanelView()
        {
            InitializeComponent();
            
            this.GetObservable(BoundsProperty).Subscribe(bounds => UpdateColumns(bounds.Width));
        }

        private void UpdateColumns(double width)
        {
            if (width <= 0) return;
            
            var availableWidth = width - 40; 
            var targetCardWidth = 240;
            
            var columns = Math.Max(1, (int)(availableWidth / targetCardWidth));
            
            GameColumns = columns;
        }

        public static readonly StyledProperty<int> GameColumnsProperty =
            AvaloniaProperty.Register<AdminPanelView, int>(nameof(GameColumns), defaultValue: 4);

        public int GameColumns
        {
            get => GetValue(GameColumnsProperty);
            set => SetValue(GameColumnsProperty, value);
        }
    }
}


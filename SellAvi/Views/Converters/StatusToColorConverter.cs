using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SellAvi.Views.Converters
{
    internal class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.LightGray;

            //var c = new BrushConverter().ConvertFromString((string)parameter) as SolidColorBrush;
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace SellAvi.Views.Converters
{
    /// <summary>
    ///     http://stackoverflow.com/questions/3361362/wpf-radiobutton-two-binding-to-boolean-value
    ///     http://www.codeproject.com/Articles/24330/WPF-Bind-to-Opposite-Boolean-Value-Using-a-Convert
    /// </summary>
    public class RadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (value is bool) return !(bool) value;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (value is bool) return !(bool) value;

            return value;
        }
    }
}
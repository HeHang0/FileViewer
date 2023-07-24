using System;
using System.Globalization;
using System.Windows.Data;

namespace FileViewer.Base.Converter
{
    public class DecreaseValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var success = double.TryParse(parameter?.ToString(), out double decrease);
            return ((double?)value ?? 0) - (success ? decrease : 0);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            var success = double.TryParse(parameter?.ToString(), out double decrease);
            return ((double?)value ?? 0) + (success ? decrease : 0);
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace FileViewer.Base.Converter
{
    internal class HalfLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double?)value ?? 0) / 2;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return ((double?)value ?? 0) * 2;
        }
    }
}

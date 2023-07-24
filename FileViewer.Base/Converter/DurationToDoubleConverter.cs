using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileViewer.Base.Converter
{
    public class DurationToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (Duration)value;
            return duration.HasTimeSpan ? duration.TimeSpan.TotalMilliseconds : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Duration(TimeSpan.FromMilliseconds((double)value));
        }
    }
}

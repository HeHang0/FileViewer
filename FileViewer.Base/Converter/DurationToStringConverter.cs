using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileViewer.Base.Converter
{
    public class DurationToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (Duration)value;
            return duration.HasTimeSpan ? duration.TimeSpan.ToString("hh\\:mm\\:ss") : new TimeSpan().ToString("hh\\:mm\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Duration(TimeSpan.FromMilliseconds(0));
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileViewer.Base.Converter
{
    public class ParentHeightToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double height = (double)value;
            _ = double.TryParse(parameter.ToString(), out double controlHarfHeight);
            Thickness margin = new()
            {
                Top = height / 2 - controlHarfHeight
            };
            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}

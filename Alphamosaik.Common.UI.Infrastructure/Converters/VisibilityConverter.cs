using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = (bool) value;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO: ConvertBack cannot throw an exception! Can't leave this like this!
            throw new NotImplementedException();
        }
    }
}
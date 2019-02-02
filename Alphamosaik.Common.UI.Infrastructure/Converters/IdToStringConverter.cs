using System;
using System.Globalization;
using System.Windows.Data;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class IdToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Convert the Id in the Model object to a string. If the Id is 0, convert it to an empty.string
            if (value == null) return string.Empty;

            long result;
            long.TryParse(value.ToString(), out result);
            return (result == 0) ? string.Empty : result.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Convert the Id in the textbox to an integer. If it is blank, convert it to a 0
            long result;
            long.TryParse(value.ToString(), out result);
            return (result == 0) ? (object)null : result;
        }
    }
}
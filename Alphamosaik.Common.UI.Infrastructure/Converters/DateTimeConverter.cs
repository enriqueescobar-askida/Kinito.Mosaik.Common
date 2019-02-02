using System;
using System.Globalization;
using System.Windows.Data;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            DateTime theDate = (DateTime)value;
            string formattedDate = string.Empty;

            if (parameter == null)
                formattedDate = theDate.ToString(culture);
            else
                formattedDate = theDate.ToString(parameter as string, culture);

            return formattedDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string formattedDate = value.ToString();
            DateTime theDate;
            bool canConvert = DateTime.TryParse(formattedDate, out theDate);
            if (!canConvert)
                return null;
            return theDate;
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //int decimalDigit;
            //int.TryParse(parameter.ToString(), out decimalDigit);

            NumberFormatInfo nfi = culture.NumberFormat;
            nfi.CurrencyDecimalDigits = 4;
            //nfi.CurrencyDecimalDigits = decimalDigit;

            decimal amount = System.Convert.ToDecimal(value);
            string c = amount.ToString("C", nfi);

            return c;
        }

        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    decimal amount = System.Convert.ToDecimal(value);
        //    parameter = (parameter.ToString().ToUpper() == "C" )? ".0000" : "C";
        //    string c = amount.ToString(parameter as string, culture);
        //    return c;
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string amountString = value.ToString();

            decimal amount;
            //decimal.TryParse(amountString, NumberStyles.Currency, CultureInfo.CurrentCulture, out amount);
            decimal.TryParse(amountString, NumberStyles.Currency, culture, out amount);
            
            //decimal amount = 0;
            //if (!decimal.TryParse(amountString, out amount))
            //    amount = 0;
            return amount;
        }
    }
}
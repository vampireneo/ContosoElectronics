using System;
using Windows.UI.Xaml.Data;

namespace ContosoElectronics.Common
{
    internal class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                decimal price;
                return decimal.TryParse(value.ToString(), out price) ? price.ToString("F2") : "0.00";
            }
            catch (Exception)
            {
                return "0.00";
            }
        }

        public static object Convert(object value)
        {
            try
            {
                decimal price;
                return decimal.TryParse(value.ToString(), out price) ? price.ToString("F2") : "0.00";
            }
            catch (Exception)
            {
                return "0.00";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

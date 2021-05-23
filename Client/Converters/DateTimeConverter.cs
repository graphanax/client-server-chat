using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return default;

            if (!(value is DateTime))
                throw new ArgumentException(nameof(value));

            var date = (DateTime)value;
            return date.ToString("HH:mm:ss");                      
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

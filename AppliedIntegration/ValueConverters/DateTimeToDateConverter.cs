using System;
using System.Globalization;
using System.Windows.Data;

namespace AppliedIntegration.ValueConverters;

// Value converter for converting DateTime to Date
public class DateTimeToDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var dateTime = (DateTime)value;
        return dateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
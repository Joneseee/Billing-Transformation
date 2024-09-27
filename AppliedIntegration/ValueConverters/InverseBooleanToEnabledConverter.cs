using System;
using System.Globalization;
using System.Windows.Data;

namespace AppliedIntegration.ValueConverters;

// Value converter for determining if a View element should be enabled
public class InverseBooleanToEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
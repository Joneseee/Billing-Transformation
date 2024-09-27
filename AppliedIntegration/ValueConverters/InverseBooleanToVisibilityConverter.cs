using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppliedIntegration.ValueConverters;

// Value converter for determining if a View element should be visible
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace AppliedIntegration.ValueConverters;

public class DecimalToCurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        decimal d = (decimal)value;
        return d.ToString("c");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? s = value.ToString();
        if (!string.IsNullOrWhiteSpace(s))
        {
            return decimal.Parse(s);
        }
        return string.Empty;
    }
}
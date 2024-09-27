using System;
using System.Text.RegularExpressions;

namespace IntegrationLogic.Extensions;

public static class StringExtension
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        value = value.Trim();
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    public static int StrToInt(this string value, int @default)
    {
        if(string.IsNullOrEmpty(value)) return @default;
        return int.TryParse(value.Trim(), out int number) ? number : @default;
    }

    public static decimal StrToDec(this string value, decimal @default)
    {
        if (string.IsNullOrEmpty(value)) return @default;
        return decimal.TryParse(value, out decimal number) ? number : @default;
    }

    public static bool ValidPostalCode(this string value)
    {
        Regex regEx = new(@"^((\d{5}-\d{4})|(\d{5})|([A-Z]\d[A-Z]\s\d[A-Z]\d))$");

        if (value.Trim().Length != 9) return regEx.IsMatch(value.Trim());

        if (value.Substring(5, 1) != "-")
        {
            value = value.Insert(5, "-");
        }
        return regEx.IsMatch(value.Trim());
    }

    public static bool ValidNorthAmericanPhoneNumber(this string value)
    {
        Regex regEx = new(@"(?<!\d)\d{10}(?!\d)");

        if(!regEx.IsMatch(value))
        {
            return false;
        }

        if (value.Substring(0, 1).Equals("0", StringComparison.OrdinalIgnoreCase) ||
            value.Substring(3, 1).Equals("0", StringComparison.OrdinalIgnoreCase) ||
            value.Substring(0, 1).Equals("1", StringComparison.OrdinalIgnoreCase) ||
            value.Substring(3, 1).Equals("1", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static bool ValidForeignPhoneNumber(this string value)
    {
        Regex regEx = new(@"^[a-zA-Z0-9]+$");

        if (!regEx.IsMatch(value))
        {
            return false;
        }

        return value.Length <= 15;
    }
}
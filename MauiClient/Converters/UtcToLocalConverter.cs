using System.Globalization;

namespace MauiClient.Converters;

public class UtcToLocalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToLocalTime();
        }
        else if (DateTime.TryParse(value?.ToString(), out DateTime parsedDateTime))
        {
            return parsedDateTime.ToLocalTime();
        }

        return DateTime.MinValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToUniversalTime();
        }
        else if (DateTime.TryParse(value?.ToString(), out DateTime parsedDateTime))
        {
            return parsedDateTime.ToUniversalTime();
        }

        return DateTime.MinValue;
    }
}

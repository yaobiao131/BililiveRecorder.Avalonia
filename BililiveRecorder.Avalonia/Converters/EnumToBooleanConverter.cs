using System.Globalization;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

internal class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        return value?.Equals(parameter) == true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        return value?.Equals(true) == true ? parameter : null;
    }
}

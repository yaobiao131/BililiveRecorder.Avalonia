using System.Globalization;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

internal class ShortRoomIdToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        return value is int i && i != 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}

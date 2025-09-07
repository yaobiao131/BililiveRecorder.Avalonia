using System.Globalization;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

public class MultiBoolToValueConverter : IMultiValueConverter
{
    public object? FalseValue { get; set; }
    public object? TrueValue { get; set; }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is false)
            {
                return FalseValue;
            }
        }

        return TrueValue;
    }
}

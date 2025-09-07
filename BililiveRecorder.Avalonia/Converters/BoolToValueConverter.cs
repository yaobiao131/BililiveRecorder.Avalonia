using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

public class BoolToValueConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<object> TrueValueProperty = AvaloniaProperty.Register<BoolToValueConverter, object>(nameof(TrueValue));

    public static readonly StyledProperty<object> FalseValueProperty = AvaloniaProperty.Register<BoolToValueConverter, object>(nameof(FalseValue));

    public object TrueValue
    {
        get => GetValue(TrueValueProperty);
        set => SetValue(TrueValueProperty, value);
    }

    public object FalseValue
    {
        get => GetValue(FalseValueProperty);
        set => SetValue(FalseValueProperty, value);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? FalseValue : (bool)value ? TrueValue : FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && value.Equals(TrueValue);
    }
}
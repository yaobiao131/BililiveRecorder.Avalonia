using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

public class IsNaNToValueConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<object> TrueValueProperty =
        AvaloniaProperty.Register<IsNaNToValueConverter, object>(nameof(TrueValue));

    public static readonly StyledProperty<object> FalseValueProperty =
        AvaloniaProperty.Register<IsNaNToValueConverter, object>(nameof(FalseValue));

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

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double.NaN ? TrueValue : FalseValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

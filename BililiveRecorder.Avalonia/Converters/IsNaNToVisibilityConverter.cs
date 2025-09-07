using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

internal class IsNaNToVisibilityConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<bool> TrueValueProperty =
        AvaloniaProperty.Register<IsNaNToVisibilityConverter, bool>(nameof(TrueValue));

    public static readonly StyledProperty<bool> FalseValueProperty =
        AvaloniaProperty.Register<IsNaNToVisibilityConverter, bool>(nameof(FalseValue));

    public bool TrueValue
    {
        get => GetValue(TrueValueProperty);
        set => SetValue(TrueValueProperty, value);
    }

    public bool FalseValue
    {
        get => GetValue(FalseValueProperty);
        set => SetValue(FalseValueProperty, value);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo) =>
        value is double.NaN ? TrueValue : FalseValue;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

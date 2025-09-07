using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BililiveRecorder.Avalonia.Converters;

public class RatioToArrowIconConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<object> UpArrowProperty =
        AvaloniaProperty.Register<RatioToArrowIconConverter, object>(nameof(UpArrow));

    public static readonly StyledProperty<object> DownArrowProperty =
        AvaloniaProperty.Register<RatioToArrowIconConverter, object>(nameof(DownArrow));

    public object UpArrow
    {
        get => GetValue(UpArrowProperty);
        set => SetValue(UpArrowProperty, value);
    }

    public object DownArrow
    {
        get => GetValue(DownArrowProperty);
        set => SetValue(DownArrowProperty, value);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo) =>
        value is double num ? num < 0.97 ? DownArrow : num > 1.03 ? UpArrow : null : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo) =>
        throw new NotImplementedException();
}

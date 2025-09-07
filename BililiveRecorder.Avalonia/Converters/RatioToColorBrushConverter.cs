using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BililiveRecorder.Avalonia.Converters;

public class RatioToColorBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Disabled = new(Colors.AliceBlue);
    private static readonly SolidColorBrush[] ColorMap;

    static RatioToColorBrushConverter()
    {
        ColorMap = Enumerable
            .Range(0, 21)
            .Select(i => new SolidColorBrush(GradientPick(i / 20d, Colors.Red, Colors.Yellow, Colors.Lime)))
            .ToArray();
        return;

        static Color GradientPick(double percentage, Color c1, Color c2, Color c3) =>
            percentage < 0.5 ? ColorInterp(c1, c2, percentage / 0.5) :
            Math.Abs(percentage - 0.5) < 0.5 ? c2 : ColorInterp(c2, c3, (percentage - 0.5) / 0.5);

        static Color ColorInterp(Color start, Color end, double percentage) =>
            Color.FromArgb(255, LinearInterp(start.R, end.R, percentage), LinearInterp(start.G, end.G, percentage),
                LinearInterp(start.B, end.B, percentage));

        static byte LinearInterp(byte start, byte end, double percentage) =>
            (byte)(start + Math.Round(percentage * (end - start)));
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        if (value == null) return null;

        var input = (double)value;

        if (double.IsNaN(input))
            return Disabled;

        var i = (int)Math.Ceiling((1.1d - Math.Abs((1d - input) * 4d)) * 20d);
        return i switch
        {
            < 0 => ColorMap[0],
            > 20 => ColorMap[20],
            _ => ColorMap[i]
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo) =>
        throw new NotImplementedException();
}

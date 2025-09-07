using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BililiveRecorder.Common;

namespace BililiveRecorder.Avalonia.Converters;

internal class PlatformIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        if (value is not Platform platform)
            return null;
        return platform switch
        {
            Platform.BiliBili => new Bitmap(AssetLoader.Open(new Uri("avares://BililiveRecorder.Avalonia/Assets/PlatformIcon/bilibili.png"))),
            Platform.Douyin => new Bitmap(AssetLoader.Open(new Uri("avares://BililiveRecorder.Avalonia/Assets/PlatformIcon/douyin.png"))),
            Platform.Huya => new Bitmap(AssetLoader.Open(new Uri("avares://BililiveRecorder.Avalonia/Assets/PlatformIcon/huya.png"))),
            Platform.Douyu => new Bitmap(AssetLoader.Open(new Uri("avares://BililiveRecorder.Avalonia/Assets/PlatformIcon/douyu.png"))),
            _ => ""
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultureInfo)
    {
        throw new NotImplementedException();
    }
}

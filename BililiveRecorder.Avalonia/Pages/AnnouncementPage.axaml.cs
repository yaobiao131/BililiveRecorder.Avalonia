using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BililiveRecorder.Avalonia.Controls;

namespace BililiveRecorder.Avalonia.Pages;

public partial class AnnouncementPage : UserControl
{
    private static readonly HttpClient client;

    private static MemoryStream? AnnouncementCache = null;
    private static DateTimeOffset AnnouncementCacheTime = DateTimeOffset.MinValue;
    internal static CultureInfo CultureInfo = CultureInfo.CurrentUICulture;

    static AnnouncementPage()
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", $"BililiveRecorder/{GitVersionInformation.FullSemVer}");
    }

    public AnnouncementPage()
    {
        InitializeComponent();
        // Dispatcher.UIThread.Invoke(
        //     (Func<Task>)(async () => await this.LoadAnnouncementAsync(ignore_cache: false, show_error: false)),
        //     DispatcherPriority.Normal);
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Button_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            await LoadAnnouncementAsync(ignore_cache: true, show_error: false);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task LoadAnnouncementAsync(bool ignore_cache, bool show_error)
    {
        MemoryStream? data;
        bool success;

        Container.Child = null;
        Error.IsVisible = false;
        Loading.IsVisible = true;
        if (AnnouncementCache is not null && !ignore_cache)
        {
            data = AnnouncementCache;
            success = true;
        }
        else
        {
            try
            {
                var uri = $"https://rec.danmuji.org/wpf/announcement.xml?c={CultureInfo.Name}";

                var resp = await client.GetAsync(uri);
                var stream = await resp.EnsureSuccessStatusCode().Content.ReadAsStreamAsync();
                var mstream = new MemoryStream();
                await stream.CopyToAsync(mstream);
                AnnouncementCacheTime = DateTimeOffset.Now;
                data = mstream;
                success = true;
            }
            catch (Exception ex)
            {
                data = null;
                success = false;
                if (show_error) MessageBox.Show(ex.ToString(), "Loading Error");
            }

            if (success && data is not null)
            {
                try
                {
                    using var stream = new MemoryStream();
                    data.Position = 0;
                    await data.CopyToAsync(stream);
                    stream.Position = 0;
                    var xaml = Encoding.Default.GetString(stream.ToArray());
                    xaml = xaml.Replace("ui:ThemeShadowChrome", "Border")
                        .Replace("ui:SimpleStackPanel", "StackPanel")
                        .Replace("ui:PathIcon", "PathIcon")
                        .Replace("Style TargetType", "ControlTheme TargetType")
                        .Replace("/Style", "/ControlTheme")
                        .Replace("Style", "Theme")
                        .Replace("GroupBox", "HeaderedContentControl Classes=\"GroupBox\"")
                        .Replace("xmlns:ui=\"http://schemas.modernwpf.com/2019\"", "")
                        .Replace("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                            "xmlns=\"https://github.com/avaloniaui\"")
                        .Replace("BililiveRecorder.WPF", "BililiveRecorder.Avalonia");
                    var reader = AvaloniaRuntimeXamlLoader.Load(xaml);
                    // using var reader = new XamlXmlReader(stream, System.Windows.Markup.XamlReader.GetWpfSchemaContext());
                    // var obj = System.Windows.Markup.XamlReader.Load(reader);
                    if (reader is Control elem)
                        Container.Child = elem;
                }
                catch (Exception ex)
                {
                    data = null;
                    success = false;
                    if (show_error) MessageBox.Show(ex.ToString(), "Loading Error");
                }
            }
        }
    }
}

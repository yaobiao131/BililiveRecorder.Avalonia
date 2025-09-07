using System.Collections.Specialized;
using Avalonia.Controls;

namespace BililiveRecorder.Avalonia.Controls;

public partial class LogPanel : UserControl
{
    public LogPanel()
    {
        InitializeComponent();
        ((INotifyCollectionChanged)LogView.ItemsSource).CollectionChanged += LogView_CollectionChanged;
    }

    private void LogView_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        try
        {
            LogView.ScrollIntoView(LogView.ItemsSource.Cast<object>().Last(), null);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
    private DateTimeOffset lastSizeChanged = DateTimeOffset.MinValue;

    private void LogView_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            if (sender is not DataGrid view) return;

            var now = DateTimeOffset.Now;

            if (now - lastSizeChanged < OneSecond)
                return;

            lastSizeChanged = now;

            var w = view.Bounds.Width - 115 - 70 - 120;

            view.Columns[0].Width = new DataGridLength(120);
            view.Columns[1].Width = new DataGridLength(70);
            view.Columns[2].Width = new DataGridLength(115);
            view.Columns[3].Width = new DataGridLength(w);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}

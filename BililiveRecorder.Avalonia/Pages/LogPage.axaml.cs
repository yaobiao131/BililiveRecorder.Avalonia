using Avalonia.Controls;

namespace BililiveRecorder.Avalonia.Pages;

public partial class LogPage : UserControl
{
    public LogPage()
    {
        InitializeComponent();
        VersionTextBlock.Text = " " + GitVersionInformation.FullSemVer;
        ToolTip.SetTip(VersionTextBlock, GitVersionInformation.InformationalVersion);
    }
}

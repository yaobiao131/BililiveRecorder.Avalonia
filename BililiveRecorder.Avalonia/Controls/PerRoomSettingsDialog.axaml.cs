using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

/// <summary>
/// Interaction logic for PerRoomSettingsDialog.xaml
/// </summary>
public partial class PerRoomSettingsDialog : ContentDialog
{
    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public PerRoomSettingsDialog()
    {
        InitializeComponent();
    }
}

using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

public partial class CloseWindowConfirmDialog : ContentDialog
{
    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public CloseWindowConfirmDialog()
    {
        InitializeComponent();
    }
}

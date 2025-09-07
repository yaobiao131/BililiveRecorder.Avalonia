using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

public partial class DeleteRoomConfirmDialog : ContentDialog
{
    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public DeleteRoomConfirmDialog()
    {
        InitializeComponent();
    }
}

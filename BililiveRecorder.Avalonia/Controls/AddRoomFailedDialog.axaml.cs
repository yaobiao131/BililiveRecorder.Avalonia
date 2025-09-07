using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

public partial class AddRoomFailedDialog : ContentDialog
{
    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public AddRoomFailedDialog()
    {
        InitializeComponent();
    }

    public enum AddRoomFailedErrorText
    {
        InvalidInput,
        Duplicate,
        RoomIdZero,
        RoomIdNegative,
    }
}

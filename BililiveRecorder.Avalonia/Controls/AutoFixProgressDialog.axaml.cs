using Avalonia;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

/// <summary>
/// Interaction logic for AutoFixProgressDialog.xaml
/// </summary>
public partial class AutoFixProgressDialog : ContentDialog
{
    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public AutoFixProgressDialog()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<int> ProgressProperty =
        AvaloniaProperty.Register<AutoFixProgressDialog, int>(nameof(Progress));

    public int Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly StyledProperty<bool> CancelButtonVisibilityProperty =
        AvaloniaProperty.Register<AutoFixProgressDialog, bool>(nameof(CancelButtonVisibility));

    public bool CancelButtonVisibility
    {
        get => GetValue(CancelButtonVisibilityProperty);
        set => SetValue(CancelButtonVisibilityProperty, value);
    }

    public CancellationTokenSource? CancellationTokenSource { get; set; }

    private void Button_Click(object sender, RoutedEventArgs e) => CancellationTokenSource?.Cancel();
}

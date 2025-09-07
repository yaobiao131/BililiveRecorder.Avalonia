using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Data;

namespace BililiveRecorder.Avalonia.Controls;

[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class SettingWithDefault : ContentControl
{
    public static readonly StyledProperty<string> HeaderProperty
        = AvaloniaProperty.Register<SettingWithDefault, string>(nameof(Header), string.Empty);

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<bool> IsSettingNotUsingDefaultProperty
        = AvaloniaProperty.Register<SettingWithDefault, bool>(nameof(IsSettingNotUsingDefault), defaultBindingMode: BindingMode.TwoWay);

    public bool IsSettingNotUsingDefault
    {
        get => GetValue(IsSettingNotUsingDefaultProperty);
        set => SetValue(IsSettingNotUsingDefaultProperty, value);
    }
}

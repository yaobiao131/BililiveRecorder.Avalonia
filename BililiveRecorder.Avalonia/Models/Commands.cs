using System.Windows.Input;
using Avalonia.Labs.Input;
using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Models;

public class Commands : ICommand
{
    #region Static Commands

    public static Commands OpenLink { get; } = new()
    {
        ExecuteDelegate = o =>
        {
            try
            {
                var link = o?.ToString();
                if (link == null) return;
                App.TopLevel.Launcher.LaunchUriAsync(new Uri(link));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    };

    public static Commands OpenContentDialog { get; } = new()
    {
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
        ExecuteDelegate = async void (o) =>
        {
            try
            {
                await (o as ContentDialog)!.ShowAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
    };

    public static Commands Copy { get; } = new()
    {
        ExecuteDelegate = async void (e) =>
        {
            try
            {
                var clipboard = App.TopLevel.Clipboard;
                if (e is string str && clipboard != null) await clipboard.SetTextAsync(str);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    };

    public static Commands SetLang { get; } = new()
    {
        ExecuteDelegate = void (e) => { }
    };

    #endregion

    public Predicate<object?>? CanExecuteDelegate { get; set; }
    public Action<object?>? ExecuteDelegate { get; set; }

    #region ICommand Members

    public void Execute(object? parameter) => ExecuteDelegate?.Invoke(parameter);

    public bool CanExecute(object? parameter) => CanExecuteDelegate switch
    {
        null => true,
        _ => CanExecuteDelegate(parameter),
    };

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    #endregion
}

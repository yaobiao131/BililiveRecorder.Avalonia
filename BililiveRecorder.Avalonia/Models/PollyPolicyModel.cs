using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BililiveRecorder.Avalonia.Pages;
using BililiveRecorder.Core;

namespace BililiveRecorder.Avalonia.Models;

public class PollyPolicyModel : INotifyPropertyChanged
{
    private readonly PollyPolicy? policy;

    public PollyPolicyModel() : this((PollyPolicy?)RootPage.ServiceProvider?.GetService(typeof(PollyPolicy)))
    {
    }

    public PollyPolicyModel(PollyPolicy? policy)
    {
        this.policy = policy;

        ResetAllPolicy = new Commands
        {
            ExecuteDelegate = _ =>
            {
                if (this.policy is { } p)
                {
                    p.IpBlockedHttp412CircuitBreakerPolicy.Reset();
                    p.RequestFailedCircuitBreakerPolicy.Reset();
                    p.memoryCache.Compact(1);
                }
            }
        };
    }

    public ICommand ResetAllPolicy { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

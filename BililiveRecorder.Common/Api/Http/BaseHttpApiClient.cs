using System.ComponentModel;
using System.Net;
using BililiveRecorder.Common.Api.Model;
using GlobalConfig = BililiveRecorder.Common.Config.V3.GlobalConfig;

namespace BililiveRecorder.Common.Api.Http;

public abstract class BaseHttpApiClient : IApiClient
{
    protected readonly GlobalConfig config;
    protected HttpClient? Client;

    protected bool disposedValue;

    protected BaseHttpApiClient(GlobalConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));

        config.PropertyChanged += Config_PropertyChanged;

        Client = null;
        UpdateHttpClient();
    }

    private void UpdateHttpClient()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            UseCookies = false,
            UseDefaultCredentials = false,
            // Proxy = new WebProxy("http://127.0.0.1", 9090),
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            Timeout = TimeSpan.FromMilliseconds(config.TimingApiTimeout)
        };
        var headers = client.DefaultRequestHeaders;
        foreach (var header in Headers)
        {
            headers.Add(header.Key, header.Value);
        }

        var old = Interlocked.Exchange(ref Client, client);
        old?.Dispose();
    }

    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(config.Cookie) or nameof(config.TimingApiTimeout))
            UpdateHttpClient();
    }

    public virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                config.PropertyChanged -= Config_PropertyChanged;
                Client?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public abstract Dictionary<string, string> Headers { get; }
    public abstract Task<RoomInfo?> GetRoomInfoAsync(long roomid);
    public abstract Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null);
}

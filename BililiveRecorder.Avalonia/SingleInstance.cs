using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace BililiveRecorder.Avalonia;

public static class SingleInstance
{
    private static Mutex? singleInstanceMutex;

    // private static NamedPipeServerStream? pipeServer;
    private static volatile bool _isListening;

    public static event EventHandler? NotificationReceived;

    public static bool CheckMutex(string path)
    {
        var identifier = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes($"BililiveRecorder:SingeInstance:{path}")));

        singleInstanceMutex = new Mutex(true, identifier, out var createdNew);
        if (createdNew)
        {
            StartIpcServer(identifier);
        }
        else
        {
            NotifyFirstInstance(identifier);
        }

        return createdNew;
    }

    private static void StartIpcServer(string identifier)
    {
        _isListening = true;
        ThreadPool.QueueUserWorkItem(state =>
        {
            while (_isListening)
            {
                using var pipeServer = new NamedPipeServerStream(
                    identifier,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
                try
                {
                    pipeServer.WaitForConnection();
                    var buffer = new byte[1];
                    pipeServer.ReadExactly(buffer, 0, 1);
                    ActivateFirstInstanceCallback();
                }
                finally
                {
                    pipeServer.Disconnect();
                }

                Thread.Sleep(100);
            }
        });
    }

    private static void NotifyFirstInstance(string identifier)
    {
        using var client = new NamedPipeClientStream(
            ".",
            identifier,
            PipeDirection.Out,
            PipeOptions.None,
            TokenImpersonationLevel.None);
        try
        {
            client.Connect(1000);
            client.Write([0], 0, 1);
            client.Flush();
        }
        catch (TimeoutException)
        {
        }
    }

    public static void Cleanup()
    {
        _isListening = false;

        singleInstanceMutex?.Close();
        singleInstanceMutex = null;
    }

    private static void ActivateFirstInstanceCallback() => NotificationReceived?.Invoke(null, EventArgs.Empty);
}

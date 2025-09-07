#nullable disable
using BIliliveRecorder.Huya.Tars.Tars;
using BIliliveRecorder.Huya.Tars.Util;

namespace BIliliveRecorder.Huya.Tars.Tup;

public class RequestPacket : TarsStruct
{
    public short iVersion = 0;

    public byte cPacketType = 0;

    public int iMessageType = 0;

    public int iRequestId = 0;

    public string sServantName = null;

    public string sFuncName = null;

    public byte[] sBuffer = Array.Empty<byte>();

    public int iTimeout = 0;

    public Dictionary<string, string> context = new();

    public Dictionary<string, string> status = new();

    public RequestPacket()
    {
    }

    public RequestPacket(short iVersion, byte cPacketType, int iMessageType, int iRequestId, string sServantName,
        string sFuncName, byte[] sBuffer, int iTimeout, Dictionary<string, string> context,
        Dictionary<string, string> status)
    {
        this.iVersion = iVersion;
        this.cPacketType = cPacketType;
        this.iMessageType = iMessageType;
        this.iRequestId = iRequestId;
        this.sServantName = sServantName;
        this.sFuncName = sFuncName;
        this.sBuffer = sBuffer;
        this.iTimeout = iTimeout;
        this.context = context;
        this.status = status;
    }

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(iVersion, 1);
        _os.Write(cPacketType, 2);
        _os.Write(iMessageType, 3);
        _os.Write(iRequestId, 4);
        _os.Write(sServantName, 5);
        _os.Write(sFuncName, 6);
        _os.Write(sBuffer, 7);
        _os.Write(iTimeout, 8);
        _os.Write(context, 9);
        _os.Write(status, 10);
    }

    private static byte[] _cacheSBuffer;

    public override void ReadFrom(TarsInputStream inputStream)
    {
        try
        {
            iVersion = inputStream.Read(iVersion, 1, true);
            cPacketType = inputStream.Read(cPacketType, 2, true);
            iMessageType = inputStream.Read(iMessageType, 3, true);
            iRequestId = inputStream.Read(iRequestId, 4, true);
            sServantName = inputStream.readString(5, true);
            sFuncName = inputStream.readString(6, true);

            _cacheSBuffer ??= [0];

            sBuffer = (byte[])inputStream.Read<byte[]>(_cacheSBuffer, 7, true);
            iTimeout = inputStream.Read(iTimeout, 8, true);

            Dictionary<string, string> cacheContext = null;
            if (cacheContext == null) return;
            context = (Dictionary<string, string>)inputStream.Read(cacheContext, 9, true);
            status = (Dictionary<string, string>)inputStream.Read(cacheContext, 10, true);
        }
        catch (Exception e)
        {
            QTrace.Trace(this + " ReadFrom Exception: " + e.Message);
            throw;
        }
    }
}

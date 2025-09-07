using BIliliveRecorder.Huya.Tars.Tars;
using BIliliveRecorder.Huya.Tars.Tup;

namespace BIliliveRecorder.Huya.Proto;

public class BaseWup : TarsStruct
{
    public readonly RequestPacket TarsServantRequest = new();
    public readonly UniAttribute UniAttribute = new();

    private const short Version3 = 0x03;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(Version3, 1);
        _os.Write(TarsServantRequest.cPacketType, 2);
        _os.Write(TarsServantRequest.iMessageType, 3);
        _os.Write(TarsServantRequest.iRequestId, 4);
        _os.Write(TarsServantRequest.sServantName, 5);
        _os.Write(TarsServantRequest.sFuncName, 6);
        _os.Write(UniAttribute.Encode(), 7);
        _os.Write(TarsServantRequest.iTimeout, 8);
        _os.Write(TarsServantRequest.context, 9);
        _os.Write(TarsServantRequest.status, 10);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        TarsServantRequest.iVersion = inputStream.Read(TarsServantRequest.iVersion, 1, false);
        TarsServantRequest.cPacketType = inputStream.Read(TarsServantRequest.cPacketType, 2, false);
        TarsServantRequest.iMessageType = inputStream.Read(TarsServantRequest.iMessageType, 3, false);
        TarsServantRequest.iRequestId = inputStream.Read(TarsServantRequest.iRequestId, 4, false);
        TarsServantRequest.sServantName = inputStream.Read(TarsServantRequest.sServantName, 5, false);
        TarsServantRequest.sFuncName = inputStream.Read(TarsServantRequest.sFuncName, 6, false);
        UniAttribute.Decode(inputStream.Read(Array.Empty<byte>(), 7, false));
        TarsServantRequest.iTimeout = inputStream.Read(TarsServantRequest.iTimeout, 8, false);
        TarsServantRequest.context = (Dictionary<string, string>)inputStream.Read(TarsServantRequest.context, 9, false);
        TarsServantRequest.status = (Dictionary<string, string>)inputStream.Read(TarsServantRequest.status, 10, false);
    }

    public byte[] Encode()
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        // 创建 TarsOutputStream 对象并将数据写入其中
        var wupTarsOutputStream = new TarsOutputStream();
        WriteTo(wupTarsOutputStream);

        // 获取 TarsOutputStream 的字节数据
        var tarsData = wupTarsOutputStream.toByteArray();

        // 计算总长度
        var totalLength = 4 + tarsData.Length;

        // 写入总长度
        writer.Write(totalLength);

        // 写入 TarsOutputStream 的数据
        writer.Write(tarsData);

        // 获取最终的字节数组
        var result = memoryStream.ToArray();

        return result;
    }

    public void Decode(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        using var reader = new BinaryReader(memoryStream);
        // 读取总长度
        var size = reader.ReadInt64();

        if (size < 4)
        {
            return;
        }

        // 读取剩余数据
        var data = reader.ReadBytes((int)(memoryStream.Length - memoryStream.Position));

        // 调用 readFrom 方法解析数据
        ReadFrom(HuyaCodecUtil.NewUtf8TarsInputStream(data));
    }
}

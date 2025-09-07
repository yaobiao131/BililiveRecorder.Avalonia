using BIliliveRecorder.Huya.Tars.Tars;
using BIliliveRecorder.Huya.Tars.Util;

namespace BIliliveRecorder.Huya.Tars.Tup;

public class UniPacket : UniAttribute
{
    public static readonly int UniPacketHeadSize = 4;

    protected RequestPacket _package = new();

    /**
     * 获取请求的service名字
     *
     * @return
     */
    public string ServantName
    {
        get => _package.sServantName;
        set => _package.sServantName = value;
    }

    /**
     * 获取请求的函数名字
     *
     * @return
     */
    public string FuncName
    {
        get => _package.sFuncName;
        set => _package.sFuncName = value;
    }

    /**
     * 获取消息序列号
     *
     * @return
     */
    public int RequestId
    {
        get => _package.iRequestId;
        set => _package.iRequestId = value;
    }

    public UniPacket()
    {
        _package.iVersion = 2;
    }

    public void SetVersion(short iVer)
    {
        _iVer = iVer;
        _package.iVersion = iVer;
    }

    public short GetVersion()
    {
        return _package.iVersion;
    }

    /**
     * 将put的对象进行编码
     */
    public new byte[] Encode()
    {
        if (_package.sServantName.Equals(""))
        {
            throw new ArgumentException("servantName can not is null");
        }

        if (_package.sFuncName.Equals(""))
        {
            throw new ArgumentException("funcName can not is null");
        }

        var _os = new TarsOutputStream(0);
        _os.setServerEncoding(EncodeName);
        if (_package.iVersion == Const.PACKET_TYPE_TUP)
        {
            _os.Write(_data, 0);
        }
        else
        {
            _os.write(NewData, 0);
        }

        _package.sBuffer = TarsUtil.GetTarsBufArray(_os.getMemoryStream());

        _os = new TarsOutputStream(0);
        _os.setServerEncoding(EncodeName);
        WriteTo(_os);
        var bodys = TarsUtil.GetTarsBufArray(_os.getMemoryStream());
        var size = bodys.Length;

        MemoryStream ms = new MemoryStream(size + UniPacketHeadSize);

        using (var bw = new BinaryWriter(ms))
        {
            // 整个数据包长度
            bw.Write(ByteConverter.ReverseEndian(size + UniPacketHeadSize));
            bw.Write(bodys);
        }

        return ms.ToArray();
    }

    /**
     * 对传入的数据进行解码 填充可get的对象
     */
    public new void Decode(byte[] buffer, int index = 0)
    {
        if (buffer.Length < UniPacketHeadSize)
        {
            throw new ArgumentException("Decode namespace must include size head");
        }

        try
        {
            var _is = new TarsInputStream(buffer, UniPacketHeadSize + index);
            _is.setServerEncoding(EncodeName);
            //解码出RequestPacket包
            ReadFrom(_is);

            //设置tup版本
            _iVer = _package.iVersion;

            _is = new TarsInputStream(_package.sBuffer);
            _is.setServerEncoding(EncodeName);

            if (_package.iVersion == Const.PACKET_TYPE_TUP)
            {
                _data = (Dictionary<string, Dictionary<string, byte[]>>)_is
                    .ReadMap<Dictionary<string, Dictionary<string, byte[]>>>(0, false);
            }
            else
            {
                NewData = (Dictionary<string, byte[]>)_is.ReadMap<Dictionary<string, byte[]>>(0, false);
            }
        }
        catch (Exception e)
        {
            QTrace.Trace(this + " Decode Exception: " + e.Message);
            throw;
        }
    }

    public override void WriteTo(TarsOutputStream _os)
    {
        _package.WriteTo(_os);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        _package.ReadFrom(inputStream);
    }

    public int OldRespIRet { get; set; }

    /**
     * 为兼容最早发布的客户端版本解码使用 iret字段始终为0
     *
     * @return
     */
    public byte[] CreateOldRespEncode()
    {
        var _os = new TarsOutputStream(0);
        _os.setServerEncoding(EncodeName);
        _os.Write(_data, 0);
        var oldSBuffer = TarsUtil.GetTarsBufArray(_os.getMemoryStream());
        _os = new TarsOutputStream(0);
        _os.setServerEncoding(EncodeName);
        _os.Write(_package.iVersion, 1);
        _os.Write(_package.cPacketType, 2);
        _os.Write(_package.iRequestId, 3);
        _os.Write(_package.iMessageType, 4);
        _os.Write(OldRespIRet, 5);
        _os.Write(oldSBuffer, 6);
        _os.Write(_package.status, 7);
        return TarsUtil.GetTarsBufArray(_os.getMemoryStream());
    }
}

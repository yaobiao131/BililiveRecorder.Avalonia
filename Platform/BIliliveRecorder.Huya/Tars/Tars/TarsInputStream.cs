#nullable disable
using System.Collections;
using BIliliveRecorder.Huya.Tars.Util;

namespace BIliliveRecorder.Huya.Tars.Tars;

/**
     * 数据读取流
     */
public class TarsInputStream
{
    private MemoryStream ms;
    private BinaryReader br;

    /**
     * 头数据
     */
    public class HeadData
    {
        public byte type;
        public int tag;

        public void clear()
        {
            type = 0;
            tag = 0;
        }
    }

    public TarsInputStream()
    {
        ms = new MemoryStream();
        br = null;
        br = new BinaryReader(ms);
    }


    public TarsInputStream(MemoryStream ms)
    {
        this.ms = ms;
        br = null;
        br = new BinaryReader(ms);
    }

    public TarsInputStream(byte[] bs)
    {
        ms = new MemoryStream(bs);
        br = null;
        br = new BinaryReader(ms);
    }

    public TarsInputStream(byte[] bs, int pos)
    {
        ms = new MemoryStream(bs);
        ms.Position = pos;
        br = null;
        br = new BinaryReader(ms);
    }

    public void Wrap(byte[] bs, int index = 0)
    {
        if (null != ms)
        {
            ms = null;
            ms = new MemoryStream(bs, index, bs.Length - index);
            br = null;
            br = new BinaryReader(ms);
        }
        else
        {
            ms = new MemoryStream(bs);
            br = null;
            br = new BinaryReader(ms);
        }
    }

    /**
     * 读取数据头
     * @param hd	读取到的头信息
     * @param bb	缓冲
     * @return 读取的字节数
     */
    public static int readHead(HeadData hd, BinaryReader bb)
    {
        if (bb.BaseStream.Position >= bb.BaseStream.Length)
        {
            throw new TarsDecodeException("read file to end");
        }

        var b = bb.ReadByte();
        hd.type = (byte)(b & 15);
        hd.tag = (b & (15 << 4)) >> 4;
        if (hd.tag == 15)
        {
            hd.tag = bb.ReadByte();
            return 2;
        }

        return 1;
    }

    public int readHead(HeadData hd)
    {
        return readHead(hd, br);
    }

    // 读取头信息，但不移动缓冲区的当前偏移
    private int peakHead(HeadData hd)
    {
        var curPos = ms.Position;
        var len = readHead(hd);
        ms.Position = curPos;
        return len;
    }

    // 跳过若干字节
    private void skip(int len)
    {
        ms.Position += len;
    }

    // 跳到指定的tag的数据之前
    public bool skipToTag(int tag)
    {
        try
        {
            var hd = new HeadData();
            while (true)
            {
                var len = peakHead(hd);
                if (tag <= hd.tag || hd.type == (byte)TarsStructType.StructEnd)
                {
                    return tag == hd.tag;
                }

                skip(len);
                skipField(hd.type);
            }
        }
        catch (TarsDecodeException e)
        {
            QTrace.Trace(e.Message);
        }

        return false;
    }

    // 跳到当前结构的结束位置
    public void skipToStructEnd()
    {
        var hd = new HeadData();
        do
        {
            readHead(hd);
            skipField(hd.type);
        } while (hd.type != (byte)TarsStructType.StructEnd);
    }

    // 跳过一个字段
    private void skipField()
    {
        var hd = new HeadData();
        readHead(hd);
        skipField(hd.type);
    }

    private void skipField(byte type)
    {
        switch (type)
        {
            case (byte)TarsStructType.Byte:
                skip(1);
                break;
            case (byte)TarsStructType.Short:
                skip(2);
                break;
            case (byte)TarsStructType.Int:
                skip(4);
                break;
            case (byte)TarsStructType.Long:
                skip(8);
                break;
            case (byte)TarsStructType.Float:
                skip(4);
                break;
            case (byte)TarsStructType.Double:
                skip(8);
                break;
            case (byte)TarsStructType.String1:
            {
                int len = br.ReadByte();
                if (len < 0)
                    len += 256;
                skip(len);
                break;
            }
            case (byte)TarsStructType.String4:
            {
                skip(ByteConverter.ReverseEndian(br.ReadInt32()));
                break;
            }
            case (byte)TarsStructType.Map:
            {
                var size = Read(0, 0, true);
                for (var i = 0; i < size * 2; ++i)
                {
                    skipField();
                }

                break;
            }
            case (byte)TarsStructType.List:
            {
                var size = Read(0, 0, true);
                for (var i = 0; i < size; ++i)
                {
                    skipField();
                }

                break;
            }
            case (byte)TarsStructType.SimpleList:
            {
                var hd = new HeadData();
                readHead(hd);
                if (hd.type != (byte)TarsStructType.Byte)
                {
                    throw new TarsDecodeException("skipField with invalid type, type value: " + type + ", " + hd.type);
                }

                var size = Read(0, 0, true);
                skip(size);
                break;
            }
            case (byte)TarsStructType.StructBegin:
                skipToStructEnd();
                break;
            case (byte)TarsStructType.StructEnd:
            case (byte)TarsStructType.ZeroTag:
                break;
            default:
                throw new TarsDecodeException("invalid type.");
        }
    }

    public bool Read(bool b, int tag, bool isRequire)
    {
        var c = Read((byte)0x0, tag, isRequire);
        return c != 0;
    }

    public char Read(char c, int tag, bool isRequire)
    {
        return (char)Read((byte)c, tag, isRequire);
    }

    public byte Read(byte c, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.ZeroTag:
                    c = 0x0;
                    break;
                case (byte)TarsStructType.Byte:
                {
                    c = br.ReadByte();
                    break;
                }
                default:
                {
                    throw new TarsDecodeException("type mismatch.");
                }
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return c;
    }

    public short Read(short n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Byte => br.ReadByte(),
                (byte)TarsStructType.Short => ByteConverter.ReverseEndian(br.ReadInt16()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public ushort Read(ushort n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Byte => br.ReadByte(),
                (byte)TarsStructType.Short => ByteConverter.ReverseEndian(br.ReadUInt16()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public int Read(int n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);

            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Byte => br.ReadByte(),
                (byte)TarsStructType.Short => ByteConverter.ReverseEndian(br.ReadInt16()),
                (byte)TarsStructType.Int => ByteConverter.ReverseEndian(br.ReadInt32()),
                (byte)TarsStructType.Long => ByteConverter.ReverseEndian(br.ReadInt32()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public uint Read(uint n, int tag, bool isRequire)
    {
        return (uint)Read((long)n, tag, isRequire);
    }

    public long Read(long n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Byte => br.ReadByte(),
                (byte)TarsStructType.Short => ByteConverter.ReverseEndian(br.ReadInt16()),
                (byte)TarsStructType.Int => ByteConverter.ReverseEndian(br.ReadInt32()),
                (byte)TarsStructType.Long => ByteConverter.ReverseEndian(br.ReadInt64()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public ulong Read(ulong n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Byte => br.ReadByte(),
                (byte)TarsStructType.Short => ByteConverter.ReverseEndian(br.ReadUInt16()),
                (byte)TarsStructType.Int => ByteConverter.ReverseEndian(br.ReadUInt32()),
                (byte)TarsStructType.Long => ByteConverter.ReverseEndian(br.ReadUInt64()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public float Read(float n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Float => ByteConverter.ReverseEndian(br.ReadSingle()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public double Read(double n, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            n = hd.type switch
            {
                (byte)TarsStructType.ZeroTag => 0,
                (byte)TarsStructType.Float => ByteConverter.ReverseEndian(br.ReadSingle()),
                (byte)TarsStructType.Double => ByteConverter.ReverseEndian(br.ReadDouble()),
                _ => throw new TarsDecodeException("type mismatch.")
            };
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return n;
    }

    public string ReadByteString(string s, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.String1:
                {
                    {
                        var len = 0;
                        len = br.ReadByte();
                        if (len < 0)
                        {
                            len += 256;
                        }

                        var ss = new byte[len];
                        ss = br.ReadBytes(len);
                        s = HexUtil.bytes2HexStr(ss);
                    }
                }
                    break;
                case (byte)TarsStructType.String4:
                {
                    {
                        var len = 0;
                        len = ByteConverter.ReverseEndian(br.ReadInt32());

                        if (len > TarsStruct.TarsMaxStringLength || len < 0)
                        {
                            throw new TarsDecodeException("string too long: " + len);
                        }

                        var ss = new byte[len];
                        ss = br.ReadBytes(len);
                        s = HexUtil.bytes2HexStr(ss);
                    }
                }
                    break;
                default:
                {
                    throw new TarsDecodeException("type mismatch.");
                }
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return s;
    }

    private string ReadString1()
    {
        {
            var len = 0;
            len = br.ReadByte();
            if (len < 0)
            {
                len += 256;
            }

            var ss = new byte[len];
            ss = br.ReadBytes(len);

            return ByteConverter.Bytes2String(ss);
        }
    }

    private string ReadString4()
    {
        {
            var len = 0;
            len = ByteConverter.ReverseEndian(br.ReadInt32());
            if (len > TarsStruct.TarsMaxStringLength || len < 0)
            {
                throw new TarsDecodeException("string too long: " + len);
            }

            var ss = new byte[len];
            ss = br.ReadBytes(len);

            return ByteConverter.Bytes2String(ss);
        }
    }

    public string Read(string s, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.String1:
                {
                    s = ReadString1();
                }
                    break;
                case (byte)TarsStructType.String4:
                {
                    s = ReadString4();
                }
                    break;
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return s;
    }

    public string readString(int tag, bool isRequire)
    {
        string s = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.String1:
                {
                    s = ReadString1();
                }
                    break;
                case (byte)TarsStructType.String4:
                {
                    s = ReadString4();
                }
                    break;
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return s;
    }

    public string[] Read(string[] s, int tag, bool isRequire)
    {
        return readArray(s, tag, isRequire);
    }

    public IDictionary ReadMap<T>(int tag, bool isRequire)
    {
        var m = (T)BasicClassTypeUtil.CreateObject<T>();
        return readMap(m, tag, isRequire);
    }

    public IDictionary readMap<T>(T arg, int tag, bool isRequire)
    {
        if (BasicClassTypeUtil.CreateObject(arg.GetType()) is not IDictionary m)
        {
            return null;
        }

        var type = m.GetType();
        var argsType = type.GetGenericArguments();
        if (argsType == null || argsType.Length < 2)
        {
            return null;
        }

        var mk = BasicClassTypeUtil.CreateObject(argsType[0]);
        var mv = BasicClassTypeUtil.CreateObject(argsType[1]);

        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.Map:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                    {
                        throw new TarsDecodeException("size invalid: " + size);
                    }

                    for (var i = 0; i < size; ++i)
                    {
                        mk = Read(mk, 0, true);
                        mv = Read(mv, 1, true);

                        if (mk != null)
                        {
                            if (m.Contains(mk))
                            {
                                m[mk] = mv;
                            }
                            else
                            {
                                m.Add(mk, mv);
                            }
                        }
                    }
                }
                    break;
                default:
                {
                    throw new TarsDecodeException("type mismatch.");
                }
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return m;
    }

    public bool[] Read(bool[] l, int tag, bool isRequire)
    {
        bool[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new bool[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public byte[] Read(byte[] l, int tag, bool isRequire)
    {
        byte[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.SimpleList:
                {
                    var hh = new HeadData();
                    readHead(hh);
                    if (hh.type != (byte)TarsStructType.Byte)
                    {
                        throw new TarsDecodeException("type mismatch, tag: " + tag + ", type: " + hd.type + ", " +
                                                      hh.type);
                    }

                    var size = Read(0, 0, true);
                    if (size < 0)
                    {
                        throw new TarsDecodeException("invalid size, tag: " + tag + ", type: " + hd.type + ", " +
                                                      hh.type + ", size: " + size);
                    }

                    lr = new byte[size];


                    try
                    {
                        lr = br.ReadBytes(size);
                    }
                    catch (Exception e)
                    {
                        QTrace.Trace(e.Message);
                        return null;
                    }

                    break;
                }
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new byte[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public short[] Read(short[] l, int tag, bool isRequire)
    {
        short[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new short[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public int[] Read(int[] l, int tag, bool isRequire)
    {
        int[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new int[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public long[] Read(long[] l, int tag, bool isRequire)
    {
        long[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new long[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public float[] Read(float[] l, int tag, bool isRequire)
    {
        float[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new float[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public double[] Read(double[] l, int tag, bool isRequire)
    {
        double[] lr = null;
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                        throw new TarsDecodeException("size invalid: " + size);
                    lr = new double[size];
                    for (var i = 0; i < size; ++i)
                        lr[i] = Read(lr[0], 0, true);
                    break;
                }
                default:
                    throw new TarsDecodeException("type mismatch.");
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return lr;
    }

    public T[] readArray<T>(T[] l, int tag, bool isRequire)
    {
        // 生成代码时已经往List里面添加了一个元素，纯粹用来作为类型识别，否则java无法识别到List里面放的是什么类型的数据
        if (l == null || l.Length == 0)
            throw new TarsDecodeException("unable to get type of key and value.");
        return (T[])readArrayImpl(l[0], tag, isRequire);
    }

    public IList readList<T>(T l, int tag, bool isRequire)
    {
        // 生成代码时已经往List里面添加了一个元素，纯粹用来作为类型识别，否则java无法识别到List里面放的是什么类型的数据
        if (l == null)
        {
            return null;
        }

        var list = BasicClassTypeUtil.CreateObject(l.GetType()) as IList;
        if (list == null)
        {
            return null;
        }

        var objItem = BasicClassTypeUtil.CreateListItem(list.GetType());

        var array = readArrayImpl(objItem, tag, isRequire);

        if (array != null)
        {
            list.Clear();
            foreach (var obj in array)
            {
                list.Add(obj);
            }

            return list;
        }

        return null;
    }

    public List<T> readArray<T>(List<T> l, int tag, bool isRequire)
    {
        // 生成代码时已经往List里面添加了一个元素，纯粹用来作为类型识别，否则java无法识别到List里面放的是什么类型的数据
        if (l == null || l.Count == 0)
        {
            return new List<T>();
        }

        var tt = (T[])readArrayImpl(l[0], tag, isRequire);
        if (tt == null) return null;
        var ll = new List<T>();
        for (var i = 0; i < tt.Length; ++i)
        {
            ll.Add(tt[i]);
        }

        return ll;
    }

    ////@SuppressWarnings("unchecked")
    private Array readArrayImpl<T>(T mt, int tag, bool isRequire)
    {
        if (skipToTag(tag))
        {
            var hd = new HeadData();
            readHead(hd);
            switch (hd.type)
            {
                case (byte)TarsStructType.List:
                {
                    var size = Read(0, 0, true);
                    if (size < 0)
                    {
                        throw new TarsDecodeException("size invalid: " + size);
                    }

                    var lr = Array.CreateInstance(mt.GetType(), size);
                    for (var i = 0; i < size; ++i)
                    {
                        var t = (T)Read(mt, 0, true);
                        lr.SetValue(t, i);
                    }

                    return lr;
                }

                case (byte)TarsStructType.SimpleList:
                {
                    var hh = new HeadData();
                    readHead(hh);
                    if (hh.type == (byte)TarsStructType.ZeroTag)
                    {
                        return Array.CreateInstance(mt.GetType(), 0);
                    }

                    if (hh.type != (byte)TarsStructType.Byte)
                    {
                        throw new TarsDecodeException("type mismatch, tag: " + tag + ", type: " + hd.type + ", " +
                                                      hh.type);
                    }

                    var size = Read(0, 0, true);
                    if (size < 0)
                    {
                        throw new TarsDecodeException("invalid size, tag: " + tag + ", type: " + hd.type + ", size: " +
                                                      size);
                    }

                    var lr = new T[size];

                    try
                    {
                        var lrtmp = br.ReadBytes(size);
                        for (var i = 0; i < lrtmp.Length; i++)
                        {
                            object obj = lrtmp[i];
                            lr[i] = (T)obj;
                        }

                        return lr;
                    }
                    catch (Exception e)
                    {
                        QTrace.Trace(e.Message);
                        return null;
                    }
                }
                default:
                {
                    throw new TarsDecodeException("type mismatch.");
                }
            }
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return null;
    }

    public TarsStruct directRead(TarsStruct o, int tag, bool isRequire)
    {
        //o必须有一个无参的构造函数
        TarsStruct reff = null;
        if (skipToTag(tag))
        {
            try
            {
                reff = (TarsStruct)BasicClassTypeUtil.CreateObject(o.GetType());
            }
            catch (Exception e)
            {
                throw new TarsDecodeException(e.Message);
            }

            var hd = new HeadData();
            readHead(hd);
            if (hd.type != (byte)TarsStructType.StructBegin)
            {
                throw new TarsDecodeException("type mismatch.");
            }

            reff.ReadFrom(this);
            skipToStructEnd();
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return reff;
    }

    public TarsStruct Read(TarsStruct o, int tag, bool isRequire)
    {
        //o必须有一个无参的构造函数
        TarsStruct reff = null;
        if (skipToTag(tag))
        {
            try
            {
                // 必须重新创建一个，否则，会导致在同一个对象上赋值，这是由于C#的引用引起的
                reff = (TarsStruct)BasicClassTypeUtil.CreateObject(o.GetType());
            }
            catch (Exception e)
            {
                throw new TarsDecodeException(e.Message);
            }

            var hd = new HeadData();
            readHead(hd);
            if (hd.type != (byte)TarsStructType.StructBegin)
            {
                throw new TarsDecodeException("type mismatch.");
            }

            reff.ReadFrom(this);
            skipToStructEnd();
        }
        else if (isRequire)
        {
            throw new TarsDecodeException("require field not exist.");
        }

        return reff;
    }

    public TarsStruct[] Read(TarsStruct[] o, int tag, bool isRequire)
    {
        return readArray(o, tag, isRequire);
    }

    public object Read<T>(T o, int tag, bool isRequire)
    {
        if (o == null)
        {
            o = (T)BasicClassTypeUtil.CreateObject<T>();
        }

        if (o is byte || o is char)
        {
            return Read((byte)0x0, tag, isRequire);
        }
        else if (o is char)
        {
            return Read((char)0x0, tag, isRequire);
        }
        else if (o is bool)
        {
            return Read(false, tag, isRequire);
        }
        else if (o is short)
        {
            return Read((short)0, tag, isRequire);
        }
        else if (o is ushort)
        {
            return Read((ushort)0, tag, isRequire);
        }
        else if (o is int)
        {
            return Read((int)0, tag, isRequire);
        }
        else if (o is uint)
        {
            return Read((uint)0, tag, isRequire);
        }
        else if (o is long)
        {
            return Read((long)0, tag, isRequire);
        }
        else if (o is ulong)
        {
            return Read((ulong)0, tag, isRequire);
        }
        else if (o is float)
        {
            return Read((float)0, tag, isRequire);
        }
        else if (o is double)
        {
            return Read((double)0, tag, isRequire);
        }
        else if (o is string)
        {
            return readString(tag, isRequire);
        }
        else if (o is TarsStruct)
        {
            object oo = o;
            return Read((TarsStruct)oo, tag, isRequire);
        }
        else if (o != null && o.GetType().IsArray)
        {
            if (o is byte[])
            {
                return Read((byte[])null, tag, isRequire);
            }
            else if (o is bool[])
            {
                return Read((bool[])null, tag, isRequire);
            }
            else if (o is short[])
            {
                return Read((short[])null, tag, isRequire);
            }
            else if (o is int[])
            {
                return Read((int[])null, tag, isRequire);
            }
            else if (o is long[])
            {
                return Read((long[])null, tag, isRequire);
            }
            else if (o is float[])
            {
                return Read((float[])null, tag, isRequire);
            }
            else if (o is double[])
            {
                return Read((double[])null, tag, isRequire);
            }
            else
            {
                object oo = o;
                return readArray((Object[])oo, tag, isRequire);
            }
        }
        else if (o is IList)
        {
            return readList<T>(o, tag, isRequire);
        }
        else if (o is IDictionary)
        {
            return readMap<T>(o, tag, isRequire);
        }
        else
        {
            throw new TarsDecodeException("read object error: unsupport type." + o.ToString());
        }
    }

    protected string sServerEncoding = "utf-8";

    public int setServerEncoding(string se)
    {
        sServerEncoding = se;
        return 0;
    }

    internal object read(object proxy, int tag, bool isRequired)
    {
        return Read(proxy, tag, isRequired);
    }
}

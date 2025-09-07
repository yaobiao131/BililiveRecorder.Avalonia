#nullable disable
using System.Collections;
using System.Text;
using BIliliveRecorder.Huya.Tars.Util;

namespace BIliliveRecorder.Huya.Tars.Tars;

public class TarsOutputStream
{
    private MemoryStream ms;
    private BinaryWriter bw;

    public TarsOutputStream(MemoryStream ms)
    {
        this.ms = ms;
        bw = new BinaryWriter(ms, Encoding.BigEndianUnicode);
    }

    public TarsOutputStream(int capacity)
    {
        ms = new MemoryStream(capacity);
        bw = new BinaryWriter(ms, Encoding.BigEndianUnicode);
    }

    public TarsOutputStream()
    {
        ms = new MemoryStream(128);
        bw = new BinaryWriter(ms, Encoding.BigEndianUnicode);
    }

    public MemoryStream getMemoryStream()
    {
        return ms;
    }

    public byte[] toByteArray()
    {
        var newBytes = new byte[ms.Position];
        Array.Copy(ms.GetBuffer(), 0, newBytes, 0, (int)ms.Position);
        return newBytes;
    }

    public void reserve(int len)
    {
        var nRemain = ms.Capacity - (int)ms.Length;
        if (nRemain < len)
        {
            ms.Capacity = (ms.Capacity + len) << 1; // -nRemain;
        }
    }

    // 写入头信息
    public void writeHead(byte type, int tag)
    {
        if (tag < 15)
        {
            var b = (byte)((tag << 4) | type);

            try
            {
                {
                    bw.Write(b);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
        else if (tag < 256)
        {
            try
            {
                var b = (byte)((15 << 4) | type);
                {
                    bw.Write(b);
                    bw.Write((byte)tag);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(this + " writeHead: " + e.Message);
            }
        }
        else
        {
            throw new TarsEncodeException("tag is too large: " + tag);
        }
    }

    public void Write(bool b, int tag)
    {
        var by = (byte)(b ? 0x01 : 0);
        Write(by, tag);
    }

    public void Write(byte b, int tag)
    {
        reserve(3);
        if (b == 0)
        {
            writeHead((byte)TarsStructType.ZeroTag, tag);
        }
        else
        {
            writeHead((byte)TarsStructType.Byte, tag);
            try
            {
                {
                    bw.Write(b);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
    }

    public void Write(short n, int tag)
    {
        reserve(4);
        if (n >= -128 && n <= 127)
        {
            Write((byte)n, tag);
        }
        else
        {
            writeHead((byte)TarsStructType.Short, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(n));
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(this + " Write: " + e.Message);
            }
        }
    }

    public void Write(ushort n, int tag)
    {
        Write((short)n, tag);
    }

    public void Write(int n, int tag)
    {
        reserve(6);

        if (n >= short.MinValue && n <= short.MaxValue)
        {
            Write((short)n, tag);
        }
        else
        {
            writeHead((byte)TarsStructType.Int, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(n));
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
    }

    public void Write(uint n, int tag)
    {
        Write((long)n, tag);
    }

    public void Write(ulong n, int tag)
    {
        Write((long)n, tag);
    }

    public void Write(long n, int tag)
    {
        reserve(10);
        if (n is >= int.MinValue and <= int.MaxValue)
        {
            Write((int)n, tag);
        }
        else
        {
            writeHead((byte)TarsStructType.Long, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(n));
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
    }

    public void Write(float n, int tag)
    {
        reserve(6);
        writeHead((byte)TarsStructType.Float, tag);
        try
        {
            {
                bw.Write(ByteConverter.ReverseEndian(n));
            }
        }
        catch (Exception e)
        {
            QTrace.Trace(e.Message);
        }
    }

    public void Write(double n, int tag)
    {
        reserve(10);
        writeHead((byte)TarsStructType.Double, tag);
        try
        {
            {
                bw.Write(ByteConverter.ReverseEndian(n));
            }
        }
        catch (Exception e)
        {
            QTrace.Trace(e.Message);
        }
    }

    public void writeStringByte(string s, int tag)
    {
        var by = HexUtil.hexStr2Bytes(s);
        reserve(10 + by.Length);
        if (by.Length > 255)
        {
            // 长度大于255，为String4类型
            writeHead((byte)TarsStructType.String4, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(by.Length));
                    bw.Write(by);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
        else
        {
            // 长度小于255，位String1类型
            writeHead((byte)TarsStructType.String1, tag);
            try
            {
                {
                    bw.Write((byte)by.Length);
                    bw.Write(by);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
    }

    public void writeByteString(string s, int tag)
    {
        reserve(10 + s.Length);
        var by = HexUtil.hexStr2Bytes(s);
        if (by.Length > 255)
        {
            writeHead((byte)TarsStructType.String4, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(by.Length));
                    bw.Write(by);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
        else
        {
            writeHead((byte)TarsStructType.String1, tag);
            try
            {
                {
                    bw.Write((byte)by.Length);
                    bw.Write(by);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
    }

    public void Write(string s, int tag, bool isLocalString = false)
    {
        byte[] by;
        try
        {
            by = ByteConverter.String2Bytes(s, isLocalString);
            if (by == null)
            {
                by = [];
            }
        }
        catch (Exception e)
        {
            QTrace.Trace(this + " write s Exception" + e.Message);
            return;
        }

        if (by != null)
        {
            reserve(10 + by.Length);
        }

        if (by != null && by.Length > 255)
        {
            writeHead((byte)TarsStructType.String4, tag);
            try
            {
                {
                    bw.Write(ByteConverter.ReverseEndian(by.Length));
                    bw.Write(by);
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(e.Message);
            }
        }
        else
        {
            writeHead((byte)TarsStructType.String1, tag);
            try
            {
                {
                    if (by != null)
                    {
                        bw.Write((byte)by.Length);
                        bw.Write(by);
                    }
                    else
                    {
                        bw.Write((byte)0);
                    }
                }
            }
            catch (Exception e)
            {
                QTrace.Trace(this + " write s(2) Exception" + e.Message);
            }
        }
    }

    public void write<TK, TV>(Dictionary<TK, TV> m, int tag) where TK : notnull
    {
        reserve(8);
        writeHead((byte)TarsStructType.Map, tag);

        Write(m == null ? 0 : m.Count, 0);
        if (m != null)
        {
            foreach (var en in m)
            {
                Write(en.Key, 0);
                Write(en.Value, 1);
            }
        }
    }

    public void Write(IDictionary m, int tag)
    {
        reserve(8);
        writeHead((byte)TarsStructType.Map, tag);
        Write(m == null ? 0 : m.Count, 0);
        if (m != null)
        {
            var keys = m.Keys;
            foreach (var objKey in keys)
            {
                Write(objKey, 0);
                Write(m[objKey], 1);
            }
        }
    }

    public void Write(bool[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);

        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void Write(byte[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8 + nLen);
        writeHead((byte)TarsStructType.SimpleList, tag);
        writeHead((byte)TarsStructType.Byte, 0);
        Write(nLen, 0);

        try
        {
            if (l != null)
            {
                bw.Write(l);
            }
        }
        catch (Exception e)
        {
            QTrace.Trace(e.Message);
        }
    }

    public void Write(short[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);
        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void Write(int[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);
        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void Write(long[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);

        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void Write(float[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);
        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void Write(double[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);

        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    public void write<T>(T[] l, int tag)
    {
        object o = l;
        writeArray((object[])o, tag);
    }

    private void writeArray(object[] l, int tag)
    {
        var nLen = 0;
        if (l != null)
        {
            nLen = l.Length;
        }

        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(nLen, 0);

        if (l != null)
        {
            foreach (var e in l)
            {
                Write(e, 0);
            }
        }
    }

    // 由于list，应该在第一个位置[0]预置一个元素（可以为空），以方便判断元素类型
    public void writeList(IList l, int tag)
    {
        reserve(8);
        writeHead((byte)TarsStructType.List, tag);
        Write(l == null ? 0 : l.Count > 0 ? l.Count : 0, 0);
        if (l != null)
        {
            for (var i = 0; i < l.Count; i++)
            {
                Write(l[i], 0);
            }
        }
    }

    public void Write(TarsStruct o, int tag)
    {
        if (o == null)
        {
            return;
        }

        reserve(2);
        writeHead((byte)TarsStructType.StructBegin, tag);
        o.WriteTo(this);
        reserve(2);
        writeHead((byte)TarsStructType.StructEnd, 0);
    }

    public void Write(object o, int tag)
    {
        if (o == null)
        {
            return;
        }

        if (o is byte)
        {
            Write((byte)o, tag);
        }
        else if (o is bool)
        {
            Write((bool)o, tag);
        }
        else if (o is short)
        {
            Write((short)o, tag);
        }
        else if (o is ushort)
        {
            Write((int)(ushort)o, tag);
        }
        else if (o is int)
        {
            Write((int)o, tag);
        }
        else if (o is uint)
        {
            Write((long)(uint)o, tag);
        }
        else if (o is long)
        {
            Write((long)o, tag);
        }
        else if (o is ulong)
        {
            Write((long)(ulong)o, tag);
        }
        else if (o is float)
        {
            Write((float)o, tag);
        }
        else if (o is double)
        {
            Write((double)o, tag);
        }
        else if (o is string)
        {
            var strObj = o as string;
            Write(strObj, tag);
        }
        else if (o is TarsStruct)
        {
            Write((TarsStruct)o, tag);
        }
        else if (o is byte[])
        {
            Write((byte[])o, tag);
        }
        else if (o is bool[])
        {
            Write((bool[])o, tag);
        }
        else if (o is short[])
        {
            Write((short[])o, tag);
        }
        else if (o is int[])
        {
            Write((int[])o, tag);
        }
        else if (o is long[])
        {
            Write((long[])o, tag);
        }
        else if (o is float[])
        {
            Write((float[])o, tag);
        }
        else if (o is double[])
        {
            Write((double[])o, tag);
        }
        else if (o.GetType().IsArray)
        {
            Write((object[])o, tag);
        }
        else if (o is IList)
        {
            writeList((IList)o, tag);
        }
        else if (o is IDictionary)
        {
            Write((IDictionary)o, tag);
        }
        else
        {
            throw new TarsEncodeException(
                "write object error: unsupport type. " + o.ToString() + "\n");
        }
    }

    protected string sServerEncoding = "UTF-8";

    public int setServerEncoding(string se)
    {
        sServerEncoding = se;
        return 0;
    }
}

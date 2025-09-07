#nullable disable
using System.Collections;
using System.Text;

namespace BIliliveRecorder.Huya.Tars.Tars;

/**
 * 格式化输出tars结构的所有属性
 * 主要用于调试或打日志
 */
public class TarsDisplayer
{
    private StringBuilder sb;
    private int _level = 0;

    private void ps(string fieldName)
    {
        for (var i = 0; i < _level; ++i)
        {
            sb.Append('\t');
        }

        if (fieldName != null)
        {
            sb.Append(fieldName).Append(": ");
        }
    }

    public TarsDisplayer(StringBuilder sb, int level)
    {
        this.sb = sb;
        this._level = level;
    }

    public TarsDisplayer(StringBuilder sb)
    {
        this.sb = sb;
    }

    public TarsDisplayer Display(bool b, string fieldName)
    {
        ps(fieldName);
        sb.Append(b ? 'T' : 'F').Append('\n');
        return this;
    }

    public TarsDisplayer Display(byte n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(char n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(short n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(int n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(long n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(float n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(double n, string fieldName)
    {
        ps(fieldName);
        sb.Append(n).Append('\n');
        return this;
    }

    public TarsDisplayer Display(string s, string fieldName)
    {
        ps(fieldName);
        if (null == s)
        {
            sb.Append("null").Append('\n');
        }
        else
        {
            sb.Append(s).Append('\n');
        }

        return this;
    }

    public TarsDisplayer Display(byte[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(char[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(short[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(int[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
            jd.Display(o, null);
        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(long[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(float[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display(double[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display<TK, TV>(Dictionary<TK, TV> m, string fieldName)
    {
        ps(fieldName);
        if (null == m)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (m.Count == 0)
        {
            sb.Append(m.Count).Append(", {}").Append('\n');
            return this;
        }

        sb.Append(m.Count).Append(", {").Append('\n');
        var jd1 = new TarsDisplayer(sb, _level + 1);
        var jd = new TarsDisplayer(sb, _level + 2);
        foreach (var en in m)
        {
            jd1.Display('(', null);
            jd.Display(en.Key, null);
            jd.Display(en.Value, null);
            jd1.Display(')', null);
        }

        Display('}', null);
        return this;
    }

    public TarsDisplayer Display<T>(T[] v, string fieldName)
    {
        ps(fieldName);
        if (null == v)
        {
            sb.Append("null").Append('\n');
            return this;
        }

        if (v.Length == 0)
        {
            sb.Append(v.Length).Append(", []").Append('\n');
            return this;
        }

        sb.Append(v.Length).Append(", [").Append('\n');
        var jd = new TarsDisplayer(sb, _level + 1);
        foreach (var o in v)
        {
            jd.Display(o, null);
        }

        Display(']', null);
        return this;
    }

    public TarsDisplayer Display<T>(List<T> v, string fieldName)
    {
        if (null == v)
        {
            ps(fieldName);
            sb.Append("null").Append('\n');
            return this;
        }
        else
        {
            for (var i = 0; i < v.Count; i++)
            {
                Display(v[i], fieldName);
            }

            return this;
        }
    }

    ////@SuppressWarnings("unchecked")
    public TarsDisplayer Display<T>(T o, string fieldName)
    {
        object oObject = o;

        if (null == o)
        {
            sb.Append("null").Append('\n');
        }

        else if (o is byte)
        {
            Display((byte)oObject, fieldName);
        }
        else if (o is bool)
        {
            Display((bool)oObject, fieldName);
        }
        else if (o is short)
        {
            Display((short)oObject, fieldName);
        }
        else if (o is int)
        {
            Display((int)oObject, fieldName);
        }
        else if (o is long)
        {
            Display((long)oObject, fieldName);
        }
        else if (o is float)
        {
            Display((float)oObject, fieldName);
        }
        else if (o is Double)
        {
            Display((Double)oObject, fieldName);
        }
        else if (o is string)
        {
            Display((string)oObject, fieldName);
        }
        else if (o is TarsStruct)
        {
            Display((TarsStruct)oObject, fieldName);
        }
        else if (o is byte[])
        {
            Display((byte[])oObject, fieldName);
        }
        else if (o is bool[])
        {
            Display((bool[])oObject, fieldName);
        }
        else if (o is short[])
        {
            Display((short[])oObject, fieldName);
        }
        else if (o is int[])
        {
            Display((int[])oObject, fieldName);
        }
        else if (o is long[])
        {
            Display((long[])oObject, fieldName);
        }
        else if (o is float[])
        {
            Display((float[])oObject, fieldName);
        }
        else if (o is double[])
        {
            Display((double[])oObject, fieldName);
        }
        else if (o.GetType().IsArray)
        {
            Display((Object[])oObject, fieldName);
        }
        else if (o is IList)
        {
            var list = (IList)oObject;

            var tmplist = new List<object>();
            foreach (var obj in list)
            {
                tmplist.Add(obj);
            }

            Display(tmplist, fieldName);
        }
        else
        {
            throw new TarsEncodeException("write object error: unsupport type.");
        }

        return this;
    }

    public TarsDisplayer Display(TarsStruct v, string fieldName)
    {
        Display('{', fieldName);
        if (null == v)
        {
            sb.Append('\t').Append("null");
        }
        else
        {
            v.Display(sb, _level + 1);
        }

        Display('}', null);
        return this;
    }
}

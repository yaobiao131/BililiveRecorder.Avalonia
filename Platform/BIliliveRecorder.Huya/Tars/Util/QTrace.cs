using System.Collections;

namespace BIliliveRecorder.Huya.Tars.Util;

public class QTrace
{
    public static void Trace(string value)
    {
#if DEBUG
        var strTrace = DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2") + ":" +
                       DateTime.Now.Second.ToString("D2") + ":" + DateTime.Now.Millisecond.ToString("D3");
        strTrace += "\t" + value;
        System.Diagnostics.Debug.WriteLine(strTrace);
#endif
    }

    static public void Trace(string value, object arg)
    {
#if DEBUG
        var strTrace = $"{value}{arg}";
        System.Diagnostics.Debug.WriteLine(value);
#endif
    }

    public static string Trace(byte[] value)
    {
        var strValue = "\r\n";
        var i = 0;
        foreach (var bt in value)
        {
            strValue += $" {bt,2:x}";
            i++;
            if ((i % 16) == 0)
            {
                strValue += "\n";
            }
        }

        Trace(strValue);
        return strValue;
    }

    public static void Trace(IDictionary dict)
    {
        if (dict == null)
        {
            return;
        }

        var strTrace = " Dictionary: ";
        foreach (var key in dict.Keys)
        {
            strTrace += key.ToString();
            strTrace += "\t\t";
            strTrace += dict[key]?.ToString();
            strTrace += "\r\n";
        }

        Trace(strTrace);
    }

    static public void Trace(IList list)
    {
        if (list == null)
        {
            return;
        }

        var strTrace = " List: ";
        foreach (var item in list)
        {
            strTrace += item.ToString();
            strTrace += "\r\n";
        }

        Trace(strTrace);
    }

    static public void Assert(bool condition)
    {
        System.Diagnostics.Debug.Assert(condition);
    }

    static public string Output(byte[] value)
    {
        var strValue = "";
#if DEBUG
        var i = 0;
        foreach (var bt in value)
        {
            strValue += $"0x{bt:x},";
            i++;
            if ((i % 16) == 0)
            {
                strValue += "\n";
            }
        }
#endif
        return strValue;
    }
}

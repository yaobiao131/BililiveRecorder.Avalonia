using System.Collections;
using System.Text;

namespace BililiveRecorder.Douyu;

internal static class DouyuUtil
{
    private static readonly Dictionary<string, int> ColorMap = new()
    {
        { "1", 0xff2e2e },
        { "2", 0x00ccff },
        { "3", 0x66ff00 },
        { "4", 0xff6600 },
        { "5", 0xcc00ff },
        { "6", 0xf6447f }
    };

    internal static int GetColor(string? col)
    {
        if (string.IsNullOrEmpty(col)) return 0xFFFFFF;
        ColorMap.TryGetValue(col, out var color);
        return color == 0 ? color : 0xFFFFFF;
    }

    internal static byte[] EncodeMessage(string data)
    {
        var length = 9 + data.Length;

        var packetBytes = new List<byte>();

        packetBytes.AddRange(BitConverter.GetBytes(length));
        packetBytes.AddRange(BitConverter.GetBytes(length));

        packetBytes.Add(0xb1);
        packetBytes.Add(0x02);
        packetBytes.Add(0x00);
        packetBytes.Add(0x00);
        packetBytes.AddRange(Encoding.ASCII.GetBytes(data));
        packetBytes.Add(0x00);

        return packetBytes.ToArray();
    }

    private static string? Escape(string? v)
    {
        return v?.Replace("@", "@A").Replace("/", "@S");
    }

    private static string? Unescape(string? v)
    {
        return v?.Replace("@S", "/").Replace("@A", "@");
    }

    internal static string Serialize(object? obj)
    {
        if (obj == null) throw new ArgumentNullException("obj", "Can't serialize null value");

        if (obj is Array array)
        {
            return string.Join("", array.Cast<object>().Select(Serialize));
        }

        if (obj is IEnumerable enumerable and not string)
        {
            return string.Join("", enumerable.Cast<object>().Select(Serialize));
        }

        if (obj.GetType().IsClass && obj.GetType() != typeof(string))
        {
            var properties = obj.GetType().GetProperties();
            return string.Join("", properties.Select(prop =>
            {
                var key = prop.Name;
                var value = prop.GetValue(obj);
                return $"{key}@={Serialize(value)}";
            }));
        }

        return Escape(obj.ToString()) + "/";
    }

    internal static object? Deserialize(string raw)
    {
        if (raw.Contains("//"))
        {
            return raw.Split(["//"], StringSplitOptions.RemoveEmptyEntries)
                .Select(Deserialize)
                .ToList();
        }

        if (raw.Contains("@="))
        {
            var parts = raw.Split('/')
                .Where(part => !string.IsNullOrEmpty(part))
                .ToList();

            var dict = new Dictionary<string, object?>();
            foreach (var part in parts)
            {
                var split = part.Split(["@="], 2, StringSplitOptions.None);
                var key = split[0];
                var val = split.Length > 1 ? split[1] : "";
                dict[key] = val != "" ? Deserialize(val) : "";
            }

            return dict;
        }

        return Unescape(raw);
    }
}

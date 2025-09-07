﻿using System.Security.Cryptography;
using System.Text;

namespace BililiveRecorder.BiliBili;

internal class Wbi
{
    public const string Wts = "wts";
    public const string WRid = "w_rid";

    private static ReadOnlySpan<byte> KeyMap =>
    [
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35,
        27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13,
        37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4,
        22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11, 36, 20, 34, 44, 52
    ];

    public string? Key { get; private set; }

    public void UpdateKey(string img, string sub)
    {
        const int KEY_LENGTH = 32;
        var key = new char[KEY_LENGTH];
        var full = img + sub;
        for (var i = 0; i < KEY_LENGTH; i++)
        {
            key[i] = full[KeyMap[i]];
        }

        Key = new string(key);
    }

    public (string ts, string sign) Sign(IEnumerable<KeyValuePair<string, string>> query) => Sign(query, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    public (string ts, string sign) Sign(IEnumerable<KeyValuePair<string, string>> query, long ts)
    {
        if (Key == null)
            throw new InvalidOperationException("Key is not set.");

        var tsStr = ts.ToString();

        var toBeHashed = query
            .Select(static x =>
                new KeyValuePair<string, string>(x.Key, new string(x.Value.Where(static c => c is not '!' and not '\'' and not '(' and not ')' and not '*').ToArray())))
            .Append(new KeyValuePair<string, string>(Wts, tsStr))
            .OrderBy(static x => x.Key, StringComparer.Ordinal);

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var contentString = new FormUrlEncodedContent(toBeHashed).ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(contentString + Key));
        var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return (tsStr, sign);
    }
}

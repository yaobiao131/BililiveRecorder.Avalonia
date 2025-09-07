﻿#nullable disable
namespace BIliliveRecorder.Huya.Tars.Util;

internal class HexUtil
{
    private static char[] digits =
    [
        '0', '1', '2', '3', '4', //
        '5', '6', '7', '8', '9', //
        'A', 'B', 'C', 'D', 'E', //
        'F'
    ];

    public static byte[] emptybytes = [];

    /**
     * 将单个字节转成Hex string
     * @param b   字节
     * @return string Hex string
     */
    public static string byte2HexStr(byte b)
    {
        char[] buf = new char[2];
        buf[1] = digits[b & 0xF];
        b = (byte)(b >> 4);
        buf[0] = digits[b & 0xF];
        return new string(buf);
    }

    /**
     * 将字节数组转成Hex string
     * @param b
     * @return string
     */
    public static string bytes2HexStr(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        var buf = new char[2 * bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            buf[2 * i + 1] = digits[b & 0xF];
            b = (byte)(b >> 4);
            buf[2 * i + 0] = digits[b & 0xF];
        }

        return new string(buf);
    }

    /**
     * 将单个hex Str转换成字节
     * @param str
     * @return byte
     */
    public static byte hexStr2Byte(string str)
    {
        if (str != null && str.Length == 1)
        {
            return char2Byte(str[0]);
        }
        else
        {
            return 0;
        }
    }

    /**
     * 字符到字节
     * @param ch
     * @return byte
     */
    public static byte char2Byte(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return (byte)(ch - '0');
        }
        else if (ch >= 'a' && ch <= 'f')
        {
            return (byte)(ch - 'a' + 10);
        }
        else if (ch >= 'A' && ch <= 'F')
        {
            return (byte)(ch - 'A' + 10);
        }
        else
        {
            return 0;
        }
    }

    public static byte[] hexStr2Bytes(string str)
    {
        if (str == null || str.Equals(""))
        {
            return emptybytes;
        }

        var bytes = new byte[str.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var high = str[i << 1];
            var low = str[(i << 1) + 1];
            bytes[i] = (byte)(char2Byte(high) * 16 + char2Byte(low));
        }

        return bytes;
    }

    public static byte[] ReverseBytes(byte[] inArray)
    {
        byte temp;
        var highCtr = inArray.Length - 1;

        for (var ctr = 0; ctr < inArray.Length / 2; ctr++)
        {
            temp = inArray[ctr];
            inArray[ctr] = inArray[highCtr];
            inArray[highCtr] = temp;
            highCtr -= 1;
        }

        return inArray;
    }
}

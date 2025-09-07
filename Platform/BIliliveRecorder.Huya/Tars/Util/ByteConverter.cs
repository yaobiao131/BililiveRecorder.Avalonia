#nullable disable
using System.Text;

namespace BIliliveRecorder.Huya.Tars.Util;

internal class ByteConverter
{
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

    public static short ReverseEndian(short value)
    {
        return BitConverter.ToInt16(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static ushort ReverseEndian(ushort value)
    {
        return BitConverter.ToUInt16(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static int ReverseEndian(int value)
    {
        return BitConverter.ToInt32(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static uint ReverseEndian(uint value)
    {
        return BitConverter.ToUInt32(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static long ReverseEndian(long value)
    {
        return BitConverter.ToInt64(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static ulong ReverseEndian(ulong value)
    {
        return BitConverter.ToUInt64(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static float ReverseEndian(float value)
    {
        return BitConverter.ToSingle(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static double ReverseEndian(double value)
    {
        return BitConverter.ToDouble(ReverseBytes(BitConverter.GetBytes(value)), 0);
    }

    public static string Bytes2String(byte[] bytes)
    {
        string strOutput = "";

        int nLenWithoutZero = 0;
        for (; nLenWithoutZero < bytes.Length; nLenWithoutZero++)
        {
            if (bytes[nLenWithoutZero] == 0)
            {
                break;
            }
        }

        var byteWithoutZero = new byte[nLenWithoutZero];

        Array.Copy(bytes, byteWithoutZero, byteWithoutZero.Length);

        var listIndexes = new List<int>();
        for (var i = 0; i < byteWithoutZero.Length - 1; i++)
        {
            // 检查字符串为零
            if (byteWithoutZero[i] == 0)
            {
                break;
            }

            if (byteWithoutZero[i] == 0x14)
            {
                listIndexes.Add(i);
                i++;
            }
        }

        if (listIndexes.Count > 0)
        {
            if (listIndexes[0] > 0)
            {
                strOutput += Encoding.UTF8.GetString(byteWithoutZero, 0, listIndexes[0]);
            }

            strOutput += (char)byteWithoutZero[listIndexes[0]];
            strOutput += (char)byteWithoutZero[listIndexes[0] + 1];
        }

        for (var i = 1; i < listIndexes.Count; i++)
        {
            var num = listIndexes[i] - listIndexes[i - 1] - 2;
            if (num > 0)
            {
                strOutput += Encoding.UTF8.GetString(byteWithoutZero, listIndexes[i - 1] + 2, num);
            }

            strOutput += (char)byteWithoutZero[listIndexes[i]];
            strOutput += (char)byteWithoutZero[listIndexes[i] + 1];
        }


        var leftIndex = 0;
        if (listIndexes.Count > 0)
        {
            leftIndex = listIndexes[listIndexes.Count - 1] + 2;
        }

        if (leftIndex < byteWithoutZero.Length)
        {
            strOutput += Encoding.UTF8.GetString(byteWithoutZero, leftIndex, byteWithoutZero.Length - leftIndex);
        }

        return strOutput;
    }

    public static bool IsCharValidate(char ch)
    {
        var high = (byte)((ch >> 8) & 0xff);
        var low = (byte)(ch & 0xff);
        return high != 0 || ((low & 0x80) == 0);
    }

    /// <summary>
    /// 写本地文件是使用
    /// </summary>
    /// <param name="strInput"></param>
    /// <param name="IsLocalString"></param>
    /// <returns></returns>
    public static byte[] String2Bytes(string strInput, bool IsLocalString)
    {
        return !IsLocalString ? String2Bytes(strInput) : Encoding.UTF8.GetBytes(strInput);
    }

    public static byte[] String2Bytes(string strInput)
    {
        if (strInput == null)
        {
            return null;
        }

        var chars = strInput.ToCharArray();

        var listIndexes = new List<int>();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!IsCharValidate(chars[i]))
            {
                listIndexes.Add(i);
            }
        }

        var bytes = new byte[Encoding.UTF8.GetByteCount(strInput)];
        byte[] temp = null;
        var index = 0;

        if (listIndexes.Count > 0)
        {
            if (listIndexes[0] > 0)
            {
                temp = Encoding.UTF8.GetBytes(chars, 0, listIndexes[0]);
                Array.Copy(temp, 0, bytes, 0, temp.Length);
                index += temp.Length;
            }

            bytes.SetValue((byte)chars[listIndexes[0]], index);
            index += 1;
        }

        for (var i = 1; i < listIndexes.Count; i++)
        {
            var num = listIndexes[i] - listIndexes[i - 1] - 1;
            if (num > 0)
            {
                temp = Encoding.UTF8.GetBytes(chars, listIndexes[i - 1] + 1, num);
                Array.Copy(temp, 0, bytes, index, temp.Length);
                index += temp.Length;
            }

            bytes.SetValue((byte)chars[listIndexes[i]], index);
            index += 1;
        }


        var leftIndex = 0;
        if (listIndexes.Count > 0)
        {
            leftIndex = listIndexes[listIndexes.Count - 1] + 1;
        }

        if (leftIndex < bytes.Length)
        {
            temp = Encoding.UTF8.GetBytes(chars, leftIndex, chars.Length - leftIndex);
            Array.Copy(temp, 0, bytes, index, temp.Length);
            index += temp.Length;
        }

        var output = new byte[index];
        Array.Copy(bytes, output, index);
        return output;
    }
}

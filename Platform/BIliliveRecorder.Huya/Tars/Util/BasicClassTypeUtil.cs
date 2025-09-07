﻿#nullable disable
using System.Text;

namespace BIliliveRecorder.Huya.Tars.Util;

public class BasicClassTypeUtil
{
    /**
         * 将嵌套的类型转成字符串
         * @param listTpye
         * @return
         */
    public static string TransTypeList(List<string> listTpye)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < listTpye.Count; i++)
        {
            listTpye[i] = CS2UniType(listTpye[i]);
        }

        listTpye.Reverse();

        for (var i = 0; i < listTpye.Count; i++)
        {
            var type = (string)listTpye[i];

            if (type == null)
            {
                continue;
            }

            if (type.Equals("list"))
            {
                listTpye[i - 1] = "<" + listTpye[i - 1];
                listTpye[0] = listTpye[0] + ">";
            }
            else if (type.Equals("map"))
            {
                listTpye[i - 1] = "<" + listTpye[i - 1] + ",";
                listTpye[0] = listTpye[0] + ">";
            }
            else if (type.Equals("Array"))
            {
                listTpye[i - 1] = "<" + listTpye[i - 1];
                listTpye[0] = listTpye[0] + ">";
            }
        }

        listTpye.Reverse();
        foreach (var s in listTpye)
        {
            sb.Append(s);
        }

        return sb.ToString();
    }

    public static object CreateObject<T>()
    {
        return CreateObject(typeof(T));
    }

    public static object CreateObject(Type type)
    {
        try
        {
            // String类型没有缺少构造函数，
            if (type.ToString() == "System.String")
            {
                return "";
            }
            else if (type == typeof(byte[]))
            {
                return Array.Empty<byte>();
            }
            else if (type == typeof(short[]))
            {
                return Array.Empty<short>();
            }
            else if (type == typeof(ushort[]))
            {
                return Array.Empty<ushort>();
            }
            else if (type == typeof(int[]))
            {
                return Array.Empty<int>();
            }
            else if (type == typeof(uint[]))
            {
                return Array.Empty<uint>();
            }
            else if (type == typeof(long[]))
            {
                return Array.Empty<long>();
            }
            else if (type == typeof(ulong[]))
            {
                return Array.Empty<ulong>();
            }
            else if (type == typeof(char[]))
            {
                return Array.Empty<char>();
            }

            return Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static object CreateListItem(Type typeList)
    {
        var itemType = typeList.GetGenericArguments();
        if (itemType == null || itemType.Length == 0)
        {
            return null;
        }

        return CreateObject(itemType[0]);
    }

    public static string CS2UniType(string srcType)
    {
        if (srcType.Equals("System.Int16"))
        {
            return "short";
        }
        else if (srcType.Equals("System.UInt16"))
        {
            return "ushort";
        }
        else if (srcType.Equals("System.Int32"))
        {
            return "int32";
        }
        else if (srcType.Equals("System.UInt32"))
        {
            return "uint32";
        }
        else if (srcType.Equals("System.Boolean"))
        {
            return "bool";
        }
        else if (srcType.Equals("System.Byte"))
        {
            return "char";
        }
        else if (srcType.Equals("System.Double"))
        {
            return "double";
        }
        else if (srcType.Equals("System.Single"))
        {
            return "float";
        }
        else if (srcType.Equals("System.Int64"))
        {
            return "int64";
        }
        else if (srcType.Equals("System.UInt64"))
        {
            return "uint64";
        }
        else if (srcType.Equals("System.String"))
        {
            return "string";
        }
        else if (srcType.IndexOf("System.Collections.Generic.QDictionary") == 0)
        {
            return "map";
        }
        else if (srcType.IndexOf("System.Collections.Generic.List") == 0)
        {
            return "list";
        }
        else
        {
            return srcType;
        }
    }

    public static bool IsQDictionary(string cls)
    {
        return cls.IndexOf("System.Collections.Generic.QDictionary") == 0;
    }
}

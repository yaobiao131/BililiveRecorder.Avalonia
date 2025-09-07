#nullable disable
using System.Collections;
using BIliliveRecorder.Huya.Tars.Tars;
using BIliliveRecorder.Huya.Tars.Util;

namespace BIliliveRecorder.Huya.Tars.Tup;

public class UniAttribute : TarsStruct
{
    /**
         * PACKET_TYPE_TUP类型
         */
    protected Dictionary<string, Dictionary<string, byte[]>> _data = null;

    /**
     * 精简版tup，PACKET_TYPE_TUP3类型
     */
    protected Dictionary<string, byte[]> NewData = null;

    /**
     * 存储get后的数据 避免多次解析
     */
    private readonly Dictionary<string, object> _cachedData = new(128);

    protected short _iVer = Const.PACKET_TYPE_TUP3;

    public short Version
    {
        get => _iVer;
        set => _iVer = value;
    }

    private string _encodeName = "UTF-8";

    public string EncodeName
    {
        get => _encodeName;
        set => _encodeName = value;
    }

    TarsInputStream _is = new();

    public UniAttribute()
    {
        _data = new Dictionary<string, Dictionary<string, byte[]>>();
        NewData = new Dictionary<string, byte[]>();
    }

    /**
     * 清除缓存的解析过的数据
     */
    public void ClearCacheData()
    {
        _cachedData.Clear();
    }

    public bool IsEmpty()
    {
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            return NewData.Count == 0;
        }
        else
        {
            return _data.Count == 0;
        }
    }

    public int Size
    {
        get { return _iVer == Const.PACKET_TYPE_TUP3 ? NewData.Count : _data.Count; }
    }

    public bool ContainsKey(string key)
    {
        return _iVer == Const.PACKET_TYPE_TUP3 ? NewData.ContainsKey(key) : _data.ContainsKey(key);
    }

    /**
     * 放入一个元素
     * @param <T>
     * @param name
     * @param t
     */
    public void Put<T>(string name, T t)
    {
        if (name == null)
        {
            throw new ArgumentException("put key can not is null");
        }

        if (t == null)
        {
            throw new ArgumentException("put value can not is null");
        }

        var _out = new TarsOutputStream();
        _out.setServerEncoding(_encodeName);
        _out.Write(t, 0);
        byte[] sBuffer = TarsUtil.GetTarsBufArray(_out.getMemoryStream());

        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            _cachedData.Remove(name);

            if (NewData.ContainsKey(name))
            {
                NewData[name] = sBuffer;
            }
            else
            {
                NewData.Add(name, sBuffer);
            }
        }
        else
        {
            var listType = new List<string>();
            CheckObjectType(listType, t);
            var className = BasicClassTypeUtil.TransTypeList(listType);

            var pair = new Dictionary<string, byte[]>(1) { { className, sBuffer } };
            _cachedData.Remove(name);

            if (_data.ContainsKey(name))
            {
                _data[name] = pair;
            }
            else
            {
                _data.Add(name, pair);
            }
        }
    }

    private object DecodeData(byte[] data, object proxy)
    {
        _is.Wrap(data);
        _is.setServerEncoding(_encodeName);
        var o = _is.Read(proxy, 0, true);
        return o;
    }

    /// <summary>
    /// 获取tup精简版本编码的数据,兼容旧版本tup
    /// </summary>
    /// <param name="name"></param>
    /// <param name="proxy"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ObjectCreateException"></exception>
    public T GetByClass<T>(string name, T proxy)
    {
        object obj = null;
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            if (!NewData.ContainsKey(name))
            {
                return (T)obj;
            }
            else if (_cachedData.ContainsKey(name))
            {
                obj = _cachedData.GetValueOrDefault(name);

                return (T)obj;
            }
            else
            {
                try
                {
                    byte[] data = [];
                    NewData.TryGetValue(name, out data);

                    var o = DecodeData(data, proxy);
                    if (null != o)
                    {
                        SaveDataCache(name, o);
                    }

                    return (T)o;
                }
                catch (Exception ex)
                {
                    throw new ObjectCreateException(ex);
                }
            }
        }
        else //兼容tup2
        {
            return Get<T>(name);
        }
    }


    /**
     * 获取一个元素,只能用于tup版本2，如果待获取的数据为tup3，则抛异常
     * @param <T>
     * @param name
     * @return
     * @throws ObjectCreateException
     */
    public T Get<T>(string name)
    {
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            throw new Exception("data is encoded by new version, please use getTarsStruct(String name,T proxy)");
        }

        object obj = null;

        if (!_data.ContainsKey(name))
        {
            return (T)obj;
        }
        else if (_cachedData.ContainsKey(name))
        {
            obj = _cachedData.GetValueOrDefault(name);

            return (T)obj;
        }
        else
        {
            Dictionary<string, byte[]> pair;
            _data.TryGetValue(name, out pair);

            var strBasicType = "";
            string className = null;
            var data = new byte[0];

            // 找到和T类型对应的数据data
            foreach (var e in pair)
            {
                className = e.Key;
                data = e.Value;

                if (className == null || className == string.Empty)
                {
                    continue;
                }

                // 比较基本类型
                strBasicType = BasicClassTypeUtil.CS2UniType(typeof(T).ToString());
                if (className.Length > 0 && className == strBasicType)
                {
                    break;
                }

                if (strBasicType == "map" && className.Length >= 3 && className.Substring(0, 3).ToLower() == "map")
                {
                    break;
                }

                if (typeof(T).IsArray && className.Length > 3 && className.Substring(0, 4).ToLower() == "list")
                {
                    break;
                }

                if (strBasicType == "list" && className.Length > 3 && className.Substring(0, 4).ToLower() == "list")
                {
                    break;
                }
            }

            try
            {
                var objtmp = GetCacheProxy<T>(className);
                if (objtmp == null)
                {
                    return (T)objtmp;
                }

                obj = DecodeData(data, objtmp);
                if (obj != null)
                {
                    SaveDataCache(name, obj);
                }

                return (T)obj;
            }
            catch (Exception ex)
            {
                QTrace.Trace(this + " Get Exception: " + ex.Message);
                throw new ObjectCreateException(ex);
            }
        }
    }

    /**
      * 获取一个元素,tup新旧版本都兼容
      * @param Name
      * @param DefaultObj
      * @return
      * @throws ObjectCreateException
      */
    public T Get<T>(string name, T defaultObj)
    {
        try
        {
            object result = null;

            if (_iVer == Const.PACKET_TYPE_TUP3)
            {
                result = GetByClass<T>(name, defaultObj);
            }
            else //tup2
            {
                result = Get<T>(name);
            }

            if (result == null)
            {
                return defaultObj;
            }

            return (T)result;
        }
        catch
        {
            return defaultObj;
        }
    }

    private object GetCacheProxy<T>(string className)
    {
        return BasicClassTypeUtil.CreateObject<T>();
    }

    private void SaveDataCache(string name, Object o)
    {
        _cachedData.Add(name, o);
    }

    /**
     * 检测传入的元素类型
     *
     * @param listTpye
     * @param o
     */
    private void CheckObjectType(List<string> listType, object o)
    {
        if (o == null)
        {
            throw new Exception("object is null");
        }

        if (o.GetType().IsArray)
        {
            var elementType = o.GetType().GetElementType();
            listType.Add("list");
            CheckObjectType(listType, BasicClassTypeUtil.CreateObject(elementType));
        }
        else if (o is IList)
        {
            listType.Add("list");

            var list = (IList)o;
            if (list.Count > 0)
            {
                CheckObjectType(listType, list[0]);
            }
            else
            {
                listType.Add("?");
            }
        }
        else if (o is IDictionary)
        {
            listType.Add("map");
            var map = (IDictionary)o;
            if (map.Count > 0)
            {
                foreach (object key in map.Keys)
                {
                    listType.Add(BasicClassTypeUtil.CS2UniType(key.GetType().ToString()));
                    CheckObjectType(listType, map[key]);
                    break;
                }
            }
            else
            {
                listType.Add("?");
                listType.Add("?");
                //throw new ArgumentException("map  can not is empty");
            }
        }
        else
        {
            listType.Add(BasicClassTypeUtil.CS2UniType(o.GetType().ToString()));
        }
    }

    public byte[] Encode()
    {
        var os = new TarsOutputStream(0);
        os.setServerEncoding(_encodeName);
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            os.Write(NewData, 0);
        }
        else
        {
            os.Write(_data, 0);
        }

        return TarsUtil.GetTarsBufArray(os.getMemoryStream());
    }

    public void Decode(byte[] buffer, int index = 0)
    {
        try
        {
            //try tup2
            _is.Wrap(buffer, index);
            _is.setServerEncoding(_encodeName);
            _iVer = Const.PACKET_TYPE_TUP;
            _data = (Dictionary<string, Dictionary<string, byte[]>>)_is
                .readMap(_data, 0, false);
            return;
        }
        catch
        {
            //try tup3
            _iVer = Const.PACKET_TYPE_TUP3;
            _is.Wrap(buffer, index);
            _is.setServerEncoding(_encodeName);
            NewData = (Dictionary<string, byte[]>)_is.readMap(NewData, 0, false);
        }
    }

    public override void WriteTo(TarsOutputStream os)
    {
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            os.Write(NewData, 0);
        }
        else
        {
            os.Write(_data, 0);
        }
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        if (_iVer == Const.PACKET_TYPE_TUP3)
        {
            NewData = (Dictionary<string, byte[]>)inputStream.readMap(NewData, 0, false);
        }
        else
        {
            _data = (Dictionary<string, Dictionary<string, byte[]>>)inputStream
                .readMap(_data, 0, false);
        }
    }
}

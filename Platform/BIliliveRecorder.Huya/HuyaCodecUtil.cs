using BIliliveRecorder.Huya.Proto.Req;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya;

public enum HuyaWupFunctionEnum
{
    DoLaunch,
    Speak,
    GetPropsList,
    OnUserHeartBeat,
    GetLivingInfo,
    SendMessage,
}

public class HuyaCodecUtil
{
    public static byte[] Encode(string servantName, HuyaWupFunctionEnum function, TarsStruct req)
    {
        var wupReq = new WupReq
        {
            TarsServantRequest =
            {
                sServantName = servantName,
                sFuncName = Enum.GetName(typeof(HuyaWupFunctionEnum), function)
            }
        };
        wupReq.UniAttribute.Put("tReq", req);
        return wupReq.Encode();
    }

    public static TarsInputStream NewUtf8TarsInputStream(byte[] bytes)
    {
        var tarsInputStream = new TarsInputStream(bytes);
        tarsInputStream.setServerEncoding("utf-8");
        return tarsInputStream;
    }
}

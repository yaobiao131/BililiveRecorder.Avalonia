using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Http;
using BililiveRecorder.Common.Api.Model;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Douyu.Model;
using BililiveRecorder.Douyu.Templating;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json;

namespace BililiveRecorder.Douyu;

public partial class DouyuHttpApiClient(GlobalConfig config, IJsEngineSwitcher jsEngineSwitcher) : BaseHttpApiClient(config), IDanmakuServerApiClient, ICookieTester
{
    private const string Did = "10000000000000000000000000001501";

    private async Task<string?> GetRidAsync(long roomid)
    {
        var res = await Client!.GetStringAsync($"https://m.douyu.com/{roomid}");
        var matches = RidRegex().Matches(res);
        if (matches.Count > 0 && matches[0].Groups.Count > 1)
        {
            return matches[0].Groups[1].Value;
        }

        return null;
    }

    private async Task<DouyuRespNew?> GetRespNewAsync(long roomid)
    {
        var rid = await GetRidAsync(roomid);
        var res = await Client!.GetStringAsync($"https://www.douyu.com/betard/{rid}");
        return JsonConvert.DeserializeObject<DouyuRespNew>(res);
    }

    public override async Task<RoomInfo?> GetRoomInfoAsync(long roomid)
    {
        var respNew = await GetRespNewAsync(roomid);
        return respNew?.ToRoomInfo();
    }

    public override async Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null)
    {
        var t10 = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var rid = await GetRidAsync(roomid);
        var res = await Client!.GetStringAsync($"https://www.douyu.com/{roomid}");
        var extractMatch = StreamRegex().Match(res);
        if (extractMatch is not { Success: true, Groups.Count: > 1 })
        {
            throw new NotImplementedException();
        }

        var result = extractMatch.Groups[1].Value;
        var funcUb9 = Regex.Replace(result, @"eval.*?;\}", "strc;}");

        var jsEngine = jsEngineSwitcher.CreateDefaultEngine();

        jsEngine.Evaluate(funcUb9);
        var jsRes = jsEngine.CallFunction("ub98484234").ToString()!;

        var vMatch = Regex.Match(jsRes, @"v=(\d+)");
        if (!vMatch.Success)
            throw new Exception("Failed to extract v parameter");
        var v = vMatch.Groups[1].Value;

        var rb = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(rid + Did + t10 + v)));

        var funcSign = Regex.Replace(jsRes, @"return rt;}\);?", "return rt;}");

        funcSign = funcSign.Replace("(function (", "function sign(");
        funcSign = funcSign.Replace("CryptoJS.MD5(cb).toString()", $"\"{rb}\"");
        jsEngine.Evaluate(funcSign);

        jsRes = jsEngine.CallFunction("sign", rid, Did, t10).ToString();
        var playInfo = await GetPlayInfoAsync(rid, jsRes);

        return new StreamInfo
        {
            Url = $"{playInfo.RtmpUrl}/{playInfo.RtmpLive}",
            Qn = playInfo.Rate,
            QnName = StreamQualityNumber.MapToString(playInfo.Rate)
        };
    }

    private async Task<DouyuPlayInfo> GetPlayInfoAsync(string rid, string? param)
    {
        var res = await Client!.PostAsync($"https://www.douyu.com/lapi/live/getH5Play/{rid}?{param}", null);
        var douyuRes = JsonConvert.DeserializeObject<DouyuApiResponse<DouyuPlayInfo>>(await res.Content.ReadAsStringAsync());

        if (douyuRes != null && douyuRes.Error != 0)
        {
            throw new Exception($"Error: {douyuRes.Msg}");
        }

        var resData = douyuRes!.Data;
        if (resData == null)
        {
            throw new Exception("Error: No data");
        }

        return resData;
    }

    public Dictionary<string, string> DanmakuHeaders => Headers.Concat([
        new KeyValuePair<string, string>("Accept", "*/*"),
        new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, br"),
    ]).ToDictionary();

    public override Dictionary<string, string> Headers => new()
    {
        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
    };

    public async Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid)
    {
        var rid = await GetRidAsync(roomid);
        return new BaseDanmuInfo
        {
            Url = new Uri("wss://danmuproxy.douyu.com:8506/"),
            Transport = DanmakuTransportMode.Wss,
            RegMessage =
            [
                DouyuUtil.EncodeMessage($"type@=loginreq/roomid@={rid}/"),
                DouyuUtil.EncodeMessage($"type@=joingroup/rid@={rid}/gid@=-9999/")
            ]
        };
    }

    public Task<(bool, string)> TestCookieAsync()
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex("""rid":(\d*),"vipId""")]
    private static partial Regex RidRegex();

    [GeneratedRegex(@"(vdwdae325w_64we[\s\S]*?function ub98484234[\s\S]*?)function")]
    private static partial Regex StreamRegex();
}

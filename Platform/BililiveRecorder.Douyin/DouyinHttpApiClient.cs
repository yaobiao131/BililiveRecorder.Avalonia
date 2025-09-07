using System.Security.Cryptography;
using System.Text;
using System.Web;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Http;
using BililiveRecorder.Common.Api.Model;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Douyin.Model;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json;
using Serilog;

namespace BililiveRecorder.Douyin;

public class DouyinHttpApiClient(GlobalConfig config, IJsEngineSwitcher jsEngineSwitcher) : BaseHttpApiClient(config), IDanmakuServerApiClient, ICookieTester
{
    private readonly ILogger logger = Log.ForContext<DouyinHttpApiClient>();

    private async Task<Tuple<string, string>> GetTTwidAsync()
    {
        var acNonce = string.Empty;
        var ttwid = string.Empty;
        var res = await Client!.GetAsync($"https://live.douyin.com/83033372805");
        res.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies?.First().Split(";").ToList().ForEach(x =>
        {
            if (x.Contains("__ac_nonce"))
            {
                acNonce = x.Split(";")[0].Split("=")[1];
            }
        });
        res = await Client!.GetAsync("https://live.douyin.com/");
        res.Headers.TryGetValues("Set-Cookie", out cookies);
        cookies?.First().Split(";").ToList().ForEach(x =>
        {
            if (x.Contains("ttwid"))
            {
                ttwid = x.Split(";")[0].Split("=")[1];
            }
        });
        Client.DefaultRequestHeaders.Add("Cookie", $"ttwid={ttwid}");
        return new Tuple<string, string>(acNonce, ttwid);
    }

    private async Task<string> FetchAsTextAsync(string url)
    {
        var resp = await Client!.GetAsync(url).ConfigureAwait(false);

        // if (resp.StatusCode == (HttpStatusCode)412)
        //     throw new Http412Exception("Got HTTP Status 412 when requesting " + url);

        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private async Task<T> FetchAsync<T>(string url) where T : class
    {
        var text = await FetchAsTextAsync(url).ConfigureAwait(false);
        var obj = JsonConvert.DeserializeObject<DouyinApiResponse<T>>(text);
        return obj?.StatusCode != 0 ? throw new DouyinApiResponseCodeNotZeroException(obj?.StatusCode, text) : obj.Data!;
    }

    private async Task<DouyinInfo> FetchInfoAsync(long roomid)
    {
        await GetTTwidAsync();
        var url =
            $"https://live.douyin.com/webcast/room/web/enter/?web_rid={roomid}&aid=6383&enter_from=web_live&a_bogus=0&device_platform=web&browser_language=zh-CN&browser_platform=Win32&browser_name=Edg&browser_version=110.0.0.0";
        return await FetchAsync<DouyinInfo>(url);
    }

    public override async Task<RoomInfo?> GetRoomInfoAsync(long roomid)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(DouyinHttpApiClient));
        var douyinInfo = await FetchInfoAsync(roomid);
        return douyinInfo.ToRoomInfo(roomid);
    }

    public override async Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(DouyinHttpApiClient));
        var douyinInfo = await FetchInfoAsync(roomid);
        return GetStreamInfo(douyinInfo);
    }

    private static StreamInfo GetStreamInfo(DouyinInfo douyinInfo)
    {
        var pullData = douyinInfo.Data.First().StreamUrl!.LiveCoreSdkData.PullData;
        var options = pullData.Options;
        // if (options == null)
        // {
        //     logger.Warning("未找到清晰度配置");
        //     return;
        // }

        options.Qualities.Sort((x, y) => y.Level.CompareTo(x.Level));
        var quality = options.Qualities.First();
        pullData.StreamData.Data.TryGetValue(quality.SdkKey, out var stream);
        var flv = stream.Main.Flv;

        return new StreamInfo { Url = flv, Qn = quality.Level, QnName = quality.Name };
    }

    public override Dictionary<string, string> Headers => new()
    {
        { "authority", "live.douyin.com" },
        { "Referer", "https://live.douyin.com/" },
        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
        { "Accept", "*/*" }
    };

    public Dictionary<string, string> DanmakuHeaders
    {
        get
        {
            var (acNonce, ttwid) = GetTTwidAsync().Result;
            return new Dictionary<string, string>
            {
                { "authority", "live.douyin.com" },
                { "Referer", "https://live.douyin.com/" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
                { "Cookie", $"__ac_nonce={acNonce}; ttwid={ttwid}" },
                { "Accept", "*/*" },
                { "Accept-Encoding", "gzip, deflate, br" }
            };
        }
    }

    public async Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid)
    {
        var douyinInfo = await FetchInfoAsync(roomid);
        var userUniqueId = GetUserUniqueId();
        const string versionCode = "180800";
        const string webcastSdkVersion = "1.0.14-beta.0";
        var sigParams = new Dictionary<string, string>
        {
            { "live_id", "1" },
            { "aid", "6383" },
            { "version_code", versionCode },
            { "webcast_sdk_version", webcastSdkVersion },
            { "room_id", douyinInfo.Data[0].IdStr },
            { "sub_room_id", "" },
            { "sub_channel_id", "" },
            { "did_rule", "3" },
            { "user_unique_id", userUniqueId },
            { "device_platform", "web" },
            { "device_type", "" },
            { "ac", "" },
            { "identity", "audience" }
        };
        var signature = GetSignature(GetXMsStub(sigParams));
        var webcast5Params = new Dictionary<string, string>
        {
            { "room_id", douyinInfo.Data[0].IdStr },
            { "compress", "gzip" },
            { "version_code", versionCode },
            { "webcast_sdk_version", webcastSdkVersion },
            { "live_id", "1" },
            { "did_rule", "3" },
            { "user_unique_id", userUniqueId },
            { "identity", "audience" },
            { "signature", signature }
        };
        var wssUrl = $"wss://webcast5-ws-web-lf.douyin.com/webcast/im/push/v2/?{string.Join("&", webcast5Params.Select(kv => $"{kv.Key}={kv.Value}"))}";
        return new BaseDanmuInfo
        {
            Url = new Uri(BuildRequestUrl(wssUrl)),
            Transport = DanmakuTransportMode.Wss
        };
    }

    private static string BuildRequestUrl(string url)
    {
        var uri = new Uri(url);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);

        queryParams["aid"] = "6383";
        queryParams["device_platform"] = "web";
        queryParams["browser_language"] = "zh-CN";
        queryParams["browser_platform"] = "Win32";
        queryParams["browser_name"] = "Mozilla";
        queryParams["browser_version"] = "92.0.4515.159";

        var uriBuilder = new UriBuilder(uri)
        {
            Query = queryParams.ToString()
        };

        return uriBuilder.ToString();
    }

    private static string GetUserUniqueId()
    {
        var random = new Random();
        const long min = 7300000000000000000L;
        const long max = 7999999999999999999L;
        const double range = max - min;
        var randomLong = min + (long)(random.NextDouble() * range);
        return randomLong.ToString();
    }

    private static string GetXMsStub(Dictionary<string, string> dict)
    {
        var sigParams = string.Join(",", dict.Select(kv => $"{kv.Key}={kv.Value}"));

        var inputBytes = Encoding.UTF8.GetBytes(sigParams);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    private string GetSignature(string xMsStub)
    {
        var jsDom = """
                    document = {}
                    window = {}
                    navigator = {
                        'userAgent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36'
                    }
                    """.Trim();
        var jsEngine = jsEngineSwitcher.CreateDefaultEngine();
        jsEngine.Evaluate($"{jsDom}{LoadJs()}");
        return HttpUtility.UrlDecode(jsEngine.CallFunction("get_sign", xMsStub).ToString()) ?? "00000000";
    }

    private static string LoadJs()
    {
        using var streamReader = File.OpenText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "webmssdk.js"));
        return streamReader.ReadToEnd();
    }

    public Task<(bool, string)> TestCookieAsync()
    {
        throw new NotImplementedException();
    }
}

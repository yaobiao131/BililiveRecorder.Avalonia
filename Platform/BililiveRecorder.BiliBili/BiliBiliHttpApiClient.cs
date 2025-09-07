using System.Net;
using System.Text.RegularExpressions;
using BililiveRecorder.BiliBili.Model;
using BililiveRecorder.BiliBili.Templating;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Http;
using BililiveRecorder.Common.Api.Model;
using BililiveRecorder.Common.Config.V3;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using RoomInfo = BililiveRecorder.Common.Api.Model.RoomInfo;
using RoomPlayInfo = BililiveRecorder.BiliBili.Model.RoomPlayInfo;
using StreamCodec = BililiveRecorder.BiliBili.Model.StreamCodec;

namespace BililiveRecorder.BiliBili;

public class BiliBiliHttpApiClient(GlobalConfig config) : BaseHttpApiClient(config), IBiliBiliDanmakuServerApiClient, ICookieTester
{
    private readonly ILogger logger = Log.ForContext<BiliBiliHttpApiClient>();
    private readonly Random random = new();

    private static readonly Regex matchCookieUidRegex = new(@"DedeUserID=(\d+?);?(?=\b|$)", RegexOptions.Compiled);
    private static readonly Regex matchCookieBuvid3Regex = new(@"buvid3=(.+?);?(?=\b|$)", RegexOptions.Compiled);
    private long uid;
    private string? buvid3;

    private readonly Wbi wbi = new();
    private DateTimeOffset wbiLastUpdate = DateTimeOffset.MinValue;
    private static readonly TimeSpan wbiUpdateInterval = TimeSpan.FromHours(2);

    private readonly SemaphoreSlim wbiSemaphoreSlim = new(1, 1);

    private async Task UpdateWbiKeyAsync()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(BiliBiliHttpApiClient));

        if (wbiLastUpdate + wbiUpdateInterval > DateTimeOffset.UtcNow)
            return;

        await wbiSemaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            if (wbiLastUpdate + wbiUpdateInterval > DateTimeOffset.UtcNow)
                return;

            const string URL = "https://api.bilibili.com/x/web-interface/nav";
            var resp = await Client!.GetAsync(URL).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jo = JObject.Parse(text);

            var wbi_img = (jo["data"]?["wbi_img"]) ?? throw new Exception("failed to get wbi key");
            var img_url = wbi_img["img_url"]?.ToObject<string>() ?? throw new Exception("failed to get wbi key");
            var sub_url = wbi_img["sub_url"]?.ToObject<string>() ?? throw new Exception("failed to get wbi key");

            string img, sub;

            {
                var slash = img_url.LastIndexOf('/');
                var dot = img_url.IndexOf('.', slash);
                if (slash == -1 || dot == -1)
                    throw new Exception("failed to get wbi key");
                img = img_url.Substring(slash + 1, dot - slash - 1);
            }

            {
                var slash = sub_url.LastIndexOf('/');
                var dot = sub_url.IndexOf('.', slash);
                if (slash == -1 || dot == -1)
                    throw new Exception("failed to get wbi key");
                sub = sub_url.Substring(slash + 1, dot - slash - 1);
            }

            if (string.IsNullOrWhiteSpace(img) || string.IsNullOrWhiteSpace(sub))
                throw new Exception("failed to get wbi key");

            wbi.UpdateKey(img, sub);
            wbiLastUpdate = DateTimeOffset.UtcNow;
        }
        finally
        {
            wbiSemaphoreSlim.Release();
        }
    }

    private async Task<string> FetchAsTextAsync(string url)
    {
        var resp = await Client!.GetAsync(url).ConfigureAwait(false);

        if (resp.StatusCode == (HttpStatusCode)412)
            throw new Http412Exception("Got HTTP Status 412 when requesting " + url);

        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private async Task<T> FetchAsync<T>(string url) where T : class
    {
        var text = await FetchAsTextAsync(url).ConfigureAwait(false);
        var obj = JsonConvert.DeserializeObject<BilibiliApiResponse<T>>(text);
        return obj?.Code != 0 ? throw new BilibiliApiResponseCodeNotZeroException(obj?.Code, text) : obj.Data;
    }


    public override async Task<RoomInfo?> GetRoomInfoAsync(long roomid)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(BiliBiliHttpApiClient));

        await UpdateWbiKeyAsync().ConfigureAwait(false);

        Url url = $"{config.LiveApiHost}/xlive/web-room/v1/index/getInfoByRoom?room_id={roomid}&web_location=444.8";
        var q = url.QueryParams;

        var sign = wbi.Sign(q.Select(static x => new KeyValuePair<string, string>(x.Name, x.Value?.ToString() ?? string.Empty)));

        q.AddOrReplace(Wbi.WRid, sign.sign);
        q.AddOrReplace(Wbi.Wts, sign.ts);

        var text = await FetchAsTextAsync(url).ConfigureAwait(false);

        var jobject = JObject.Parse(text);

        var obj = jobject.ToObject<BilibiliApiResponse<Model.RoomInfo>>();
        if (obj?.Code != 0)
            throw new BilibiliApiResponseCodeNotZeroException(obj?.Code, text);
        return obj.Data?.ToCommonRoomInfo() ?? throw new BilibiliApiResponseCodeNotZeroException(obj.Code, text);
    }

    public override Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(BiliBiliHttpApiClient));
        return GetProcessStreamUrlAsync(roomid, allowedQn);
    }

    private async Task<StreamInfo> GetProcessStreamUrlAsync(long roomid, string? llowedQn)
    {
        var allowedQn = ParseAllowedQn(llowedQn);
        const int DefaultQn = 10000;
        var codecItems = await GetCodecItemInStreamUrlAsync(roomid: roomid, qn: DefaultQn).ConfigureAwait(false);
        var allAvailableCodecQn = new List<StreamCodecQn>();
        if (codecItems.avc is not null)
        {
            allAvailableCodecQn.AddRange(codecItems.avc.AcceptQn.Select(x => new StreamCodecQn
            {
                Codec = StreamCodec.AVC,
                Qn = x
            }));
        }

        if (codecItems.hevc is not null)
        {
            allAvailableCodecQn.AddRange(codecItems.hevc.AcceptQn.Select(x => new StreamCodecQn
            {
                Codec = StreamCodec.HEVC,
                Qn = x
            }));
        }

        StreamCodecQn selectedCodecQn;
        // Select first avaiable qn
        foreach (var qn in allowedQn)
        {
            if (allAvailableCodecQn.Contains(qn))
            {
                selectedCodecQn = qn;
                goto match_qn_success;
            }
        }

        logger.Information("没有符合设置要求的画质，稍后再试。设置画质 {QnSettings}, 可用画质 {AcceptQn}", allowedQn, allAvailableCodecQn);
        throw new NoMatchingQnValueException();

        match_qn_success:
        logger.Debug("设置画质 {QnSettings}, 可用画质 {AcceptQn}, 最终选择 {SelectedQn}", allowedQn, allAvailableCodecQn, selectedCodecQn);

        if (selectedCodecQn.Qn != DefaultQn)
        {
            // 最终选择的 qn 与默认不同，需要重新请求一次
            codecItems = await GetCodecItemInStreamUrlAsync(roomid: roomid, qn: selectedCodecQn.Qn).ConfigureAwait(false);
        }

        var item = selectedCodecQn.Codec switch
        {
            StreamCodec.AVC => codecItems.avc,
            StreamCodec.HEVC => codecItems.hevc,
            _ => throw new Exception("unknown codec")
        };

        if (item is null)
            throw new Exception("no supported stream url for " + selectedCodecQn);

        if (item.CurrentQn != selectedCodecQn.Qn)
            logger.Warning("返回的直播流地址的画质是 {CurrentQn} 而不是请求的 {SelectedQn}", item.CurrentQn, selectedCodecQn);

        var url_infos = item.UrlInfos;
        if (url_infos is null || url_infos.Length == 0)
            throw new Exception("no url_info");

        // https:// xy0x0x0x0xy.mcdn.bilivideo.cn:486
        var url_infos_without_mcdn = url_infos.Where(x => !x.Host.Contains(".mcdn.")).ToArray();

        var url_info = url_infos_without_mcdn.Length != 0
            ? url_infos_without_mcdn[random.Next(url_infos_without_mcdn.Length)]
            : url_infos[random.Next(url_infos.Length)];

        var fullUrl = url_info.Host + item.BaseUrl + url_info.Extra;
        return new StreamInfo
        {
            Url = fullUrl,
            QnName = StreamQualityNumber.MapToString(item.CurrentQn)
        };
        // return (fullUrl, new StreamCodecQn { Codec = selectedCodecQn.Codec, Qn = item.CurrentQn });
    }

    private Task<RoomPlayInfo> GetRoomPlayInfoAsync(long roomid, int qn)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(BiliBiliHttpApiClient));

        Url url =
            $"{config.LiveApiHost}/xlive/web-room/v2/index/getRoomPlayInfo?room_id=0&no_playurl=0&mask=1&qn=0&platform=web&protocol=0,1&format=0,1,2&codec=0,1,2&dolby=5&panorama=1&hdr_type=0,1&web_location=444.8";
        var q = url.QueryParams;

        q.AddOrReplace("room_id", roomid);
        q.AddOrReplace("qn", qn);

        var sign = wbi.Sign(q.Select(static x => new KeyValuePair<string, string>(x.Name, x.Value?.ToString() ?? string.Empty)));
        q.AddOrReplace(Wbi.WRid, sign.sign);
        q.AddOrReplace(Wbi.Wts, sign.ts);
        return FetchAsync<RoomPlayInfo>(url);
    }

    private async Task<(RoomPlayInfo.CodecItem? avc, RoomPlayInfo.CodecItem? hevc)> GetCodecItemInStreamUrlAsync(long roomid, int qn)
    {
        var apiResp = await GetRoomPlayInfoAsync(roomid: roomid, qn: qn).ConfigureAwait(false);
        var url_data = apiResp?.PlayurlInfo?.Playurl?.Streams;

        if (url_data is null) throw new Exception("playurl is null");

        var url_http_stream_flv =
            url_data.FirstOrDefault(x => x.ProtocolName == "http_stream")
                ?.Formats?.FirstOrDefault(x => x.FormatName == "flv");

        if (url_http_stream_flv?.Codecs?.Length == 0) throw new Exception("no supported stream");

        var avc = url_http_stream_flv?.Codecs?.FirstOrDefault(x => x.CodecName == "avc");
        var hevc = url_http_stream_flv?.Codecs?.FirstOrDefault(x => x.CodecName == "hevc");

        return (avc, hevc);
    }

    internal static readonly char[] QnParseSeparator = [',', '，', '、', ' '];

    private static IReadOnlyList<StreamCodecQn> ParseAllowedQn(string? allowedQn)
    {
        if (string.IsNullOrWhiteSpace(allowedQn)) return Array.Empty<StreamCodecQn>();

        var qns = allowedQn!.Split(QnParseSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(static x =>
            {
                if (int.TryParse(x, out var num))
                {
                    return new StreamCodecQn
                    {
                        Qn = num,
                        Codec = StreamCodec.AVC
                    };
                }
                else if (x.StartsWith("avc", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(x[3..], out num))
                    {
                        return new StreamCodecQn
                        {
                            Qn = num,
                            Codec = StreamCodec.AVC
                        };
                    }
                }
                else if (x.StartsWith("hevc", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(x[4..], out num))
                    {
                        return new StreamCodecQn
                        {
                            Qn = num,
                            Codec = StreamCodec.HEVC
                        };
                    }
                }

                // invalid
                return new StreamCodecQn
                {
                    Qn = -1,
                    Codec = StreamCodec.AVC
                };
            })
            .Where(x => x.Qn >= 0)
            .ToList();

        return qns;
    }

    public long GetUid() => uid;

    public string? GetBuvid3() => buvid3;

    public override Dictionary<string, string> Headers
    {
        get
        {
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json, text/javascript, */*; q=0.01" },
                { "Accept-Language", "zh-CN" },
                { "Origin", "https://live.bilibili.com" },
                { "Referer", "https://live.bilibili.com/" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
            };
            var cookie_string = config.Cookie;
            if (!string.IsNullOrWhiteSpace(cookie_string))
            {
                headers.Add("Cookie", cookie_string);
                _ = long.TryParse(matchCookieUidRegex.Match(cookie_string).Groups[1].Value, out var uid);
                this.uid = uid;
                var buvid3 = matchCookieBuvid3Regex.Match(cookie_string).Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(buvid3))
                    this.buvid3 = buvid3;
                else
                    this.buvid3 = null;
            }
            else
            {
                uid = 0;
                buvid3 = Buvid.GenerateLocalId();
                headers.Add("Cookie", $"buvid3={buvid3}");
            }

            return headers;
        }
    }

    public Dictionary<string, string> DanmakuHeaders => new()
    {
        { "Origin", "https://live.bilibili.com" },
        { "Referer", "https://live.bilibili.com/" },
        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
        { "Accept-Language", "zh-CN" },
        { "Accept", "*/*" },
        { "Pragma", "no-cache" },
        { "Cache-Control", "no-cache" },
    };

    public async Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(BiliBiliHttpApiClient));

        var url = $"{config.LiveApiHost}/xlive/web-room/v1/index/getDanmuInfo?id={roomid}&type=0";
        var danmuInfo = await FetchAsync<DanmuInfo>(url);
        return danmuInfo;
    }

    public async Task<(bool, string)> TestCookieAsync()
    {
        // 需要测试 cookie 的情况不需要风控和失败检测
        var resp = await Client!
            .GetStringAsync("https://api.live.bilibili.com/xlive/web-ucenter/user/get_user_info")
            .ConfigureAwait(false);
        var jo = JObject.Parse(resp);
        if (jo["code"]?.ToObject<int>() != 0)
            return (false, $"Response:\n{resp}");

        var message = $@"User: {jo["data"]?["uname"]?.ToObject<string>()}
UID (from API response): {jo["data"]?["uid"]?.ToObject<string>()}
UID (from Cookie): {GetUid()}
BUVID3 (from Cookie): {GetBuvid3()}";
        return (true, message);
    }
}

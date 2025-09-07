using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Http;
using BililiveRecorder.Common.Api.Model;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Config.V3;
using BIliliveRecorder.Huya.Model;
using BIliliveRecorder.Huya.Proto;
using BIliliveRecorder.Huya.Proto.Dto;
using Newtonsoft.Json;

namespace BIliliveRecorder.Huya;

public partial class HuyaHttpApiClient(GlobalConfig config) : BaseHttpApiClient(config), IDanmakuServerApiClient, ICookieTester
{
    public override Dictionary<string, string> Headers => new()
    {
        {
            "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36"
        }
    };

    public Dictionary<string, string> DanmakuHeaders
    {
        get => Headers.Concat([
            new KeyValuePair<string, string>("Accept", "*/*"),
            new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, br")
        ]).ToDictionary();
    }

    [GeneratedRegex("var TT_ROOM_DATA = (.*?);")]
    private static partial Regex ProfileRegex();

    [GeneratedRegex("var hyPlayerConfig = (.*?);", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex PlayerConfigRegex();

    private async Task<HuyaRoomInfo?> GetRoomProfileAsync(long roomId)
    {
        var res = await Client!.GetStringAsync($"https://www.huya.com/{roomId}");
        var profileMatches = ProfileRegex().Match(res);
        var playerConfigMatches = PlayerConfigRegex().Match(res);
        if (!profileMatches.Success || !playerConfigMatches.Success)
        {
            return null;
        }

        var profile = JsonConvert.DeserializeObject<HuyaRoomInfo.InnerProfile>(profileMatches.Groups[1].Value);
        var playerConfig = JsonConvert.DeserializeObject<HuyaRoomInfo.InnerPlayerConfig>(playerConfigMatches.Groups[1].Value);
        return new HuyaRoomInfo
        {
            Profile = profile!,
            PlayerConfig = playerConfig!
        };
    }

    private async Task<HuyaRoomInfo?> GetApiResponseAsync(long roomid)
    {
        var res = await Client!.GetStringAsync($"https://mp.huya.com/cache.php?m=Live&do=profileRoom&roomid={roomid}");
        return JsonConvert.DeserializeObject<HuyaApiResponse<HuyaRoomInfo>>(res)?.Data;
    }

    public override async Task<RoomInfo?> GetRoomInfoAsync(long roomid)
    {
        var huyaRoomInfo = await GetRoomProfileAsync(roomid);
        return huyaRoomInfo?.ToRoomInfo();
    }

    public override async Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null)
    {
        var huyaRoomInfo = await GetRoomProfileAsync(roomid);
        // 是否星秀
        var isXingXiu = huyaRoomInfo?.Profile.Gid == 1663;
        var baseSteamInfoList = huyaRoomInfo?.PlayerConfig.Stream.Data[0].GameStreamInfoList;
        if (baseSteamInfoList == null)
            throw new Exception("Failed to get Stream.");

        // var selectCdn = roomInfo?.LiveData.MStreamRatioWeb;
        // var maxEntry = selectCdn?.Aggregate((x, y) => x.Value > y.Value ? x : y);

        var streamInfo = baseSteamInfoList.First(stream => stream.SCdnType == "TX");
        if (streamInfo == null)
            throw new Exception("Failed to get Stream.");
        const int codec = 264;
        var streamName = streamInfo.SStreamName;
        // var cdn = streamInfo.SCdnType.ToLower();
        var suffix = streamInfo.SFlvUrlSuffix;
        var antiCode = streamInfo.SFlvAntiCode;
        if (!isXingXiu)
        {
            antiCode = BuildAntiCode(streamInfo);
        }

        antiCode += $"&codec={codec}";
        var baseUrl = streamInfo.SFlvUrl.Replace("http://", "https://");
        var url = $"{baseUrl}/{streamName}.{suffix}?{antiCode}";
        return new StreamInfo
        {
            Url = url,
            Qn = 0,
            QnName = "Huya"
        };
    }

    private static string BuildAntiCode(HuyaRoomInfo.GameStreamInfoListItem streamInfo)
    {
        var uid = GetUid(streamInfo.SStreamName);
        var query = HttpUtility.ParseQueryString(streamInfo.SFlvAntiCode);
        var platformId = query["t"] ?? "100";
        var wsTime = query["wsTime"];
        var ctype = query["ctype"];
        var fs = query["fs"];
        var fmEncoded = query["fm"];
        var fm = Uri.UnescapeDataString(fmEncoded);
        var uid32 = (uint)(uid & 0xFFFFFFFF);
        var convertUid = (uid32 << 8) | (uid32 >> 24);

        var seqId = uid + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var wsTimeValue = Convert.ToInt64(wsTime, 16);
        var random = new Random();
        var randomPart = random.NextDouble();
        var ct = (long)((wsTimeValue + randomPart) * 1000);
        // Process fm
        var fmBytes = Convert.FromBase64String(fm);
        var fmDecoded = Encoding.UTF8.GetString(fmBytes);
        var wsSecretPrefix = fmDecoded.Split('_')[0];

        var hashInput = $"{seqId}|{ctype}|{platformId}";
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(hashInput));
        var wsSecretHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        var secretStr = $"{wsSecretPrefix}_{convertUid}_{streamInfo.SStreamName}_{wsSecretHash}_{wsTime}";
        var secretBytes = MD5.HashData(Encoding.UTF8.GetBytes(secretStr));
        var wsSecret = BitConverter.ToString(secretBytes).Replace("-", "").ToLower();

        // Generate UUID
        var uuidValue = (long)((ct % 10000000000 + random.NextDouble()) * 1000) % 0xFFFFFFFF;
        var uuid = uuidValue.ToString();

        // Final parameters
        var sdkSid = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var antiCodeParams = new Dictionary<string, string?>
        {
            { "wsSecret", wsSecret },
            { "wsTime", wsTime },
            { "seqid", seqId.ToString() },
            { "ctype", ctype },
            { "ver", "1" },
            { "fs", fs },
            { "t", platformId },
            { "u", convertUid.ToString() },
            { "uuid", uuid },
            { "sdk_sid", sdkSid.ToString() },
        };

        return string.Join("&", antiCodeParams.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    private static long GetUid(string streamName)
    {
        try
        {
            if (!string.IsNullOrEmpty(streamName))
            {
                var parts = streamName.Split('-');
                if (parts.Length > 0 && long.TryParse(parts[0], out var anchorUid) && anchorUid > 0)
                {
                    return anchorUid;
                }
            }
        }
        catch
        {
            // Ignore exceptions
        }

        // Generate random between 1.4e12 and 1.5e12
        var rand = new Random();
        const double range = 1.5e12 - 1.4e12;
        return (long)(rand.NextDouble() * range) + 1400000000000;
    }

    public async Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid)
    {
        var roomInfo = await GetApiResponseAsync(roomid);
        var userInfo = new WsUserInfo
        {
            LUid = roomInfo?.LiveData.Yyid ?? 0,
            BAnonymous = true,
            LTid = roomInfo?.ChTopId ?? 0,
            LSid = roomInfo?.SubChId ?? 0
        };

        var webSocketCommand = new WebSocketCommand
        {
            iCmdType = (int)EWebSocketCommandType.EWSCmd_RegisterReq,
            VData = userInfo.ToByteArray()
        };
        // var getPropsListReq = new GetPropsListReq
        // {
        //     tUserId =
        //     {
        //         lUid = roomInfo?.LiveData.Yyid ?? 0,
        //         sHuYaUA = "webh5&2309271152&websocket"
        //     },
        //     iTemplateType = (int)HuyaClientTemplateTypeEnum.TplMirror
        // };

        return new BaseDanmuInfo
        {
            Url = new Uri("wss://cdnws.api.huya.com/"),
            Transport = DanmakuTransportMode.Wss,
            RegMessage =
            [
                webSocketCommand.ToByteArray(),
                // HuyaCodecUtil.Encode("PropsUIServer", HuyaWupFunctionEnum.GetPropsList, getPropsListReq)
            ]
        };
    }

    public Task<(bool, string)> TestCookieAsync()
    {
        throw new NotImplementedException();
    }
}

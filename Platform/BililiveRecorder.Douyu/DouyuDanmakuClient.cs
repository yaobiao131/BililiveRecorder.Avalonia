using System.Buffers;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BililiveRecorder.Douyu;

internal class DouyuDanmakuClient([FromKeyedServices(Platform.Douyu)] IDanmakuServerApiClient apiClient, ILogger logger) : BaseDanmakuClient(apiClient, logger)
{
    protected override byte[] PingMessage => DouyuUtil.EncodeMessage("type@=mrkl/");

    private readonly BufferCode _bufferCode = new();

    protected override Task<Tuple<List<BaseDanmakeModel>, SequencePosition>> DecodeMessageAsync(ReadOnlySequence<byte> bytes)
    {
        var danmakus = new List<BaseDanmakeModel>();

        var messages = _bufferCode.Decode(bytes.ToArray());
        foreach (var messageObject in messages.Select(message => JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(DouyuUtil.Deserialize(message)))))
        {
            switch (messageObject?["type"]?.ToString())
            {
                case "xss":
                    if (messageObject["ss"]?.ToString() == "1")
                    {
                        danmakus.Add(new DanmakuLiveStartModel());
                    }
                    else
                    {
                        danmakus.Add(new DanmakuLiveEndModel());
                    }

                    break;
                case "chatmsg":
                {
                    danmakus.Add(new DanmakuCommentModel
                    {
                        NickName = messageObject["nn"]?.ToString(),
                        Content = messageObject["txt"]?.ToString(),
                        MsgType = DanmakuMsgType.Comment,
                        UserId = Convert.ToInt64(messageObject["uid"]),
                        Color = DouyuUtil.GetColor(messageObject["col"]?.ToString()),
                        RawObject = messageObject
                    });
                    break;
                }
                case "comm_chatmsg":
                {
                    Console.WriteLine(messageObject);
                    break;
                }
                case "dgb":
                {
                    danmakus.Add(new DanmakuGiftModel
                    {
                        MsgType = DanmakuMsgType.GiftSend,
                        NickName = messageObject["nn"]?.ToString(),
                        UserId = Convert.ToInt64(messageObject["uid"]),
                        GiftCount = Convert.ToInt64(messageObject["gfcnt"]),
                        GiftName = messageObject["gfn"]?.ToString()
                    });
                    break;
                }
                default:
                {
                    // Console.WriteLine(messageObject?["type"]?.ToString());
                    break;
                }
            }
        }

        return Task.FromResult(new Tuple<List<BaseDanmakeModel>, SequencePosition>(danmakus, bytes.End));
    }
}

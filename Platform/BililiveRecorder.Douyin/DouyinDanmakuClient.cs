using System.Buffers;
using System.IO.Compression;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Douyin.Model.Danmaku;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BililiveRecorder.Douyin;

internal class DouyinDanmakuClient([FromKeyedServices(Platform.Douyin)] IDanmakuServerApiClient apiClient, ILogger logger) : BaseDanmakuClient(apiClient, logger)
{
    protected override byte[] PingMessage => new PushFrame { PayloadType = "hb" }.ToByteArray();
    protected override int HeartBeatInterval => 10;

    protected override async Task<Tuple<List<BaseDanmakeModel>, SequencePosition>> DecodeMessageAsync(ReadOnlySequence<byte> bytes)
    {
        var danmakus = new List<BaseDanmakeModel>();
        try
        {
            var wssPackage = PushFrame.Parser.ParseFrom(bytes);
            var logId = wssPackage.LogId;
            var decompressedData = Decompress(wssPackage.Payload.ToByteArray());
            var payloadPackage = Response.Parser.ParseFrom(decompressedData);
            if (payloadPackage.NeedAck)
            {
                var obj = new PushFrame
                {
                    Payload = ByteString.CopyFromUtf8("ack"),
                    LogId = logId,
                    PayloadType = payloadPackage.InternalExt,
                };
                var ack = obj.ToByteArray() ?? [];
                if (danmakuTransport != null)
                {
                    await danmakuTransport.SendAsync(ack, 0, ack.Length);
                }
            }

            foreach (var msg in payloadPackage.MessagesList)
            {
                Console.WriteLine($"Method: {msg.Method}");
                switch (msg.Method)
                {
                    case "WebcastControlMessage":
                    {
                        var controlMessage = ControlMessage.Parser.ParseFrom(msg.Payload);
                        Console.WriteLine($"ControlMessage Status: {controlMessage.Status}");
                        if (controlMessage.Status == 3)
                        {
                            danmakus.Add(new DanmakuLiveEndModel());
                        }

                        break;
                    }
                    case "WebcastGiftMessage":
                    {
                        var giftMessage = GiftMessage.Parser.ParseFrom(msg.Payload);
                        danmakus.Add(new DanmakuGiftModel
                        {
                            MsgType = DanmakuMsgType.GiftSend,
                            NickName = giftMessage.User.NickName,
                            UserId = Convert.ToInt64(giftMessage.User.Id),
                            GiftName = giftMessage.Gift.Name,
                            GiftCount = (long)giftMessage.TotalCount
                        });
                        break;
                    }
                    case "WebcastChatMessage":
                    {
                        var chatMessage = ChatMessage.Parser.ParseFrom(msg.Payload);
                        danmakus.Add(new DanmakuCommentModel
                        {
                            MsgType = DanmakuMsgType.Comment,
                            NickName = chatMessage.User.NickName,
                            UserId = Convert.ToInt64(chatMessage.User.Id),
                            Content = chatMessage.Content,
                            Color = 0xFFFFFF,
                            RawObject = JObject.FromObject(chatMessage)
                        });
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return new Tuple<List<BaseDanmakeModel>, SequencePosition>(danmakus, bytes.End);
    }

    private static byte[] Decompress(byte[] compressedData)
    {
        using var compressedStream = new MemoryStream(compressedData);
        using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gzipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}

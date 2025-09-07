using System.Buffers;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using BIliliveRecorder.Huya.Proto;
using BIliliveRecorder.Huya.Proto.Constant;
using BIliliveRecorder.Huya.Tars.Tars;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BIliliveRecorder.Huya;

internal class HuyaDanmakuClient([FromKeyedServices(Platform.Huya)] IDanmakuServerApiClient apiClient, ILogger logger) : BaseDanmakuClient(apiClient, logger)
{
    protected override byte[] PingMessage =>
    [
        0x00, 0x03, 0x1d, 0x00, 0x00, 0x69, 0x00, 0x00, 0x00, 0x69, 0x10, 0x03, 0x2c, 0x3c, 0x4c, 0x56,
        0x08, 0x6f, 0x6e, 0x6c, 0x69, 0x6e, 0x65, 0x75, 0x69, 0x66, 0x0f, 0x4f, 0x6e, 0x55, 0x73, 0x65,
        0x72, 0x48, 0x65, 0x61, 0x72, 0x74, 0x42, 0x65, 0x61, 0x74, 0x7d, 0x00, 0x00, 0x3c, 0x08, 0x00,
        0x01, 0x06, 0x04, 0x74, 0x52, 0x65, 0x71, 0x1d, 0x00, 0x00, 0x2f, 0x0a, 0x0a, 0x0c, 0x16, 0x00,
        0x26, 0x00, 0x36, 0x07, 0x61, 0x64, 0x72, 0x5f, 0x77, 0x61, 0x70, 0x46, 0x00, 0x0b, 0x12, 0x03,
        0xae, 0xf0, 0x0f, 0x22, 0x03, 0xae, 0xf0, 0x0f, 0x3c, 0x42, 0x6d, 0x52, 0x02, 0x60, 0x5c, 0x60,
        0x01, 0x7c, 0x82, 0x00, 0x0b, 0xb0, 0x1f, 0x9c, 0xac, 0x0b, 0x8c, 0x98, 0x0c, 0xa8, 0x0c
    ];

    protected override Task<Tuple<List<BaseDanmakeModel>, SequencePosition>> DecodeMessageAsync(ReadOnlySequence<byte> bytes)
    {
        var danmakus = new List<BaseDanmakeModel>();
        var message = bytes.ToArray();
        var length = bytes.End;
        try
        {
            var stream = new TarsInputStream(message);
            var webSocketCommand = new WebSocketCommand();
            webSocketCommand.ReadFrom(stream);
            switch (webSocketCommand.iCmdType)
            {
                case (int)EWebSocketCommandType.EWSCmdS2C_MsgPushReq:
                {
                    stream.Wrap(webSocketCommand.VData);
                    var pushMessage = new WsPushMessage();
                    pushMessage.ReadFrom(stream);
                    switch ((HuyaCmdEnum)pushMessage.IUri)
                    {
                        case HuyaCmdEnum.MessageNotice:
                        {
                            stream.Wrap(pushMessage.SMsg);
                            var messageNotice = new MessageNotice();
                            messageNotice.ReadFrom(stream);
                            danmakus.Add(new DanmakuCommentModel
                            {
                                MsgType = DanmakuMsgType.Comment,
                                NickName = messageNotice.tUserInfo.SNickName,
                                Content = messageNotice.sContent,
                                Color = messageNotice.tBulletFormat.iFontColor,
                                RawObject = JObject.FromObject(messageNotice)
                            });
                            break;
                        }
                        case HuyaCmdEnum.SendItemSubBroadcastPacket:
                        {
                            stream.Wrap(pushMessage.SMsg);
                            var sendItemSubBroadcastPacket = new SendItemSubBroadcastPacket();
                            sendItemSubBroadcastPacket.ReadFrom(stream);
                            danmakus.Add(new DanmakuGiftModel
                            {
                                MsgType = DanmakuMsgType.GiftSend,
                                NickName = sendItemSubBroadcastPacket.sSenderNick,
                                UserId = sendItemSubBroadcastPacket.lSenderUid,
                                GiftName = sendItemSubBroadcastPacket.sPropsName,
                                GiftCount = sendItemSubBroadcastPacket.iItemCount,
                            });
                            break;
                        }
                    }

                    break;
                }
                case (int)EWebSocketCommandType.EWSCmdS2C_MsgPushReq_V2:
                {
                    stream.Wrap(webSocketCommand.VData);
                    var msgv2 = new WsPushMessageV2();
                    msgv2.ReadFrom(stream);

                    foreach (var msgItem in msgv2.VMsgItem)
                    {
                        switch ((HuyaCmdEnum)msgItem.iUri)
                        {
                            case HuyaCmdEnum.MessageNotice:
                            {
                                stream.Wrap(msgItem.sMsg);
                                var messageNotice = new MessageNotice();
                                messageNotice.ReadFrom(stream);
                                danmakus.Add(new DanmakuCommentModel
                                {
                                    MsgType = DanmakuMsgType.Comment,
                                    NickName = messageNotice.tUserInfo.SNickName,
                                    Content = messageNotice.sContent,
                                    Color = messageNotice.tBulletFormat.iFontColor,
                                    RawObject = JObject.FromObject(messageNotice)
                                });
                                break;
                            }
                            case HuyaCmdEnum.SendItemSubBroadcastPacket:
                            {
                                stream.Wrap(msgItem.sMsg);
                                var sendItemSubBroadcastPacket = new SendItemSubBroadcastPacket();
                                sendItemSubBroadcastPacket.ReadFrom(stream);
                                danmakus.Add(new DanmakuGiftModel
                                {
                                    MsgType = DanmakuMsgType.GiftSend,
                                    NickName = sendItemSubBroadcastPacket.sSenderNick,
                                    UserId = sendItemSubBroadcastPacket.lSenderUid,
                                    GiftName = sendItemSubBroadcastPacket.sPropsName,
                                    GiftCount = sendItemSubBroadcastPacket.iItemCount,
                                });
                                break;
                            }
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception)
        {
            length = bytes.Start;
        }

        return Task.FromResult(new Tuple<List<BaseDanmakeModel>, SequencePosition>(danmakus, length));
    }
}

// ******************************
//  GENERATED CODE, DO NOT EDIT MANUALLY.
//  SEE /config_gen/README.md
// ******************************

using System.ComponentModel;
using HierarchicalPropertyDefault;
using Newtonsoft.Json;

namespace BililiveRecorder.Common.Config.V3;

[JsonObject(MemberSerialization.OptIn)]
public sealed partial class RoomConfig
{
    /// <summary>
    /// 房间号
    /// </summary>
    public long RoomId { get => GetPropertyValue<long>(); set => SetPropertyValue(value); }
    public bool HasRoomId { get => GetPropertyHasValue(nameof(RoomId)); set => SetPropertyHasValue<long>(value, nameof(RoomId)); }
    [JsonProperty(nameof(RoomId)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<long> OptionalRoomId { get => GetPropertyValueOptional<long>(nameof(RoomId)); set => SetPropertyValueOptional(value, nameof(RoomId)); }
    
    /// <summary>
    /// 平台
    /// </summary>
    public Platform Platform { get => GetPropertyValue<Platform>(); set => SetPropertyValue(value); }
    public bool HasPlatform { get => GetPropertyHasValue(nameof(Platform)); set => SetPropertyHasValue<Platform>(value, nameof(Platform)); }
    [JsonProperty(nameof(Platform)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<Platform> OptionalPlatform { get => GetPropertyValueOptional<Platform>(nameof(Platform)); set => SetPropertyValueOptional(value, nameof(Platform)); }

    /// <summary>
    /// 自动录制
    /// </summary>
    public bool AutoRecord { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasAutoRecord { get => GetPropertyHasValue(nameof(AutoRecord)); set => SetPropertyHasValue<bool>(value, nameof(AutoRecord)); }
    [JsonProperty(nameof(AutoRecord)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalAutoRecord { get => GetPropertyValueOptional<bool>(nameof(AutoRecord)); set => SetPropertyValueOptional(value, nameof(AutoRecord)); }

    /// <summary>
    /// 录制模式
    /// </summary>
    public RecordMode RecordMode { get => GetPropertyValue<RecordMode>(); set => SetPropertyValue(value); }
    public bool HasRecordMode { get => GetPropertyHasValue(nameof(RecordMode)); set => SetPropertyHasValue<RecordMode>(value, nameof(RecordMode)); }
    [JsonProperty(nameof(RecordMode)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<RecordMode> OptionalRecordMode { get => GetPropertyValueOptional<RecordMode>(nameof(RecordMode)); set => SetPropertyValueOptional(value, nameof(RecordMode)); }

    /// <summary>
    /// 自动分段模式
    /// </summary>
    public CuttingMode CuttingMode { get => GetPropertyValue<CuttingMode>(); set => SetPropertyValue(value); }
    public bool HasCuttingMode { get => GetPropertyHasValue(nameof(CuttingMode)); set => SetPropertyHasValue<CuttingMode>(value, nameof(CuttingMode)); }
    [JsonProperty(nameof(CuttingMode)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<CuttingMode> OptionalCuttingMode { get => GetPropertyValueOptional<CuttingMode>(nameof(CuttingMode)); set => SetPropertyValueOptional(value, nameof(CuttingMode)); }

    /// <summary>
    /// 自动分段数值
    /// </summary>
    public uint CuttingNumber { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasCuttingNumber { get => GetPropertyHasValue(nameof(CuttingNumber)); set => SetPropertyHasValue<uint>(value, nameof(CuttingNumber)); }
    [JsonProperty(nameof(CuttingNumber)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalCuttingNumber { get => GetPropertyValueOptional<uint>(nameof(CuttingNumber)); set => SetPropertyValueOptional(value, nameof(CuttingNumber)); }

    /// <summary>
    /// 改标题后自动分段
    /// </summary>
    public bool CuttingByTitle { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasCuttingByTitle { get => GetPropertyHasValue(nameof(CuttingByTitle)); set => SetPropertyHasValue<bool>(value, nameof(CuttingByTitle)); }
    [JsonProperty(nameof(CuttingByTitle)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalCuttingByTitle { get => GetPropertyValueOptional<bool>(nameof(CuttingByTitle)); set => SetPropertyValueOptional(value, nameof(CuttingByTitle)); }

    /// <summary>
    /// 弹幕录制
    /// </summary>
    public bool RecordDanmaku { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmaku { get => GetPropertyHasValue(nameof(RecordDanmaku)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmaku)); }
    [JsonProperty(nameof(RecordDanmaku)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmaku { get => GetPropertyValueOptional<bool>(nameof(RecordDanmaku)); set => SetPropertyValueOptional(value, nameof(RecordDanmaku)); }

    /// <summary>
    /// 弹幕录制-原始数据
    /// </summary>
    public bool RecordDanmakuRaw { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuRaw { get => GetPropertyHasValue(nameof(RecordDanmakuRaw)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuRaw)); }
    [JsonProperty(nameof(RecordDanmakuRaw)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuRaw { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuRaw)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuRaw)); }

    /// <summary>
    /// 弹幕录制-SuperChat
    /// </summary>
    public bool RecordDanmakuSuperChat { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuSuperChat { get => GetPropertyHasValue(nameof(RecordDanmakuSuperChat)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuSuperChat)); }
    [JsonProperty(nameof(RecordDanmakuSuperChat)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuSuperChat { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuSuperChat)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuSuperChat)); }

    /// <summary>
    /// 弹幕录制-礼物
    /// </summary>
    public bool RecordDanmakuGift { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuGift { get => GetPropertyHasValue(nameof(RecordDanmakuGift)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuGift)); }
    [JsonProperty(nameof(RecordDanmakuGift)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuGift { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuGift)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuGift)); }

    /// <summary>
    /// 弹幕录制-上船
    /// </summary>
    public bool RecordDanmakuGuard { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuGuard { get => GetPropertyHasValue(nameof(RecordDanmakuGuard)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuGuard)); }
    [JsonProperty(nameof(RecordDanmakuGuard)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuGuard { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuGuard)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuGuard)); }

    /// <summary>
    /// 保存直播封面
    /// </summary>
    public bool SaveStreamCover { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasSaveStreamCover { get => GetPropertyHasValue(nameof(SaveStreamCover)); set => SetPropertyHasValue<bool>(value, nameof(SaveStreamCover)); }
    [JsonProperty(nameof(SaveStreamCover)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalSaveStreamCover { get => GetPropertyValueOptional<bool>(nameof(SaveStreamCover)); set => SetPropertyValueOptional(value, nameof(SaveStreamCover)); }

    /// <summary>
    /// 直播画质
    /// </summary>
    public string? RecordingQuality { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasRecordingQuality { get => GetPropertyHasValue(nameof(RecordingQuality)); set => SetPropertyHasValue<string>(value, nameof(RecordingQuality)); }
    [JsonProperty(nameof(RecordingQuality)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalRecordingQuality { get => GetPropertyValueOptional<string>(nameof(RecordingQuality)); set => SetPropertyValueOptional(value, nameof(RecordingQuality)); }

    /// <summary>
    /// FLV修复-检测到可能缺少数据时分段
    /// </summary>
    public bool FlvProcessorSplitOnScriptTag { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasFlvProcessorSplitOnScriptTag { get => GetPropertyHasValue(nameof(FlvProcessorSplitOnScriptTag)); set => SetPropertyHasValue<bool>(value, nameof(FlvProcessorSplitOnScriptTag)); }
    [JsonProperty(nameof(FlvProcessorSplitOnScriptTag)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalFlvProcessorSplitOnScriptTag { get => GetPropertyValueOptional<bool>(nameof(FlvProcessorSplitOnScriptTag)); set => SetPropertyValueOptional(value, nameof(FlvProcessorSplitOnScriptTag)); }

    /// <summary>
    /// FLV修复-检测到 H264 Annex-B 时禁用修复分段
    /// </summary>
    public bool FlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasFlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyHasValue(nameof(FlvProcessorDisableSplitOnH264AnnexB)); set => SetPropertyHasValue<bool>(value, nameof(FlvProcessorDisableSplitOnH264AnnexB)); }
    [JsonProperty(nameof(FlvProcessorDisableSplitOnH264AnnexB)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalFlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyValueOptional<bool>(nameof(FlvProcessorDisableSplitOnH264AnnexB)); set => SetPropertyValueOptional(value, nameof(FlvProcessorDisableSplitOnH264AnnexB)); }

    /// <summary>
    /// 不录制的标题匹配正则
    /// </summary>
    public string? TitleFilterPatterns { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasTitleFilterPatterns { get => GetPropertyHasValue(nameof(TitleFilterPatterns)); set => SetPropertyHasValue<string>(value, nameof(TitleFilterPatterns)); }
    [JsonProperty(nameof(TitleFilterPatterns)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalTitleFilterPatterns { get => GetPropertyValueOptional<string>(nameof(TitleFilterPatterns)); set => SetPropertyValueOptional(value, nameof(TitleFilterPatterns)); }

    /// <summary>
    /// 录制文件名模板
    /// </summary>
    public string? FileNameRecordTemplate => GetPropertyValue<string>();

    /// <summary>
    /// 是否在视频文件写入直播信息 metadata
    /// </summary>
    public bool FlvWriteMetadata => GetPropertyValue<bool>();

    /// <summary>
    /// WebhookV1
    /// </summary>
    public string? WebHookUrls => GetPropertyValue<string>();

    /// <summary>
    /// WebhookV2
    /// </summary>
    public string? WebHookUrlsV2 => GetPropertyValue<string>();

    /// <summary>
    /// 桌面版在界面显示标题和分区
    /// </summary>
    public bool WpfShowTitleAndArea => GetPropertyValue<bool>();

    /// <summary>
    /// 桌面版开播时弹出系统通知
    /// </summary>
    public bool WpfNotifyStreamStart => GetPropertyValue<bool>();

    /// <summary>
    /// Cookie
    /// </summary>
    public string? Cookie => GetPropertyValue<string>();

    /// <summary>
    /// API Host
    /// </summary>
    public string? LiveApiHost => GetPropertyValue<string>();

    /// <summary>
    /// 主动检查时间间隔 秒
    /// </summary>
    public uint TimingCheckInterval => GetPropertyValue<uint>();

    /// <summary>
    /// 请求mikufansAPI超时时间 毫秒
    /// </summary>
    public uint TimingApiTimeout => GetPropertyValue<uint>();

    /// <summary>
    /// 录制断开重连时间间隔 毫秒
    /// </summary>
    public uint TimingStreamRetry => GetPropertyValue<uint>();

    /// <summary>
    /// 录制无指定画质重连时间间隔 秒
    /// </summary>
    public uint TimingStreamRetryNoQn => GetPropertyValue<uint>();

    /// <summary>
    /// 连接直播服务器超时时间 毫秒
    /// </summary>
    public uint TimingStreamConnect => GetPropertyValue<uint>();

    /// <summary>
    /// 弹幕服务器重连时间间隔 毫秒
    /// </summary>
    public uint TimingDanmakuRetry => GetPropertyValue<uint>();

    /// <summary>
    /// 最大未收到直播数据时间 毫秒
    /// </summary>
    public uint TimingWatchdogTimeout => GetPropertyValue<uint>();

    /// <summary>
    /// 触发刷新弹幕写入缓冲的个数
    /// </summary>
    public uint RecordDanmakuFlushInterval => GetPropertyValue<uint>();

    /// <summary>
    /// 使用的弹幕服务器传输协议
    /// </summary>
    public DanmakuTransportMode DanmakuTransport => GetPropertyValue<DanmakuTransportMode>();

    /// <summary>
    /// 使用直播间主播的uid进行弹幕服务器认证
    /// </summary>
    public bool DanmakuAuthenticateWithStreamerUid => GetPropertyValue<bool>();

    /// <summary>
    /// 是否使用系统代理
    /// </summary>
    public bool NetworkTransportUseSystemProxy => GetPropertyValue<bool>();

    /// <summary>
    /// 允许使用的 IP 网络类型
    /// </summary>
    public AllowedAddressFamily NetworkTransportAllowedAddressFamily => GetPropertyValue<AllowedAddressFamily>();

    /// <summary>
    /// 自定义脚本
    /// </summary>
    public string? UserScript => GetPropertyValue<string>();

}

[JsonObject(MemberSerialization.OptIn)]
public sealed partial class GlobalConfig : HierarchicalObject<DefaultConfig, GlobalConfig>
{
    /// <summary>
    /// 录制模式
    /// </summary>
    public RecordMode RecordMode { get => GetPropertyValue<RecordMode>(); set => SetPropertyValue(value); }
    public bool HasRecordMode { get => GetPropertyHasValue(nameof(RecordMode)); set => SetPropertyHasValue<RecordMode>(value, nameof(RecordMode)); }
    [JsonProperty(nameof(RecordMode)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<RecordMode> OptionalRecordMode { get => GetPropertyValueOptional<RecordMode>(nameof(RecordMode)); set => SetPropertyValueOptional(value, nameof(RecordMode)); }

    /// <summary>
    /// 自动分段模式
    /// </summary>
    public CuttingMode CuttingMode { get => GetPropertyValue<CuttingMode>(); set => SetPropertyValue(value); }
    public bool HasCuttingMode { get => GetPropertyHasValue(nameof(CuttingMode)); set => SetPropertyHasValue<CuttingMode>(value, nameof(CuttingMode)); }
    [JsonProperty(nameof(CuttingMode)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<CuttingMode> OptionalCuttingMode { get => GetPropertyValueOptional<CuttingMode>(nameof(CuttingMode)); set => SetPropertyValueOptional(value, nameof(CuttingMode)); }

    /// <summary>
    /// 自动分段数值
    /// </summary>
    public uint CuttingNumber { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasCuttingNumber { get => GetPropertyHasValue(nameof(CuttingNumber)); set => SetPropertyHasValue<uint>(value, nameof(CuttingNumber)); }
    [JsonProperty(nameof(CuttingNumber)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalCuttingNumber { get => GetPropertyValueOptional<uint>(nameof(CuttingNumber)); set => SetPropertyValueOptional(value, nameof(CuttingNumber)); }

    /// <summary>
    /// 改标题后自动分段
    /// </summary>
    public bool CuttingByTitle { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasCuttingByTitle { get => GetPropertyHasValue(nameof(CuttingByTitle)); set => SetPropertyHasValue<bool>(value, nameof(CuttingByTitle)); }
    [JsonProperty(nameof(CuttingByTitle)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalCuttingByTitle { get => GetPropertyValueOptional<bool>(nameof(CuttingByTitle)); set => SetPropertyValueOptional(value, nameof(CuttingByTitle)); }

    /// <summary>
    /// 弹幕录制
    /// </summary>
    public bool RecordDanmaku { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmaku { get => GetPropertyHasValue(nameof(RecordDanmaku)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmaku)); }
    [JsonProperty(nameof(RecordDanmaku)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmaku { get => GetPropertyValueOptional<bool>(nameof(RecordDanmaku)); set => SetPropertyValueOptional(value, nameof(RecordDanmaku)); }

    /// <summary>
    /// 弹幕录制-原始数据
    /// </summary>
    public bool RecordDanmakuRaw { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuRaw { get => GetPropertyHasValue(nameof(RecordDanmakuRaw)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuRaw)); }
    [JsonProperty(nameof(RecordDanmakuRaw)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuRaw { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuRaw)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuRaw)); }

    /// <summary>
    /// 弹幕录制-SuperChat
    /// </summary>
    public bool RecordDanmakuSuperChat { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuSuperChat { get => GetPropertyHasValue(nameof(RecordDanmakuSuperChat)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuSuperChat)); }
    [JsonProperty(nameof(RecordDanmakuSuperChat)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuSuperChat { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuSuperChat)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuSuperChat)); }

    /// <summary>
    /// 弹幕录制-礼物
    /// </summary>
    public bool RecordDanmakuGift { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuGift { get => GetPropertyHasValue(nameof(RecordDanmakuGift)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuGift)); }
    [JsonProperty(nameof(RecordDanmakuGift)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuGift { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuGift)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuGift)); }

    /// <summary>
    /// 弹幕录制-上船
    /// </summary>
    public bool RecordDanmakuGuard { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuGuard { get => GetPropertyHasValue(nameof(RecordDanmakuGuard)); set => SetPropertyHasValue<bool>(value, nameof(RecordDanmakuGuard)); }
    [JsonProperty(nameof(RecordDanmakuGuard)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalRecordDanmakuGuard { get => GetPropertyValueOptional<bool>(nameof(RecordDanmakuGuard)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuGuard)); }

    /// <summary>
    /// 保存直播封面
    /// </summary>
    public bool SaveStreamCover { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasSaveStreamCover { get => GetPropertyHasValue(nameof(SaveStreamCover)); set => SetPropertyHasValue<bool>(value, nameof(SaveStreamCover)); }
    [JsonProperty(nameof(SaveStreamCover)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalSaveStreamCover { get => GetPropertyValueOptional<bool>(nameof(SaveStreamCover)); set => SetPropertyValueOptional(value, nameof(SaveStreamCover)); }

    /// <summary>
    /// 直播画质
    /// </summary>
    public string? RecordingQuality { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasRecordingQuality { get => GetPropertyHasValue(nameof(RecordingQuality)); set => SetPropertyHasValue<string>(value, nameof(RecordingQuality)); }
    [JsonProperty(nameof(RecordingQuality)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalRecordingQuality { get => GetPropertyValueOptional<string>(nameof(RecordingQuality)); set => SetPropertyValueOptional(value, nameof(RecordingQuality)); }

    /// <summary>
    /// 录制文件名模板
    /// </summary>
    public string? FileNameRecordTemplate { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasFileNameRecordTemplate { get => GetPropertyHasValue(nameof(FileNameRecordTemplate)); set => SetPropertyHasValue<string>(value, nameof(FileNameRecordTemplate)); }
    [JsonProperty(nameof(FileNameRecordTemplate)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalFileNameRecordTemplate { get => GetPropertyValueOptional<string>(nameof(FileNameRecordTemplate)); set => SetPropertyValueOptional(value, nameof(FileNameRecordTemplate)); }

    /// <summary>
    /// FLV修复-检测到可能缺少数据时分段
    /// </summary>
    public bool FlvProcessorSplitOnScriptTag { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasFlvProcessorSplitOnScriptTag { get => GetPropertyHasValue(nameof(FlvProcessorSplitOnScriptTag)); set => SetPropertyHasValue<bool>(value, nameof(FlvProcessorSplitOnScriptTag)); }
    [JsonProperty(nameof(FlvProcessorSplitOnScriptTag)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalFlvProcessorSplitOnScriptTag { get => GetPropertyValueOptional<bool>(nameof(FlvProcessorSplitOnScriptTag)); set => SetPropertyValueOptional(value, nameof(FlvProcessorSplitOnScriptTag)); }

    /// <summary>
    /// FLV修复-检测到 H264 Annex-B 时禁用修复分段
    /// </summary>
    public bool FlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasFlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyHasValue(nameof(FlvProcessorDisableSplitOnH264AnnexB)); set => SetPropertyHasValue<bool>(value, nameof(FlvProcessorDisableSplitOnH264AnnexB)); }
    [JsonProperty(nameof(FlvProcessorDisableSplitOnH264AnnexB)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalFlvProcessorDisableSplitOnH264AnnexB { get => GetPropertyValueOptional<bool>(nameof(FlvProcessorDisableSplitOnH264AnnexB)); set => SetPropertyValueOptional(value, nameof(FlvProcessorDisableSplitOnH264AnnexB)); }

    /// <summary>
    /// 是否在视频文件写入直播信息 metadata
    /// </summary>
    public bool FlvWriteMetadata { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasFlvWriteMetadata { get => GetPropertyHasValue(nameof(FlvWriteMetadata)); set => SetPropertyHasValue<bool>(value, nameof(FlvWriteMetadata)); }
    [JsonProperty(nameof(FlvWriteMetadata)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalFlvWriteMetadata { get => GetPropertyValueOptional<bool>(nameof(FlvWriteMetadata)); set => SetPropertyValueOptional(value, nameof(FlvWriteMetadata)); }

    /// <summary>
    /// 不录制的标题匹配正则
    /// </summary>
    public string? TitleFilterPatterns { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasTitleFilterPatterns { get => GetPropertyHasValue(nameof(TitleFilterPatterns)); set => SetPropertyHasValue<string>(value, nameof(TitleFilterPatterns)); }
    [JsonProperty(nameof(TitleFilterPatterns)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalTitleFilterPatterns { get => GetPropertyValueOptional<string>(nameof(TitleFilterPatterns)); set => SetPropertyValueOptional(value, nameof(TitleFilterPatterns)); }

    /// <summary>
    /// WebhookV1
    /// </summary>
    public string? WebHookUrls { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasWebHookUrls { get => GetPropertyHasValue(nameof(WebHookUrls)); set => SetPropertyHasValue<string>(value, nameof(WebHookUrls)); }
    [JsonProperty(nameof(WebHookUrls)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalWebHookUrls { get => GetPropertyValueOptional<string>(nameof(WebHookUrls)); set => SetPropertyValueOptional(value, nameof(WebHookUrls)); }

    /// <summary>
    /// WebhookV2
    /// </summary>
    public string? WebHookUrlsV2 { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasWebHookUrlsV2 { get => GetPropertyHasValue(nameof(WebHookUrlsV2)); set => SetPropertyHasValue<string>(value, nameof(WebHookUrlsV2)); }
    [JsonProperty(nameof(WebHookUrlsV2)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalWebHookUrlsV2 { get => GetPropertyValueOptional<string>(nameof(WebHookUrlsV2)); set => SetPropertyValueOptional(value, nameof(WebHookUrlsV2)); }

    /// <summary>
    /// 桌面版在界面显示标题和分区
    /// </summary>
    public bool WpfShowTitleAndArea { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasWpfShowTitleAndArea { get => GetPropertyHasValue(nameof(WpfShowTitleAndArea)); set => SetPropertyHasValue<bool>(value, nameof(WpfShowTitleAndArea)); }
    [JsonProperty(nameof(WpfShowTitleAndArea)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalWpfShowTitleAndArea { get => GetPropertyValueOptional<bool>(nameof(WpfShowTitleAndArea)); set => SetPropertyValueOptional(value, nameof(WpfShowTitleAndArea)); }

    /// <summary>
    /// 桌面版开播时弹出系统通知
    /// </summary>
    public bool WpfNotifyStreamStart { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasWpfNotifyStreamStart { get => GetPropertyHasValue(nameof(WpfNotifyStreamStart)); set => SetPropertyHasValue<bool>(value, nameof(WpfNotifyStreamStart)); }
    [JsonProperty(nameof(WpfNotifyStreamStart)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalWpfNotifyStreamStart { get => GetPropertyValueOptional<bool>(nameof(WpfNotifyStreamStart)); set => SetPropertyValueOptional(value, nameof(WpfNotifyStreamStart)); }

    /// <summary>
    /// Cookie
    /// </summary>
    public string? Cookie { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasCookie { get => GetPropertyHasValue(nameof(Cookie)); set => SetPropertyHasValue<string>(value, nameof(Cookie)); }
    [JsonProperty(nameof(Cookie)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalCookie { get => GetPropertyValueOptional<string>(nameof(Cookie)); set => SetPropertyValueOptional(value, nameof(Cookie)); }

    /// <summary>
    /// API Host
    /// </summary>
    public string? LiveApiHost { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasLiveApiHost { get => GetPropertyHasValue(nameof(LiveApiHost)); set => SetPropertyHasValue<string>(value, nameof(LiveApiHost)); }
    [JsonProperty(nameof(LiveApiHost)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalLiveApiHost { get => GetPropertyValueOptional<string>(nameof(LiveApiHost)); set => SetPropertyValueOptional(value, nameof(LiveApiHost)); }

    /// <summary>
    /// 主动检查时间间隔 秒
    /// </summary>
    public uint TimingCheckInterval { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingCheckInterval { get => GetPropertyHasValue(nameof(TimingCheckInterval)); set => SetPropertyHasValue<uint>(value, nameof(TimingCheckInterval)); }
    [JsonProperty(nameof(TimingCheckInterval)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingCheckInterval { get => GetPropertyValueOptional<uint>(nameof(TimingCheckInterval)); set => SetPropertyValueOptional(value, nameof(TimingCheckInterval)); }

    /// <summary>
    /// 请求mikufansAPI超时时间 毫秒
    /// </summary>
    public uint TimingApiTimeout { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingApiTimeout { get => GetPropertyHasValue(nameof(TimingApiTimeout)); set => SetPropertyHasValue<uint>(value, nameof(TimingApiTimeout)); }
    [JsonProperty(nameof(TimingApiTimeout)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingApiTimeout { get => GetPropertyValueOptional<uint>(nameof(TimingApiTimeout)); set => SetPropertyValueOptional(value, nameof(TimingApiTimeout)); }

    /// <summary>
    /// 录制断开重连时间间隔 毫秒
    /// </summary>
    public uint TimingStreamRetry { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingStreamRetry { get => GetPropertyHasValue(nameof(TimingStreamRetry)); set => SetPropertyHasValue<uint>(value, nameof(TimingStreamRetry)); }
    [JsonProperty(nameof(TimingStreamRetry)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingStreamRetry { get => GetPropertyValueOptional<uint>(nameof(TimingStreamRetry)); set => SetPropertyValueOptional(value, nameof(TimingStreamRetry)); }

    /// <summary>
    /// 录制无指定画质重连时间间隔 秒
    /// </summary>
    public uint TimingStreamRetryNoQn { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingStreamRetryNoQn { get => GetPropertyHasValue(nameof(TimingStreamRetryNoQn)); set => SetPropertyHasValue<uint>(value, nameof(TimingStreamRetryNoQn)); }
    [JsonProperty(nameof(TimingStreamRetryNoQn)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingStreamRetryNoQn { get => GetPropertyValueOptional<uint>(nameof(TimingStreamRetryNoQn)); set => SetPropertyValueOptional(value, nameof(TimingStreamRetryNoQn)); }

    /// <summary>
    /// 连接直播服务器超时时间 毫秒
    /// </summary>
    public uint TimingStreamConnect { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingStreamConnect { get => GetPropertyHasValue(nameof(TimingStreamConnect)); set => SetPropertyHasValue<uint>(value, nameof(TimingStreamConnect)); }
    [JsonProperty(nameof(TimingStreamConnect)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingStreamConnect { get => GetPropertyValueOptional<uint>(nameof(TimingStreamConnect)); set => SetPropertyValueOptional(value, nameof(TimingStreamConnect)); }

    /// <summary>
    /// 弹幕服务器重连时间间隔 毫秒
    /// </summary>
    public uint TimingDanmakuRetry { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingDanmakuRetry { get => GetPropertyHasValue(nameof(TimingDanmakuRetry)); set => SetPropertyHasValue<uint>(value, nameof(TimingDanmakuRetry)); }
    [JsonProperty(nameof(TimingDanmakuRetry)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingDanmakuRetry { get => GetPropertyValueOptional<uint>(nameof(TimingDanmakuRetry)); set => SetPropertyValueOptional(value, nameof(TimingDanmakuRetry)); }

    /// <summary>
    /// 最大未收到直播数据时间 毫秒
    /// </summary>
    public uint TimingWatchdogTimeout { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasTimingWatchdogTimeout { get => GetPropertyHasValue(nameof(TimingWatchdogTimeout)); set => SetPropertyHasValue<uint>(value, nameof(TimingWatchdogTimeout)); }
    [JsonProperty(nameof(TimingWatchdogTimeout)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalTimingWatchdogTimeout { get => GetPropertyValueOptional<uint>(nameof(TimingWatchdogTimeout)); set => SetPropertyValueOptional(value, nameof(TimingWatchdogTimeout)); }

    /// <summary>
    /// 触发刷新弹幕写入缓冲的个数
    /// </summary>
    public uint RecordDanmakuFlushInterval { get => GetPropertyValue<uint>(); set => SetPropertyValue(value); }
    public bool HasRecordDanmakuFlushInterval { get => GetPropertyHasValue(nameof(RecordDanmakuFlushInterval)); set => SetPropertyHasValue<uint>(value, nameof(RecordDanmakuFlushInterval)); }
    [JsonProperty(nameof(RecordDanmakuFlushInterval)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<uint> OptionalRecordDanmakuFlushInterval { get => GetPropertyValueOptional<uint>(nameof(RecordDanmakuFlushInterval)); set => SetPropertyValueOptional(value, nameof(RecordDanmakuFlushInterval)); }

    /// <summary>
    /// 使用的弹幕服务器传输协议
    /// </summary>
    public DanmakuTransportMode DanmakuTransport { get => GetPropertyValue<DanmakuTransportMode>(); set => SetPropertyValue(value); }
    public bool HasDanmakuTransport { get => GetPropertyHasValue(nameof(DanmakuTransport)); set => SetPropertyHasValue<DanmakuTransportMode>(value, nameof(DanmakuTransport)); }
    [JsonProperty(nameof(DanmakuTransport)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<DanmakuTransportMode> OptionalDanmakuTransport { get => GetPropertyValueOptional<DanmakuTransportMode>(nameof(DanmakuTransport)); set => SetPropertyValueOptional(value, nameof(DanmakuTransport)); }

    /// <summary>
    /// 使用直播间主播的uid进行弹幕服务器认证
    /// </summary>
    public bool DanmakuAuthenticateWithStreamerUid { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasDanmakuAuthenticateWithStreamerUid { get => GetPropertyHasValue(nameof(DanmakuAuthenticateWithStreamerUid)); set => SetPropertyHasValue<bool>(value, nameof(DanmakuAuthenticateWithStreamerUid)); }
    [JsonProperty(nameof(DanmakuAuthenticateWithStreamerUid)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalDanmakuAuthenticateWithStreamerUid { get => GetPropertyValueOptional<bool>(nameof(DanmakuAuthenticateWithStreamerUid)); set => SetPropertyValueOptional(value, nameof(DanmakuAuthenticateWithStreamerUid)); }

    /// <summary>
    /// 是否使用系统代理
    /// </summary>
    public bool NetworkTransportUseSystemProxy { get => GetPropertyValue<bool>(); set => SetPropertyValue(value); }
    public bool HasNetworkTransportUseSystemProxy { get => GetPropertyHasValue(nameof(NetworkTransportUseSystemProxy)); set => SetPropertyHasValue<bool>(value, nameof(NetworkTransportUseSystemProxy)); }
    [JsonProperty(nameof(NetworkTransportUseSystemProxy)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<bool> OptionalNetworkTransportUseSystemProxy { get => GetPropertyValueOptional<bool>(nameof(NetworkTransportUseSystemProxy)); set => SetPropertyValueOptional(value, nameof(NetworkTransportUseSystemProxy)); }

    /// <summary>
    /// 允许使用的 IP 网络类型
    /// </summary>
    public AllowedAddressFamily NetworkTransportAllowedAddressFamily { get => GetPropertyValue<AllowedAddressFamily>(); set => SetPropertyValue(value); }
    public bool HasNetworkTransportAllowedAddressFamily { get => GetPropertyHasValue(nameof(NetworkTransportAllowedAddressFamily)); set => SetPropertyHasValue<AllowedAddressFamily>(value, nameof(NetworkTransportAllowedAddressFamily)); }
    [JsonProperty(nameof(NetworkTransportAllowedAddressFamily)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<AllowedAddressFamily> OptionalNetworkTransportAllowedAddressFamily { get => GetPropertyValueOptional<AllowedAddressFamily>(nameof(NetworkTransportAllowedAddressFamily)); set => SetPropertyValueOptional(value, nameof(NetworkTransportAllowedAddressFamily)); }

    /// <summary>
    /// 自定义脚本
    /// </summary>
    public string? UserScript { get => GetPropertyValue<string>(); set => SetPropertyValue(value); }
    public bool HasUserScript { get => GetPropertyHasValue(nameof(UserScript)); set => SetPropertyHasValue<string>(value, nameof(UserScript)); }
    [JsonProperty(nameof(UserScript)), EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<string?> OptionalUserScript { get => GetPropertyValueOptional<string>(nameof(UserScript)); set => SetPropertyValueOptional(value, nameof(UserScript)); }

}

public sealed class DefaultConfig
{
    public static readonly DefaultConfig Instance = new DefaultConfig();
    private DefaultConfig() { }

    public RecordMode RecordMode => RecordMode.Standard;

    public CuttingMode CuttingMode => CuttingMode.Disabled;

    public uint CuttingNumber => 100;

    public bool CuttingByTitle => false;

    public bool RecordDanmaku => false;

    public bool RecordDanmakuRaw => false;

    public bool RecordDanmakuSuperChat => true;

    public bool RecordDanmakuGift => false;

    public bool RecordDanmakuGuard => true;

    public bool SaveStreamCover => false;

    public string RecordingQuality => @"avc10000,hevc10000";

    public string FileNameRecordTemplate => @"{{ roomId }}-{{ name }}/录制-{{ roomId }}-{{ ""now"" | time_zone: ""Asia/Shanghai"" | format_date: ""yyyyMMdd-HHmmss-fff"" }}-{{ title }}.flv";

    public bool FlvProcessorSplitOnScriptTag => false;

    public bool FlvProcessorDisableSplitOnH264AnnexB => false;

    public bool FlvWriteMetadata => true;

    public string TitleFilterPatterns => @"";

    public string WebHookUrls => @"";

    public string WebHookUrlsV2 => @"";

    public bool WpfShowTitleAndArea => true;

    public bool WpfNotifyStreamStart => false;

    public string Cookie => @"";

    public string LiveApiHost => @"https://api.live.bilibili.com";

    public uint TimingCheckInterval => 180;

    public uint TimingApiTimeout => 10000;

    public uint TimingStreamRetry => 6000;

    public uint TimingStreamRetryNoQn => 90;

    public uint TimingStreamConnect => 5000;

    public uint TimingDanmakuRetry => 9000;

    public uint TimingWatchdogTimeout => 10000;

    public uint RecordDanmakuFlushInterval => 20;

    public DanmakuTransportMode DanmakuTransport => DanmakuTransportMode.Wss;

    public bool DanmakuAuthenticateWithStreamerUid => false;

    public bool NetworkTransportUseSystemProxy => false;

    public AllowedAddressFamily NetworkTransportAllowedAddressFamily => AllowedAddressFamily.Any;

    public string UserScript => @"";

}

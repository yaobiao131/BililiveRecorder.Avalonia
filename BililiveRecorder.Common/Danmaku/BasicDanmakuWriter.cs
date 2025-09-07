using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Common.Config.V3;
using Serilog;

namespace BililiveRecorder.Common.Danmaku;

public partial class BasicDanmakuWriter : IBasicDanmakuWriter
{
    private static readonly XmlWriterSettings XmlWriterSettings = new()
    {
        Async = true,
        Indent = true,
        IndentChars = "  ",
        Encoding = Encoding.UTF8,
        CloseOutput = true,
        WriteEndDocumentOnClose = true,
    };

    [GeneratedRegex(@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", RegexOptions.Compiled)]
    private static partial Regex InvalidXmlChars();

    private static string RemoveInvalidXmlChars(string? text) => string.IsNullOrWhiteSpace(text) ? string.Empty : InvalidXmlChars().Replace(text, string.Empty);

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ILogger _logger;
    private XmlWriter? _xmlWriter;
    private readonly Stopwatch _dmTime = new();
    private uint _writeCount;
    private RoomConfig? _config;

    public BasicDanmakuWriter(ILogger logger)
    {
        _logger = logger.ForContext<BasicDanmakuWriter>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Disable()
    {
        if (_disposedValue) return;
        _semaphoreSlim.Wait();
        try
        {
            DisableCore();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void DisableCore()
    {
        try
        {
            if (_xmlWriter == null) return;

            _xmlWriter.Close();
            _xmlWriter.Dispose();
            _xmlWriter = null;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "关闭弹幕文件时发生错误");
            _xmlWriter = null;
        }
    }

    public void EnableWithPath(string path, IRoom room)
    {
        if (_disposedValue) return;
        _semaphoreSlim.Wait();
        try
        {
            if (_xmlWriter != null)
            {
                _xmlWriter.Close();
                _xmlWriter.Dispose();
                _xmlWriter = null;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            catch (Exception)
            {
            }

            var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);

            _config = room.RoomConfig;

            _xmlWriter = XmlWriter.Create(stream, XmlWriterSettings);
            WriteStartDocument(_xmlWriter, room);
            _dmTime.Restart();
            _writeCount = 0;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task WriteAsync(BaseDanmakeModel baseDanmakeModel)
    {
        if (_disposedValue)
            return;

        if (_xmlWriter is null || _config is null)
            return;

        await _semaphoreSlim.WaitAsync();
        try
        {
            if (_xmlWriter is null)
                return;
            var write = true;
            var recordDanmakuRaw = _config.RecordDanmakuRaw;
            switch (baseDanmakeModel.MsgType)
            {
                case DanmakuMsgType.Comment:
                {
                    var danmakuModel = baseDanmakeModel as DanmakuCommentModel;
                    const int type = 1;
                    var size = danmakuModel?.FontSize;
                    const long st = 0L;
                    var color = danmakuModel?.Color;
                    var ts = Math.Max(_dmTime.Elapsed.TotalSeconds, 0d);
                    await _xmlWriter.WriteStartElementAsync(null, "d", null).ConfigureAwait(false);
                    await _xmlWriter.WriteAttributeStringAsync(null, "p", null, $"{ts:F3},{type},{size},{color},{st},0,{danmakuModel?.UserId},0").ConfigureAwait(false);
                    await _xmlWriter.WriteAttributeStringAsync(null, "user", null, RemoveInvalidXmlChars(danmakuModel?.NickName)).ConfigureAwait(false);
                    if (recordDanmakuRaw)
                        await _xmlWriter.WriteAttributeStringAsync(null, "raw", null, RemoveInvalidXmlChars(danmakuModel?.RawObject?.ToString(Newtonsoft.Json.Formatting.None)))
                            .ConfigureAwait(false);
                    _xmlWriter.WriteValue(RemoveInvalidXmlChars(danmakuModel?.Content));
                    await _xmlWriter.WriteEndElementAsync().ConfigureAwait(false);
                }
                    break;
                case DanmakuMsgType.GiftSend:
                    if (_config.RecordDanmakuGift)
                    {
                        var danmakuGiftModel = baseDanmakeModel as DanmakuGiftModel;
                        await _xmlWriter.WriteStartElementAsync(null, "gift", null).ConfigureAwait(false);
                        var ts = Math.Max(_dmTime.Elapsed.TotalSeconds, 0d);
                        await _xmlWriter.WriteAttributeStringAsync(null, "ts", null, ts.ToString("F3")).ConfigureAwait(false);
                        await _xmlWriter.WriteAttributeStringAsync(null, "user", null, RemoveInvalidXmlChars(danmakuGiftModel?.NickName)).ConfigureAwait(false);
                        await _xmlWriter.WriteAttributeStringAsync(null, "uid", null, danmakuGiftModel?.UserId.ToString()).ConfigureAwait(false);
                        await _xmlWriter.WriteAttributeStringAsync(null, "giftname", null, RemoveInvalidXmlChars(danmakuGiftModel?.GiftName)).ConfigureAwait(false);
                        await _xmlWriter.WriteAttributeStringAsync(null, "giftcount", null, danmakuGiftModel?.GiftCount.ToString()).ConfigureAwait(false);
                        if (recordDanmakuRaw)
                            await _xmlWriter.WriteAttributeStringAsync(null, "raw", null,
                                RemoveInvalidXmlChars(danmakuGiftModel?.RawObject?.ToString(Newtonsoft.Json.Formatting.None))).ConfigureAwait(false);
                        await _xmlWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    break;
                default:
                    write = false;
                    break;
            }

            if (write && _writeCount++ >= _config.RecordDanmakuFlushInterval)
            {
                await _xmlWriter.FlushAsync();
                _writeCount = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "写入弹幕时发生错误");
            DisableCore();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private static void WriteStartDocument(XmlWriter writer, Common.IRoom room)
    {
        writer.WriteStartDocument();
        writer.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"#s\"");
        writer.WriteStartElement("i");
        writer.WriteComment("\nmikufans录播姬 " + GitVersionInformation.InformationalVersion +
                            "\nhttps://rec.danmuji.org/user/danmaku/\n本文件的弹幕信息兼容mikufans主站视频弹幕XML格式\n本XML自带样式可以在浏览器里打开（推荐使用Chrome）\n\nsc 为SuperChat\ngift为礼物\nguard为上船\n\nattribute \"raw\" 为原始数据\n");
        writer.WriteElementString("chatserver", "chat.bilibili.com");
        writer.WriteElementString("chatid", "0");
        writer.WriteElementString("mission", "0");
        writer.WriteElementString("maxlimit", "1000");
        writer.WriteElementString("state", "0");
        writer.WriteElementString("real_name", "0");
        writer.WriteElementString("source", "0");

        writer.WriteStartElement("BililiveRecorder");
        writer.WriteAttributeString("version", GitVersionInformation.FullSemVer);
        writer.WriteEndElement();

        writer.WriteStartElement("BililiveRecorderRecordInfo");
        writer.WriteAttributeString("roomid", room.RoomConfig.RoomId.ToString());
        writer.WriteAttributeString("shortid", room.ShortId.ToString());
        writer.WriteAttributeString("name", RemoveInvalidXmlChars(room.Name));
        writer.WriteAttributeString("title", RemoveInvalidXmlChars(room.Title));
        writer.WriteAttributeString("areanameparent", RemoveInvalidXmlChars(room.AreaNameParent));
        writer.WriteAttributeString("areanamechild", RemoveInvalidXmlChars(room.AreaNameChild));
        writer.WriteAttributeString("start_time", DateTimeOffset.Now.ToString("O"));
        writer.WriteEndElement();

        // see BililiveRecorder.ToolBox\Tool\DanmakuMerger\DanmakuMergerHandler.cs
        const string style =
            """<z:stylesheet version="1.0" id="s" xml:id="s" xmlns:z="http://www.w3.org/1999/XSL/Transform"><z:output method="html"/><z:template match="/"><html><meta name="viewport" content="width=device-width"/><title>mikufans录播姬弹幕文件 - <z:value-of select="/i/BililiveRecorderRecordInfo/@name"/></title><style>body{margin:0}h1,h2,p,table{margin-left:5px}table{border-spacing:0}td,th{border:1px solid grey;padding:1px}th{position:sticky;top:0;background:#4098de}tr:hover{background:#d9f4ff}div{overflow:auto;max-height:80vh;max-width:100vw;width:fit-content}</style><h1><a href="https://rec.danmuji.org">mikufans录播姬</a>弹幕XML文件</h1><p>本文件不支持在 IE 浏览器里预览，请使用 Chrome Firefox Edge 等浏览器。</p><p>文件用法参考文档 <a href="https://rec.danmuji.org/user/danmaku/">https://rec.danmuji.org/user/danmaku/</a></p><table><tr><td>录播姬版本</td><td><z:value-of select="/i/BililiveRecorder/@version"/></td></tr><tr><td>房间号</td><td><z:value-of select="/i/BililiveRecorderRecordInfo/@roomid"/></td></tr><tr><td>主播名</td><td><z:value-of select="/i/BililiveRecorderRecordInfo/@name"/></td></tr><tr><td>录制开始时间</td><td><z:value-of select="/i/BililiveRecorderRecordInfo/@start_time"/></td></tr><tr><td><a href="#d">弹幕</a></td><td>共<z:value-of select="count(/i/d)"/>条记录</td></tr><tr><td><a href="#guard">上船</a></td><td>共<z:value-of select="count(/i/guard)"/>条记录</td></tr><tr><td><a href="#sc">SC</a></td><td>共<z:value-of select="count(/i/sc)"/>条记录</td></tr><tr><td><a href="#gift">礼物</a></td><td>共<z:value-of select="count(/i/gift)"/>条记录</td></tr></table><h2 id="d">弹幕</h2><div id="dm"><table><tr><th>用户名</th><th>出现时间</th><th>用户ID</th><th>弹幕</th><th>参数</th></tr><z:for-each select="/i/d"><tr><td><z:value-of select="@user"/></td><td></td><td></td><td><z:value-of select="."/></td><td><z:value-of select="@p"/></td></tr></z:for-each></table></div><script>Array.from(document.querySelectorAll('#dm tr')).slice(1).map(t=>t.querySelectorAll('td')).forEach(t=>{let p=t[4].textContent.split(','),a=p[0];t[1].textContent=`${(Math.floor(a/60/60)+'').padStart(2,0)}:${(Math.floor(a/60%60)+'').padStart(2,0)}:${(a%60).toFixed(3).padStart(6,0)}`;t[2].innerHTML=`&lt;a target=_blank rel="nofollow noreferrer" href="https://space.bilibili.com/${p[6]}"&gt;${p[6]}&lt;/a&gt;`})</script><h2 id="guard">舰长购买</h2><div><table><tr><th>用户名</th><th>用户ID</th><th>舰长等级</th><th>购买数量</th><th>出现时间</th></tr><z:for-each select="/i/guard"><tr><td><z:value-of select="@user"/></td><td><a rel="nofollow noreferrer"><z:attribute name="href"><z:text>https://space.bilibili.com/</z:text><z:value-of select="@uid" /></z:attribute><z:value-of select="@uid"/></a></td><td><z:value-of select="@level"/></td><td><z:value-of select="@count"/></td><td><z:value-of select="@ts"/></td></tr></z:for-each></table></div><h2 id="sc">SuperChat 醒目留言</h2><div><table><tr><th>用户名</th><th>用户ID</th><th>内容</th><th>显示时长</th><th>价格</th><th>出现时间</th></tr><z:for-each select="/i/sc"><tr><td><z:value-of select="@user"/></td><td><a rel="nofollow noreferrer"><z:attribute name="href"><z:text>https://space.bilibili.com/</z:text><z:value-of select="@uid" /></z:attribute><z:value-of select="@uid"/></a></td><td><z:value-of select="."/></td><td><z:value-of select="@time"/></td><td><z:value-of select="@price"/></td><td><z:value-of select="@ts"/></td></tr></z:for-each></table></div><h2 id="gift">礼物</h2><div><table><tr><th>用户名</th><th>用户ID</th><th>礼物名</th><th>礼物数量</th><th>出现时间</th></tr><z:for-each select="/i/gift"><tr><td><z:value-of select="@user"/></td><td><a rel="nofollow noreferrer"><z:attribute name="href"><z:text>https://space.bilibili.com/</z:text><z:value-of select="@uid" /></z:attribute><z:value-of select="@uid"/></a></td><td><z:value-of select="@giftname"/></td><td><z:value-of select="@giftcount"/></td><td><z:value-of select="@ts"/></td></tr></z:for-each></table></div></html></z:template></z:stylesheet>""";

        writer.WriteStartElement("BililiveRecorderXmlStyle");
        writer.WriteRaw(style);
        writer.WriteEndElement();
        writer.Flush();
    }

    private bool _disposedValue;

    protected void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                _semaphoreSlim.Dispose();
                _xmlWriter?.Close();
                _xmlWriter?.Dispose();
                _xmlWriter = null;
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

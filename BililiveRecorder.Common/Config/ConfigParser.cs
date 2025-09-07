#nullable enable
using System.Text;
using BililiveRecorder.Common.Config.V1;
using BililiveRecorder.Common.Config.V2;
using BililiveRecorder.Common.Config.V3;
using Newtonsoft.Json;
using Serilog;

namespace BililiveRecorder.Common.Config
{
    public class ConfigParser
    {
        public const string CONFIG_FILE_NAME = "config.json";
        private static readonly ILogger logger = Log.ForContext<ConfigParser>();
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static ConfigV3? LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                logger.Warning("目标文件夹不存在");
                return null;
            }

            var path = Path.Combine(directory, CONFIG_FILE_NAME);

            return LoadFromFile(path);
        }

        public static ConfigV3? LoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    logger.Information("初始化默认设置，因为配置文件不存在 {Path}", path);
                    return new ConfigV3();
                }

                logger.Debug("Loading config from {Path}", path);
                var json = File.ReadAllText(path, Encoding.UTF8);
                return LoadJson(json);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "从文件加载设置时出错");
                return null;
            }
        }

        public static ConfigV3? LoadJson(string json)
        {
            try
            {
                logger.Debug("Config json: {Json}", json);

                var configBase = JsonConvert.DeserializeObject<ConfigBase>(json);
                switch (configBase)
                {
                    case ConfigV1Wrapper v1:
                        {
                            logger.Debug("读取到 config v1");
#pragma warning disable CS0612
                            var v1Data = JsonConvert.DeserializeObject<ConfigV1>(v1.Data ?? string.Empty);
#pragma warning restore CS0612
                            if (v1Data is null)
                                return new ConfigV3();

                            var newConfig = ConfigMapper.Map2To3(ConfigMapper.Map1To2(v1Data));
                            return newConfig;
                        }

#pragma warning disable CS0618 // Type or member is obsolete
                    case ConfigV2 v2:
#pragma warning restore CS0618 // Type or member is obsolete
                        logger.Debug("读取到 config v2");
                        return ConfigMapper.Map2To3(v2);

                    case ConfigV3 v3:
                        logger.Debug("读取到 config v3");
                        return v3;

                    default:
                        logger.Error("读取到不支持的设置版本");
                        return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "解析设置时出错");
                return null;
            }
        }

        public static bool Save(ConfigV3 config)
        {
            var directory = config.Global.WorkDirectory;

            if (config.DisableConfigSave)
            {
                logger.Debug("Skipping write config because DisableConfigSave is true.");
                return true;
            }

            var json = SaveJson(config);
            try
            {
                if (!Directory.Exists(directory))
                    return false;

                var filepath = Path.Combine(directory, CONFIG_FILE_NAME);

                if (config.ConfigPathOverride is not null)
                    filepath = config.ConfigPathOverride;

                if (json is not null)
                    WriteAllTextWithBackup(filepath, json);

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "保存设置时出错（写入文件）");
                return false;
            }
        }

        public static string? SaveJson(ConfigV3 config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.None, settings);
                return json;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "保存设置时出错（序列化）");
                return null;
            }
        }

        // https://stackoverflow.com/q/25366534 with modification
        public static void WriteAllTextWithBackup(string path, string contents)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, contents);
                return;
            }

            var ext = Path.GetExtension(path);

            var tempPath = Path.Combine(Path.GetDirectoryName(path)!, Path.ChangeExtension(path, RandomString(6) + ext));
            var backupPath = Path.ChangeExtension(path, "backup" + ext);

            // delete any existing backups
            if (File.Exists(backupPath))
                File.Delete(backupPath);

            // get the bytes
            var data = Encoding.UTF8.GetBytes(contents);

            // write the data to a temp file
            using (var tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
                tempFile.Write(data, 0, data.Length);

            // replace the contents
            File.Replace(tempPath, path, backupPath);
        }
        private static readonly Random random = new Random();
        private static string RandomString(int length) => new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

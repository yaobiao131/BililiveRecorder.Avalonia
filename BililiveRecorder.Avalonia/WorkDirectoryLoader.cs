using System.Reflection;
using BililiveRecorder.Common.Config;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace BililiveRecorder.Avalonia;

public class WorkDirectoryLoader
{
    private static readonly ILogger logger = Log.ForContext<WorkDirectoryLoader>();

    private const string fileName = "path.json";
    private readonly string? basePath;
    private readonly string filePath;

    public WorkDirectoryLoader()
    {
        var exePath = Assembly.GetEntryAssembly()?.Location;
        basePath = string.IsNullOrWhiteSpace(exePath) ? Environment.CurrentDirectory : Path.GetDirectoryName(exePath);

        // var input = basePath;
        // if (input != null && Regex.IsMatch(input, @"^.*\\app-\d+\.\d+\.\d+\\?$") && File.Exists(Path.Combine(input, "..", "Update.exe")))
        //     basePath = Path.Combine(basePath, "..");

        basePath = Path.GetFullPath(basePath);
        filePath = Path.Combine(basePath, fileName);
    }

    public WorkDirectoryData Read()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                logger.Debug("Path file {FilePath} does not exist", filePath);
                return new WorkDirectoryData();
            }
            else
            {
                logger.Debug("Reading path file from {FilePath}.", filePath);
                var str = File.ReadAllText(filePath);
                logger.Debug("Path file content: {Content}", str);
                var obj = JsonConvert.DeserializeObject<WorkDirectoryData>(str);
                return obj ?? new WorkDirectoryData();
            }
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error reading path file");
            return new WorkDirectoryData();
        }
    }

    public void Write(WorkDirectoryData data)
    {
        try
        {
            logger.Debug("Writing path file at {FilePath}", filePath);
            var str = JsonConvert.SerializeObject(data);
            ConfigParser.WriteAllTextWithBackup(filePath, str);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error writing path file at {FilePath}", filePath);
        }
    }

    public class WorkDirectoryData
    {
        public string Path { get; set; } = string.Empty;
        public bool SkipAsking { get; set; }
    }
}

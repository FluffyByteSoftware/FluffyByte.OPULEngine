using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Tools.Storage;

namespace FluffyByte.OPULEngine.Tools;

public class Constellations
{
    private static readonly Lazy<Constellations> _instance = new(() => new());
    public static Constellations Instance => _instance.Value;

    private static string StorageDirectory => $@"Storage/";
    private static string LogFilePath => @$"Logs/Constellations.log";
    private static bool DebugMode = true;

    private static string SettingsDirectory => $@"Config/";
    private static string SettingsFileName => "Constellations.config";

    private ConsoleColor ScribeRegFgColor = ConsoleColor.Green;
    private ConsoleColor ScribeWarnFgColor = ConsoleColor.Yellow;
    private ConsoleColor ScribeDebugFgColor = ConsoleColor.Cyan;
    private ConsoleColor ScribeErrorFgColor = ConsoleColor.Red;

    public string ServerName { get; private set; } = "OPULServer";
    public string HostAddress { get; private set; } = "10.0.0.84";
    public int HostPort { get; private set; } = 9997;

    private static TimeSpan CommTickRate = TimeSpan.FromMilliseconds(100);
    private static TimeSpan GameWorldTickRate = TimeSpan.FromMilliseconds(50);

    public static string TimestampUtc => DateTime.UtcNow.ToString("yy.MM.dd.HH.mm.ss.f");

    public async Task LoadSettings()
    {
        if (!Directory.Exists(StorageDirectory))
            Directory.CreateDirectory(StorageDirectory);

        if (!Directory.Exists(SettingsDirectory))
            Directory.CreateDirectory(SettingsDirectory);

        string settingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);

        if (!File.Exists(settingsFilePath))
        {
            await CreateDefaultSettingsFile(settingsFilePath);
            return;
        }

        // ---- Load existing settings ----
        IFluffyTextFile settings = await FluffyFileWizard.LoadTextFileAsync(settingsFilePath);
        string[] lines = [.. settings.Lines.ToArray()];

        foreach (string line in lines)
        {
            FileTextParser.ParseLines
            (
                [line],
                new Dictionary<string, Action<string>>
                {
                    { "ScribeRegFgColor", value => { if(Enum.TryParse(value, out ConsoleColor color))
                            ScribeRegFgColor = color; } },
                    { "ScribeWarnFgColor", value => { if(Enum.TryParse(value, out ConsoleColor color))
                            ScribeWarnFgColor = color; } },
                    { "ScribeDebugFgColor", value => { if(Enum.TryParse(value, out ConsoleColor color))
                            ScribeDebugFgColor = color; } },
                    { "ScribeErrorFgColor", value => { if(Enum.TryParse(value, out ConsoleColor color))
                            ScribeErrorFgColor = color; } },
                    { "ServerName", value => { if(!string.IsNullOrWhiteSpace(value))
                            ServerName = value; } },
                    { "DebugMode", value => { if(bool.TryParse(value, out bool debugMode))
                            DebugMode = debugMode; } },
                    {"HostAddress", value => { if(!string.IsNullOrWhiteSpace(value))
                            HostAddress = value; } },
                    { "HostPort", value => { if(int.TryParse(value, out int port) && port > 0)
                            HostPort = port; } },
                    { "CommTickRateMs", value => { if(int.TryParse(value, out int ms) && ms > 0)
                            CommTickRate = TimeSpan.FromMilliseconds(ms); } },
                    { "GameWorldTickRateMs", value => { if(int.TryParse(value, out int ms) && ms > 0)
                            GameWorldTickRate = TimeSpan.FromMilliseconds(ms); } }
                }
            );
        }

        if(DebugMode && !Scribe.DebugModeEnabled)
            Scribe.ToggleDebugMode();
        
        else if(!DebugMode && Scribe.DebugModeEnabled)
            Scribe.ToggleDebugMode();

    }

    public async Task SaveSettings()
    {
        try
        {
            if (!Directory.Exists(StorageDirectory))
                Directory.CreateDirectory(StorageDirectory);

            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            string settingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);
            FluffyTextFile settingsFile = new(settingsFilePath);

            List<string> lines =
            [ 
                $"ServerName={ServerName}",
                $"DebugMode={DebugMode}",
                $"ScribeRegFgColor={ScribeRegFgColor}",
                $"ScribeWarnFgColor={ScribeWarnFgColor}",
                $"ScribeDebugFgColor={ScribeDebugFgColor}",
                $"ScribeErrorFgColor={ScribeErrorFgColor}",
                $"HostAddress={HostAddress}",
                $"HostPort={HostPort}",
                $"CommTickRateMs={CommTickRate.TotalMilliseconds}",
                $"GameWorldTickRateMs={GameWorldTickRate.TotalMilliseconds}"
            ];

            settingsFile.ReplaceLines(lines);

            await FluffyFileWizard.SaveTextFileAsync(settingsFile);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack TraceK {ex.StackTrace}");
        }
    }

    private bool _creatingDefaults;

    private async Task CreateDefaultSettingsFile(string settingsFilePath)
    {
        if (_creatingDefaults) return; // guard
        _creatingDefaults = true;
        try
        {
            FluffyTextFile settingsFile = new(settingsFilePath);
            settingsFile.ReplaceLines(
            [ 
                $"ServerName={ServerName}",
                $"DebugMode={DebugMode}",
                $"ScribeRegFgColor={ScribeRegFgColor}",
                $"ScribeWarnFgColor={ScribeWarnFgColor}",
                $"ScribeDebugFgColor={ScribeDebugFgColor}",
                $"ScribeErrorFgColor={ScribeErrorFgColor}",
                $"HostAddress={HostAddress}",
                $"HostPort={HostPort}",
                $"CommTickRateMs={CommTickRate.TotalMilliseconds}",
                $"GameWorldTickRateMs={GameWorldTickRate.TotalMilliseconds}"
            ]);

            await FluffyFileWizard.SaveTextFileAsync(settingsFile);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack TraceK {ex.StackTrace}");
        }
        finally
        {
            _creatingDefaults = false;
        }
    }
}

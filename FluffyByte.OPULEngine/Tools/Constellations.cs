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

    public static string StorageDirectory => $@"Storage/";
    public static string LogFilePath => @$"Logs/Constellations.log";
    public bool DebugMode { get; set; } = true;

    public static string SettingsDirectory => $@"Config/";
    public static string SettingsFileName => "Constellations.config";

    public ConsoleColor ScribeRegFgColor { get; set; } = ConsoleColor.Green;
    public ConsoleColor ScribeWarnFgColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ScribeDebugFgColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor ScribeErrorFgColor { get; set; } = ConsoleColor.Red;
    public string ServerName { get; private set; } = "OPULServer";

    public string HostAddress = "10.0.0.84";
    public int HostPort = 9997;

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
                            DebugMode = debugMode; } }
                }
            );
        }
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
                $"ScribeErrorFgColor={ScribeErrorFgColor}" 
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
                $"ScribeErrorFgColor={ScribeErrorFgColor}" 
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

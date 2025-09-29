using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Tools.Storage;

namespace FluffyByte.OPULEngine.Tools;

public class Constellations
{
    private static readonly Lazy<Constellations> _instance = new(() => new());
    public static Constellations Instance => _instance.Value;

    private Constellations()
    {
        // Private constructor to prevent instantiation
    }

    public static string StorageDirectory => $@"Storage/";
    public static string LogFilePath => @$"Logs/Constellations.log";
    public bool DebugMode { get; set; } = true;

    public static string SettingsDirectory => $@"Config/";
    public static string SettingsFileName => "Constellations.config";

    public ConsoleColor ScribeRegFgColor { get; set; } = ConsoleColor.Green;
    public ConsoleColor ScribeWarnFgColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ScribeDebugFgColor { get; set; } = ConsoleColor.White;

    public ConsoleColor ScribeErrorFgColor { get; set; } = ConsoleColor.Red;

    public static string TimestampUtc => DateTime.UtcNow.ToString("yy.MM.dd.HH.mm.ss.f");

    public async Task LoadSettings()
    {
        if (!Directory.Exists(StorageDirectory))
            Directory.CreateDirectory(StorageDirectory);

        if (!Directory.Exists(SettingsDirectory))
            Directory.CreateDirectory(SettingsDirectory);

        Console.WriteLine("Pre-cacheing settings...");

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
            if (line.StartsWith("ScribeRegFgColor"))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && Enum.TryParse(parts[1], out ConsoleColor color))
                    ScribeRegFgColor = color;
            }
            else if (line.StartsWith("ScribeWarnFgColor"))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && Enum.TryParse(parts[1], out ConsoleColor color))
                    ScribeWarnFgColor = color;
            }
            else if (line.StartsWith("ScribeDebugFgColor"))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && Enum.TryParse(parts[1], out ConsoleColor color))
                    ScribeDebugFgColor = color;
            }
            else if (line.StartsWith("ScribeErrorFgColor"))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && Enum.TryParse(parts[1], out ConsoleColor color))
                    ScribeErrorFgColor = color;
            }
            else if (line.StartsWith("DebugMode"))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && bool.TryParse(parts[1], out bool debugMode))
                    DebugMode = debugMode;
            }
        }

        Constellations.Instance.ScribeRegFgColor = ScribeRegFgColor;
        Constellations.Instance.ScribeWarnFgColor = ScribeWarnFgColor;
        Constellations.Instance.ScribeDebugFgColor = ScribeDebugFgColor;
        Constellations.Instance.ScribeErrorFgColor = ScribeErrorFgColor;

        Console.WriteLine($"Debug Mode is: {DebugMode}");
        Console.WriteLine("Settings loaded.");
    }

    public async Task SaveSettings()
    {
        try
        {
            if (!Directory.Exists(StorageDirectory))
                Directory.CreateDirectory(StorageDirectory);

            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            Console.WriteLine("Saving settings...");
            string settingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);
            FluffyTextFile settingsFile = new(settingsFilePath);

            List<string> lines =
            [ $"DebugMode={DebugMode}",
            $"ScribeRegFgColor={ScribeRegFgColor}",
            $"ScribeWarnFgColor={ScribeWarnFgColor}",
            $"ScribeDebugFgColor={ScribeDebugFgColor}",
            $"ScribeErrorFgColor={ScribeErrorFgColor}" ];

            settingsFile.ReplaceLines(lines);

            Console.WriteLine("Settings File: ");
            Console.WriteLine(settingsFile.Lines);

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
            [ $"DebugMode={DebugMode}",
            $"ScribeRegFgColor={ScribeRegFgColor}",
            $"ScribeWarnFgColor={ScribeWarnFgColor}",
            $"ScribeDebugFgColor={ScribeDebugFgColor}",
            $"ScribeErrorFgColor={ScribeErrorFgColor}" ]);

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

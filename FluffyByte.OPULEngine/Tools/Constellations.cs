using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static string ConfigFilePath => $@"Config/OPUL.cfg";

    public static string TempDirectory => $@"{StorageDirectory}Temp/";

    public ConsoleColor ScribeRegFgColor { get; set; } = ConsoleColor.Green;
    public ConsoleColor ScribeWarnFgColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ScribeDebugFgColor { get; set; } = ConsoleColor.White;

    public ConsoleColor ScribeErrorFgColor { get; set; } = ConsoleColor.Red;

    public static string TimestampUtc => DateTime.UtcNow.ToString("yy.MM.dd.HH.mm.ss.f");

    public void LoadSettings()
    {
        Scribe.Debug($"Loading settings from {ConfigFilePath}...");

        // Check and build default folders.

        if (!Directory.Exists(StorageDirectory))
        {
            Directory.CreateDirectory(StorageDirectory);
        }

        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
        }

        if (!Directory.Exists("Logs"))
        {
            Directory.CreateDirectory("Logs");
        }

        if (!File.Exists(ConfigFilePath))
        {
            // Create a new Config File
            File.CreateText(ConfigFilePath).Close();

            // Write default settings to the config file
            var defaultSettings = new List<string>
            {
                $"DebugMode={DebugMode}",
                $"ScribeRegFgColor={ScribeRegFgColor}",
                $"ScribeWarnFgColor={ScribeWarnFgColor}",
                $"ScribeDebugFgColor={ScribeDebugFgColor}",
                $"ScribeErrorFgColor={ScribeErrorFgColor}"
            };

            File.WriteAllLines(ConfigFilePath, defaultSettings);
        }

        Scribe.Info("Loading settings from config file...");

        var lines = File.ReadAllLines(ConfigFilePath);

        foreach (var line in lines)
        {
            string[] parts = line.Split('=');
            
            if (parts.Length != 2) continue;
            
            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch (key)
            {
                case "DebugMode":
                    if (bool.TryParse(value, out var debugMode))
                    {
                        
                        DebugMode = debugMode;

                        if (DebugMode)
                        {
                            Scribe.Info("Debug mode enabled.");
                        }
                        else
                            Scribe.Info("Debug mode disabled.");
                    }
                    break;
                case "ScribeRegFgColor":
                    if (Enum.TryParse<ConsoleColor>(value, out var regColor))
                    {
                        ScribeRegFgColor = regColor;
                        Scribe.Info($"ScribeRegFgColor set to {regColor}");
                    }
                    break;
                case "ScribeWarnFgColor":
                    if (Enum.TryParse<ConsoleColor>(value, out var warnColor))
                    {
                        ScribeWarnFgColor = warnColor;
                        Scribe.Info($"ScribeWarnFgColor set to {warnColor}");
                    }
                    break;
                case "ScribeDebugFgColor":
                    if (Enum.TryParse<ConsoleColor>(value, out var debugColor))
                    {
                        ScribeDebugFgColor = debugColor;
                        Scribe.Info($"ScribeDebugFgColor set to {debugColor}");
                    }
                    break;
                case "ScribeErrorFgColor":
                    if (Enum.TryParse<ConsoleColor>(value, out var errorColor))
                    {
                        ScribeErrorFgColor = errorColor;
                        Scribe.Info($"ScribeErrorFgColor set to {errorColor}");
                    }
                    break;
            }
        }
    }
}

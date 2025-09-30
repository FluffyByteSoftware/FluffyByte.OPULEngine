using System.Text;

namespace FluffyByte.OPULEngine.Tools;

public static class Scribe
{


    public const ConsoleColor DefaultColor = ConsoleColor.White;
    public const ConsoleColor RegularColor = ConsoleColor.Green;
    public const ConsoleColor WarningColor = ConsoleColor.Yellow;
    public const ConsoleColor DebugColor = ConsoleColor.Cyan;
    public const ConsoleColor ErrorColor = ConsoleColor.Red;

    public static bool DebugModeEnabled { get; private set; } = false;

    public static void Info(string message) 
        => WriteMessage(message, LogLevel.Info);

    public static void Warn(string warning)
        => WriteMessage(warning, LogLevel.Warn);

    public static void Debug(string debugMessage)
    {
        if(DebugModeEnabled)
            WriteMessage(debugMessage, LogLevel.Debug);
    }

    public static void Error(Exception ex)
    {
        StringBuilder output = new();
        output.AppendLine($"Exception Encountered: {ex.Message}");
        output.AppendLine($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            output.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            output.AppendLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
        }

        WriteMessage(output.ToString(), LogLevel.Error);
    }

    public static void ToggleDebugMode()
        => DebugModeEnabled = !DebugModeEnabled;

    public static void Error(string errorMessage, Exception ex)
    {
        
        WriteMessage(errorMessage, LogLevel.Error);
        Error(ex);
    }

    private static void WriteMessage(string message, LogLevel severity)
    {
        StringBuilder output = new();
        ConsoleColor fgColor;

        output.Append($"[{Constellations.TimestampUtc}::");
        switch (severity)
        {
            case LogLevel.Info:
                output.Append("INFO]");
                fgColor = RegularColor;
                break;
            case LogLevel.Warn:
                output.Append("WARN]");
                fgColor = WarningColor;
                break;
            case LogLevel.Error:
                output.Append("ERROR]");
                fgColor = ErrorColor;
                break;
            case LogLevel.Debug:
                output.Append("DEBUG]");
                fgColor = DebugColor;
                break;
            default:
                output.Append("UNKNOWN]");
                fgColor = DefaultColor;
                break;
        }
         
        output.AppendLine($" {message}");

        Console.ForegroundColor = fgColor;
        // Write to console
        Console.Write(output.ToString());
        Console.ResetColor();
    }



}

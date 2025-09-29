using System.Text;

namespace FluffyByte.OPULEngine.Tools;

public class Scribe
{
    private static readonly Lazy<Scribe> _instance = new(() => new());
    public static Scribe Instance => _instance.Value;


    public ConsoleColor DefaultColor = ConsoleColor.White;
    public ConsoleColor RegularColor = ConsoleColor.Green;
    public ConsoleColor WarningColor = ConsoleColor.Yellow;
    public ConsoleColor DebugColor = ConsoleColor.Cyan;
    public ConsoleColor ErrorColor = ConsoleColor.Red;

    public void Info(string message) 
        => WriteMessage(message, LogLevel.Info);

    public void Warn(string warning)
        => WriteMessage(warning, LogLevel.Warn);

    public void Debug(string debugMessage)
    {
        if(Constellations.Instance.DebugMode)
            WriteMessage(debugMessage, LogLevel.Debug);
    }

    public void Error(Exception ex)
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

    public void Error(string errorMessage, Exception ex)
    {
        
        WriteMessage(errorMessage, LogLevel.Error);
        Error(ex);
    }

    private void WriteMessage(string message, LogLevel severity)
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
        Console.WriteLine(output.ToString());
        Console.ResetColor();
    }



}

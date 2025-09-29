using System.Text;

namespace FluffyByte.OPULEngine.Tools;

public static class Scribe
{
    private static readonly ConsoleColor _defaultColor;
    private static readonly ConsoleColor _regColor;
    private static readonly ConsoleColor _warnColor;
    private static readonly ConsoleColor _debugColor;
    private static readonly ConsoleColor _errorColor;

    static Scribe()
    {
        _regColor = Constellations.Instance.ScribeRegFgColor;
        _warnColor = Constellations.Instance.ScribeWarnFgColor;
        _debugColor = Constellations.Instance.ScribeDebugFgColor;
        _errorColor = Constellations.Instance.ScribeErrorFgColor;
        _defaultColor = Console.ForegroundColor;
    }

    public static void Info(string message) 
        => WriteMessage(message, LogLevel.Info);

    public static void Warn(string warning)
        => WriteMessage(warning, LogLevel.Warn);

    public static void Debug(string debugMessage)
    {
        if(Constellations.Instance.DebugMode)
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
                fgColor = _regColor;
                break;
            case LogLevel.Warn:
                output.Append("WARN]");
                fgColor = _warnColor;
                break;
            case LogLevel.Error:
                output.Append("ERROR]");
                fgColor = _errorColor;
                break;
            case LogLevel.Debug:
                output.Append("DEBUG]");
                fgColor = _debugColor;
                break;
            default:
                output.Append("UNKNOWN]");
                fgColor = _defaultColor;
                break;
        }
         
        output.AppendLine($" {message}");

        Console.ForegroundColor = fgColor;
        // Write to console
        Console.WriteLine(output.ToString());
        Console.ResetColor();
    }



}

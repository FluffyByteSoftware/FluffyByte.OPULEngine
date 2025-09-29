using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public class FluffyFileWizard
{
    private static readonly Lazy<FluffyFileWizard> _instance = new(() => new());
    public static FluffyFileWizard Instance => _instance.Value;
    private FluffyFileWizard()
    {
        // Private constructor to prevent instantiation
    }

    public static async Task<IFluffyTextFile> LoadTextFileAsync(string path, 
        FluffyTextFile.StorageKind kind = FluffyTextFile.StorageKind.List,
        Encoding? defaultEncoding = null,
        CancellationToken cancellationToken = default)
    {
        FluffyTextFile file = new(path, kind, defaultEncoding);

        if(!File.Exists(path))
        {
            // Non-existent -> empty, clean
            (file as FluffyTextFile)!.SetFromWizard([], file.Encoding, markClean: true);
            return file;
        }

        List<string> lines = [];
        Encoding detected;

        using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 
            4096, useAsync: true);

        using StreamReader sr = new(fs, defaultEncoding ?? Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: true);
        {
            string? line;
         
            while((line = await sr.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lines.Add(line);
            }
            detected = sr.CurrentEncoding;
        }

        (file as FluffyTextFile)!.SetFromWizard(lines, detected, markClean: true);
        return file;
    }

    public static async Task SaveTextFileAsync(IFluffyTextFile file, 
        CancellationToken cancellationToken = default)
    {
        if(file is null)
        {
            Exception nullEx = new ArgumentNullException(nameof(file),
                 "File is null in SaveTextFileAsync");

            throw nullEx;
        }

        string directory = file.FileInfo.DirectoryName ?? "";
        
        if(directory.Length > 0 && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = file.FileInfo.FullName + ".tmp";

        var snapshot = file.Lines;
        var encoding = file.Encoding;

        await using FileStream fs = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None,
            4096, useAsync: true);
        await using StreamWriter writer = new(fs, encoding);

        for(int i = 0; i < snapshot.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await writer.WriteAsync(snapshot[i]);
            if(i < snapshot.Count - 1)
                await writer.WriteAsync(Environment.NewLine);
        }

        if (File.Exists(file.FileInfo.FullName))
            File.Replace(tempPath, file.FileInfo.FullName, null);
        else
            File.Move(tempPath, file.FileInfo.FullName);

        if (file is FluffyTextFile concrete)
            concrete.MarkCleanAfterSave();

    }

}

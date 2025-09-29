using System;
using System.Collections.Concurrent;
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

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = [];

    // -------------------- TEXT --------------------

    public static async Task<IFluffyTextFile> LoadTextFileAsync(string path, 
        FluffyTextFile.StorageKind kind = FluffyTextFile.StorageKind.List,
        Encoding? defaultEncoding = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            FluffyTextFile file = new(path, kind, defaultEncoding);

            if (!File.Exists(path))
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

                while ((line = await sr.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lines.Add(line);
                }
                detected = sr.CurrentEncoding;
            }
            
            (file as FluffyTextFile)!.SetFromWizard(lines, detected, markClean: true);

            return file;
        }
        catch(Exception ex)
        {

            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack TraceK {ex.StackTrace}");
            throw;
        }
    }

    public static async Task SaveTextFileAsync(IFluffyTextFile file,
    CancellationToken cancellationToken = default)
    {
        var fileLock = _fileLocks.GetOrAdd(file.FileInfo.FullName, _ => new SemaphoreSlim(1, 1));
        await fileLock.WaitAsync(cancellationToken);

        try
        {
            string directory = file.FileInfo.DirectoryName ?? "";
            if (directory.Length > 0 && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string tempPath = file.FileInfo.FullName + ".tmp";

            var snapshot = file.Lines;
            var encoding = file.Encoding;

            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await using (var writer = new StreamWriter(fs, encoding))
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteAsync(snapshot[i]);
                    if (i < snapshot.Count - 1)
                        await writer.WriteAsync(Environment.NewLine);
                }
                await writer.FlushAsync(cancellationToken);
            }

            // Small retry loop for transient locks
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (File.Exists(file.FileInfo.FullName))
                        File.Replace(tempPath, file.FileInfo.FullName, null);
                    else
                        File.Move(tempPath, file.FileInfo.FullName);
                    break;
                }
                catch (IOException) when (i < 2)
                {
                    await Task.Delay(50, cancellationToken);
                }
            }

            if (file is FluffyTextFile concrete)
                concrete.MarkCleanAfterSave();
        }
        finally
        {
            fileLock.Release();
        }
    }



    // -------------------- BINARY --------------------

    public static async Task<IFluffyBinaryFile> LoadBinaryFileAsync(
        string path, int bufferSize = 64 * 1024, CancellationToken cancellationToken = default)
    {
        try
        {
            var file = new FluffyBinaryFile(path);

            if (!File.Exists(path))
            {
                file.SetFromWizard(ReadOnlyMemory<byte>.Empty, markClean: true);
                return file;
            }

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, useAsync: true);
            if (fs.Length == 0)
            {
                file.SetFromWizard(ReadOnlyMemory<byte>.Empty, markClean: true);
                return file;
            }

            // why: chunked read supports cancellation and large files without double-alloc
            var rented = new byte[Math.Min(bufferSize, 1024 * 1024)];
            using var ms = fs.Length <= int.MaxValue ? new MemoryStream((int)fs.Length) : new MemoryStream();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int read = await fs.ReadAsync(rented.AsMemory(0, rented.Length), cancellationToken).ConfigureAwait(false);

                if (read == 0) break;
                ms.Write(rented, 0, read);
            }

            file.SetFromWizard(ms.ToArray(), markClean: true);

            return file;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack TraceK {ex.StackTrace}");
            throw;
        }
    }

    public static async Task SaveBinaryFileAsync(
        IFluffyBinaryFile file,
        CancellationToken cancellationToken = default)
    {
        var _fileLock = _fileLocks.GetOrAdd(file.FileInfo.FullName, _ => new SemaphoreSlim(1, 1));

        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file), "File is null in SaveBinaryFileAsync");

            var directory = file.FileInfo.DirectoryName ?? "";
            if (directory.Length > 0 && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = file.FileInfo.FullName + ".tmp";

            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            {
                var data = file.Data;
                await fs.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(file.FileInfo.FullName))
                File.Replace(tempPath, file.FileInfo.FullName, null);
            else
                File.Move(tempPath, file.FileInfo.FullName);

            if (file is FluffyBinaryFile concreteBin)
                concreteBin.MarkCleanAfterSave();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack TraceK {ex.StackTrace}");
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }
}

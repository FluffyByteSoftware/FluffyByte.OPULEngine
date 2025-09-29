using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public interface IFluffyTextFile
{
    FileInfo FileInfo { get; }
    Encoding Encoding { get; }
    bool IsDirty { get; }

    int LineCount { get; }
    IReadOnlyList<string> Lines { get; } // Read-only snapshot
    IEnumerable<string> ReadLines();
    string ReadAllText();

    void ReplaceLines(IEnumerable<string> newLines);
    void AppendLines(IEnumerable<string> lines);
    void Clear();
}

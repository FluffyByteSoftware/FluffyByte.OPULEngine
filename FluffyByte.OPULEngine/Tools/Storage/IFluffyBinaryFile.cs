using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public interface IFluffyBinaryFile
{
    FileInfo FileInfo { get; }
    bool IsDirty { get; }
    int Length { get; }

    ReadOnlyMemory<byte> Data { get; }

    void ReplaceBytes(ReadOnlyMemory<byte> bytes);
    void AppendBytes(ReadOnlyMemory<byte> bytes);
    void Clear();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public sealed class FluffyBinaryFile(string path) : IFluffyBinaryFile
{
    private readonly Lock _sync = new();
    private byte[] _buffer = [];
    private bool _dirty;

    public FileInfo FileInfo { get; private set; } = new(path);
    public bool IsDirty { get { lock (_sync) { return _dirty; } } }

    public int Length { get { lock (_sync) { return _buffer.Length; } } }

    public ReadOnlyMemory<byte> Data { get { lock (_sync) { return _buffer; } } }


    public void ReplaceBytes(ReadOnlyMemory<byte> bytes)
    {
        lock (_sync)
        {
            _buffer = bytes.ToArray();       // why: own the memory
            _dirty = true;
        }
    }

    public void AppendBytes(ReadOnlyMemory<byte> bytes)
    {
        lock (_sync)
        {
            if (bytes.Length == 0) return;
            var old = _buffer;
            var res = new byte[old.Length + bytes.Length];
            Buffer.BlockCopy(old, 0, res, 0, old.Length);
            bytes.Span.CopyTo(res.AsSpan(old.Length));
            _buffer = res;
            _dirty = true;
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _buffer = [];
            _dirty = true;
        }
    }

    // Wizard hooks
    internal void SetFromWizard(ReadOnlyMemory<byte> bytes, bool markClean)
    {
        lock (_sync)
        {
            _buffer = bytes.ToArray();
            _dirty = !markClean;
        }
    }

    internal void MarkCleanAfterSave()
    {
        lock (_sync) _dirty = false;
        FileInfo.Refresh();
    }
}
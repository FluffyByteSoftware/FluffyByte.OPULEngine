using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FluffyByte.OPULEngine.Tools.Storage;
public sealed class FluffyTextFile : IFluffyTextFile
{
    public enum StorageKind { Array, List }

    private readonly Lock _sync = new();                // why: plain monitor is portable
    private readonly StorageKind _kind;

    private string[]? _array;
    private List<string>? _list;
    private IReadOnlyList<string> _roView = [];
    private bool _dirty;

    public FileInfo FileInfo { get; }
    public Encoding Encoding { get; private set; }

    public bool IsDirty { get { lock (_sync) return _dirty; } }

    public int LineCount
    {
        get { lock (_sync) return _kind == StorageKind.Array ? _array!.Length : _list!.Count; }
    }

    public IReadOnlyList<string> Lines
    {
        get { lock (_sync) return _roView; }
    }

    public FluffyTextFile(string path, StorageKind kind = StorageKind.List, Encoding? defaultEncoding = null)
    {
        FileInfo = new(path);
        _kind = kind;
        Encoding = defaultEncoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        InitEmpty();
    }

    public IEnumerable<string> ReadLines()
    {
        IReadOnlyList<string> snapshot;
        lock (_sync) snapshot = _roView;
        for (int i = 0; i < snapshot.Count; i++) yield return snapshot[i];
    }

    public string ReadAllText()
    {
        lock (_sync) return string.Join(Environment.NewLine, _roView);
    }

    public void ReplaceLines(IEnumerable<string> newLines)
    {
        var mat = newLines as string[] ?? [];
        lock (_sync)
        {
            if (_kind == StorageKind.Array)
            {
                _array = mat;
                _roView = Array.AsReadOnly(_array);
            }
            else
            {
                _list = [.. mat];
                _roView = [.. _list];
            }
            _dirty = true;
        }
    }

    public void AppendLines(IEnumerable<string> lines)
    {
        lock (_sync)
        {
            EnsureWritableList();                     // why: array→list promotion on first write
            _list!.AddRange(lines);
            _roView = new ReadOnlyCollection<string>(_list);
            _dirty = true;
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            if (_kind == StorageKind.Array)
            {
                _array = [];
                _roView = [];
            }
            else
            {
                _list = [];
                _roView = [];
            }

            _dirty = true;
        }
    }

    // ---- Wizard-only hooks (no I/O here) ----
    internal void SetFromWizard(IEnumerable<string> lines, Encoding encoding, bool markClean)
    {
        var mat = lines as string[] ?? [];
        lock (_sync)
        {
            if (_kind == StorageKind.Array)
            {
                _array = mat;
                _roView = [.. _array];
            }
            else
            {
                _list = [.. mat];
                _roView = [.. _list];
            }
            Encoding = encoding;
            _dirty = !markClean;
        }
    }

    internal void MarkCleanAfterSave()
    {
        lock (_sync) _dirty = false;
        FileInfo.Refresh();
    }

    private void InitEmpty()
    {
        lock (_sync)
        {
            if (_kind == StorageKind.Array)
            {
                _array = [];
                _roView = [.. _array];
            }
            else
            {
                _list = [];
                _roView = [.. _list];
            }
            _dirty = false;
        }
    }

    private void EnsureWritableList()
    {
        if (_kind == StorageKind.List) return;
        _list = _array is null ? [] : [.. _array];
        _array = null;
        _roView = [.. _list];
    }
}

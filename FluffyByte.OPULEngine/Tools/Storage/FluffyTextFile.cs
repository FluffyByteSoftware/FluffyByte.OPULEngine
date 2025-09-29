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

    private readonly Lock _lock = new();

    private readonly StorageKind _kind;

    private string[]? _array;       // compact array for read-only
    
    private List<string?> _list = [];
    private IReadOnlyList<string> _roView = [];
    
    private bool _dirty = false;

    public FileInfo FileInfo { get; }

    public Encoding Encoding { get; private set; }

    public bool IsDirty 
    {
        get 
        {
            return _dirty; 
        }
    }

    public int LineCount
    {
        get
        {
            lock (_lock)
            {
                return _kind == StorageKind.Array ? _array!.Length : _list!.Count;
            }
        }
    }

    public IReadOnlyList<string> Lines
    {
        get
        {
            lock (_lock)
            {
                return _roView;
            }
        }
    }

    public FluffyTextFile(string path, StorageKind kind = StorageKind.List, 
        Encoding? defaultEncoding = null)
    {
        FileInfo = new(path);

        _kind = kind;
        Encoding = defaultEncoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        InitEmpty();
    }

    public IEnumerable<string> ReadLines()
    {
        IReadOnlyList<string> shot;

        lock (_lock) 
            shot = _roView;

        for(int i = 0; i < shot.Count; i++)
        {
            yield return shot[i];
        }
    }

    public string ReadAllText()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine, _roView);
        }
    }

    public void ReplaceLines(IEnumerable<string> newLines)
    {
        string[] mat = newLines as string[] ?? [];

        lock (_lock)
        {
            if (_kind == StorageKind.Array)
            {
                _array = mat;
                _roView = Array.AsReadOnly(_array);
            }
            else
            {
                _list = [];
                _roView = [];
            }

            _dirty = true;
        }
    }

    public void AppendLines(IEnumerable<string> lines)
    {
        lock (_lock)
        {
            EnsureWritableList();
            _list!.AddRange(lines);
            _roView = [];
            _dirty = true;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            if(_kind == StorageKind.Array)
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

    internal void SetFromWizard(IEnumerable<string> lines, Encoding encoding,
        bool markClean)
    {
        var mat = lines as string[] ?? [];
        lock (_lock)
        {
            if(_kind == StorageKind.Array)
            {
                _array = mat;
                _roView = Array.AsReadOnly(_array);
            }
            else
            {
                _list = [];
                _roView = [];
            }

            Encoding = encoding;
            
            _dirty = !markClean;
        }
    }

    internal void MarkCleanAfterSave()
    {
        lock (_lock) _dirty = false;
        FileInfo.Refresh();
    }

    private void InitEmpty()
    {
        lock (_lock)
        {
            if(_kind == StorageKind.Array)
            {
                _array = [];
                _roView = [];
            }
            else
            {
                _list = [];
                _roView = [];
            }
        }
    }

    private void EnsureWritableList()
    {
        if (_kind == StorageKind.List) return;

        _list = _array is null ? [] : [.. _array];
        _array = null;
        _roView = [];
    }
}

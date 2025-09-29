using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public interface IFluffyFile
{
    FileInfo FileINfo { get; }
    bool IsReadOnly { get; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools.Storage;

public interface IFluffyTextFile : IFluffyFile
{
    string[] Contents { get; set; }

    void AppendLine(string line);
    void AppendLineToTop(string line);
}

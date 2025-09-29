using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Startup;

public class ProcessManager
{
    private static readonly Lazy<ProcessManager> _instance = new(() => new());
    public static ProcessManager Instance => _instance.Value;

    private ProcessManager()
    {
        // Private constructor to prevent instantiation
    }


}

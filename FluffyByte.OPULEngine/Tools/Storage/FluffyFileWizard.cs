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


}

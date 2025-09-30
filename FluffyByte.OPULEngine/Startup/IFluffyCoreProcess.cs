using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Startup;
public interface IFluffyCoreProcess
{
    public string Name { get; }
    public ProcessState State { get; }
     
    Task RequestStart();
    Task RequestStop();
}

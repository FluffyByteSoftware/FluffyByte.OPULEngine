using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Networking;
using FluffyByte.OPULEngine.TickSystem;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Startup;

public sealed class ServerOperator : FluffyCoreProcess
{
    private static readonly Lazy<ServerOperator> _instance = new(() => new ServerOperator());
    public static ServerOperator Instance => _instance.Value;

    public override string Name => "Server Operator";
    
    protected override async Task OnStart()
    {
        await Constellations.Instance.LoadSettings();
        
        await Sentinel.Instance.RequestStart();
        await Conductor.Instance.RequestStart();
    }

    protected override async Task OnStop()
    {
        await Constellations.Instance.SaveSettings();

        await Sentinel.Instance.RequestStop();
        await Conductor.Instance.RequestStop();
    }
}

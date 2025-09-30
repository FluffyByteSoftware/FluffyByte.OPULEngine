using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Startup;

public sealed class ServerOperator : FluffyCoreProcess
{
    private static readonly Lazy<ServerOperator> _instance = new(() => new ServerOperator());
    public static ServerOperator Instance => _instance.Value;

    public override string Name => "Server Operator";
    
    protected override async Task OnStart()
    {
        await Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        await Task.CompletedTask;
    }


}

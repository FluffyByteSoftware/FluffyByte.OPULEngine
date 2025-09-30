using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Startup;

namespace FluffyByte.OPULEngine.TickSystem;

public sealed class Conductor : FluffyCoreProcess
{
    private static readonly Lazy<Conductor> _instance = new(() => new Conductor());
    public static Conductor Instance => _instance.Value;

    public override string Name => "Game Conductor";



    protected override async Task OnStart()
    {
        await Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        await Task.CompletedTask;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Startup;

namespace FluffyByte.OPULEngine.Game.Processes;

public sealed class Conductor : FluffyCoreProcess
{
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

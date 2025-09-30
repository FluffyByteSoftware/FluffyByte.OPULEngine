using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Startup;

public abstract class FluffyCoreProcess : IFluffyCoreProcess
{
    public abstract string Name { get; }

    public ProcessState State { get; protected set; } = ProcessState.Stopped;
    public CancellationTokenSource CancellationTokenSource { get; protected set; } = new();

    public async Task RequestStart()
    {
        if(State is ProcessState.Running or ProcessState.Starting)
        {
            Scribe.Warn($"{Name} is already running? State: {State}");
            return;
        }

        try
        {
            State = ProcessState.Stopping;
            Scribe.Info($"Stopping {Name}...");

            await OnStop();

            State = ProcessState.Stopped;
            Scribe.Info($"{Name} is now stopped.");

        }
        catch(Exception ex)
        {
            State = ProcessState.Stopped;
            Scribe.Error($"Failed to stop {Name}.", ex);
        }
    }

    public async Task RequestStop()
    {
        if(State is ProcessState.Stopped or ProcessState.Stopping)
        {
            Scribe.Warn($"{Name} is already stopped.");
            return;
        }

        try
        {
            State = ProcessState.Stopping;
            Scribe.Info($"Stopping {Name}...");

            await OnStop();

            State = ProcessState.Stopped;
            Scribe.Info($"{Name} is now stopped.");
        }
        catch(Exception ex)
        {
            State = ProcessState.Stopped;
            Scribe.Error($"Failed to stop {Name}.", ex);
        }
    }

    public virtual void Dispose()
    {
        if (!CancellationTokenSource.IsCancellationRequested)
            CancellationTokenSource.Cancel();

        CancellationTokenSource.Dispose();
    }

    // Remember to override in subclasses
    protected abstract Task OnStart();
    protected abstract Task OnStop();
}

using System.Diagnostics;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.TickSystem;

public sealed class Heartbeat(TimeSpan? tickInterval = null, bool parallelHandlers = false) : IDisposable
{
    private CancellationTokenSource _cts = new();
    private readonly TimeSpan _tickInterval = tickInterval ?? TimeSpan.FromMilliseconds(50);
    private readonly bool _parallelHandlers = parallelHandlers;
    private Task? _loopTask;

    // async-aware event
    public event Func<uint, Task>? OnTick;

    public void Start(CancellationTokenSource ctsReference)
    {
        if (_loopTask is not null)
            throw new InvalidOperationException("Heartbeat already started.");

        _cts = ctsReference;
        _loopTask = TickLoop();
    }

    public async Task StopAsync()
    {
        _cts.Cancel();

        if (_loopTask is not null)
            await _loopTask;
    }

    public void Stop() => _cts.Cancel();
    public void Dispose() => _cts.Cancel();

    private async Task TickLoop()
    {
        try
        {
            uint tick = 0;
            var sw = new Stopwatch();

            while (!_cts.IsCancellationRequested)
            {
                sw.Restart();
                tick++;

                try
                {
                    if (OnTick is not null)
                    {
                        if (_parallelHandlers)
                        {
                            // run all handlers in parallel
                            var handlers = OnTick.GetInvocationList()
                                .Cast<Func<uint, Task>>()
                                .Select(h => h(tick));

                            await Task.WhenAll(handlers);
                        }
                        else
                        {
                            // run handlers sequentially
                            foreach (var h in OnTick.GetInvocationList().Cast<Func<uint, Task>>())
                                await h(tick);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Scribe.Error("Tick error!", ex);
                }

                TimeSpan elapsed = sw.Elapsed;
                TimeSpan delay = _tickInterval - elapsed;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, _cts.Token);
                else
                    Scribe.Warn(
                        $"Tick overrun: took {elapsed.TotalMilliseconds} ms " +
                        $"(interval {_tickInterval.TotalMilliseconds} ms)");
            }
        }
        catch (TaskCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            Scribe.Error(ex);
        }
    }
}

using System.Diagnostics;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.TickSystem;

public sealed class Heartbeat(TimeSpan? tickInterval = null) : IDisposable
{
    private CancellationTokenSource _cts = new();
    private readonly TimeSpan _tickInterval = tickInterval ?? TimeSpan.FromMilliseconds(50);
    private Task? _loopTask;

    public event Action<uint>? OnTick;


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
                    OnTick?.Invoke(tick);
                }
                catch (Exception ex)
                {
                    Scribe.Error($"Tick error!", ex);
                }

                TimeSpan elapsed = sw.Elapsed;
                TimeSpan delay = _tickInterval - elapsed;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, _cts.Token);
                else
                    Scribe.Warn($"Tick overrun: took {elapsed.TotalMilliseconds} " +
                        "ms (interval {_tickInterval.TotalMilliseconds} ms)");
            }
        }
        catch (TaskCanceledException)
        {

        }
        catch(Exception ex)
        {
            Scribe.Error(ex);
        }
    }
}

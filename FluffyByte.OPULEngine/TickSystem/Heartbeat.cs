using System.Diagnostics;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.TickSystem;

public sealed class Heartbeat(TimeSpan? tickInterval = null) : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _tickInterval = tickInterval ?? TimeSpan.FromMilliseconds(50);
    private Task? _loopTask;

    public event Action<int>? OnTick;


    public void Start()
    {
        if (_loopTask is not null)
            throw new InvalidOperationException("Heartbeat already started.");

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
        int tick = 0;
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
                Scribe.Instance.Error($"Tick error!", ex);
            }

            var elapsed = sw.Elapsed;
            var delay = _tickInterval - elapsed;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, _cts.Token);
            else
                Scribe.Instance.Warn($"Tick overrun: took {elapsed.TotalMilliseconds} ms (interval {_tickInterval.TotalMilliseconds} ms)");
        }
    }
}

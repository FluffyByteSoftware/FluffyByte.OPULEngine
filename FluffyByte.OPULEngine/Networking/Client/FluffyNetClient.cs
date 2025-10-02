using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using FluffyByte.OPULEngine.Networking.Client.Components;
using FluffyByte.OPULEngine.TickSystem;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Networking.Client;

public sealed class FluffyNetClient : IDisposable
{
    private static int _nextId = 0;
    private readonly CancellationTokenSource _cts = new();

    public int ConnectionID { get; }
    public TcpClient TcpC { get; }
    public UdpClient UdpC { get; }
    public string Name { get; }
    public IPEndPoint EndPoint { get; }
    public NetMessenger NetMessenger { get; }
    public ConcurrentQueue<string> IncomingMessages { get; set; } = new();

    private readonly Task? _readerTask;
    private bool _disconnecting = false;

    public FluffyNetClient(TcpClient tcp)
    {
        TcpC = tcp;
        UdpC = new();
        EndPoint = (IPEndPoint)tcp.Client.RemoteEndPoint!;
        ConnectionID = Interlocked.Increment(ref _nextId);
        Name = $"Client_{ConnectionID}_{EndPoint.Address}:{EndPoint.Port}";

        NetMessenger = new(this);

        // start background reader
        _readerTask = Task.Run(() => ReaderLoop(_cts.Token));
    }

    private async Task ReaderLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && TcpC.Connected)
            {
                string line = await NetMessenger.ReceiveTcpMessage(token);
                if (string.IsNullOrWhiteSpace(line))
                    continue; // no message, skip

                IncomingMessages.Enqueue(line);
            }
        }
        catch (OperationCanceledException)
        {
            // expected during shutdown
        }
        catch (Exception ex)
        {
            Scribe.Error($"Reader loop error for {Name}", ex);
        }
        finally
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (_disconnecting) return;
        _disconnecting = true;

        Conductor.Instance.UnregisterClient(this);

        try
        {
            _cts.Cancel();
            TcpC.Close();
            UdpC.Close();
        }
        catch { /* swallow cleanup errors */ }

        Scribe.Info($"Client {Name} disconnected.");
    }

    public void Dispose() => Disconnect();
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Networking.Client;
using FluffyByte.OPULEngine.Startup;
using FluffyByte.OPULEngine.TickSystem;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Networking;

public sealed class Sentinel : FluffyCoreProcess
{
    private static readonly Lazy<Sentinel> _instance = new(() => new());
    public static Sentinel Instance => _instance.Value;

    public override string Name { get; } = "Networking Sentinel";

    private readonly int HostPort = Constellations.Instance.HostPort;
    private readonly IPAddress HostAddress = IPAddress.Parse(Constellations.Instance.HostAddress);

    private TcpListener? _listener;
    private readonly List<Task> _clientTasks = [];

    protected override async Task OnStart()
    {
        _listener = new TcpListener(HostAddress, HostPort);
        _listener.Start();

        _ = ListenForClients();

        await Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        await CancellationTokenSource.CancelAsync();

        _listener?.Stop();

        await Task.CompletedTask;

    }

    private async Task ListenForClients()
    {
        if (_listener is null) return;

        while (!CancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                TcpClient newClient = await _listener.AcceptTcpClientAsync(CancellationTokenSource.Token);
                
                FluffyNetClient fluffyClient = new(newClient);


                // Test communication
                byte[] buffer = Encoding.ASCII.GetBytes("Hello World \r\n");
                await newClient.GetStream().WriteAsync(buffer, CancellationTokenSource.Token);
                await newClient.GetStream().FlushAsync();

                await fluffyClient.NetMessenger.SendTcpMessageRaw("Test \r\n");
                await fluffyClient.NetMessenger.SendTcpMessage("Hello world!");
                

                Conductor.Instance.RegisterClient(fluffyClient);

                Scribe.Info($"Client {fluffyClient.Name} registered with the Conductor.");
            }
            catch (OperationCanceledException) { break; }
            catch(ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                Scribe.Error("Error accepting new client.", ex);
            }
        }
    }
}

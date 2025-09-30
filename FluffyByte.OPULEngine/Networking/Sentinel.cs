using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Networking.Client;
using FluffyByte.OPULEngine.Startup;
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
                var task = HandleClient(newClient);

                _clientTasks.Add(task);
            }
            catch (OperationCanceledException) { break; }
            catch(ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                Scribe.Error("Error accepting new client.", ex);
            }
        }
    }

    private async Task HandleClient(TcpClient newClient)
    {
        if (_listener is null) await Task.CompletedTask;

        try
        {
            Scribe.Info($"Client connected from {newClient.Client.RemoteEndPoint}");

            using var netClient = new FluffyNetClient(newClient);

            await netClient.NetMessenger.SendTcpMessage("Hello from server!");

            while (newClient.Connected)
            {
                string response = await netClient.NetMessenger.ReceiveTcpMessage();

                if (string.IsNullOrWhiteSpace(response))
                {
                    continue;
                }

                Scribe.Info($"Received: {response}");
                await netClient.NetMessenger.SendTcpMessage($"Echo: {response}");
            }
        }
        catch(Exception ex)
        {
            Scribe.Error(ex);
        }
        finally
        {
            Scribe.Info($"Closing TcpClient: {newClient.Client.RemoteEndPoint}");
            newClient.Close();
        }
    }
}

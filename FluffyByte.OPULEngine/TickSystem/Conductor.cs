using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Startup;
using FluffyByte.OPULEngine.Tools;
using FluffyByte.OPULEngine.Networking.Client;

namespace FluffyByte.OPULEngine.TickSystem;

public sealed class Conductor : FluffyCoreProcess
{
    private static readonly Lazy<Conductor> _instance = new(() => new Conductor());
    public static Conductor Instance => _instance.Value;

    public override string Name => "Game Conductor";

    private bool _acceptClients = false;

    private readonly List<FluffyNetClient> _clients = [];

    public Heartbeat CommunicationTick { get; private set; }
    public Heartbeat StateHeartbeat { get; private set; }

    private Conductor()
    {
        CommunicationTick = new Heartbeat(TimeSpan.FromMilliseconds(100));
        StateHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(50));
    
        CommunicationTick.OnTick += OnCommunicationTick;
        StateHeartbeat.OnTick += OnStateTick;
    }

    protected override async Task OnStart()
    {
        CommunicationTick = new(TimeSpan.FromMilliseconds(100));
        StateHeartbeat = new(TimeSpan.FromMilliseconds(50));

        CommunicationTick.Start(CancellationTokenSource);
        StateHeartbeat.Start(CancellationTokenSource);

        await Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        _acceptClients = false;
        
        await Task.CompletedTask;
    }

    public void RegisterClient(FluffyNetClient client)
    {
        lock (_clients)
            _clients.Add(client);
    }

    public void UnRegisterClient(FluffyNetClient client)
    {
        lock (_clients)
            _clients.Remove(client);
    }

    private void OnCommunicationTick(uint tick)
    {
        if (!_acceptClients || State is not ProcessState.Running) return;

        // Handle communication with clients
        foreach (var client in _clients.ToList())
        {
            try
            {
                //client.ProcessIncomingMessages();
                //client.SendOutgoingMessages();
            }
            catch (Exception ex)
            {
                Scribe.Error($"Error processing client {client.Name}", ex);
                _clients.Remove(client);
            }
        }
    }

    private void OnStateTick(uint tick)
    {
        if (!_acceptClients || State is not ProcessState.Running) return;
        

    }

    public static async Task ConductorLoopTask(FluffyNetClient client, CancellationTokenSource cts)
    {
        while(cts.IsCancellationRequested is false)
        {
            try
            {
                
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Scribe.Error($"Error in conductor loop for client {client.Name}", ex);
                break;
            }
        }

        await Task.CompletedTask;
    }
}

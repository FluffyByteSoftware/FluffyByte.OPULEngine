using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Networking.Client;
using FluffyByte.OPULEngine.Startup;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Networking;

public class ConnectionTracker : FluffyCoreProcess
{
    public override string Name => "Connection Tracker";
    
    private static readonly Lazy<ConnectionTracker> _instance = new(() => new ConnectionTracker());
    public static ConnectionTracker Instance => _instance.Value;

    public List<FluffyNetClient> ConnectedClients { get; private set; } = [];
    private readonly Lock _lock = new();

    private bool _listening = false;

    private ConnectionTracker() { }

    protected override async Task OnStart()
    {
        ClearClients();
        
        _listening = true;

        await Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        _listening = false;

        foreach(var client in ConnectedClients)
        {
            try
            {
                client.Disconnect();
            }
            catch(Exception ex)
            {
                Scribe.Error(ex);
            }
        }

        ClearClients();

        await Task.CompletedTask;
    }

    public void AddClient(FluffyNetClient client)
    {
        if (!_listening) return;

        lock (_lock)
        {
            if (!ConnectedClients.Contains(client))
            {
                ConnectedClients.Add(client);
            }
        }
    }

    public void RemoveClient(FluffyNetClient client)
    {
        if (!_listening) return;

        lock (_lock)
        {
            ConnectedClients.Remove(client);
        }
    }

    public async Task BroadcastMessage(string message)
    {
        try
        {
            foreach(var client in ConnectedClients.ToList())
            {
                await client.NetMessenger.SendTcpMessage($"Broadcast: {message}");
            }
        }
        catch(Exception ex)
        {
            Scribe.Error(ex);
        }
    }

    private void ClearClients()
    {
        lock (_lock)
        {
            ConnectedClients.Clear();
        }
    }
    

}

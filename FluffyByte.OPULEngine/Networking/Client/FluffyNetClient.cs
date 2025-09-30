using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Networking.Client.Components;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Networking.Client;

public sealed class FluffyNetClient : IDisposable
{
    private static int _nextId = 0;
    
    public int ConnectionID { get; private set; }
    
    public UdpClient UdpC { get; }
    public TcpClient TcpC { get; }

    public string Name { get; }
    public IPEndPoint EndPoint { get; }

    public IPAddress Address { get; }
    public string DNSAddress { get; }

    public NetMessenger NetMessenger { get; private set; }

    private bool _disconnecting = false;

    public FluffyNetClient(TcpClient tcp)
    {
        UdpC = new();

        TcpC = tcp;
        
        EndPoint = (IPEndPoint)tcp.Client.RemoteEndPoint!;

        _nextId++;
        ConnectionID = _nextId;

        Name = $"Client_{ConnectionID}_{EndPoint.Address}:{EndPoint.Port}";
        Address = EndPoint.Address;

        DNSAddress = Dns.GetHostEntry(Address).HostName;

        NetMessenger = new(this);
    }   

    public void Disconnect()
    {
        if (_disconnecting) return;

        Scribe.Info($"Disconnecting client {Name}...");
        _disconnecting = true;

        TcpC.Close();
        UdpC.Close();

        Scribe.Info($"Client {Name} disconnected.");
    }

    public void Dispose() => Disconnect();
}

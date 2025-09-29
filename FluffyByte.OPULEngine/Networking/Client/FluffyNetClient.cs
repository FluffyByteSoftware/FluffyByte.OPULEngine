using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Networking.Client;

public sealed class FluffyNetClient
{
    private static int _nextId = 0;
    
    public int ConnectionID { get; private set; }
    
    public UdpClient UdpC { get; }
    public TcpClient TcpC { get; }

    public string Name { get; }
    public IPEndPoint EndPoint { get; }

    public IPAddress Address { get; }
    public string DNSAddress { get; }

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
    }
}

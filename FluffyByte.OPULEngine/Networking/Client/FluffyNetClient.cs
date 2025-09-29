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
    public UdpClient UdpC { get; }
    public TcpClient TcpC { get; }

    public string Name { get; }
    public IPEndPoint EndPoint { get; }

    public FluffyNetClient(string name, UdpClient udp, TcpClient tcp)
    {
        Name = name;
        UdpC = udp;
        TcpC = tcp;
        EndPoint = (IPEndPoint)udp.Client.RemoteEndPoint!;
    }
}

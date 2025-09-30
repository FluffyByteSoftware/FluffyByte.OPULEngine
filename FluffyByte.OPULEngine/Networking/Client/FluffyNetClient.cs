using System;
using System.Net;
using System.Net.Sockets;
using FluffyByte.OPULEngine.Networking.Client.Components;
using FluffyByte.OPULEngine.TickSystem;
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

        Address = EndPoint.Address;

        // Try reverse DNS lookup, fall back to IP string
        try
        {
            var entry = Dns.GetHostEntry(Address);
            DNSAddress = entry.HostName;
        }
        catch
        {
            DNSAddress = Address.ToString();
        }

        Name = $"Client_{ConnectionID}_{DNSAddress}:{EndPoint.Port}";

        NetMessenger = new(this);
    }

    public void Disconnect()
    {
        if (_disconnecting) return;

        Conductor.Instance.UnregisterClient(this);

        Scribe.Info($"Disconnecting client {Name}...");
        _disconnecting = true;

        TcpC.Close();
        UdpC.Close();

        Scribe.Info($"Client {Name} disconnected.");
    }

    public void Dispose() => Disconnect();
}

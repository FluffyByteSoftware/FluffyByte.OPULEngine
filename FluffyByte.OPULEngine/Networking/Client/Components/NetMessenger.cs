using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Networking.Client.Components;

public sealed class NetMessenger(FluffyNetClient parent)
{
    private readonly FluffyNetClient _parent = parent;

    private readonly NetworkStream _stream = parent.TcpC.GetStream();
    private readonly StreamReader _reader = new(parent.TcpC.GetStream());
    private readonly StreamWriter _writer = new(parent.TcpC.GetStream()) { AutoFlush = true };
    
    public async Task SendMessage(string message)
    {
        try
        {
            await _writer.WriteLineAsync(message);
        }
        catch(Exception ex)
        {
            Scribe.Instance.Error(ex);
        }
    }

    public async Task SendMessageRaw(string message)
    {
        try
        {
            await _writer.WriteAsync(message);
        }
        catch(Exception ex)
        {
            Scribe.Instance.Error(ex);
        }
    }

    public async Task<string> ReceiveMessage()
    {
        try
        {
            string? response = await _reader.ReadLineAsync();

            return response ?? string.Empty;
        }
        catch(Exception ex)
        {
            Scribe.Instance.Error(ex);

            return string.Empty;
        }
    }
}
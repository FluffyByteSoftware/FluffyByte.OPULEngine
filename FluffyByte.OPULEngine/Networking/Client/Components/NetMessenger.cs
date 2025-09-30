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

    private readonly StreamReader _reader = new(parent.TcpC.GetStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
    private readonly StreamWriter _writer = new(parent.TcpC.GetStream(), bufferSize: 1024, leaveOpen: true) { AutoFlush = true };

    public async Task SendTcpMessage(string message)
    {
        await SendTcpMessageRaw(message.EndsWith('\n') ? message : message + "\n");
    }

    public async Task SendTcpMessageRaw(string message)
    {
        try
        {
            await _writer.WriteAsync(message);
            await _writer.FlushAsync();
        }
        catch(IOException ioEx) when (ioEx.InnerException is SocketException sockEx)
        {
            Scribe.Warn($"Socket closed: {sockEx.SocketErrorCode}");
            _parent.Disconnect();
        }
        catch (ObjectDisposedException)
        {
            Scribe.Warn("Stream has been disposed.");
            _parent.Disconnect();
        }
        catch (Exception ex)
        {
            Scribe.Error(ex);
            _parent.Disconnect();
        }
    }

    public async Task<string> ReceiveTcpMessage()
    {
        try
        {
            string? response = await _reader.ReadLineAsync();

            return response ?? string.Empty;
        }
        catch(IOException ioEx) when (ioEx.InnerException is SocketException sockEx)
        {
            Scribe.Warn($"Socket closed: {sockEx.SocketErrorCode}");
            _parent.Disconnect();

            return string.Empty;
        }
        catch (ObjectDisposedException)
        {
            Scribe.Warn("Stream has been disposed.");
            _parent.Disconnect();
            return string.Empty;
        }
        catch(Exception ex)
        {
            Scribe.Error(ex);
            _parent.Disconnect();
            return string.Empty;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using FluffyByte.OPULEngine.Startup;
using FluffyByte.OPULEngine.Tools;
using FluffyByte.OPULEngine.Networking.Client;

namespace FluffyByte.OPULEngine.TickSystem
{
    public sealed class Conductor : FluffyCoreProcess
    {
        private static readonly Lazy<Conductor> _instance = new(() => new Conductor());
        public static Conductor Instance => _instance.Value;

        public override string Name => "Game Conductor";

        private readonly List<FluffyNetClient> _clients = [];
        private Heartbeat? _communicationTick;
        private Heartbeat? _stateHeartbeat;

        private Conductor() { }

        protected override async Task OnStart()
        {
            // reset cancellation
            CancellationTokenSource = new CancellationTokenSource();

            // Comm heartbeat: parallel (fan-out to many clients without blocking each other)
            _communicationTick = new Heartbeat(TimeSpan.FromMilliseconds(250), parallelHandlers: true);

            // State heartbeat: sequential (deterministic game-loop style)
            _stateHeartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), parallelHandlers: false);

            _communicationTick.OnTick += OnCommunicationTick;
            _stateHeartbeat.OnTick += OnStateTick;

            _communicationTick.Start(CancellationTokenSource);
            _stateHeartbeat.Start(CancellationTokenSource);

            await Task.CompletedTask;
        }

        protected override async Task OnStop()
        {
            try
            {
                if (_communicationTick is not null)
                    await _communicationTick.StopAsync();
                if (_stateHeartbeat is not null)
                    await _stateHeartbeat.StopAsync();
            }
            catch (Exception ex)
            {
                Scribe.Error("Error stopping Conductor ticks", ex);
            }

            lock (_clients) _clients.Clear();
            await Task.CompletedTask;
        }

        public void RegisterClient(FluffyNetClient client)
        {
            // give each client its own queue
            client.IncomingMessages = new ConcurrentQueue<string>();

            lock (_clients)
                _clients.Add(client);
        }

        public void UnregisterClient(FluffyNetClient client)
        {
            lock (_clients)
                _clients.Remove(client);
        }

        // Communication tick: gather input
        private async Task OnCommunicationTick(uint tick)
        {
            if (State is not ProcessState.Running) return;

            List<FluffyNetClient> snapshot;
            lock (_clients)
                snapshot = [.. _clients];

            // just send heartbeats now, no more ReceiveTcpMessage here
            var tasks = snapshot.Select(client =>
                client.NetMessenger.SendTcpMessage($"Tick {tick} from Conductor."));
            await Task.WhenAll(tasks);
        }


        // State tick: deterministic game loop
        private async Task OnStateTick(uint tick)
        {
            if (State is not ProcessState.Running) return;

            List<FluffyNetClient> snapshot;

            lock (_clients)
                snapshot = [.. _clients];

            foreach (var client in snapshot)
            {
                while (client.IncomingMessages != null && client.IncomingMessages.TryDequeue(out var msg))
                {
                    if (msg.StartsWith('\r') || msg.StartsWith('\n'))
                        continue;

                    try
                    {
                        if (msg.Equals("quit", StringComparison.OrdinalIgnoreCase))
                        {
                            Scribe.Info($"Client {client.Name} requested disconnection.");
                            client.Disconnect();
                        }
                        else
                        {
                            await client.NetMessenger.SendTcpMessage($"Echo: {msg}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Scribe.Info($"Processing disconnect for {client.Name}");
                    }
                    catch (Exception ex)
                    {
                        Scribe.Error($"Error processing client {client.Name}", ex);
                        client.Disconnect();
                    }
                }
            }


            await Task.CompletedTask;
        }
    }
}

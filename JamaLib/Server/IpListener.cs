using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piksel.GrowlLib.Server
{
    public class IpListener
    {
        const int SleepTime = 100;

        bool legacy;

        private Thread listenerThread;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public bool Started { get; private set; } = false;

        // Thread-safe static information
        private readonly Info info;

        // Thread fields
        private readonly IPEndPoint endpoint;
        private readonly TcpListener tcp;
        private readonly UdpClient udp;

        internal IpListener(IPAddress address, int port, bool legacy)
        {
            endpoint = new IPEndPoint(address, port);
            info = new Info(endpoint, legacy);

            this.legacy = legacy;

            if (legacy)
            {
                udp = new UdpClient(port);
            }
            else
            {
                tcp = new TcpListener(endpoint);
            }
        }

        public IpListener Start()
        {
            if (Started)
            {
                throw new InvalidOperationException("Listener has already been started");
            }

            Started = true;

            if (legacy)
            {
                listenerThread = new Thread(ListenerLegacy);
            }
            else
            {
                tcp.Start();

                listenerThread = new Thread(ListenerMain);
            }
            listenerThread.Start(cts.Token);

            return this;
        }

        public bool Stop()
        {
            cts.Cancel();
            if (!listenerThread.Join(2000))
            {
                listenerThread.Abort();
                return false;
            }
            return true;
        }

        public Task<bool> StopAsync()
            => Task.Factory.StartNew(Stop);

        private void ListenerMain(object param)
        {
            if (!(param is CancellationToken ct))
            {
                ct = new CancellationToken();
            }

            while (!ct.IsCancellationRequested)
            {
                while (tcp.Pending())
                {
                    var client = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ConnectionMain, client);
                }

                Thread.Sleep(SleepTime);
            }
        }

        private void ConnectionMain(object param)
        {
            if (!(param is TcpClient client))
            {
                return;
            }

            using (var stream = client.GetStream())
            {
                OnClientConnected(new ClientConnectedEventArgs(client.Client.RemoteEndPoint as IPEndPoint, stream, info));
            }
        }

        private void ListenerLegacy(object param)
        {
            if (!(param is CancellationToken ct))
            {
                ct = new CancellationToken();
            }

            while (!ct.IsCancellationRequested)
            {
                while (udp.Available > 0)
                {
                    var client = new IPEndPoint(IPAddress.Any, 0);
                    var data = udp.Receive(ref client);
                    using (var inStream = new MemoryStream(data, false))
                    using (var outStream = new MemoryStream())
                    {
                        OnClientConnected(new ClientConnectedEventArgs(client, inStream, outStream, info));
                    }
                }

                Thread.Sleep(SleepTime);
            }
        }

        public event ClientConnectedEventHandler ClientConnected;
        protected virtual void OnClientConnected(ClientConnectedEventArgs eventArgs)
            => ClientConnected?.Invoke(eventArgs);

        public struct Info
        {
            public IPAddress Address { get; }
            public int Port { get; }
            public bool Legacy { get; }

            internal Info(IPEndPoint ep, bool legacy)
            {
                Address = new IPAddress(ep.Address.GetAddressBytes());
                Port = ep.Port;
                Legacy = legacy;
            }
        }

    }
}

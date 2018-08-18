using Piksel.GrowlLib.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Piksel.GrowlLib.Server
{
    public delegate void ClientConnectedEventHandler(ClientConnectedEventArgs eventArgs);

    public class Listener
    {
        private bool enableIPv4 = true;
        private bool enableIPv6 = false;
        private bool enableLegacy = true;

        private IpListener listenerIp4;
        private IpListener listenerIp6;
        private IpListener listenerLegacy;

        // Events

        public event ClientConnectedEventHandler ClientConnected;

        internal bool Stop()
        {
            return (listenerIp4?.Stop() ?? true)
                && (listenerIp6?.Stop() ?? true)
                && (listenerLegacy?.Stop() ?? true);
        }


        // Named constructors

        public Listener()
        {

        }

        public Listener WithIPv4(bool enabled = true)
        {
            enableIPv4 = enabled;
            return this;
        }

        public Listener WithIPv6(bool enabled = true)
        {
            enableIPv6 = enabled;
            return this;
        }

        public Listener WithLegacy(bool enabled = true)
        {
            enableLegacy = enabled;
            return this;
        }

        public Listener OnClientConnect(ClientConnectedEventHandler eventHandler)
        {
            ClientConnected += eventHandler;
            return this;
        }

        public Listener Listen()
        {
            listenerIp4 = enableIPv4 ? AddListener(IPAddress.Any, false) : null;
            listenerIp6 = enableIPv6 ? AddListener(IPAddress.IPv6Any, false) : null;
            listenerLegacy = enableLegacy ? AddListener(IPAddress.Any, true) : null;

            return this;
        }

        private IpListener AddListener(IPAddress address, bool legacy)
        {
            var port = legacy ? Protocol.LegacyPort : Protocol.DefaultPort;
            var listener = new IpListener(address, port, legacy);
            listener.ClientConnected += ClientConnected;
            return listener.Start();
        }
    }
}

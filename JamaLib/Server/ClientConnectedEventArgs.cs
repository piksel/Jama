using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Piksel.GrowlLib.Server
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnectedEventArgs(IPEndPoint client, Stream stream, IpListener.Info info) 
            : this(client, stream, stream, info) { }

        public ClientConnectedEventArgs(IPEndPoint client, Stream inStream, Stream outStream, IpListener.Info info)
        {
            Client = client;
            InputStream = inStream;
            OutputStream = outStream;
            LocalPort = info.Port;
            AddressFamily = info.Address.AddressFamily;
            Legacy = info.Legacy;
        }

        public bool Legacy { get; }
        public IPEndPoint Client { get; }
        public Stream InputStream { get; }
        public Stream OutputStream { get; }
        public int LocalPort { get; }
        public AddressFamily AddressFamily { get; }
        public bool IPv6 => AddressFamily == AddressFamily.InterNetworkV6;
    }
}

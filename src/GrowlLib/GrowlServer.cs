using Piksel.GrowlLib.Common;
using Piksel.GrowlLib.Events;
using Piksel.GrowlLib.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace Piksel.GrowlLib
{
    public class GrowlServer
    {
        private Listener listener;

        public IEnumerable<string> GetPorts()
        {
            if (EnableIPv4) yield return $"tcp://{IPAddress.Any}:{Protocol.DefaultPort}";
            if (EnableIPv6) yield return $"tcp://{IPAddress.IPv6Any}:{Protocol.DefaultPort}";
            if (EnableLegacy) yield return $"udp://{IPAddress.Any}:{Protocol.LegacyPort}";
        }

        public bool EnableIPv4 { get; set; } = true;
        public bool EnableIPv6 { get; set; } = false;

        public bool EnableLegacy { get; set; } = true;

        public event EventHandler<RegistrationAttemptEventArgs> RegistrationAttempt;

        public HashSet<string> RegisteredApps { get; } = new HashSet<string>();


        public bool Stop()
            => listener.Stop();

        public void Start()
        {
            listener = new Listener()
                .WithIPv4(EnableIPv4)
                .WithIPv6(EnableIPv6)
                .WithLegacy(EnableLegacy)
                .OnClientConnect(ClientConnected)
                .Listen();
        }

        

        private void ClientConnected(ClientConnectedEventArgs eventArgs)
        {
            var clientId = eventArgs.Client.ToString();

            Console.WriteLine(">> Start of request: " + clientId);

            var request = Request.FromStream(eventArgs.InputStream, eventArgs.Legacy);

            var response = HandleRequest(request);

            Console.WriteLine(">> End of request: " + clientId);
            Console.WriteLine();

            Console.WriteLine("<< Start of response: " + clientId);

            response.EncryptionKey = EncryptionKey.Unencrypted;

            response.ToStream(eventArgs.OutputStream, eventArgs.Legacy);

            Console.WriteLine("<< End of response: " + clientId);


            Console.WriteLine();
            
        }

        private Response HandleRequest(Request request)
        {
            if (!request.ValidProtoHeader)
            {
                return Response.Error(StatusCode.InvalidRequest);
            }

            switch (request.Type)
            {
                case RequestType.Register:
                    if (RegistrationAttempt != null)
                    {
                        var ea = new RegistrationAttemptEventArgs();
                        RegistrationAttempt?.Invoke(this, ea);
                        if (!ea.Allowed)
                        {
                            return Response.Error(StatusCode.Unauthorized);
                        }
                    }
                    RegisteredApps.Add(request.Headers.Application.Name);
                    return Response.Ok();

                case RequestType.Notify:
                    if (!RegisteredApps.Contains(request.Headers.Application.Name))
                    {
                        return Response.Error(StatusCode.UnknownApp);
                    }
                    return Response.Ok();

                default:
                    return Response.Error(StatusCode.UnknownRequest);
            }
        }
    }
}

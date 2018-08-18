using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Piksel.GrowlLib.Common;
using System.IO;

namespace Piksel.GrowlLib.Server
{
    public abstract class Message
    {
        public bool ValidProtoHeader { get; private set; }

        protected abstract Headers MessageHeaders { get; }

        public bool ParseRequestLine(string line)
        {
            var match = reReqLine.Match(line);
            if (match.Success)
            {
                Version = match.Groups[1].Value;
                TypeString = match.Groups[2].Value;
            }
            ValidProtoHeader = match.Success;

            return ValidProtoHeader;
        }

        protected abstract string TypeString { get; set; }


        public EncryptionKey EncryptionKey { get; set; }
        public string Version { get; set; }

        static Regex reReqLine = new Regex(@"GNTP/(\d\.\d)\s+(\S+)\s+(\S+)?(?:\s+(\S+))?");
        static Regex reHeadLine = new Regex(@"(?:([a-z|A-Z]+)-)+([a-z|A-Z]+):\s+(.*)");

        public bool ParseHeader(string line)
        {
            var match = reHeadLine.Match(line);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();

            var key = match.Groups[2].Value;
            var value = match.Groups[3].Value;

            MessageHeaders.Add(key, value, new ArraySegment<string>(groups));

            return true;
        }

        public IEnumerable<string> GetLines()
        {
            yield return $"GNTP/{Version} {TypeString} {EncryptionKey.GetHeaderString()}";
            foreach (var headerRow in MessageHeaders.GetStrings())
            {
                yield return headerRow;
            }
        }
    }

    public sealed class Request : Message
    {
        protected override Headers MessageHeaders => Headers;

        public RequestHeaders Headers { get; set; } = new RequestHeaders();

        public RequestType Type { get; private set; }

        protected override string TypeString
        {
            set => Type = Enum.TryParse(value, true, out RequestType type) ? type : RequestType.Unknown;
            get => Type.ToString().ToUpper();
        }

        internal static Request FromStream(Stream stream, bool legacy)
        {
            var request = new Request();

            using (var sr = new StreamReader(stream))
            {
                if (legacy)
                {
                    Console.WriteLine("Bytes:");
                    int intByte;
                    while ((intByte = sr.Read()) != -1)
                    {
                        var b = (byte)intByte;
                        Console.Write($"{b:x2} ");
                    }
                    Console.WriteLine();

                    stream.Seek(0, SeekOrigin.Begin);
                    Console.WriteLine("UTF8:");
                    Console.WriteLine(sr.ReadToEnd());
                    Console.WriteLine();
                }
                else
                {
                    var reqLine = sr.ReadLine();

                    if (!request.ParseRequestLine(reqLine))
                    {
                        Console.WriteLine($"Error: Failed to parse request line: \"{reqLine}\"");
                    }
                    else
                    {
                        string line;
                        while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                        {
                            if (!request.ParseHeader(line))
                            {
                                Console.WriteLine($"Error: Failed to parse header line: \"{line}\"");
                            }
                        }
                    }
                }
            }

            return request;
        }
    }

    public sealed class Response : Message
    {
        protected override Headers MessageHeaders => Headers;

        public ResponseHeaders Headers { get; set; } = new ResponseHeaders();

        public ResponseType Type { get; private set; }

        protected override string TypeString
        {
            set => Type = Enum.TryParse(value, true, out ResponseType type) ? type : ResponseType.Unknown;
            get => Type.ToString().ToUpper();
        }

        private Response(ResponseType type)
        {
            Version = "1.0";
            Type = type;
        }

        private Response(ResponseType type, StatusCode code): this(type)
        {
            Headers.Error.Code = (int)code;
            Headers.Error.Description = code.ToString();
        }

        public static Response Error(StatusCode code)
            => new Response(ResponseType.Error, code);

        public static Response Ok()
            => new Response(ResponseType.Ok);

        internal void ToStream(Stream stream, bool legacy)
        {
            using (var sw = new StreamWriter(stream))
            {
                foreach (var respLine in GetLines())
                {
                    sw.WriteLine(respLine);
                    Console.WriteLine("< " + respLine);
                }
            }
        }
    }
}

using Piksel.GrowlLib;
using System;

namespace ExampleGrowlServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating GrowlServer instance...");
            var server = new GrowlServer();

            try
            {
                server.EnableIPv6 = true;
                server.Start();
                Console.WriteLine($"Listening on {string.Join(", ", server.GetPorts())}. Press any key to stop.");
                Console.WriteLine();
                Console.WriteLine("Server output:");
                Console.ReadKey();
                server.Stop();
            }
            catch (Exception x)
            {
                Console.WriteLine($"Failed to start listening on port: {x.GetType().Name}: {x.Message}");
                Console.WriteLine(x.StackTrace);
                WaitForAnyKey("exit");
            }
        }

        private static void WaitForAnyKey(string action = "continue")
        {
            Console.Write($"Press any key to {action}...");
            Console.ReadKey();
        }
    }
}

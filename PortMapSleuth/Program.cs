using System;
using Funq;
using ServiceStack;
using Mono.Unix;
using Mono.Unix.Native;

namespace PortMapSleuth {
    internal class Program {
        private static void Main(string[] args) {
            var listeningOn = args.Length == 0 ? "http://*:80/" : "http://*:" + args[0] + "/";
            var appHost = new AppHost()
                .Init()
                .Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

            // ------------------------------------------------------------- //
            // The below code is to support Unix daemonization.
            // Remove this and the Mono requirement to run it on Windows
            UnixSignal[] signals = new[] { 
                new UnixSignal(Signum.SIGINT), 
                new UnixSignal(Signum.SIGTERM), 
            };

            // Wait for a unix signal:
            for (bool exit = false; !exit; ) {
                int id = UnixSignal.WaitAny(signals);
                if (id >= 0 && id < signals.Length)
                    if (signals[id].IsSet)
                        exit = true;
            }
            // ------------------------------------------------------------- //
        }

        //Web Service Host Configuration
        public class AppHost : AppHostHttpListenerBase {
            public AppHost() : base("Port Test Request Service", typeof (PortTestRequest).Assembly) {}

            public override void Configure(Container container) { }
        }
    }
}
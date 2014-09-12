using System;
using Funq;
using ServiceStack;

namespace PortMapSleuth {
    internal class Program {
        private static void Main(string[] args) {
            var listeningOn = args.Length == 0 ? "http://*:80/" : "http://*:" + args[0] + "/";
            var appHost = new AppHost()
                .Init()
                .Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

            Console.ReadKey();
        }

        //Web Service Host Configuration
        public class AppHost : AppSelfHostBase {
            public AppHost() : base("Port Test Request Service", typeof (PortTestRequest).Assembly) {}

            public override void Configure(Container container) { }
        }
    }
}

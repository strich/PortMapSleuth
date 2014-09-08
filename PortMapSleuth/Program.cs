using System;
using System.Collections.Generic;
using System.Linq;
using Funq;
using ServiceStack;

namespace PortMapSleuth {
    internal class Program {
        private static void Main(string[] args) {
            var listeningOn = args.Length == 0 ? "http://*:1337/" : args[0];
            var appHost = new AppHost()
                .Init()
                .Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}",
                              DateTime.Now, listeningOn);

            Console.ReadKey();
        }

        //Web Service Host Configuration
        public class AppHost : AppSelfHostBase {
            public AppHost() : base("Port Test Requests", typeof (PortTestRequest).Assembly) {}

            public override void Configure(Container container) {
                container.Register(new PortTestRepository());
            }
        }
    }
}

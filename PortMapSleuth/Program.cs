using System;
using Funq;
using ServiceStack;

namespace PortMapSleuth {
    internal class Program {
        private static void Main() {
            var appHost = new AppHost()
                .Init()
                .Start("http://*/");

            Console.WriteLine("AppHost Created at {0}, listening on port 80", DateTime.Now);

            Console.ReadKey();
        }

        //Web Service Host Configuration
        public class AppHost : AppSelfHostBase {
            public AppHost() : base("Port Test Request Service", typeof (PortTestRequest).Assembly) {}

            public override void Configure(Container container) { }
        }
    }
}

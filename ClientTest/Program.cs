using System;
using System.Collections.Generic;
using PortMapSleuth;

namespace ClientTest {
    internal class Program {
        private static void Main() {

            var pms = new PortTestClient(PortTestFinishedEventHandler);
            pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });

            Console.ReadKey();
        }

        private static void PortTestFinishedEventHandler(PortTestFinishedEventArgs eventArgs) {
            Console.WriteLine(eventArgs.PortTestResult);
        }
    }
}
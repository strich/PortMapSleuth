using System;
using System.Collections.Generic;

namespace ClientTest {
    internal class Program {
        private static void Main() {

            var pms = new PortMapSleuth.PortTestClient();
            var portsOpen = pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });

            Console.WriteLine(portsOpen);

            while (true) {}
        }
    }
}
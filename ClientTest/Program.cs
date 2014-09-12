using System;

namespace ClientTest {
    internal class Program {
        private static void Main() {

            var pms = new PortMapSleuth.PortTestClient();
            var portsOpen = pms.TestPorts(IPProtocol.UDP, new int[] {27015, 27016});

            Console.WriteLine(portsOpen);

            while (true) {}
        }
    }
}
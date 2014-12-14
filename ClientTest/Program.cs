#define PORTTESTPOLLING

using System;
using System.Collections.Generic;
using PortMapSleuth;

namespace ClientTest {
    internal class Program {
        private static void Main() {
            const string portMapSluethURL = "http://127.0.0.1:8080/request";

            // Use events to get the result:
            #if !PORTTESTPOLLING
            var pms = new PortTestClient(portMapSluethURL, PortTestFinishedEventHandler);
            pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });

            Console.ReadKey();
#endif

            // Use poll to get the result:
            #if PORTTESTPOLLING
            var pms = new PortTestClient(portMapSluethURL);
            pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });
            bool portTestFinished = false;
            while (!portTestFinished) {
                if (pms.IsPortTestFinished) {
                    Console.WriteLine(pms.PortTestResult);
                    portTestFinished = true;
                }
            }
            Console.ReadKey();
            #endif
        }

        private static void PortTestFinishedEventHandler(PortTestFinishedEventArgs eventArgs) {
            Console.WriteLine(eventArgs.PortTestResult);
        }
    }
}
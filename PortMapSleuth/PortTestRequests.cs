using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServiceStack;

namespace PortMapSleuth {
    [Route("/request", "POST")]
    public class PortTestRequest : IReturn<PortTestRequest> {
        public List<int> Ports { get; set; }
        public IPProtocol IPProtocol { get; set; }
    }

    public enum IPProtocol {
        TCP, UDP
    }

    public enum PortTestResult {
        /// <summary>
        /// We could not contact the port test server or it otherwise failed.
        /// </summary>
        Unknown,
        /// <summary>
        /// None of the ports in the test succeeded.
        /// </summary>
        Fail,
        /// <summary>
        /// Some of the ports in the test succeeded.
        /// </summary>
        PartialSuccess,
        /// <summary>
        /// All ports in the test succeeded.
        /// </summary>
        FullSuccess
    }

    public class PortTestService : Service {
        public object Post(PortTestRequest portTest) {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Got port test request from:");
            Console.WriteLine("IP: " + Request.RemoteIp);
            Console.WriteLine("IP Protocol: " + portTest.IPProtocol);
            Console.WriteLine("Ports: ");
            foreach (var port in portTest.Ports) {
                Console.WriteLine(port); 
            }

            var result = TestUDPPorts(portTest);
            Console.WriteLine("Test result: " + result.Headers["PortTestResult"]);

            return result;
        }

        /// <summary>
        /// Tests all the ports as requested, TCP or UDP.
        /// </summary>
        /// <param name="portTestRequest"></param>
        /// <returns></returns>
        private HttpResult TestUDPPorts(PortTestRequest portTestRequest) {
            List<bool> results = new List<bool>();

            // Test all ports requested
            foreach (int port in portTestRequest.Ports) {
                bool testResult;
                if (portTestRequest.IPProtocol == IPProtocol.UDP)
                    testResult = TestUDPPort(Request.RemoteIp, port);
                else
                    testResult = TestTCPPort(Request.RemoteIp, port);

                results.Add(testResult);
            }

            var httpResult = new HttpResult(HttpStatusCode.OK);
            if (results.Contains(false) && results.Contains(true)) {
                httpResult.Headers.Add("PortTestResult", PortTestResult.PartialSuccess.ToString());
            } else if (results.Contains(true)) {
                httpResult.Headers.Add("PortTestResult", PortTestResult.FullSuccess.ToString());
            } else {
                httpResult.Headers.Add("PortTestResult", PortTestResult.Fail.ToString());
            }

            return httpResult;
        }

        private bool TestUDPPort(string ipAddress, int port) {
            try {
                using (UdpClient udpClient = new UdpClient()) {
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    IPEndPoint remoteIpEndPointReply = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Port test.");

                    udpClient.Client.ReceiveTimeout = 1000; // milliseconds
                    udpClient.Send(sendBytes, sendBytes.Length, remoteIpEndPoint);
                    udpClient.Receive(ref remoteIpEndPointReply);

                    return true;
                }
            } catch (SocketException e) {
                // Error codes: http://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
                switch (e.ErrorCode) {
                    case 10060:
                        // Connection timed out.
                        Console.WriteLine(e.ErrorCode + " WSAETIMEDOUT");
                        break;
                    default:
                        Console.WriteLine(e.ErrorCode + e.Message);
                        break;
                }

                return false;
            }
        }

        private bool TestTCPPort(string ipAddress, int port) {
            // TODO
            return false;
        }
    }
}

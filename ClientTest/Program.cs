using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ClientTest {
    internal class Program {
        private const string PortMapSleuthURL = "http://pms.subterraneangames.com:1337/requests";
        //private const string PortMapSleuthURL = "http://localhost:1337/requests";

        private static void Main() {
            var portMapRequest = new PortTestRequest {
                Ports = new int[] {27015, 27016},
                PortProtocol = IPProtocol.UDP,
                IPAddress = "124.168.105.223"
                //IPAddress = "127.0.0.1"
            };
            string portMapRequestJSON = JsonConvert.SerializeObject(portMapRequest);

            // Create the thread objects:
            Thread portListener1 = new Thread(() => StartUDPListener(27015));
            Thread portListener2 = new Thread(() => StartUDPListener(27016));

            // Start the threads:
            portListener1.Start();
            portListener2.Start();

            StartWebRequest(portMapRequestJSON);

            // Join the threads:
            portListener1.Join();
            portListener2.Join();

            Console.WriteLine("Finished.");
            while (true) {}
        }

        private static void StartWebRequest(string payload) {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(PortMapSleuthURL);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Proxy = null; // Setting this to null will save some time.

            // Write the payload into the request stream:
            try {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(payload);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            

            // Send the request and response callback:
            httpWebRequest.BeginGetResponse(FinishPortTestWebRequest, httpWebRequest);
        }

        private static void FinishPortTestWebRequest(IAsyncResult result) {
            try {
                var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result) as HttpWebResponse;

                Console.WriteLine(response.StatusCode);
                Console.WriteLine(response.Headers.Get("PortTestResult"));
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private static void StartUDPListener(int port) {
            UdpClient listener = new UdpClient(port);
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try {
                while (true) {
                    Console.WriteLine("Waiting for broadcast");

                    byte[] bytes = listener.Receive(ref remoteIPEndPoint);

                    Console.WriteLine("Received packet from {0}\n",
                        remoteIPEndPoint);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes));

                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Port test reply.");
                    listener.Send(sendBytes, sendBytes.Length, remoteIPEndPoint);

                    return;
                }
            
            } catch (SocketException e) {
                // Error codes: http://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
                switch (e.ErrorCode) {
                    case 10060:
                        // Connection timed out.
                        Console.WriteLine(e.ErrorCode + " WSAETIMEDOUT");
                        break;
                }
                Console.WriteLine(e.ToString());
            } finally {
                listener.Close();
            }
        }
    }
}
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
            };
            string portMapRequestJSON = JsonConvert.SerializeObject(portMapRequest);

            // Create the thread object, passing in the Alpha.Beta method
            // via a ThreadStart delegate. This does not start the thread.
            Thread oThread = new Thread(StartListener);

            // Start the thread
            oThread.Start();

            Thread.Sleep(5000);

            StartWebRequest(portMapRequestJSON);

            // Request that oThread be stopped
            //oThread.Abort();

            // Wait until oThread finishes. Join also has overloads
            // that take a millisecond interval or a TimeSpan object.
            oThread.Join();

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

        private static void StartListener() {
            bool done = false;

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("178.62.47.176"), 27015);
            UdpClient listener = new UdpClient(27015);
            

            try {
                while (!done) {
                    Console.WriteLine("Waiting for broadcast");
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] bytes = listener.Receive(ref RemoteIpEndPoint);

                    //if (bytes.Length == 0)
                    //    continue;

                    //Console.WriteLine("Received broadcast from {0} :\n {1}\n",
                    //    groupEP.ToString(),
                    //    Encoding.ASCII.GetString(bytes,0,bytes.Length));
                    Console.WriteLine("Received broadcast from {0} :\n",
                        groupEP.ToString());
                    Console.WriteLine(bytes.ToString());

                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Port test reply.");
                    listener.Send(sendBytes, sendBytes.Length, groupEP);

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
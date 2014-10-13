using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace PortMapSleuth {
    public class PortTestClient {
        public string PortMapSleuthURL;
        public PortTestResult PortTestResult;
        public delegate void PortTestFinishedEventHandler(PortTestFinishedEventArgs e);
        public event PortTestFinishedEventHandler PortTestFinished;
        public bool IsPortTestFinished;
        public List<UdpClient> UdpClientListeners;

        public PortTestClient(string portMapSleuthURL) {
            PortMapSleuthURL = portMapSleuthURL;
        }

        public PortTestClient(string portMapSleuthURL, PortTestFinishedEventHandler portTestFinishedEventHandler) {
            PortMapSleuthURL = portMapSleuthURL;
            PortTestFinished += portTestFinishedEventHandler;
        }

        /// <summary>
        /// Starts port listeners and sends a request to the PortMapSleuth server for testing.
        /// </summary>
        /// <param name="ipProtocol"></param>
        /// <param name="ports"></param>
        /// <returns>Returns successful if all ports succeeded.</returns>
        public void TestPorts(IPProtocol ipProtocol, List<int> ports) {
            var portMapRequest = new PortTestRequest {
                Ports = ports,
                IPProtocol = ipProtocol
            };
            string portMapRequestJSON = JsonConvert.SerializeObject(portMapRequest);

            UdpClientListeners = new List<UdpClient>();

            try {
                // Create the udp listeners:
                foreach (var port in ports) {
                    UdpClientListeners.Add(StartUDPListener(port));
                }

                // Send out the port test request:
                StartWebRequest(portMapRequestJSON);
            } catch(Exception e) {
                // Error codes: http://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
                Console.WriteLine(e.ToString());

                PortTestException();
            }
        }

        private void PortTestException() {
            PortTestResult = PortTestResult.Unknown;
            IsPortTestFinished = true;
            if(PortTestFinished != null)
                PortTestFinished( new PortTestFinishedEventArgs(PortTestResult));
        }

        private void StartWebRequest(string payload) {
            try {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(PortMapSleuthURL);
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Proxy = null; // Setting this to null will save some time.

                // Write the payload into the request stream:
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(payload);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                // Send the request and response callback:
                httpWebRequest.BeginGetResponse(FinishPortTestWebRequest, httpWebRequest);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void FinishPortTestWebRequest(IAsyncResult result) {
            try {
                var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result) as HttpWebResponse;

                PortTestResult = (PortTestResult)Enum.Parse(typeof(PortTestResult), response.Headers.Get("PortTestResult"));

                // Send the result to our subscribers:
                IsPortTestFinished = true;
                if (PortTestFinished != null)
                    PortTestFinished(new PortTestFinishedEventArgs(PortTestResult));

            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private UdpClient StartUDPListener(int port) {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Start the listener and start an async wait for a packet:
            UdpClient listener = new UdpClient(port) {
                Client = { ReceiveTimeout = 5000 }
            };

            listener.BeginReceive(ReceiveCallback,
                new UdpState {IPEndPoint = remoteIPEndPoint, UdpClient = listener});

            return listener;
        }

        public static void ReceiveCallback(IAsyncResult ar) {
            UdpClient udpClient = ((UdpState)(ar.AsyncState)).UdpClient;
            IPEndPoint ipEndPoint = ((UdpState)(ar.AsyncState)).IPEndPoint;

            Byte[] receiveBytes = udpClient.EndReceive(ar, ref ipEndPoint);
            //string receiveString = Encoding.ASCII.GetString(receiveBytes);

            Console.WriteLine("Received packet from {0}\n", ipEndPoint);

            Byte[] sendBytes = Encoding.ASCII.GetBytes("Port test reply.");
            udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
            udpClient.Close();
        }
    }

    public class PortTestFinishedEventArgs : EventArgs {
        public PortTestResult PortTestResult { get; internal set; }
        public PortTestFinishedEventArgs(PortTestResult portTestResult) {
            PortTestResult = portTestResult;
        }
    }

    public class UdpState {
        public UdpClient UdpClient;
        public IPEndPoint IPEndPoint;
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

    public class PortTestRequest {
        public List<int> Ports { get; set; }
        public IPProtocol IPProtocol { get; set; }
    }

    public enum IPProtocol {
        TCP,
        UDP
    }
}

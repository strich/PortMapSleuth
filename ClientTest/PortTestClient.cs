﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientTest;
using Newtonsoft.Json;

namespace PortMapSleuth {
    public class PortTestClient {
        public const string PortMapSleuthURL = "http://pms.subterraneangames.com/request";
        //public const string PortMapSleuthURL = "http://127.0.0.1/request";
        private PortTestResult _portTestResult;
        public delegate void PortTestFinishedEventHandler(PortTestFinishedEventArgs e);
        public event PortTestFinishedEventHandler PortTestFinished;

        public PortTestClient(PortTestFinishedEventHandler portTestFinishedEventHandler) {
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
            List<Thread> listenerThreads = new List<Thread>(); 

            // Create the thread objects:
            foreach (var port in ports) {
                listenerThreads.Add(new Thread(() => StartUDPListener(port)));
            }

            // Start the threads:
            foreach (var listenerThread in listenerThreads) {
                listenerThread.Start();
            }

            // Send out the port test request:
            StartWebRequest(portMapRequestJSON);

            // Join the threads:
            foreach (var listenerThread in listenerThreads) {
                listenerThread.Join();
            }
        }

        private void StartWebRequest(string payload) {
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
            try {
                httpWebRequest.BeginGetResponse(FinishPortTestWebRequest, httpWebRequest);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void FinishPortTestWebRequest(IAsyncResult result) {
            try {
                var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result) as HttpWebResponse;

                _portTestResult = (PortTestResult)Enum.Parse(typeof(PortTestResult), response.Headers.Get("PortTestResult"));
                // Send the result to our subscribers:
                PortTestFinished(new PortTestFinishedEventArgs(_portTestResult));
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void StartUDPListener(int port) {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Loop until a reply is received of the ReceiveTimeout has elasped:
            try {
                UdpClient listener = new UdpClient(port) {
                    Client = { ReceiveTimeout = 5000 }
                };

                while (true) {
                    byte[] bytes = listener.Receive(ref remoteIPEndPoint);

                    Console.WriteLine("Received packet from {0}\n", remoteIPEndPoint);

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
            }
        }
    }

    public class PortTestFinishedEventArgs : EventArgs {
        public PortTestResult PortTestResult { get; internal set; }
        public PortTestFinishedEventArgs(PortTestResult portTestResult) {
            PortTestResult = portTestResult;
        }
    }
}

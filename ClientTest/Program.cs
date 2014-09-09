using System;
using System.IO;
using System.Net;
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

            StartWebRequest(portMapRequestJSON);

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
    }
}
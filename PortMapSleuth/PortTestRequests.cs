using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServiceStack;

namespace PortMapSleuth {
    [Route("/requests", "POST")]
    [Route("/requests/{Id}", "PUT")]
    public class PortTestRequest : IReturn<PortTestRequest> {
        public int[] Ports { get; set; }
        public IPProtocol PortProtocol { get; set; }
        public bool Done { get; set; }
        public long Id { get; set; }
        public string IPAddress { get; set; }
    }

    [Route("/requests")]
    [Route("/requests/{Id}")]
    public class PortTestRequests : IReturn<PortTestRequest> {
        public long[] Ids { get; set; }
        public PortTestRequests(params long[] ids) {
            Ids = ids;
        }
    }

    public enum IPProtocol {
        TCP, UDP
    }

    public enum PortTestResult {
        Fail,
        PartialSuccess,
        FullSuccess
    }

    public class PortTestService : Service {
        public object Post(PortTestRequest portTest) {
            Console.WriteLine("Got port test request:");
            Console.WriteLine("IP (ServiceStack): " + Request.RemoteIp);
            Console.WriteLine("IP: " + portTest.IPAddress);
            Console.WriteLine("Ports IP Protocol: " + portTest.PortProtocol);
            Console.WriteLine("Ports: ");
            foreach (var port in portTest.Ports) {
                Console.WriteLine(port); 
            }
            Console.WriteLine("------------------------------------");
            
            return TestUDPPorts(portTest);
        }

        private HttpResult TestUDPPorts(PortTestRequest portTestRequest) {
            List<bool> results = new List<bool>();

            foreach (int port in portTestRequest.Ports) {
                results.Add(TestUDPPort(Request.RemoteIp, port));
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
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    IPEndPoint RemoteIpEndPointReply = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Port test.");

                    udpClient.Client.ReceiveTimeout = 2000; // milliseconds
                    udpClient.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                    udpClient.Receive(ref RemoteIpEndPointReply);

                    return true;
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

                return false;
            }
        }
    }

    //public class PortTestService : Service {
    //    public PortTestRepository Repository { get; set; }

    //    public object Get(PortTestRequests request) {
    //        return request.Ids.IsEmpty()
    //            ? Repository.GetAll()
    //            : Repository.GetByIds(request.Ids);
    //    }

    //    public object Post(PortTestRequest portTest) {
    //        var httpResult = new HttpResult(HttpStatusCode.OK);
    //        httpResult.Headers.Add("PortTestResult", PortTestResult.FullSuccess.ToString());
    //        return httpResult;
    //    }

    //    public object Put(PortTestRequest portTest) {
    //        return Repository.Store(portTest);
    //    }

    //    public void Delete(PortTestRequests request) {
    //        Repository.DeleteByIds(request.Ids);
    //    }
    //}

    //public class PortTestRepository {
    //    private readonly List<PortTestRequest> _portTests = new List<PortTestRequest>();

    //    public List<PortTestRequest> GetByIds(long[] ids) {
    //        return _portTests.Where(x => ids.Contains(x.Id)).ToList();
    //    }

    //    public List<PortTestRequest> GetAll() {
    //        return _portTests;
    //    }

    //    public PortTestRequest Store(PortTestRequest portTest) {
    //        var existing = _portTests.FirstOrDefault(x => x.Id == portTest.Id);
    //        if (existing == null) {
    //            var newId = _portTests.Count > 0 ? _portTests.Max(x => x.Id) + 1 : 1;
    //            portTest.Id = newId;
    //        }
    //        _portTests.Add(portTest);
    //        return portTest;
    //    }

    //    public void DeleteByIds(params long[] ids) {
    //        _portTests.RemoveAll(x => ids.Contains(x.Id));
    //    }
    //}
}

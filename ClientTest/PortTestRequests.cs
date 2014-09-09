namespace ClientTest {
    public class PortTestRequest {
        public Port[] Ports { get; set; }
        public bool Done { get; set; }
        public long Id { get; set; }
        public string IPAddress { get; set; }
    }

    public class Port {
        public IPProtocol Protocol;
        public int PortNumber;
    }

    public enum IPProtocol {
        TCP,
        UDP
    }

    public enum PortTestResult {
        Fail,
        PartialSuccess,
        FullSuccess
    }
}
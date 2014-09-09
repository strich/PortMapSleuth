namespace ClientTest {
    public class PortTestRequest {
        public int[] Ports { get; set; }
        public IPProtocol PortProtocol { get; set; }
        public bool Done { get; set; }
        public long Id { get; set; }
        public string IPAddress { get; set; }
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
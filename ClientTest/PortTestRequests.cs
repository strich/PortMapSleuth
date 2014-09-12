using System.Collections.Generic;

namespace ClientTest {
    public class PortTestRequest {
        public List<int> Ports { get; set; }
        public IPProtocol IPProtocol { get; set; }
    }

    public enum IPProtocol {
        TCP,
        UDP
    }

    public enum PortTestResult {
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
}
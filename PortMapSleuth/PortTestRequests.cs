using System.Collections.Generic;
using System.Linq;
using System.Net;
using ServiceStack;

namespace PortMapSleuth {
    [Route("/requests", "POST")]
    [Route("/requests/{Id}", "PUT")]
    public class PortTestRequest : IReturn<PortTestRequest> {
        public Port[] Ports { get; set; }
        public bool Done { get; set; }
        public long Id { get; set; }
    }

    [Route("/requests")]
    [Route("/requests/{Id}")]
    public class PortTestRequests : IReturn<PortTestRequest> {
        public long[] Ids { get; set; }
        public PortTestRequests(params long[] ids) {
            Ids = ids;
        }
    }

    public struct Port {
        public IPProtocol Protocol;
        public int PortNumber;
    }

    public enum IPProtocol {
        TCP, UDP
    }

    public class PortTestService : Service {
        public PortTestRepository Repository { get; set; }

        public object Get(PortTestRequests request) {
            return request.Ids.IsEmpty()
                ? Repository.GetAll()
                : Repository.GetByIds(request.Ids);
        }

        public object Post(PortTestRequest portTest) {
            //return Repository.Store(portTest);

            //base.Response.StatusCode = (int)HttpStatusCode.Redirect;
            //base.Response.AddHeader("Location", "http://path/to/new/uri");
            //base.Response.StatusDescription = "Computer says no";

            return new HttpResult(HttpStatusCode.OK) {
                StatusDescription = "Computer says no",
            };
        }

        public object Put(PortTestRequest portTest) {
            return Repository.Store(portTest);
        }

        public void Delete(PortTestRequests request) {
            Repository.DeleteByIds(request.Ids);
        }
    }

    public class PortTestRepository {
        private readonly List<PortTestRequest> _portTests = new List<PortTestRequest>();

        public List<PortTestRequest> GetByIds(long[] ids) {
            return _portTests.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<PortTestRequest> GetAll() {
            return _portTests;
        }

        public PortTestRequest Store(PortTestRequest portTest) {
            var existing = _portTests.FirstOrDefault(x => x.Id == portTest.Id);
            if (existing == null) {
                var newId = _portTests.Count > 0 ? _portTests.Max(x => x.Id) + 1 : 1;
                portTest.Id = newId;
            }
            _portTests.Add(portTest);
            return portTest;
        }

        public void DeleteByIds(params long[] ids) {
            _portTests.RemoveAll(x => ids.Contains(x.Id));
        }
    }
}

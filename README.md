PortMapSleuth
=============

Server-based JSON service and example client to test for open ports to help verify UPnP port maps.

Built using ServiceStack and Newtonsoft.JSON.


#The Server
The server service is written with Linux in mind and so the Main() function has some Unix-specific code to support service daemons. However it is simple to remove and setup for Windows-based hosted.

The server will listen on a well known port (80 by default) for the JSON service. When it accepts a port test request it will send a packet to the destination and wait a short amount of time for a reply. The result will be posted back to the client via the JSON service.

#The Client

The provided client example implements a .NET 3.5 friendly class for listening to the ports to test, sending the port test request, and finally returning the success of the test.

Implementing the client class is quite simple:

```csharp
var pms = new PortMapSleuth.PortTestClient();
var portsOpen = pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });
Console.WriteLine(portsOpen);
```

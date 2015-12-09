PortMapSleuth
=============

![Alt text](/PortMapSleuth - Port Test Failed.gif?raw=true "PortMapSleuth - Port Test Failed")

Server-based JSON service and example client to test for open ports to help verify UPnP port maps.

See an example of a video game running the port test as part of its multiplayer service [here](https://www.youtube.com/watch?v=DTaZ04NHE1g).

Built using ServiceStack and Newtonsoft.JSON.


#The Server
The server service supports Windows and Linux (supports service daemons).

The server will listen on a well known port (8080 by default) for the JSON service. When it accepts a port test request it will send a packet to the destination and wait a short amount of time for a reply. The result will be posted back to the client via the JSON service.

To install the service on a Linux host simply copy the application and associated dll's to /opt/ and copy the UpStart config file to /etc/init/.
You can run the service using 'start PortMapSleuth' and 'stop PortMapSleuth'.

#The Client

The provided client example implements a .NET 3.5 friendly class for listening to the ports to test, sending the port test request, and finally returning the success of the test.

Implementing the client class is quite simple:

```csharp
var pms = new PortMapSleuth.PortTestClient();
var portsOpen = pms.TestPorts(IPProtocol.UDP, new List<int> { 27015, 27016 });
Console.WriteLine(portsOpen);
```
This is a synchronous example and will block the thread until it is complete. See the example client code for an asynchronous method.

using System;
using System.IO.Ports;
using static tests.Testmodel_sut;

using System.Net.NetworkInformation;
using System.Text;

/* var properties = IPGlobalProperties.GetIPGlobalProperties();
var stats = NetworkInterfaceComponent.IPv4 switch{
    NetworkInterfaceComponent.IPv4 => properties.GetTcpIPv4Statistics(),
        _ => properties.GetTcpIPv6Statistics()
};

Console.WriteLine($"TCP/{NetworkInterfaceComponent.IPv4} Statistics");
Console.WriteLine($"  Minimum Transmission Timeout : {stats.MinimumTransmissionTimeout:#,#}");
Console.WriteLine($"  Maximum Transmission Timeout : {stats.MaximumTransmissionTimeout:#,#}");
Console.WriteLine("  Connection Data");
Console.WriteLine($"      Current :                  {stats.CurrentConnections:#,#}");
Console.WriteLine($"      Cumulative :               {stats.CumulativeConnections:#,#}");
Console.WriteLine($"      Initiated  :               {stats.ConnectionsInitiated:#,#}");
Console.WriteLine($"      Accepted :                 {stats.ConnectionsAccepted:#,#}");
Console.WriteLine($"      Failed Attempts :          {stats.FailedConnectionAttempts:#,#}");
Console.WriteLine($"      Reset :                    {stats.ResetConnections:#,#}");
Console.WriteLine("  Segment Data");
Console.WriteLine($"      Received :                 {stats.SegmentsReceived:#,#}");
Console.WriteLine($"      Sent :                     {stats.SegmentsSent:#,#}");
Console.WriteLine($"      Retransmitted :            {stats.SegmentsResent:#,#}");
Console.WriteLine();

Ping pingSender = new Ping ();
PingOptions options = new PingOptions ();
options.DontFragment = true;

// Create a buffer of 32 bytes of data to be transmitted.
string target = "8.8.8.8";
string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
byte[] buffer = Encoding.ASCII.GetBytes (data);
int timeout = 120;
PingReply reply = pingSender.Send (target, timeout, buffer, options);
if (reply.Status == IPStatus.Success){
    Console.WriteLine ("Address: {0}", reply.Address.ToString ());
    Console.WriteLine ("RoundTrip time: {0}", reply.RoundtripTime);
    Console.WriteLine ("Time to live: {0}", reply.Options.Ttl);
    Console.WriteLine ("Don't fragment: {0}", reply.Options.DontFragment);
    Console.WriteLine ("Buffer size: {0}", reply.Buffer.Length);
} */
tests.Testmodel_sut test = new tests.Testmodel_sut();
test.createSerialPort("COM4", 9600);
test.serialTest();
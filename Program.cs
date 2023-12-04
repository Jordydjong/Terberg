using Gtk;
using System;

class Hello
{
    static void Main ()
    {
        Application.Init ();

        Window window = new Window ("Hello Mono World");
        window.Show ();

        Application.Run ();
    }
}
/* 
using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic; 
using System.Net.NetworkInformation;

using static tests.Testmodel_tester;
using static storage.Resultstorage;

tests.Testmodel_tester test = new tests.Testmodel_tester();
//test.createSerialPort("/dev/ttyUSB0", 9600);
//bool serial_result = test.serialTest();
//Console.WriteLine(serial_result.ToString());

//int[] own_pins = new int[]{17, 27, 22};
//int[] sut_pins = new int[]{5, 6, 13};
//test.createGpioController();
//bool[] gpio_result_list = test.digitalGpioTest(sut_pins, own_pins);
//Console.WriteLine(gpio_result_list[0].ToString());
List<bool> ethernet_test = await test.test_ethernet_port();
Console.WriteLine(ethernet_test[0]);
Console.WriteLine(ethernet_test[1]);


string real_url = $"https://hutcm.wkong.nl/report";
string test_url = $"https://httpbin.org/post";

storage.Resultstorage report = new storage.Resultstorage(test_url, 121);
//if (serial_result){
//    report.add_testdata("Serial port test", "Test of the serial port succeeded", true);
//}else{
//    report.add_testdata("Serial port test", "Test of the serial port failed", false);
//}

//if (gpio_result_list[0]){
//    report.add_testdata("GPIO pin test", "Test of the GPIO pin succeeded", true);
//}else{
//    report.add_testdata("GPIO pin test", "Test of the GPIO pin failed", false);
//}
if (ethernet_test[0]){
    report.add_testdata("Ethernet port test ip", "SUT has an IP address", true);
}else{
    report.add_testdata("Ethernet port test ip", "SUT doesn't have an IP address", false);
}

if (ethernet_test[1]){
    report.add_testdata("Ethernet port test ping", "SUT can ping target ip", true);
}else{
    report.add_testdata("Ethernet port test ping", "SUT can't ping target ip, timeout exception", false);
}

bool result = await report.send_testreport();
Console.WriteLine(result.ToString());
 */
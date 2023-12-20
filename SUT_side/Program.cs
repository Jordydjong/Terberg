using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic; 
using System.Net.NetworkInformation;

using static tests.Testmodel_sut;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

string wait_for_message(SerialPort serialPort){
    while (true){
        try{
            string message = serialPort.ReadLine();
            return message;
        }catch (TimeoutException) { }
    }
}

tests.Testmodel_sut test = new tests.Testmodel_sut();
SerialPort serialPort = test.createSerialPort("/dev/ttyUSB0", 9600);
test.serialTest();
string message = wait_for_message(serialPort);
if (message == "Start ethernet test"){
    List<bool> ethernet_test = await test.test_ethernet_port();
    serialPort.WriteLine("Result ethernet test;"+ethernet_test[0].ToString()+";"+ethernet_test[1].ToString());
}
int[] SUTpins = new int[]{18, 24};  

test.createGpioController();

foreach(int j in SUTpins){
    test.PulseWaveTest(j);
}

serialPort.Close();
 

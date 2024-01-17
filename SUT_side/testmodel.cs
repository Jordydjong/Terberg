using System.Device.Gpio;
using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Ports;
using System.Device.Gpio;
using System.Threading;
using System.Linq;
using System.Net; 
using System.Net.NetworkInformation;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Net.Http;
using System.Net.Cache;

//deze is voor SUT
namespace tests{
    /// <summary>
    /// Class written for the Subject under test side of our test system
    /// </summary>
    class Testmodel_sut{
        SerialPort serialPort;
        GpioController controller;
        HttpClient client;

        public Testmodel_sut(int timeout = 5){
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);
        }
        
        /// <summary>
        /// Function for the serial port's creation and setup
        /// </summary>
        /// <param name="port">String that refers to the serial port you want to use</param>
        /// <param name="baudrate">Int for the baudrate</param>
        public SerialPort createSerialPort(string port, int baudrate){
            serialPort = new SerialPort(port, baudrate);
            serialPort.ReadTimeout = 3000;
            serialPort.WriteTimeout = 2000;
            serialPort.RtsEnable = true;
            serialPort.DtrEnable = true;
            serialPort.Open();
            return serialPort;
        }
        /// <summary>
        ///Function to create GpioController object used for communicating with GPIO. 
        /// </summary>
        public void createGpioController(){
            controller = new GpioController();
        }


        /// <summary>
        /// Function to test the serial connection.
        /// It performs a handshake with the SUT. If it fails max_attempts times then it returns false. If it succeeds then it returns true
        /// </summary>
        public void serialTest(){
            while (true){
                try{
                    string message = serialPort.ReadLine();
                    Console.WriteLine(message);
                    if (message == "Hello there"){
                        serialPort.Write("General Kenobi\n");
                        message = serialPort.ReadLine();
                        Console.WriteLine(message);
                        if (message == "Greeting complete?"){
                            for (int i = 0; i < 3; i++){ //repeat 3 times because sometimes it doesn't catch the first time
                                serialPort.Write("Greeting complete!\n");
                            }
                            break;
                        }
                    }
                }catch (TimeoutException) { }
            }
            
        }

        /// <summary>
        /// Private function to check if the device has an IP address
        /// </summary>
        /// <returns>Boolean true if the device has an IP address, else it returns false</returns>
        private bool ip_test(){
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName()+ "." + domain;
            Console.WriteLine(domain);
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString(); 
            if (myIP != ""){
                return true;
            }else{
                return false;
            }
        }
        /// <summary>
        /// Function to test the connection to the World Wide Web. It sends a request to an URL and
        /// if it gets something back then it returns true, else it returns false
        /// </summary>
        /// <param name="target_url">String that contains the URL of the webpage you want to check</param>
        /// <returns>Boolean true if the test succeeded. False if it failed</returns>
        private async Task<bool> www_test(string target_url){
            try{
                // Above three lines can be replaced with new helper method below
                string responseBody = await client.GetStringAsync(target_url);

                if (responseBody != ""){
                    return true;
                }else{
                    return false;
                }
            }catch (HttpRequestException e){
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Function that tests the ethernet port. It uses the private functions ip_test() and www_test()
        /// 
        /// </summary>
        /// <param name="ping_target">String of the URL of the webpage you want the device to be able to reach</param>
        /// <returns>Returns a list of booleans for each test one. The type is Task<List<bool>> because the function is asynchronous"<</returns>
        public async Task<List<bool>> test_ethernet_port(string ping_target= "https://google.com"){
            List<bool> results = new List<bool>();
            bool ip = ip_test();
            results.Add(ip);
            
            bool ping = await www_test(ping_target);
            results.Add(ping);

            return results;
        }
        /// <summary>
        /// receives a digital pulsewave from the testsystem. also sends if the test was succesful via serial port to the testsystem.
        /// </summary>
        /// <param name="readpin">list of pins needed to be tested.</param>
        public async void PulseWaveTest(int readpin){
                Stopwatch stopwatch = new Stopwatch();
                PinValue[] result = new PinValue[]{};
                controller.OpenPin(readpin, PinMode.Input);
                bool receiving = true;
                PinValue last_value = controller.Read(readpin);
                while (receiving == true){
                    PinValue read_val = controller.Read(readpin);
                    if (read_val != last_value){
                        stopwatch.Stop();
                        stopwatch.Reset();
                        result = result.Append<PinValue>(read_val).ToArray();
                        last_value = read_val;
                        stopwatch.Start();
                    }
                    if (stopwatch.Elapsed.TotalSeconds > 3){
                            Console.WriteLine("3 seconden verstreken");
                            int counthigh = 0;
                            int countlow = 0;
                            foreach (PinValue j in result){
                                if (j == PinValue.High){
                                    // Console.WriteLine("HIGH");
                                    counthigh++;
                                }
                                else if (j == PinValue.Low){
                                    // Console.WriteLine("LOW");
                                    countlow++;
                                }
                            }
                            if (counthigh == 1000 && countlow == 1000){
                                Console.WriteLine("true gestuurd");
                                serialPort.WriteLine("true");
                                serialPort.Write("Done sending.\n");
                                // Console.WriteLine("done 1 gestuurd");
                                receiving = false;
                            }
                            else{
                                Console.WriteLine("false gestuurd");
                                serialPort.WriteLine("false");
                                serialPort.Write("Done sending.\n");
                                // Console.WriteLine("done 0 gestuurd");
                                Console.WriteLine(result.Length);
                                receiving = false;
                            } 
                        }
                }
            serialPort.DiscardOutBuffer();
            stopwatch.Reset();
        }
    }
};


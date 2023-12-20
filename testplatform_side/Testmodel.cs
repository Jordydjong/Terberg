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
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using static storage.Resultstorage;
//deze is voor test module
namespace tests{
    /// <summary>
    /// A Testmodel.
    /// </summary>
    class Testmodel_tester{
        SerialPort serialPort;
        GpioController controller;
        HttpClient client;
        Stopwatch stopWatch;
        long elapsed_time;
        storage.Resultstorage report;
        string exception;
            
        public Testmodel_tester(storage.Resultstorage report_input, int timeout = 5){
            report = report_input;
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);
            stopWatch = new Stopwatch();
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
        /// Function to test the serial connection.
        /// It performs a handshake with the SUT. If it fails max_attempts times then it returns false. If it succeeds then it returns true
        /// </summary>
        /// <param name="max_attempts">Number of attempts to connect before the function returns false</param>
        /// <returns>Bool true if test succeeds, false if failed</returns>
        public bool serialTest(int max_attempts = 5){
            stopWatch.Start();
            int number_attempts = 0;
            while (number_attempts < max_attempts){
                number_attempts += 1;
                serialPort.Write("Hello there\n");
                string message;
                try{
                    message = serialPort.ReadLine();
                    Console.WriteLine(message);
                    if (message == "General Kenobi"){
                        serialPort.Write("Greeting complete?\n");
                        message = serialPort.ReadLine();
                        Console.WriteLine(message);
                        if (message == "Greeting complete!"){
                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
                            Console.WriteLine(elapsed_time.ToString());
                            stopWatch.Reset();
                            report.add_testdata("Serial port test", "Test of the serial port succeeded", true, elapsed_time);
                            return true;
                        }
                    }
                }catch (Exception e) {
                    exception = e.ToString();
                }
            }
            stopWatch.Stop();
            elapsed_time = stopWatch.ElapsedMilliseconds;
            Console.WriteLine(elapsed_time.ToString());
            stopWatch.Reset();
            report.add_testdata("Serial port test", exception, false, elapsed_time);
            return false;
        }

        private string wait_for_message(SerialPort serialPort){
            while (true){
                try{
                    string message = serialPort.ReadLine();
                    return message;
                }catch (TimeoutException) { }
            }
        }   

        /// <summary>
        /// Function that tests the ethernet port. It uses the private functions ip_test() and www_test()
        /// 
        /// </summary>
        /// <param name="ping_target">String of the URL of the webpage you want the device to be able to reach</param>
        /// <returns>Returns a list of booleans for each test one. The type is Task<List<bool>> because the function is asynchronous"<</returns>
        public List<bool> test_ethernet_port(string ping_target= "https://google.com"){
            serialPort.WriteLine("Start ethernet test");
            stopWatch.Start();
            string message = wait_for_message(serialPort);
            while (!message.Contains("ethernet")){
                message = wait_for_message(serialPort);
                Console.WriteLine(message);
            };

            stopWatch.Stop();
            elapsed_time = stopWatch.ElapsedMilliseconds;
            Console.WriteLine(elapsed_time.ToString());
            stopWatch.Reset();

            List<string> message_cut = message.Split(';').ToList();
            List<bool> ethernet_test = new List<bool>{Convert.ToBoolean(message_cut[1]), Convert.ToBoolean(message_cut[2])};
            if (ethernet_test[0]){
                report.add_testdata("Ethernet port test ip", "SUT has an IP address", true, elapsed_time);
            }else{
                report.add_testdata("Ethernet port test ip", "SUT doesn't have an IP address", false, elapsed_time);
            }

            if (ethernet_test[1]){
                report.add_testdata("Ethernet port test ping", "SUT can ping target ip", true, elapsed_time);
            }else{
                report.add_testdata("Ethernet port test ping", "SUT can't ping target ip, timeout exception", false, elapsed_time);
            }
            return ethernet_test;
        } 

        /// <summary>
        ///Function to create GpioController object used for communicating with GPIO. 
        /// </summary>
        public void createGpioController(){
            controller = new GpioController();
        }

        /// <summary>
        /// This function creates a rectangular pulse wave on a pin. and reads this pulse wave via a second pin.
        /// Then it validates if the second pin read the correct values.
        /// </summary>
        /// <param name="period">the total duration of an on and off state</param>
        /// <param name="width">the duration of the on state</param>
        /// <param name="amount_of_pulses">Amount of on- and off pulses</param>
        /// <param name="writepin">pin that is used to send pulse</param>
        /// <param name="readpin">pin that is used to receive pulse</param>
        /// <returns>bool. 1, is validation has passed. 0, validation has failed.</returns>
        async Task<string> PulseWaveControl(int period, int width, int amount_of_pulses, int writepin){
            List<PinValue> validation = new List<PinValue>();
            controller.OpenPin(writepin, PinMode.Output);
            for (int i = 0; i < amount_of_pulses; i++){
                validation.Add(PinValue.High);
                validation.Add(PinValue.Low);
            }

            for (int i = 0; i < amount_of_pulses; i++){
                controller.Write(writepin, PinValue.High);
                //Thread.Sleep(width);
                await Task.Delay(width);
                controller.Write(writepin, PinValue.Low);
                //Thread.Sleep(period - width);
                await Task.Delay(period - width);
            }
            return validation.ToString();
        }

        /// <summary>
        /// Tests Functionality of Digital GPIO pins of subject under test. 
        /// </summary>
        /// <param name="SUTpins">List of digital pins from subject under test that are to be tested.</param>
        /// <param name="Ownpins">List of digital pins from testing module that are used for validation.</param>
        /// <param name="Validation">List of pin states that are used for testing. </param>
        /// <returns>List of bools showing if pin has passed the test.</returns>
        public async Task<bool[]> digitalGpioTest(int[] Ownpins){
            string validation = null;
            bool[] Result_per_pin = new bool[]{}; 
            int number_of_fails = 0;

            for (int i = 0; i < Ownpins.Length; i++){
                stopWatch.Start();
                await PulseWaveControl(80, 40, 1000, Ownpins[i]);
                serialPort.DiscardInBuffer();
                bool receiving = true;
                while(receiving == true){
                    serialPort.Write("Done sending.\n");
                    try{
                        string message = serialPort.ReadLine();
                        Console.Write("Message: ");
                        Console.WriteLine(message);
                        
                        if (message != "Done sending."){
                            validation = message;
                        }

                        else{
                            Console.WriteLine("done");
                            Console.WriteLine(message);
                            Console.WriteLine(validation);

                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
                            Console.WriteLine(elapsed_time.ToString());
                            stopWatch.Reset();

                            if (validation == "true"){
                                Result_per_pin = Result_per_pin.Append<bool>(true).ToArray();
                                report.add_testdata("Digital GPIO pin "+Ownpins[i].ToString(), "Pin works correct", true, elapsed_time);
                            }else{
                                Result_per_pin = Result_per_pin.Append<bool>(false).ToArray();
                                report.add_testdata("Digital GPIO pin "+Ownpins[i].ToString(), "Pin does not work correct", false, elapsed_time);
                            }
                            receiving = false;
                            await Task.Delay(500);
                        }
                    }
                    catch(TimeoutException e){
                        if (number_of_fails > 3){
                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
                            Console.WriteLine(elapsed_time.ToString());
                            stopWatch.Reset();

                            report.add_testdata("Digital GPIO pin "+Ownpins[i].ToString(), "Pin does not work correct", false, elapsed_time);
                            receiving = false;
                            number_of_fails = 0;
                            Result_per_pin = Result_per_pin.Append<bool>(false).ToArray();
                        }else{
                            number_of_fails += 1;
                            Console.WriteLine("\nException Caught!");
                            Console.WriteLine("Message :{0} ", e.Message);
                        }
                    }
                }
            }
            return Result_per_pin;
        }
        
        /// <summary>
        /// Private function to check if the device has an IP address
        /// </summary>
        /// <returns>Boolean true if the device has an IP address, else it returns false</returns>
        private bool ip_test(){
            try{
                IPAddress[] ipaddress = Dns.GetHostAddresses(Dns.GetHostName() + ".medewerkers.ad.hvu.nl");  
                /* Console.WriteLine("IP Address of Machine is");  
                foreach(IPAddress ip in ipaddress)  
                {  
                    Console.WriteLine(ip.ToString());  
                }  */ 
            }catch (Exception e){
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Function to test the connection to the World Wide Web. It sends a request to an URL and
        /// if it gets something back then it returns true, else it returns false
        /// </summary>
        /// <param name="target_url">String that contains the URL of the webpage you want to check</param>
        /// <returns>Boolean true if the test succeeded. False if it failed</returns>
        private async Task<bool> www_test(string target_url){
            try{
                string responseBody = await client.GetStringAsync(target_url);

                //Console.WriteLine(responseBody);
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

    }
};



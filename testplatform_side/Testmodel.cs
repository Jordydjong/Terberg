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
using System.Threading.Tasks;
using System.Collections.Generic;

//deze is voor test module
namespace tests{
    /// <summary>
    /// A Testmodel.
    /// </summary>
    class Testmodel_tester{
        SerialPort serialPort;
        GpioController controller;
        HttpClient client;

        public Testmodel_tester(int timeout = 5){
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);
        }

        /// <summary>
        /// Private function to check if the device has an IP address
        /// </summary>
        /// <returns>Boolean true if the device has an IP address, else it returns false</returns>
        private bool ip_test(){
            string hostName = Dns.GetHostName();
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
        bool PulseWaveControl(int period, int width, int amount_of_pulses, int writepin, int readpin){
            PinValue[] result = new PinValue[]{};
            PinValue[] validation = new PinValue[]{};
            controller.OpenPin(writepin, PinMode.Output);
            controller.OpenPin(readpin, PinMode.Input);
            for (int i = 0; i < amount_of_pulses; i++){
                controller.Write(writepin, PinValue.High);
                validation.Append<PinValue>(PinValue.High).ToArray();
                System.Threading.Tasks.Task.Delay(width);
                result.Append<PinValue>(controller.Read(readpin)).ToArray();
                controller.Write(writepin, PinValue.Low);
                validation.Append<PinValue>(PinValue.Low).ToArray();
                System.Threading.Tasks.Task.Delay(period - width);
                result.Append<PinValue>(controller.Read(readpin)).ToArray();
            }
            if (Enumerable.SequenceEqual(result, validation)){
                return true;
            }
            else{
                return false;
            }
        }

        /// <summary>
        /// Tests Functionality of Digital GPIO pins of subject under test. 
        /// </summary>
        /// <param name="SUTpins">List of digital pins from subject under test that are to be tested.</param>
        /// <param name="Ownpins">List of digital pins from testing module that are used for validation.</param>
        /// <param name="Validation">List of pin states that are used for testing. </param>
        /// <returns>List of bools showing if pin has passed the test.</returns>
        public bool[] digitalGpioTest(int[] SUTpins, int[] Ownpins){
            bool[] result = new bool[]{};
            for (int i = 0; i < SUTpins.Length; i++){
                result.Append<bool>(PulseWaveControl(300, 150, 1000, SUTpins[i], Ownpins[i])).ToArray();
            }
            return result;
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
            serialPort.Open();
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
                            serialPort.Close();
                            return true;
                        }
                    }
                }catch (TimeoutException) { }
            }
            serialPort.Close();
            return false;
        }

    }
};



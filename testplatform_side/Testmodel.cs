using System;
using System.IO.Ports;
using System.Device.Gpio;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace tests
{
    /// <summary>
    /// Class that holds the tests on the testplatform side
    /// </summary>
    class Testmodel_tester{
        SerialPort serialPort;
        GpioController controller;
        HttpClient client;
        Stopwatch stopWatch;
        long elapsed_time;
        storage.Resultstorage report;
        string exception;
        
        /// <summary>
        /// Constructor function for the testmodel
        /// </summary>
        /// <param name="report_input">Reference to the result storage to store the test's results</param>
        /// <param name="timeout">Integer to set the HttpClient's timeout</param>
        public Testmodel_tester(storage.Resultstorage report_input, int timeout = 5){
            report = report_input;
            client = new HttpClient{
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            stopWatch = new Stopwatch();
        }
        
        /// <summary>
        /// Function for the serial port's creation and setup
        /// </summary>
        /// <param name="port">String that refers to the serial port you want to use</param>
        /// <param name="baudrate">Int for the baudrate</param>
        /// <returns>Returns a SerialPort reference to be used in the main program</returns>
        public SerialPort createSerialPort(string port, int baudrate){
            serialPort = new SerialPort(port, baudrate)
            {
                ReadTimeout = 3000,
                WriteTimeout = 2000,
                RtsEnable = true,
                DtrEnable = true
            };
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
                    if (message == "General Kenobi"){
                        serialPort.Write("Greeting complete?\n");
                        message = serialPort.ReadLine();
                        if (message == "Greeting complete!"){
                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
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
            stopWatch.Reset();
            report.add_testdata("Serial port test", exception, false, elapsed_time);
            return false;
        }

        /// <summary>
        /// Function that loops untill a message is received
        /// </summary>
        /// <param name="serialPort">Reference to the SerialPort</param>
        /// <returns>String with the message that has been received</returns>
        private string wait_for_message(SerialPort serialPort){
            while (true){
                try{
                    string message = serialPort.ReadLine();
                    return message;
                }catch (TimeoutException) { }
            }
        }   

        /// <summary>
        /// Function that tests the ethernet port. It sends the start signal to the SUT and then waits for a response.
        /// </summary>
        /// <returns>Returns a list of booleans with the testresults<</returns>
        public List<bool> test_ethernet_port(){
            serialPort.WriteLine("Start ethernet test");
            stopWatch.Start();
            string message = wait_for_message(serialPort);
            while (!message.Contains("ethernet")){
                message = wait_for_message(serialPort);
            };

            stopWatch.Stop();
            elapsed_time = stopWatch.ElapsedMilliseconds;
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
                await Task.Delay(width);
                controller.Write(writepin, PinValue.Low);
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
                        if (message != "Done sending."){
                            validation = message;
                        }

                        else{
                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
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
                    catch(TimeoutException){
                        if (number_of_fails > 3){
                            stopWatch.Stop();
                            elapsed_time = stopWatch.ElapsedMilliseconds;
                            stopWatch.Reset();

                            report.add_testdata("Digital GPIO pin "+Ownpins[i].ToString(), "Pin does not work correct", false, elapsed_time);
                            receiving = false;
                            number_of_fails = 0;
                            Result_per_pin = Result_per_pin.Append<bool>(false).ToArray();
                        }else{
                            number_of_fails += 1;
                        }
                    }
                }
            }
            return Result_per_pin;
        }
    }
};



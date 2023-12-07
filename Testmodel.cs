using System;
using System.IO.Ports;

//deze is voor SUT
namespace tests{
    class Testmodel_sut{
        SerialPort serialPort;
        
        /// <summary>
        /// Function for the serial port's creation and setup
        /// </summary>
        /// <param name="port">String that refers to the serial port you want to use</param>
        /// <param name="baudrate">Int for the baudrate</param>
        public void createSerialPort(string port, int baudrate){
            serialPort = new SerialPort(port, baudrate);
            serialPort.ReadTimeout = 3000;
            serialPort.WriteTimeout = 2000;
            serialPort.RtsEnable = true;
            serialPort.DtrEnable = true;
        }

        /// <summary>
        /// Function to test the serial connection.
        /// It performs a handshake with the SUT. If it fails max_attempts times then it returns false. If it succeeds then it returns true
        /// </summary>
        public void serialTest(){
            while (true){
                serialPort.Open();
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
                serialPort.Close();
            }
            serialPort.Close();
        }

    }
};



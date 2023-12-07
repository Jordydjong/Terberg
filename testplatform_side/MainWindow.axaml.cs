using System;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Interactivity;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Ports;


namespace avaloniaExtTest.Views;

using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic; 
using System.Net.NetworkInformation;

using static tests.Testmodel_tester;
using static storage.Resultstorage;
using System.Linq;

public partial class MainWindow : Window{
    bool done = false;
    List<bool> testresults = new List<bool>{};
    public MainWindow(){
        InitializeComponent();
    }

    string wait_for_message(SerialPort serialPort){
    while (true){
        try{
            string message = serialPort.ReadLine();
            return message;
        }catch (TimeoutException) { }
    }
}

    async public void ClickHandler(object sender, RoutedEventArgs args){
        Window hostWindow = (Window)VisualRoot;
        if (!done){
            TextBlock1.Text = "Testplatform bezig";
            Button1.IsEnabled = false;
            hostWindow.Background = Avalonia.Media.Brushes.Orange;

            tests.Testmodel_tester test = new tests.Testmodel_tester();
            SerialPort serialPort = test.createSerialPort("/dev/ttyUSB0", 9600);
            string url = $"https://hutcm.wkong.nl/report";
            storage.Resultstorage report = new storage.Resultstorage(url, 121);

            TextBlock_serial.Text = "Seriele test bezig";
            bool serial_result = test.serialTest();
            testresults.Add(serial_result);
            if (serial_result){
                report.add_testdata("Serial port test", "Test of the serial port succeeded", true);
            }else{
                report.add_testdata("Serial port test", "Test of the serial port failed", false);
            }
            if (serial_result){
                TextBlock_serial.Text = "Seriele test geslaagd";
                TextBlock_serial.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                TextBlock_serial.Text = "Seriele test gefaald";
                TextBlock_serial.Foreground = Avalonia.Media.Brushes.DarkRed;
            }

            
            TextBlock_ethernet.Text = "Ethernet test bezig";
            serialPort.WriteLine("Start ethernet test");
            string message = wait_for_message(serialPort);
            List<string> message_cut = message.Split(';').ToList();
            List<bool> ethernet_test = new List<bool>{Convert.ToBoolean(message_cut[1]), Convert.ToBoolean(message_cut[2])};
            testresults.Add(ethernet_test[0]);
            testresults.Add(ethernet_test[1]);
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
            if (ethernet_test[0] == ethernet_test[1] == true){
                TextBlock_ethernet.Text = "Ethernet test geslaagd";
                TextBlock_ethernet.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                TextBlock_ethernet.Text = "Ethernet test gefaald";
                TextBlock_ethernet.Foreground = Avalonia.Media.Brushes.DarkRed;
            }

            TextBlock_digitalGPIO.Text = "Digitale GPIO test bezig";
            await Task.Delay(1000);
            testresults.Add(false);
            if (testresults[3]){
                TextBlock_digitalGPIO.Text = "Digitale GPIO test geslaagd";
                TextBlock_digitalGPIO.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                TextBlock_digitalGPIO.Text = "Digitale GPIO test gefaald";
                TextBlock_digitalGPIO.Foreground = Avalonia.Media.Brushes.DarkRed;
            }

            TextBlock_analogGPIO.Text = "Analoge GPIO test bezig";
            await Task.Delay(1000);
            testresults.Add(false);
            if (testresults[4]){
                TextBlock_analogGPIO.Text = "Analoge GPIO test geslaagd";
                TextBlock_analogGPIO.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                TextBlock_analogGPIO.Text = "Analoge GPIO test gefaald";
                TextBlock_analogGPIO.Foreground = Avalonia.Media.Brushes.DarkRed;
            }

            TextBlock1.Text = "Test programma is klaar";
            if (testresults.Contains(false)){
                hostWindow.Background = Avalonia.Media.Brushes.IndianRed;
            }else{
                hostWindow.Background = Avalonia.Media.Brushes.LightGreen;
            }
            serialPort.Close();
            bool result = await report.send_testreport();

            await Task.Delay(2000);
            Button1.Content = "Klik om platform te resetten";
            Button1.IsEnabled = true;
            done = true;

        }else{
            TextBlock1.Text = "Testplatform beschikbaar";
            Button1.Content = "Klik om het testprogramma te starten";
            hostWindow.Background = Avalonia.Media.Brushes.White;
            TextBlock_analogGPIO.Text = "";
            TextBlock_analogGPIO.Foreground = Avalonia.Media.Brushes.Black;
            
            TextBlock_digitalGPIO.Text = "";
            TextBlock_digitalGPIO.Foreground = Avalonia.Media.Brushes.Black;

            TextBlock_ethernet.Text = "";
            TextBlock_ethernet.Foreground = Avalonia.Media.Brushes.Black;

            TextBlock_serial.Text = "";
            TextBlock_serial.Foreground = Avalonia.Media.Brushes.Black;
            done = false;
        }
    }
}

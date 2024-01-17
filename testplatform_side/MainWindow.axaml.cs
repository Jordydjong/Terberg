using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Ports;

using System.Linq;

namespace avaloniaExtTest.Views;

public partial class MainWindow : Window{
    bool done = false;
    List<bool> testresults = new List<bool>{};

    public MainWindow(){
        InitializeComponent();
    }

    
    /// <summary>
    /// Function 
    /// </summary>
    async public void ClickHandler(object sender, RoutedEventArgs args){
        Window hostWindow = (Window)VisualRoot;
        if (!done){
            
            TextBlock1.Text = "Testplatform bezig";
            Button1.IsEnabled = false;
            hostWindow.Background = Avalonia.Media.Brushes.Orange;
            
            string url = $"https://hutcm.wkong.nl/report";
            storage.Resultstorage report = new storage.Resultstorage(url, 121);
            tests.Testmodel_tester test = new tests.Testmodel_tester(report);
            SerialPort serialPort = test.createSerialPort("/dev/ttyUSB0", 9600);
            await Task.Delay(500);
            
            TextBlock_serial.Text = "Seriele test bezig";
            await Task.Delay(500);
            bool serial_result = test.serialTest();
            if (serial_result){
                testresults.Add(true);
                TextBlock_serial.Text = "Seriele test geslaagd";
                TextBlock_serial.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                testresults.Add(false);
                TextBlock_serial.Text = "Seriele test gefaald";
                TextBlock_serial.Foreground = Avalonia.Media.Brushes.DarkRed;
            }
            await Task.Delay(1000);
            
            TextBlock_ethernet.Text = "Ethernet test bezig";
            await Task.Delay(500);
            List<bool> ethernet_test = test.test_ethernet_port();
            if (ethernet_test[0] == ethernet_test[1] == true){
                testresults.Add(true);
                TextBlock_ethernet.Text = "Ethernet test geslaagd";
                TextBlock_ethernet.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }else{
                testresults.Add(false);
                TextBlock_ethernet.Text = "Ethernet test gefaald";
                TextBlock_ethernet.Foreground = Avalonia.Media.Brushes.DarkRed;
            } 
            await Task.Delay(1000);
            
            test.createGpioController();
            int[] own_pins = new int[]{23, 25};
            TextBlock_digitalGPIO.Text = "Digitale GPIO test bezig";
            await Task.Delay(500);
            bool[] result = await test.digitalGpioTest(own_pins);
            if (result.Contains(false)){
                testresults.Add(false);
                TextBlock_digitalGPIO.Text = "Digitale GPIO test gefaald";
                TextBlock_digitalGPIO.Foreground = Avalonia.Media.Brushes.DarkRed;
            }else{
                testresults.Add(true);
                TextBlock_digitalGPIO.Text = "Digitale GPIO test geslaagd";
                TextBlock_digitalGPIO.Foreground = Avalonia.Media.Brushes.DarkGreen;
            }
            await Task.Delay(1000);

            TextBlock1.Text = "Test programma is klaar";
            await Task.Delay(500);
            Console.WriteLine(testresults);
            for (int i = 0; i < testresults.Count; i++){
                Console.WriteLine(testresults[i]);
            }
            if (testresults.Contains(false)){
                hostWindow.Background = Avalonia.Media.Brushes.IndianRed;
            }else{
                hostWindow.Background = Avalonia.Media.Brushes.LightGreen;
            }
            serialPort.Close();
            bool result_send = await report.send_testreport();
            testresults = new List<bool>{};

            await Task.Delay(2000);
            Button1.Content = "Klik om platform te resetten";
            Button1.IsEnabled = true;
            done = true;

        }else{
            TextBlock1.Text = "Testplatform beschikbaar";
            Button1.Content = "Klik om het testprogramma te starten";
            hostWindow.Background = Avalonia.Media.Brushes.White;
            
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

using System;
using System.Device.Gpio;
using System.Threading;
using System.Linq;

namespace tests{
/// <summary>
/// A Testmodel.
/// </summary>
    class Testmodel
    {
        GpioController controller;
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
        public bool[] digitalGpioTest(int[] SUTpins, int[] Ownpins, PinValue[] Validation){
            bool[] result = new bool[]{};
            for (int i = 0; i < SUTpins.Length; i++){
                result.Append<bool>(Generatepulse(300, 150, 1000, SUTpins[i], Ownpins[i])).ToArray();
            }
            return result;
        }


    }
}
using System;
using System.Device.Gpio;
using System.Threading;
using System.Linq;

namespace tests{
/// <summary>
/// A Testmodel.
/// </summary>
    class Tests
    {
        GpioController controller;
        /// <summary>
        ///Function to create GpioController object used for communicating with GPIO. 
        /// </summary>
        public void createGpioController(){
            controller = new GpioController();
        }

        /// <summary>
        /// Tests Functionality of Digital GPIO pins of subject under test. 
        /// </summary>
        /// <param name="SUTpins">List of digital pins from subject under test that are to be tested.</param>
        /// <param name="Ownpins">List of digital pins from testing module that are used for validation.</param>
        /// <param name="Validation">List of pin states that are used for testing. </param>
        /// <returns>List of bools showing if pin has passed the test.</returns>
        public bool[] digitalGpioTest(int[] SUTpins, int[] Ownpins, PinValue[] Validation){
            bool[] finalresults = new bool[]{};
            PinValue[] Testresults = new PinValue[]{};

            for(int i = 0; i < SUTpins.Length; i++){
                controller.OpenPin(SUTpins[i], PinMode.Output);
                controller.OpenPin(Ownpins[i], PinMode.Input);
                for (int j = 0; j < Validation.Length; j++){
                controller.Write(SUTpins[i], Validation[j]);                
                Testresults = Testresults.Append<PinValue>(controller.Read(Ownpins[i])).ToArray();
                }

                if (Enumerable.SequenceEqual(Testresults, Validation)){
                    finalresults = finalresults.Append<bool>(true).ToArray();
                }
                else{
                    finalresults = finalresults.Append<bool>(false).ToArray();
                }
                Testresults = new PinValue[]{};
            }
            return finalresults;
        }
    }
}
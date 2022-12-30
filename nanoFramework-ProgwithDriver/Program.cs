using Drivers;
using System.Device.Gpio;
using System.Threading;

namespace nanoFramework_ProgwithDriver
{
    public class Program
    {
        public static NHD420cwSingleton.NHD420cwSpi _oledDisplay;


        //temporary put not used hardware CS in high state
        private static GpioController TestCsPort;
        private static int pinCs_W5500 = 5;
        private static int pinCs_BoardFlash = 16;
        private static int pinCs_23LC1024 = 4;
        private static int pinCs_25AA0E48 = 0;



        public static void Main()
        {
            //temporary put not used hardware CS in high state
            StetupCs();

            _oledDisplay = NHD420cwSingleton._oledDisplay;
            _oledDisplay.ClearDisplay();

            //_oledDisplay.OledWriteLine(0, 0, "....................");
            _oledDisplay.OledWriteLine(3, 1, "..ADCD..");
            _oledDisplay.OledWriteLine(3, 2, "--ertyuop--");
            _oledDisplay.OledWriteLine(3, 3, "Booting..");

            Thread.Sleep(Timeout.Infinite);

        }


        static void StetupCs()
        {
            TestCsPort = new GpioController();
            TestCsPort.OpenPin(pinCs_W5500, PinMode.Output);
            TestCsPort.OpenPin(pinCs_BoardFlash, PinMode.Output);
            TestCsPort.OpenPin(pinCs_23LC1024, PinMode.Output);
            TestCsPort.OpenPin(pinCs_25AA0E48, PinMode.Output);

            TestCsPort.Write(pinCs_W5500, PinValue.High);
            TestCsPort.Write(pinCs_BoardFlash, PinValue.High);
            TestCsPort.Write(pinCs_23LC1024, PinValue.High);
            TestCsPort.Write(pinCs_25AA0E48, PinValue.High);
        }

    }
}

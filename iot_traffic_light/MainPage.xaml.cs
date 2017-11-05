using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace iot_traffic_light
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GpioController _gpio;

        GpioPin _pinRed;
        GpioPin _pinYellow;
        GpioPin _pinGreen;

        DispatcherTimer _timer;

        bool _toRed;

        public MainPage()
        {
            this.InitializeComponent();
            Debug("starting iot_traffic_light app");
            this.InitializeGpio();
        }

        private void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine("iot_traffic_light " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss") + " >> " + message);
        }

        private void InitializeGpio()
        {
            Debug("initializing gpio");
            _gpio = GpioController.GetDefault();
            if (_gpio != null)
            {
                _pinRed = _gpio.OpenPin(16);
                _pinRed.SetDriveMode(GpioPinDriveMode.Output);
                _pinRed.Write(GpioPinValue.High);
                _toRed = false;

                _pinYellow = _gpio.OpenPin(20);
                _pinYellow.SetDriveMode(GpioPinDriveMode.Output);
                _pinYellow.Write(GpioPinValue.Low);

                _pinGreen = _gpio.OpenPin(21);
                _pinGreen.SetDriveMode(GpioPinDriveMode.Output);
                _pinGreen.Write(GpioPinValue.Low);

                Debug("initializing timer");
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(20);
                _timer.Tick += _timer_Tick;
                _timer.Start();
            }
            else
            {
                Debug("gpio not detected... exiting app");
                Task.Delay(3000);
                Application.Current.Exit();
            }
        }

        private void _timer_Tick(object sender, object e)
        {
            _timer.Stop();
            Debug("timer fired!");

            if (_pinRed.Read() == GpioPinValue.High)
            {
                Debug("setting yellow light for 3 seconds");
                _pinRed.Write(GpioPinValue.Low);
                _pinYellow.Write(GpioPinValue.High);
                _timer.Interval = TimeSpan.FromSeconds(3);
            }
            else 
            if (_pinYellow.Read() == GpioPinValue.High)
            {
                if(_toRed==true)
                {
                    Debug("setting red light for 20 seconds");
                    _pinRed.Write(GpioPinValue.High);
                    _pinYellow.Write(GpioPinValue.Low);
                    _timer.Interval = TimeSpan.FromSeconds(20);
                    _toRed = false;
                }
                else
                {
                    Debug("setting green light for 20 seconds");
                    _pinGreen.Write(GpioPinValue.High);
                    _pinYellow.Write(GpioPinValue.Low);
                    _timer.Interval = TimeSpan.FromSeconds(20);
                    _toRed = true;
                }
            }
            else
            {
                Debug("setting yellow light for 3 seconds");
                _pinGreen.Write(GpioPinValue.Low);
                _pinYellow.Write(GpioPinValue.High);
                _timer.Interval = TimeSpan.FromSeconds(3);
            }

            _timer.Start();
        }
    }
}

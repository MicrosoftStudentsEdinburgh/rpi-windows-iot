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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorldNative
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPin led;
        GpioPin button;

        DateTime start;
        int delay = 500;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            gpio = await GpioController.GetDefaultAsync();

            led = gpio.OpenPin(24);
            led.SetDriveMode(GpioPinDriveMode.Output);

            button = gpio.OpenPin(18);
            button.SetDriveMode(GpioPinDriveMode.Input);
            button.ValueChanged += Button_ValueChanged;

            BlinkLoop();
        }

        private async void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            await Dispatcher.RunIdleAsync(a =>
            {
                if (args.Edge == GpioPinEdge.FallingEdge)
                {
                    start = DateTime.Now;
                    runButton.Text = "Pressed";
                }
                else
                {
                    delay = Math.Max(10, Math.Min(5000, (int)(DateTime.Now - start).TotalMilliseconds));
                    runButton.Text = "Not Pressed";
                    runDelay.Text = delay.ToString();
                }
            });
        }

        private async void BlinkLoop()
        {
            while(true)
            {
                await Task.Delay(delay);
                led.Write(GpioPinValue.High);
                runLed.Text = "On";
                await Task.Delay(delay);
                led.Write(GpioPinValue.Low);
                runLed.Text = "Off";
            }
        }
    }
}

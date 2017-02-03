using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PingPongLed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPin btn1;
        GpioPin btn2;
        int score1;
        int score2;
        int pos;
        bool pressed = false;
        bool lost = false;

        GpioPin[] ledPins;
        Ellipse[] ellipses;
        int currentLed = -1;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // initialize the ping pong controller and choose GPIO 23 and GPIO 24 
            // as the switches for the players
            gpio = await GpioController.GetDefaultAsync();
            btn1 = gpio.OpenPin(22);
            btn2 = gpio.OpenPin(18);

            // add event handlers so we can listen for button presses
            btn1.ValueChanged += Btn1_ValueChanged;
            btn2.ValueChanged += Btn2_ValueChanged;

            // setup the pins for the leds
            ledPins = new int[] { 2, 3, 4, 17, 27, 23 }.Select(a => gpio.OpenPin(a)).ToArray();
            // add ellipses to the gui and save them so we can show them on screen
            ellipses = ledPins.Select(a =>
            {
                var ellipse = new Ellipse()
                {
                    Margin = new Thickness(10),
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(Colors.Red)
                };
                ledPanel.Children.Add(ellipse);
                return ellipse;
            }).ToArray();

            // set the led pins as outputs
            foreach (var p in ledPins)
            {
                p.SetDriveMode(GpioPinDriveMode.Output);
                p.Write(GpioPinValue.Low);
            }

            // start the ping pong loop
            PingPongLoop();
        }

        /// <summary>
        ///     This function sets all leds to off except for the one indexed by led
        /// </summary>
        private void SetLed(int led)
        {
            for (int i = 0; i < ledPins.Count(); i++)
            {
                ledPins[i].Write(i == led ? GpioPinValue.High : GpioPinValue.Low);
                ellipses[i].Fill = new SolidColorBrush(i == led ? Colors.Green : Colors.Red);
            }
        }

        private void IncPlayer1Score()
        {
            score1++;
            player1Score.Text = score1.ToString();
        }

        private void IncPlayer2Score()
        {
            score2++;
            player2Score.Text = score2.ToString();
        }

        /// <summary>
        ///     This cycles the leds in order
        /// </summary>
        private async void PingPongLoop()
        {
            while (true)
            {
                int delay = 1000;
                // this is our position counter, it runs from
                // 0 .. (number of leds * 2 - 3)
                // number vs l

                // leds: 0  1  2  3  4  5
                // pos:  0  1  2  3  4  5
                //          9  8  7  6
                pos = 1;
                lost = false;

                while (true)
                {
                    if (lost)
                    {
                        break;
                    }

                    if (pos == 0 && !pressed)
                    {
                        IncPlayer2Score();
                        break;
                    }
                    else if (pos == ledPins.Count() - 1 && !pressed)
                    {
                        IncPlayer1Score();
                        break;
                    }

                    pressed = false;

                    // move to the next position
                    pos++;
                    if (pos >= ledPins.Count() * 2 - 2)
                    {
                        // if we have reached the end of a cycle reset the position
                        // and decrease the delay to make it go faster
                        pos = 0;
                        delay -= 50;
                    }

                    // find out the led position
                    int led = pos;
                    if (led >= ledPins.Count())
                        led = ledPins.Count() - (led - ledPins.Count()) - 2;

                    // change it so only this led is on
                    SetLed(led);


                    // wait for the delay in ms
                    await Task.Delay(delay);
                }

            }
        }

        private async void Btn2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            await Dispatcher.RunIdleAsync(a =>
            {
                if (args.Edge == GpioPinEdge.RisingEdge)
                {
                    if (pos == 5)
                        pressed = true;
                    else
                    {
                        IncPlayer1Score();
                        lost = true;
                    }
                }
            });
        }

        private async void Btn1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            await Dispatcher.RunIdleAsync(a =>
            {
                if (args.Edge == GpioPinEdge.RisingEdge)
                {
                    if (pos == 0)
                        pressed = true;
                    else
                    {
                        IncPlayer2Score();
                        lost = true;
                    }
                }
            });
        }
    }
}

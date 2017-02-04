using GHIElectronics.UWP.Shields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeatherStation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RunWeatherLoop();
        }
        
        private async void RunWeatherLoop()
        {
            var fez = await FEZHAT.CreateAsync();

            while (true)
            {
                var light = fez.GetLightLevel();
                runLight.Text = light.ToString();
                runTemperature.Text = fez.GetTemperature().ToString();
                var brightness = 255 * (1 - light);
                fez.D2.Color = new FEZHAT.Color((byte)brightness, (byte)brightness, (byte)brightness);

                btnEllipse.Fill = new SolidColorBrush(fez.IsDIO18Pressed() ? Colors.Green : Colors.Red);
               
                await Task.Delay(100);
            }
        }
    }
}

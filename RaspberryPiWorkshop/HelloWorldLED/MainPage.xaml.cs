using GHIElectronics.UWP.Shields;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorldLED
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }
        
        // rather perform initialization in loaded event
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // initialize the FEZHAT
            var fez = await FEZHAT.CreateAsync();

            // set the color of the red-green-blue LED labelled 
            // D2 (diode 2) on the board to yellow
            fez.D2.Color = FEZHAT.Color.Yellow;
        }
    }
}

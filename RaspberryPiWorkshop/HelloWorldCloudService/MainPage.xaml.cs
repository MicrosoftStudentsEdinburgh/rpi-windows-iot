using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

namespace HelloWorldCloudService
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ServiceClient client;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public async void ConnectAzure()
        {
            client = ServiceClient.CreateFromConnectionString(textConnection.Text, Microsoft.Azure.Devices.TransportType.Amqp);
            await client.OpenAsync();

            textConnection.Text = "Connected!";
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectAzure();
        }
       
        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            var data = Encoding.UTF8.GetBytes(textSend.Text);
            var msg = new Message(data);

            await client.SendAsync("MyDevice2", msg);
        }

    }
}

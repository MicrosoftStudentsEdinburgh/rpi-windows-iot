using Microsoft.Azure.Devices.Client;
using Microsoft.Devices.Tpm;
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

namespace HelloWorldCloud
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DeviceClient client;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectAzure();
        }

        public async void ConnectAzure()
        {
            var tpm = new TpmDevice(0);
            /* var hostName = tpm.GetHostName();
            var deviceId = tpm.GetDeviceId();
            var token = tpm.GetSASToken(); */
            var connectionString = tpm.GetConnectionString();

            /*client = DeviceClient.Create(
                hostName, 
                AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, token),
                TransportType.Amqp_Tcp_Only);*/
            client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Amqp_Tcp_Only);
            await client.OpenAsync();

            ReceiveMessages();
        }

        public async void ReceiveMessages()
        {
            while(true)
            {
                var msg = await client.ReceiveAsync(TimeSpan.FromSeconds(30));

                if (msg != null)
                {
                    var text = Encoding.UTF8.GetString(msg.GetBytes());
                    runMessage.Text = text;

                    await client.CompleteAsync(msg);
                }
            }
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            var data = Encoding.UTF8.GetBytes(textSend.Text);
            var msg = new Message(data);
            await client.SendEventAsync(msg);
            textSend.Text = "";
        }
    }
}

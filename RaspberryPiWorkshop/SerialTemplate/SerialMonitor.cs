using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace SerialTemplate
{
    public class SerialMonitor
    {
        public static SerialMonitor _singleton;
        public static SerialMonitor Default
        {
            get
            {
                if (_singleton == null)
                    _singleton = new SerialMonitor();
                return _singleton;
            }
        }

        private SerialDevice dev;
        private Task startTask;
        private DataReader dataReaderObject;
        private DataWriter dataWriterObject;
        private string receivedText;
        private CancellationToken cancellationToken;

        public event Action<object, string, IEnumerable<string>> ReceivedCommand;

        private SerialMonitor()
        {
            startTask = SetupSerial();
        }

        public bool IsConnected
        {
            get
            {
                if (startTask != null)
                    startTask.Wait();
                return dev != null;
            }
        }

        public async Task WaitConnected()
        {
            await startTask;
        }

        private async Task ConnectSerial()
        {
            var devSelector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(devSelector);
            foreach (var dev in devices)
            {
                if (dev.Name == "FT232R USB UART")
                {
                    this.dev = await SerialDevice.FromIdAsync(dev.Id);
                    return;
                }
            }
        }

        private async Task SetupSerial()
        {
            await ConnectSerial();

            Listen();
        }

        private async void Listen()
        {
            try
            {
                if (dev != null)
                {
                    dev.BaudRate = 115200;
                    dev.Parity = SerialParity.None;
                    dev.StopBits = SerialStopBitCount.One;
                    dev.ReadTimeout = TimeSpan.FromMilliseconds(10);
                    dev.WriteTimeout = TimeSpan.FromMilliseconds(10);
                    dev.DataBits = 8;

                    dataReaderObject = new DataReader(this.dev.InputStream);
                    dataWriterObject = new DataWriter(this.dev.OutputStream);
                    this.cancellationToken = new CancellationToken();

                    while (true)
                    {
                        await ReadAsync(this.cancellationToken);
                        NotifyCommands();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }

        private void NotifyCommands()
        {
            var cmds = this.receivedText.Split('\n');
            foreach (var c in cmds.Take(cmds.Length - 1))
            {
                var parts = c.Replace("\r", "").Split(' ');

                if (this.ReceivedCommand != null)
                    this.ReceivedCommand(this, parts.First(), parts.Skip(1));
            }
            this.receivedText = cmds.Last();
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            var t1 = DateTime.Now;
            UInt32 bytesRead = await loadAsyncTask;
            var dt = DateTime.Now.Subtract(t1).TotalMilliseconds;
            Debug.WriteLine(dt);

            if (bytesRead > 0)
            {
                receivedText += dataReaderObject.ReadString(bytesRead);
            }
        }

        public async Task Send(string format, params object[] args)
        {
            var msg = string.Format(format, args) + '\n';
            dataWriterObject.WriteString(msg);
            await dataWriterObject.StoreAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Java.Util;
using LinkOS.Plugin.Abstractions;
using MovilBusiness.Droid.Printers;
using Xamarin.Forms;

[assembly: Dependency(typeof(ConnectionImpl))]
namespace MovilBusiness.Droid.Printers
{
    public class ConnectionImpl : IConnection
    {
        private static readonly UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private BluetoothSocket socket;

        private string MacAddress;


        public ConnectionImpl()
        {

        }


        public IConnection Build(string mac)
        {
            MacAddress = mac;





            return this;
        }

        public bool IsConnected => socket != null && socket.IsConnected;

        public int MaxTimeoutForRead { get; set; }
        public int TimeToWaitForMoreData { get; set; }
        public int TimeToWaitAfterWrite { get; set; }
        public int TimeToWaitAfterRead { get; set; }

        public int BytesAvailable()
        {
            return 0;
        }

        public void Close()
        {
            if (socket == null)
            {
                return;
            }

            socket.OutputStream.Close();
            socket.InputStream.Close();
            socket.Close();
            socket = null;

            Task.Delay(1000).Wait();

        }

        public void Initialize(string connectionString)
        {
            MacAddress = connectionString;
        }

        public void Open()
        {
           BluetoothDevice device = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(MacAddress);

            if (device == null)
            {
                return;
            }

            socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
            socket.Connect();
        }

        public byte[] Read()
        {
            return new byte[] { };
        }

        public byte[] SendAndWaitForResponse(byte[] dataToSend, int initialResponseTimeout, int responseCompletionTimeout, string terminator = null)
        {
            return new byte[] { };
        }

        public void WaitForData(int maxTimeout)
        {

        }

        public void Write(byte[] data)
        {
            if (socket == null)
            {
                BluetoothDevice device = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(MacAddress);

                if (device == null)
                {
                    return;
                }

                socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);

                /*IntPtr createRfcommSocket = JNIEnv.GetMethodID(device.Class.Handle, "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                IntPtr _socket = JNIEnv.CallObjectMethod(device.Handle, createRfcommSocket, new global::Android.Runtime.JValue(1));
                socket = Java.Lang.Object.GetObject<BluetoothSocket>(_socket, JniHandleOwnership.TransferLocalRef);
                */
                socket.Connect();
            }

            socket.OutputStream.Write(data, 0, data.Length);
        }
    }
}
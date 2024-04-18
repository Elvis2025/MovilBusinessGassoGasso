using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Printers;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

[assembly: Dependency(typeof(ConnectionImpl))]
namespace MovilBusiness.iOS.Printers
{
    public class ConnectionImpl : iOSEscPosPrinterConn
    {
        public bool IsConnected => CurrentDevice != null && CurrentCharasteristic != null;


        private Guid MacAddress;

        public ConnectionImpl()
        {

        }

        public void Initialize(string mac)
        {
            try
            {
                MacAddress = Guid.Parse(mac);
            }
            catch (Exception)
            {
                throw new Exception("Error cargando la MacAddress de la impresora");
            }
        }

        public async Task Close()
        {
            if(CrossBluetoothLE.Current == null || !CrossBluetoothLE.Current.IsAvailable || !CrossBluetoothLE.Current.IsOn)
            {
                CurrentDevice = null;
                CurrentCharasteristic = null;
                return;
            }

            var adapter = CrossBluetoothLE.Current.Adapter;

            if (CurrentDevice != null)
            {
                await adapter.DisconnectDeviceAsync(CurrentDevice);
            }

            CurrentCharasteristic = null;
            CurrentDevice = null;
        }
        
        private IDevice CurrentDevice;
        private ICharacteristic CurrentCharasteristic;

        public async Task Open()
        {
            var adapter = CrossBluetoothLE.Current.Adapter;

            CurrentDevice = null;
            CurrentCharasteristic = null;

           // var list = adapter.ConnectedDevices;

            //CurrentDevice = list.Where(x => x.Id == MacAddress).FirstOrDefault();

            CurrentDevice = await adapter.ConnectToKnownDeviceAsync(MacAddress);

            var services = await CurrentDevice.GetServicesAsync();
            
            foreach(var ser in services)
            {
                var characteristics = await ser.GetCharacteristicsAsync();

                var cha = characteristics.Where(x => x.CanWrite).FirstOrDefault();

                if(cha != null)
                {
                    CurrentCharasteristic = cha;
                    break;
                }
            }

            if(CurrentCharasteristic == null)
            {
                throw new Exception("Error abriendo la conexion con la impresora");
            }
        }

        public byte[] Read()
        {
            return new byte[] { };
        }

        public async Task Write(byte[] data)
        {
           if(CurrentCharasteristic == null)
            {
                return;
            }

            await CurrentCharasteristic.WriteAsync(data);
        }
    }
}
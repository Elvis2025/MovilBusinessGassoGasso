using System;
using CoreBluetooth;
using CoreFoundation;
using ExternalAccessory;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Printers;
using MovilBusiness.Printer.Model;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Forms;

[assembly: Dependency(typeof(PrinterDiscoveryImpl))]
namespace MovilBusiness.iOS.Printers
{
    public class PrinterDiscoveryImpl : IPrinterDiscovery
    {
        private Action<BTDevice> OnDeviceFound;
        private Action<string> OnError;
        private Action<string> OnPairDevice;
        private Action OnDiscoveryStopped;

        private CBCentralManager bluetoothManager;

        private IAdapter adapter;
        private IBluetoothLE bluetoothBLE;

        public PrinterDiscoveryImpl()
        {
            bluetoothManager = new CBCentralManager(new CbCentralDelegate(), DispatchQueue.DefaultGlobalQueue,
                                                        new CBCentralInitOptions { ShowPowerAlert = true });

            bluetoothBLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            adapter.ScanTimeout = 10000;
            //adapter.ScanMode = ScanMode.Balanced;
            adapter.ScanMode = ScanMode.LowLatency;
            adapter.DeviceDiscovered += OnDeviceFounded;
        }

        public bool IsEnabled()
        {
            try
            {
                return bluetoothBLE.IsAvailable && bluetoothBLE.IsOn && bluetoothBLE.State == BluetoothState.On;
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }

            return false;
        }

        private void OnDeviceFounded(object sender, DeviceEventArgs a)
        {
            OnDeviceFound(new BTDevice()
            {
                Address = a.Device.Id.ToString(),
                Name = a.Device.Name
            });
        }

        private class CbCentralDelegate : CBCentralManagerDelegate
        {
            public override void UpdatedState(CBCentralManager central)
            {
                if (central.State == CBCentralManagerState.PoweredOn)
                {
                    System.Console.WriteLine("Powered On");
                }
            }
        }

        public void OnDiscoveryStop(Action action)
        {
            OnDiscoveryStopped = action;
        }

        public void PairDevice(string Address)
        {
            OnPairDevice(Address);
        }

        public void SetOnDevicePaired(Action<string> action)
        {
            OnPairDevice = action;
        }

        public void SetOnDevicesFound(Action<BTDevice> DeviceFounded)
        {
            this.OnDeviceFound = DeviceFounded;
        }

        public void SetOnError(Action<string> action)
        {
            OnError = action;
        }

        public async void StartDiscovery(bool forEscPos)
        {
            try
            {
                if (!forEscPos)
                {
                    var connectedAccessories = EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories;

                    foreach (var accessory in connectedAccessories)
                    {
                        /*foreach (var pro in accessory.ProtocolStrings)
                        {
                            if (pro.ToLower() == "com.zebra.rawport" && !serialNumbers.Contains(accessory))
                            {
                                serialNumbers.Add(accessory);
                                Console.WriteLine("DEBUG::found external zebra device with s/n:" + accessory.SerialNumber);
                            }


                        }*/
                        OnDeviceFound(new BTDevice() { Address = accessory.SerialNumber, Name = accessory.Name });
                    }

                }
                else
                {
                    if (adapter.IsScanning)
                    {
                        await adapter.StopScanningForDevicesAsync();
                    }

                    await adapter.StartScanningForDevicesAsync();
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public async void StopDiscovery()
        {
            if (adapter != null && adapter.IsScanning)
            {
                await adapter.StopScanningForDevicesAsync();
            }
            OnDiscoveryStopped?.Invoke();
        }
    }
}
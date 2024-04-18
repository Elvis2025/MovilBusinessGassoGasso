using MovilBusiness.Printer.Model;
using System;

namespace MovilBusiness.Abstraction
{
    public interface IPrinterDiscovery
    {
        //void FindBluetoothPrinters(IDiscoveryEventHandler handler);
        void StartDiscovery(bool forEscPos);
        void StopDiscovery();
        void SetOnDevicesFound(Action<BTDevice> DeviceFounded);
        void SetOnError(Action<string> action);
        void PairDevice(string Address);
        void SetOnDevicePaired(Action<string> action);
        void OnDiscoveryStop(Action action);
        bool IsEnabled();
    }
}

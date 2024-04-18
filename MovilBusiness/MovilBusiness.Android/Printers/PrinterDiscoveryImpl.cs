
using System;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.App;
using Java.Lang.Reflect;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.Printers;
using MovilBusiness.Printer.Model;
using Xamarin.Forms;

[assembly: Dependency(typeof(PrinterDiscoveryImpl))]
namespace MovilBusiness.Droid.Printers
{
    public class PrinterDiscoveryImpl : BroadcastReceiver, IPrinterDiscovery
    {
        /*public void FindBluetoothPrinters(IDiscoveryEventHandler handler)
        {
            BluetoothDiscoverer.Current.FindPrinters(MainActivity.Instance, handler); ///esta en otro paquete si no funciona en este probar con el otro
        }*/
        private BluetoothAdapter btAdapter;

        private Action<string> OnDevicePaired, OnError;
        private Action OnDiscoveryStopped;
        private Action<BTDevice> OnDeviceFounded;

        public PrinterDiscoveryImpl()
        {
            btAdapter = BluetoothAdapter.DefaultAdapter;

            InitComponents();
        }

        public override void OnReceive(Android.Content.Context context, Intent intent)
        {
            try
            {
                string action = intent.Action;

                if (action == null)
                {
                    return;
                }

                if (action == Android.Bluetooth.BluetoothDevice.ActionFound)
                {
                    BluetoothDevice device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice).JavaCast<BluetoothDevice>();
                    OnDeviceFounded?.Invoke(new BTDevice() { Name = device.Name, Address = device.Address });

                }
                else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryStopped?.Invoke();
                }
                else if (action == BluetoothAdapter.ActionDiscoveryStarted)
                {

                }
                else if (action == Android.Bluetooth.BluetoothDevice.ActionBondStateChanged)
                {
                    int state = intent.GetIntExtra(BluetoothDevice.ExtraBondState, BluetoothDevice.Error);
                    int prevState = intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, BluetoothDevice.Error);

                    if (state == (int)Bond.Bonded && prevState == (int)Bond.Bonding)
                    {
                        BluetoothDevice device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice).JavaCast<BluetoothDevice>();

                        OnDevicePaired?.Invoke(device.Address);
                    }
                }

            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
        }

        public void PairDevice(string Address)
        {
            Android.Bluetooth.BluetoothDevice device = btAdapter.GetRemoteDevice(Address);

            if (device == null)
            {
                throw new System.Exception("Dispositivo no encontrado");
            }

            if (device.BondState == Bond.Bonded)
            {
                OnDevicePaired?.Invoke(Address);
            }
            else
            {
                Java.Lang.Class clazz = Java.Lang.Class.ForName("android.bluetooth.BluetoothDevice");
                Method method = clazz.GetMethod("createBond");
                method.Invoke(device);
            }

        }

        public void SetOnDevicesFound(Action<BTDevice> action)
        {
            OnDeviceFounded = action;
        }

        public void SetOnDevicePaired(Action<string> action)
        {
            OnDevicePaired = action;
        }

        public void SetOnError(Action<string> action)
        {
            OnError = action;
        }

        public void StartDiscovery(bool dummy)
        {
            try
            {
                if (!btAdapter.IsEnabled)
                {
                    AlertEncenderBluetooth();
                    return;
                }

                if (btAdapter.IsDiscovering)
                {
                    btAdapter.CancelDiscovery();
                }

                btAdapter.StartDiscovery();

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

        }

        private void AlertEncenderBluetooth()
        {
            new AlertDialog.Builder(MainActivity.Instance)
                .SetTitle("Aviso")
                .SetMessage("Se necesita encender el bluetooth, para esto debes de ir a configuraciones")
                .SetPositiveButton("Encender", (a, s) =>
                {
                    Intent i = new Intent(BluetoothAdapter.ActionRequestEnable);
                    MainActivity.Instance.StartActivityForResult(i, 1);
                })
                .SetNegativeButton("Cancelar", (s, a) => { })
                .SetCancelable(false)
                .Show();
        }

        public void StopDiscovery()
        {
            btAdapter.CancelDiscovery();
            OnDiscoveryStopped?.Invoke();
        }

        private void InitComponents()
        {
            IntentFilter i = new IntentFilter();
            i.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            i.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            i.AddAction(Android.Bluetooth.BluetoothDevice.ActionFound);
            i.AddAction(Android.Bluetooth.BluetoothDevice.ActionBondStateChanged);

            MainActivity.Instance.RegisterReceiver(this, i);
        }

        public void OnDiscoveryStop(Action action)
        {
            OnDiscoveryStopped = action;
        }

        public bool IsEnabled()
        {
            return btAdapter != null && btAdapter.IsEnabled;
        }

    }
}
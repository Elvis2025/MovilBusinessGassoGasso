using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class TransaccionesTrackingViewModel : BaseViewModel
    {
        public string CliNombre { get; set; }

        private ObservableCollection<TransaccionesTracking> record;
        public ObservableCollection<TransaccionesTracking> Record { get => record; set { record = value; RaiseOnPropertyChanged(); } }

        public ExpandListItem<Estados> DatosTransaccion { get; set; }
        public Transaccion CurrentTransaccion { get; set; }

        private DS_TransaccionesTracking myTra;

        public TransaccionesTrackingViewModel(Page page, Transaccion transaccion, ExpandListItem<Estados> datosTransaccion) : base(page)
        {
            myTra = new DS_TransaccionesTracking();
            DatosTransaccion = datosTransaccion;
            CurrentTransaccion = transaccion;

            if (DatosTransaccion.Data.UseClient && transaccion.CliID != 0 && transaccion.CliID != -1)
            {
                var cliente = new DS_Clientes().GetClienteById(transaccion.CliID);

                if(cliente !=null){
                    CliNombre = cliente.CliNombre;
                }
            }
        }

        public async void LoadTracking()
        {
            IsBusy = true;

            try
            {
                var traId = Functions.GetTraIdByTabla(DatosTransaccion.Data.EstTabla.ToUpper());                

                Record = new ObservableCollection<TransaccionesTracking>(myTra.GetTracking(traId, CurrentTransaccion.TraKey));

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }
    }
}

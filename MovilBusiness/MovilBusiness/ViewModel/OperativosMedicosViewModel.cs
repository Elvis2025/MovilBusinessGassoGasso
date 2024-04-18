using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class OperativosMedicosViewModel : BaseViewModel
    {
        public ICommand OnClickCommand { get; private set; }

        private ObservableCollection<OperativosDetalle> detalles;
        public ObservableCollection<OperativosDetalle> Detalles { get => detalles; set { detalles = value; RaiseOnPropertyChanged(); } }

        private DS_OperativosMedicos myOpe;

        private string cmnombre;
        public string CMNombre { get => cmnombre; set { cmnombre = value; RaiseOnPropertyChanged(); } }

        public OperativosMedicosViewModel(Page page) : base(page)
        {
            myOpe = new DS_OperativosMedicos();

            OnClickCommand = new Command(OnClick);

            Detalles = new ObservableCollection<OperativosDetalle>();
        }

        private async void OnClick(object Id)
        {
            switch (Id.ToString()){
                case "1": //save operativo
                    SaveOperativo();
                    break;
                case "2": //add detalle
                    await PushModalAsync(new AgregarOperativoDetalleModal(OnDetalleAdded, productosFiltrados));
                    break;
                case "3": //filtrar productos
                    await PushModalAsync(new FiltrarProductosModal(FiltrarProductos));
                    break;
            }
        }

        private List<Productos> productosFiltrados = new List<Productos>();
        private void FiltrarProductos(List<Productos> productosElegidos)
        {
            productosFiltrados = productosElegidos;
        }

        private void OnDetalleAdded(OperativosDetalle detalle)
        {
            Detalles.Add(detalle);
            RaiseOnPropertyChanged(nameof(Detalles));
        }

        private async void SaveOperativo()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {

                if(Detalles == null || Detalles.Count == 0)
                {
                    await DisplayAlert(AppResource.NoDetailsAdded, AppResource.Aceptar);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CMNombre))
                {
                    await DisplayAlert(AppResource.MustSpecifyMedicalCenterName, AppResource.Aceptar);
                    return;
                }

                IsBusy = true;


                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => 
                {
                    myOpe.GuardarOperativoMedico(Detalles.ToList(), CMNombre);
                });

                await DisplayAlert(AppResource.Success, AppResource.OperativeSavedSuccessfully, AppResource.Aceptar);

                await PopAsync(false);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingOperative, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;
        }

        public void DeleteDetalle(string rowguid)
        {
            var item = Detalles.Where(x => x.rowguid == rowguid).FirstOrDefault();

            if(item == null)
            {
                return;
            }

            Detalles.Remove(item);

            RaiseOnPropertyChanged(nameof(Detalles));
        }
    }
}

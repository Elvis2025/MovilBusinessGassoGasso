using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ReclamacionesViewModel : BaseViewModel
    {
        private DS_Productos myProd;

        public ICommand SaveClaimCommand { get; private set; }
        public List<UsosMultiples> Motivos { get; set; }

        private UsosMultiples currentmotivo;
        public UsosMultiples CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); } }

        private string searchvalue;
        public string SearchValue { get => searchvalue; set { searchvalue = value; OnSearchValueChanged(); RaiseOnPropertyChanged(); } }

        private string currentcantidad;
        public string CurrentCantidad { get => currentcantidad; set { currentcantidad = value; RaiseOnPropertyChanged(); } }

        private string currentunidades;
        public string CurrentUnidades { get => currentunidades; set { currentunidades = value; RaiseOnPropertyChanged(); } }

        private string currentlote;
        public string CurrentLote { get => currentlote; set { currentlote = value; RaiseOnPropertyChanged(); } }

        private string descripcion;
        public string Descripcion { get => descripcion; set { descripcion = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<Productos> productos;
        public ObservableCollection<Productos> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private bool islistvisible;
        public bool IsListVisible { get => islistvisible; set { islistvisible = value; RaiseOnPropertyChanged(); } }

        private bool AutoComplete = true, AutoCompleteValidate = false;

        private Productos CurrentProducto;

        public int RecSecuencia { get; private set; }

        public ReclamacionesViewModel(Page page) : base(page)
        {
            myProd = new DS_Productos();
            Motivos = new DS_UsosMultiples().GetMotivosReclamaciones();
            SaveClaimCommand = new Command(SaveReclamacion);

            RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Reclamaciones");
        }

        private async void SaveReclamacion()
        {
            if(CurrentProducto == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectAProduct);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentCantidad) && string.IsNullOrWhiteSpace(CurrentUnidades))
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSpecifyQuantityOrUnits);
                return;
            }

            if(CurrentMotivo == null)
            {
                await DisplayAlert(AppResource.Warning, AppResource.MustSpecifyReasonForClaim);
                return;
            }

            try
            {
                IsBusy = true;

                int.TryParse(CurrentCantidad, out int cantidad);
                int.TryParse(CurrentUnidades, out int unidades);

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { new DS_Reclamaciones().SaveReclamacion(CurrentMotivo.CodigoUso, Arguments.Values.CurrentClient.CliID, Descripcion, cantidad, unidades, CurrentLote, CurrentProducto.ProID); });

                await DisplayAlert(AppResource.Success, AppResource.ClaimSavedSuccessfully);

                await PopAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingClaim, e.Message);
            }

            IsBusy = false;
        }

        public void OnProductSelected(Productos data)
        {
            CurrentProducto = data;
            IsListVisible = false;

            if(CurrentProducto == null)
            {
                return;
            }
            AutoCompleteValidate = true;
            AutoComplete = false;
            SearchValue = CurrentProducto.ProDescripcion;
        }

        private void OnSearchValueChanged()
        {
            if (!AutoComplete)
            {
                AutoComplete = true;
                AutoCompleteValidate = true;
                return;
            }
            if (AutoCompleteValidate)
            {
                AutoCompleteValidate = false;
                return;
            }

            if(string.IsNullOrWhiteSpace(SearchValue) || SearchValue.Length < 3)
            {
                Productos = null;
                CurrentProducto = null;
                IsListVisible = false;
                return;
            }

            Productos = new ObservableCollection<Productos>(myProd.GetByProDescripcion(SearchValue));

            IsListVisible = Productos != null && Productos.Count > 0;
        }
    }
}

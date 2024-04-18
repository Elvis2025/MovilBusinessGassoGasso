
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using MovilBusiness.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DevolverFacturaCompletaModal : ContentPage
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public List<MotivosDevolucion> MotivosDevoluciones { get; set; }

        private MotivosDevolucion currentmotivo;
        public MotivosDevolucion CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); } }
        private DS_Devoluciones myDev;

        public DevolverFacturaCompletaModal(Action OnFacturaSelected)
        {
            BindingContext = new DevolverFacturaCompletaViewModel(this, OnFacturaSelected);

            myDev = new DS_Devoluciones();

            
            InitializeComponent();

            MotivosDevoluciones = myDev.GetMotivosDevolucion();
           // CurrentMotivo = MotivosDevoluciones.FirstOrDefault();
        }

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            if(BindingContext is DevolverFacturaCompletaViewModel vm)
            {
                vm.CargarProductosFactura(e.SelectedItem as CuentasxCobrar);               
            }
        }


        public class DevolverFacturaCompletaViewModel : BaseViewModel
        {
            public ICommand OnClickCommand { get; private set; }

            public List<CuentasxCobrar> Documentos { get; set; }

            private CuentasxCobrar CurrentCxc;
            private DS_CuentasxCobrar myCxc;

            private ObservableCollection<CuentasxCobrarDetalle> producto;
            public ObservableCollection<CuentasxCobrarDetalle> Productos { get => producto; set { producto = value; RaiseOnPropertyChanged(); } }
            //public new event PropertyChangedEventHandler PropertyChanged;
            public List<MotivosDevolucion> MotivosDevoluciones { get; set; }

            private DS_Devoluciones myDev;

            private MotivosDevolucion currentmotivo2;
            public MotivosDevolucion CurrentMotivo2 { get => currentmotivo2; set { currentmotivo2 = value; RaiseOnPropertyChanged(); } }

            private Action OnFacturaSelected;

            public DevolverFacturaCompletaViewModel(Page page, Action onFacturaSelected) : base(page)
            {
                this.OnFacturaSelected = onFacturaSelected;

                myCxc = new DS_CuentasxCobrar();

                myDev = new DS_Devoluciones();

                MotivosDevoluciones = myDev.GetMotivosDevolucion();
                //CurrentMotivo2 = MotivosDevoluciones.FirstOrDefault();

                OnClickCommand = new Command(OnClick);

                var docs = myCxc.GetAllCuentasPendientesByCliente(Arguments.Values.CurrentClient.CliID);

                if (docs != null)
                {
                    Documentos = docs.Where(x => x.Origen > 0).ToList();
                }
            }

            private void OnClick(object id)
            {
                switch (id.ToString())
                {
                    case "0":
                        PopModalAsync(false);
                        return;
                    case "1":
                        AceptarFactura();
                        return;
                }
            }

            public void CargarProductosFactura(CuentasxCobrar item)
            {
                try
                {
                    CurrentCxc = item;

                    if (item == null)
                    {
                        return;
                    }

                    Productos = new ObservableCollection<CuentasxCobrarDetalle>(myCxc.GetCuentasxCobrarDetalle(CurrentCxc.CxcReferencia));
                }catch(Exception e)
                {
                    DisplayAlert(AppResource.ErrorLoadingProducts, e.Message);
                }
            }

            private void AceptarFactura()
            {
                try
                {
                    if (CurrentCxc == null)
                    {
                        DisplayAlert(AppResource.Warning, AppResource.MustSelectInvoiceToReturn);
                        return;
                    }

                    if (Productos == null || Productos.Count == 0)
                    {
                        DisplayAlert(AppResource.Warning, AppResource.NoProductsToReturn);
                        return;
                    }

                    if (CurrentMotivo2 == null)
                    {
                        DisplayAlert(AppResource.Warning, AppResource.SpecifyReasonWarning);
                        return;
                    }

                    var dsProd = new DS_Productos();

                    dsProd.ClearTemp((int)Arguments.Values.CurrentModule);
                    

                    foreach (var producto in Productos)
                    {
                        var temp = new ProductosTemp();
                        temp.Descripcion = producto.ProDescripcion;
                        temp.Itbis = producto.CxcItbis;
                        temp.Cantidad = producto.CxcCantidad;
                        temp.CantidadDetalle = producto.CxcCantidadDetalle;
                        temp.Descuento = producto.CxcDescuento;
                        temp.UnmCodigo = producto.UnmCodigo;
                        temp.ProID = producto.ProID;
                        temp.rowguid = Guid.NewGuid().ToString();
                        temp.Lote = producto.CxcLote;
                        temp.IndicadorOferta = producto.CxcIndicadorOferta;
                        temp.TitID = (int)Arguments.Values.CurrentModule;
                        temp.CantidadOferta = producto.CxcIndicadorOferta ? producto.CxcCantidad : 0;
                        temp.MotIdDevolucion = CurrentMotivo2.MotID;

                        dsProd.InsertInTemp(temp);
                    }

                    PopModalAsync(false);

                    OnFacturaSelected?.Invoke();

                }
                catch (Exception e)
                {
                    DisplayAlert(AppResource.Warning, e.Message);
                }
            }

            public void GetMotivosDevoluciones(MotivosDevolucion Motivos)
            {
                CurrentMotivo2 = Motivos;
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ComboMotivo_SelectedIndexChanged(object sender, EventArgs e)
        {
         
            if (comboMotivoDevFact.SelectedItem == null)
            {
                return;
            }
            if (!(comboMotivoDevFact.SelectedItem is MotivosDevolucion motivo))
            {
                return;
            }

            if (BindingContext is DevolverFacturaCompletaViewModel vm)
            {
                if ((comboMotivoDevFact.SelectedItem is MotivosDevolucion motivos))
                {
                    CurrentMotivo = motivos;
                    vm.GetMotivosDevoluciones(CurrentMotivo);
                }
            }
        }
    }
}
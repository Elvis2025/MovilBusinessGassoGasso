
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RevisionProductosOfertasModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ProductosTemp> productos;
        public ObservableCollection<ProductosTemp> Productos { get => productos; set { productos = value; RaiseOnPropertyChanged(); } }

        private DS_Productos myProd;
        private PedidosDetalleViewModel mytotalQuintales;

        public PedidosDetalleViewModel MyTotalesQuintales { get => mytotalQuintales;  set { mytotalQuintales = value; RaiseOnPropertyChanged(); } }

        private bool isbusy = false;
        public new bool IsBusy { get => isbusy; set { isbusy = value; RaiseOnPropertyChanged(); } }

        public ProductosTemp CurrentProduct;

        public Action OnProductsAccepted { get; set; }

        private bool ShowRevisionDescuentos = false;
        private bool revisionDescuentosShowed = false;
        private int titId;

        private bool ParUsarLote;
        private bool ParUsarComboLote;
        private bool EsVentaDesdeEntrega = false;

        public RevisionProductosOfertasModal(List<ProductosTemp> productosOferta, DS_Productos myProd, bool GoRevisionDescuentos, int titId, bool ventaDesdeEntrega = false)
        {
            this.titId = titId;
            this.myProd = myProd;
            EsVentaDesdeEntrega = ventaDesdeEntrega;
            ShowRevisionDescuentos = GoRevisionDescuentos;
            Productos = new ObservableCollection<ProductosTemp>(productosOferta);
            var parVentasLote = DS_RepresentantesParametros.GetInstance().GetParVentasLote();

            ParUsarLote = parVentasLote == 1 || parVentasLote == 2;
            ParUsarComboLote = parVentasLote == 2;

            LoadCurrentAlmId();

            InitializeComponent();

            BindingContext = this;
        }

        private int CurrentAlmId = -1;
        private void LoadCurrentAlmId()
        {
            if (ParUsarLote)
            {
                if (new DS_EntregasRepartidorTransacciones().HayPedidosPorEntregar(Arguments.Values.CurrentClient.CliID))
                {
                    CurrentAlmId = DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho();
                }
                else if (!EsVentaDesdeEntrega)
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera() > 0)
                    {
                        CurrentAlmId = DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera();
                    }
                    else
                    {
                        CurrentAlmId = DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDevolucion();
                    }
                }
                else
                {
                    CurrentAlmId = DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho();
                }
            }
            else
            {
                CurrentAlmId = -1;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ShowRevisionDescuentos && !revisionDescuentosShowed && !DS_RepresentantesParametros.GetInstance().GetParPedidosDescuentoManual() && DS_RepresentantesParametros.GetInstance().GetParPedidosDescuentoManualGeneral() <= 0.0 && !DS_RepresentantesParametros.GetInstance().GetParCotizacionesDescuentoManual() && !DS_RepresentantesParametros.GetInstance().GetParDevolucionesDescuentoManual())
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowRevisionDescuentos = false;
                    revisionDescuentosShowed = true;
                    Navigation.PushModalAsync(new RevisionDescuentosModalxaml(titId) { FromRevisionOfertas = true });
                });
            }            
        }

        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem == null)
            {
                return;
            }

            CurrentProduct = args.SelectedItem as ProductosTemp;
            editCantidad.Text = CurrentProduct.CantidadOferta.ToString();
            lblMessage.Text = AppResource.MaximumOfferLabel + CurrentProduct.OfeCantidadMaximaTransaccion.ToString() + "\n" + AppResource.EnterDesiredQuantityLabel;
            CurrentProduct = args.SelectedItem as ProductosTemp;

            if (!DS_RepresentantesParametros.GetInstance().GetParRevisionOfertas())
            {
                editCantidad.IsEnabled = false;
                comboLote.IsEnabled = false;
            }

            if (CurrentProduct.UsaLote && ParUsarLote && Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                lblLote.IsVisible = true;

                if (ParUsarComboLote)
                {
                    comboLote.IsVisible = true;

                    var lotes = new DS_ProductosLotes().GetLotesByProId(CurrentProduct.ProID, CurrentAlmId);

                    comboLote.ItemsSource = lotes;

                    if (!string.IsNullOrWhiteSpace(CurrentProduct.Lote))
                    {
                        var lote = lotes.Where(x => x.PrlLote.Trim().ToUpper() == CurrentProduct.Lote.Trim().ToUpper()).FirstOrDefault();

                        comboLote.SelectedItem = lote;
                    }
                    else
                    {
                        comboLote.SelectedItem = null;
                    }

                }
                else
                {
                    editLote.IsVisible = true;
                    editLote.Text = CurrentProduct.Lote;
                }
                
            }
            else
            {
                lblLote.IsVisible = false;
                editLote.IsVisible = false;
                comboLote.IsVisible = false;
                comboLote.ItemsSource = null;
                editLote.Text = "";
            }

            dialogCantidad.IsVisible = true;

            list.SelectedItem = null;
        }

        private async void AceptarProductos(object sender, EventArgs args)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;
                
                foreach(var producto in Productos)
                {
                    bool cantMayorCantMax = false;
                    if (producto.Cantidad > producto.OfeCantidadMaximaTransaccion && producto.OfeCantidadMaximaTransaccion > 0)
                    {
                        await DisplayAlert(AppResource.Warning, AppResource.RequestedQuantityExceedOffer, AppResource.Aceptar);
                        cantMayorCantMax = true;
                    }

                    if (producto.UsaLote && Arguments.Values.CurrentModule == Modules.VENTAS)
                    {
                        //if (comboLote.SelectedItem == null)
                        //{
                        //    await DisplayAlert(AppResource.Warning, "Debe seleccionar el lote", AppResource.Aceptar);
                        //    IsBusy = false;
                        //    return;
                        //}

                        producto.Cantidad =  (cantMayorCantMax ? producto.OfeCantidadMaximaTransaccion : producto.CantidadOferta);

                        SqliteManager.GetInstance().Execute("update ProductosTemp set Cantidad = 0 " +
                                "where ifnull(IndicadorOferta, 0) = 1 and ProID = ? and OfeID = ? and ifnull(Lote, '') = ? ",
                                new string[] { producto.ProID.ToString(), producto.OfeID.ToString(), producto.Lote.ToString() });

                        myProd.DeleteOfertaInTemp(titId, sinCantidad: true);

                        if (string.IsNullOrWhiteSpace(producto.Lote)) {
                            
                            continue;
                        }
                        else
                        {
                            producto.rowguid = Guid.NewGuid().ToString();
                            myProd.InsertInTemp(producto, true);
                            continue;
                        }
                    }

                    if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
                    {
                        myProd.ActualizarCantidadOfertaConUnidades(producto.ProID,producto.ProIDOferta, producto.OfeID, (cantMayorCantMax ? producto.OfeCantidadMaximaTransaccion : producto.CantidadOferta), titId, (producto.UsaLote && Arguments.Values.CurrentModule == Modules.VENTAS) ? producto.Lote : null, producto.UnmCodigo);
                    }
                    else
                    {
                        myProd.ActualizarCantidadOferta(producto.ProID, producto.OfeID, (cantMayorCantMax ? producto.OfeCantidadMaximaTransaccion : producto.CantidadOferta), titId, (producto.UsaLote && Arguments.Values.CurrentModule == Modules.VENTAS) ? producto.Lote : null);
                    }
                }

                myProd.DeleteOfertaInTemp(titId, sinCantidad: true);
                

                OnProductsAccepted?.Invoke();

                await Navigation.PopModalAsync(false);
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsBusy = false;

        }

        private void OcultarDialogCantidad(object sender, EventArgs args)
        {
            dialogCantidad.IsVisible = false;
        }

        private void AceptarCantidad(object sender, EventArgs args)
        {
            try
            {
                double.TryParse(editCantidad.Text, out double cantidad);

                if (cantidad > CurrentProduct.Cantidad)
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityExceedOfferWarning, AppResource.Aceptar);
                    return;
                }

                if (cantidad > CurrentProduct.OfeCantidadMaximaTransaccion && CurrentProduct.OfeCantidadMaximaTransaccion > 0)
                {
                    DisplayAlert(AppResource.Warning, AppResource.QuantityExceedMaximumOfferWarning, AppResource.Aceptar);
                    return;
                }

                var lote = editLote.Text;

                if (CurrentProduct.UsaLote &&  DS_RepresentantesParametros.GetInstance().GetParVentasLotesAutomaticos() > 0)
                {
                    lote = CurrentProduct.Lote;
                }
                
                if (CurrentProduct.UsaLote && DS_RepresentantesParametros.GetInstance().GetParVentasLotesAutomaticos() < 1)
                {
                    if (ParUsarComboLote)
                    {
                        if(comboLote.SelectedItem is ProductosLotes pr)
                        {
                            lote = pr.PrlLote;
                        }
                        else
                        {
                            lote = null;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(lote) && Arguments.Values.CurrentModule == Modules.VENTAS)
                    {
                        DisplayAlert(AppResource.Warning, AppResource.MustSpecifyLotWarning, AppResource.Aceptar);
                        return;
                    }
                }

                if(CurrentProduct.UsaLote && Arguments.Values.CurrentModule == Modules.VENTAS)
                {
                    var t = Productos.Where(x => !string.IsNullOrWhiteSpace(x.Lote) && x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID && !x.Lote.Trim().ToUpper().Equals(lote.Trim().ToUpper()));
                    var added = t != null ? t.Sum(x => x.CantidadOferta) : 0;

                    if (cantidad > 0)
                    {
                        if ((cantidad + added) > CurrentProduct.Cantidad)
                        {
                            DisplayAlert(AppResource.Warning, AppResource.QuantityExceedOfferThatIsLabel + CurrentProduct.Cantidad, AppResource.Aceptar);
                            return;
                        }

                        //si no existe este lote en temp
                        var prodAdded = Productos.Where(x => !string.IsNullOrWhiteSpace(x.Lote) && x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID && x.Lote.Trim().ToUpper().Equals(lote.Trim().ToUpper())).FirstOrDefault();
                        if (prodAdded == null)
                        {
                            var newItemLote = CurrentProduct.Copy();
                            newItemLote.Lote = lote;
                            newItemLote.CantidadOferta = cantidad;

                            var newIndex = Productos.IndexOf(CurrentProduct);
                            Productos.Insert(newIndex, newItemLote);
                            
                            
                        }
                        else
                        {
                            var idx = Productos.IndexOf(prodAdded);
                            CurrentProduct.CantidadOferta = cantidad;
                            CurrentProduct.Lote = lote;
                            Productos[idx] = CurrentProduct.Copy();          
                        }

                        var toRemove = Productos.Where(x => x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID && string.IsNullOrWhiteSpace(x.Lote)).FirstOrDefault();
                        if (cantidad + added == CurrentProduct.Cantidad)
                        {             
                            if (toRemove != null)
                            {
                                Productos.Remove(toRemove);
                            }

                        }else if (toRemove == null)
                        {
                            var toAdd = CurrentProduct.Copy();
                            toAdd.Lote = "";
                            toAdd.CantidadOferta = 0;
                            var idx = Productos.IndexOf(Productos.Where(x => x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID).LastOrDefault());

                            if (idx != -1)
                            {
                                Productos.Insert(idx + 1, toAdd);
                            }
                        }

                        OcultarDialogCantidad(null, null);
                        RaiseOnPropertyChanged(nameof(Productos));
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(CurrentProduct.Lote) && CurrentProduct.Lote.Trim().ToUpper().Equals(lote.Trim().ToUpper()))
                        {
                            var prodAdded = Productos.Where(x => !string.IsNullOrWhiteSpace(x.Lote) && x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID && x.Lote.Trim().ToUpper().Equals(lote.Trim().ToUpper())).FirstOrDefault();

                            

                            if (Productos.Where(x => string.IsNullOrWhiteSpace(x.Lote) && x.ProID == CurrentProduct.ProID && x.OfeID == CurrentProduct.OfeID).FirstOrDefault() == null)
                            {
                                prodAdded.CantidadOferta = 0;
                                prodAdded.Lote = "";

                                var idx = Productos.IndexOf(prodAdded);

                                Productos[idx] = prodAdded.Copy();
                            }
                            else
                            {
                                Productos.Remove(prodAdded);
                            }


                            RaiseOnPropertyChanged(nameof(Productos));
                            OcultarDialogCantidad(null, null);
                            return;
                        }
                    }
                }

                var index = Productos.IndexOf(CurrentProduct);
                CurrentProduct.CantidadOferta = cantidad;

                var item = CurrentProduct.Copy();

                Productos[index] = item;

                OcultarDialogCantidad(null, null);

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
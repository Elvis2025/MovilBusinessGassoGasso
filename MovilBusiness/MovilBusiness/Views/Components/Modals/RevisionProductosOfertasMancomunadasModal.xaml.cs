using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
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
    public partial class RevisionProductosOfertasMancomunadasModal : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private Action OnProductAccepted;

        private List<Ofertas> ofertasmancomunadas;
        public List<Ofertas> OfertasMancomunadas { get => ofertasmancomunadas; private set { ofertasmancomunadas = value; RaiseOnPropertyChanged(); } }

        private Ofertas currentoferta;
        public Ofertas CurrentOferta { get => currentoferta; set { currentoferta = value; } }

        public ObservableCollection<ProductosTemp> productosofertas;
        public ObservableCollection<ProductosTemp> ProductosOfertas { get => productosofertas; private set { productosofertas = value; RaiseOnPropertyChanged(); } }

        private bool uselabelcantidad;
        public bool UseLabelCantidad { get => uselabelcantidad; set { uselabelcantidad = value; RaiseOnPropertyChanged(); } }

        private bool isvalidenabled;
        public bool IsValidEnabled { get => isvalidenabled; set { isvalidenabled = value; RaiseOnPropertyChanged(); } }

        private int cantidadofertasshow;
        public int CantidadOfertasShow { get => cantidadofertasshow; set { cantidadofertasshow = value; RaiseOnPropertyChanged(); } }

        private int ofeCantidadaDar;

        public bool IsOfeMancomunadaCombo;
        //public int OfeCantidadADar { get => ofeCantidadaDar; set { ofeCantidadaDar = value; RaiseOnPropertyChanged(); } }

        private DS_Ofertas myOfe;
        private DS_Productos myProd;
        private DS_Inventarios myInv;

        private int CantidadAdarOferta = 0;
        public bool OfertaAutomatica = false;
        public int countOfeLast = 0;
        public int cantOf = 0;
        public bool onchange = false;
        public double cantidadTotal = 0.00;

        private Action onProductsCancel;


        public RevisionProductosOfertasMancomunadasModal(List<Ofertas> mancomunadas, Action OnProductAccepted, Action onProductsCancel, DS_Productos myProd, bool OnChange = true)
        {
            this.onProductsCancel = onProductsCancel;
            this.myProd = myProd;
            myOfe = new DS_Ofertas();
            myInv = new DS_Inventarios();
            onchange = OnChange;

            this.OnProductAccepted = OnProductAccepted;            

            OfertasMancomunadas = mancomunadas.Where(m => m.OfeTipo != "14").ToList();

            if (mancomunadas != null)
            {
                AplicarOfertaMancomunadaPorDefault(mancomunadas);
            }

            IsValidEnabled = (CantidadOfertasShow = OfertasMancomunadas.Count) > 1;            

            if (OfertasMancomunadas == null || OfertasMancomunadas.Count == 0)
            {
                Navigation.PopModalAsync(false);
                return;
            }

            if (OfertasMancomunadas != null && OfertasMancomunadas.Count > 0)
            {
                CurrentOferta = OfertasMancomunadas[0];

            }            

            InitializeComponent();

            BindingContext = this;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private ProductosTemp CurrentProduct;
        private void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                return;
            }

            CurrentProduct = args.SelectedItem as ProductosTemp;

            lblMessage.Text = CurrentProduct.Descripcion;

            var cantidadTemp = myProd.GetCantidadProductoOferta((int)Arguments.Values.CurrentModule, CurrentProduct.ProID, ofeId: CurrentOferta.OfeID, fordetalle: CurrentOferta.OfeTipo == "13");

            if (cantidadTemp > 0)
            {
                editCantidad.Text = cantidadTemp.ToString();
            }
            else
            {
                editCantidad.Text = "";
            }

            dialogCantidad.IsVisible = true;

            list.SelectedItem = null;
        }

        private void ClearComponents()
        {
            CurrentProduct = null;
            ProductosOfertas = null;
            CurrentOferta = null;
            lblMessage.Text = "";
            editCantidadOferta.Text = "";
        }

        private void OcultarDialogCantidad(object sender, EventArgs args)
        {
            dialogCantidad.IsVisible = false;
        }

        public void AceptarCantidad(object sender, EventArgs args)
        {
            try
            {
                int.TryParse(editCantidad.Text, out int cantidad);

                /*if (!onchange)
                {
                    cantidad = CurrentProduct.CantidadMaximaOferta;
                }
                **/
                if (ProductosOfertas.Count == 1 && !onchange)
                {
                    cantidad = CantidadAdarOferta;
                }

                if (Arguments.Values.CurrentModule == Modules.VENTAS)
                {
                    var cantidadPro = (cantidad - myProd.GetCantidadProductoOferta((int)Arguments.Values.CurrentModule, CurrentProduct.ProID, CurrentOferta.OfeID, -1, CurrentOferta.OfeTipo == "13")) + myProd.GetCantidadTotalInTemp((int)Arguments.Values.CurrentModule, true, CurrentOferta.OfeTipo == "13", CurrentProduct.ProID);

                    if (!myInv.HayExistencia(CurrentProduct.ProID, cantidadPro, 0, almId: DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera(), isCantidadTotal: CurrentOferta.OfeTipo == "13"))
                    {
                        throw new Exception(AppResource.TotalQuantityExceedStock);
                    }
                }

                var cantidadInTemp = myProd.GetCantidadProductoOferta((int)Arguments.Values.CurrentModule, -1, CurrentOferta.OfeID, CurrentProduct.ProID, CurrentOferta.OfeTipo == "13");

                cantidadTotal = cantidad + cantidadInTemp;

                if (CurrentOferta.OfeTipo == "14" && cantidad > CurrentProduct.CantidadMaximaOferta)
                {
                    throw new Exception(AppResource.RequestedQuantityExceedOffer);
                }
                else if (CurrentOferta.OfeTipo != "14" && ((cantidadTotal > CantidadAdarOferta) || (CurrentOferta.OfeCantidadMaximaTransaccion != 0 && cantidadTotal > CurrentOferta.OfeCantidadMaximaTransaccion)/*(CurrentOferta.OfeCantidadMax > 0 && cantidadTotal > CurrentOferta.OfeCantidadMax)*/))
                {
                    throw new Exception(AppResource.RequestedQuantityExceedOffer);
                }

                var listPro = myOfe.GetDetalleProductosOfertaMancomunada(CurrentOferta.grpCodigoOferta, CurrentOferta.OfeID, (int)Arguments.Values.CurrentModule);
                if (listPro.Count == 1 && (cantidadTotal < CantidadAdarOferta))
                {
                    throw new Exception(AppResource.MustGiveTheMaximunOffer);
                }

                var detalleofeprecio = myOfe.GetDetalleOfertaMancomunadaById(CurrentOferta.OfeID, (int)CurrentOferta.CantidadTemp);
                var item = new ProductosTemp
                {
                    ProID = CurrentProduct.ProID,

                    Descripcion = CurrentProduct.Descripcion,
                    Itbis = CurrentProduct.Itbis,
                    PrecioTemp = CurrentProduct.Precio,
                    OfeID = CurrentOferta.OfeID,
                    IndicadorOferta = true,
                    UnmCodigo = CurrentProduct.UnmCodigo,
                    Precio = detalleofeprecio != null && !string.IsNullOrWhiteSpace(detalleofeprecio[0].OfePrecio.ToString()) && detalleofeprecio[0].OfePrecio > 0
                        ? (detalleofeprecio[0].OfePrecio * CurrentProduct.Precio) / 100
                        : 0
                };

                if (CurrentOferta.OfeTipo == "13")
                {
                    var cantidadReal = cantidad;

                    var proUnidades = CurrentProduct.ProUnidades;

                    if (proUnidades < 1)
                    {
                        proUnidades = 1;
                    }

                    var result = (double)cantidadReal / (double)proUnidades;

                    var cantidadUn = Math.Truncate(result);
                    var detalle = (int)Math.Round((result - Math.Truncate(result)) * proUnidades);

                    item.Cantidad = cantidadUn;
                    item.CantidadDetalle = detalle;

                    // SUM(((CAST(ifnull(CantidadDetalle, 0)AS REAL)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + Cantidad)) as CantidadTotal
                    /*var resultRaw = cantidadInventario - cantidadRealRestar;

                    var resultRaw2 = resultRaw / inv.ProUnidades;

                    var invCantidad = Math.Truncate(resultRaw2);
                    var invCantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * inv.ProUnidades);
*/
                    //item.CantidadDetalle = cantidad;
                }
                else
                {
                    item.Cantidad = cantidad;
                }

                myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, proId: CurrentProduct.ProID, UnmCodigo: (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida() ? CurrentProduct.UnmCodigo : ""));
                myProd.InsertInTemp(item, true);

                myOfe.InsertOfeIdInProductTemp(CurrentOferta);

                var index = ProductosOfertas.IndexOf(CurrentProduct);

                CurrentProduct.Cantidad = item.Cantidad;
                CurrentProduct.CantidadDetalle = item.CantidadDetalle;

                if (index != -1)
                {
                    ProductosOfertas[index] = CurrentProduct.Copy();
                }

                if (ProductosOfertas.Count >= 1 /*&& DS_RepresentantesParametros.GetInstance().GetParItemsOfertadosObligatorios()*/)
                {

                    if (cantidad == CantidadAdarOferta || (CurrentOferta.OfeCantidadMaximaTransaccion > 0 && cantidadTotal == CurrentOferta.OfeCantidadMaximaTransaccion))
                    {
                        var oIndex = OfertasMancomunadas.IndexOf(CurrentOferta);

                        if (oIndex != -1)
                        {
                            var o = CurrentOferta.Copy();
                            o.OfeIndicadoCompleta = true;
                            o.OfeIndicadoInCompleta = false;
                            OfertasMancomunadas[oIndex] = o;
                        }
                    }
                    else
                    {
                        var oIndex = OfertasMancomunadas.IndexOf(CurrentOferta);

                        if (oIndex != -1)
                        {
                            var o = CurrentOferta.Copy();
                            o.OfeIndicadoCompleta = false;
                            o.OfeIndicadoInCompleta = true;
                            OfertasMancomunadas[oIndex] = o;
                        }
                    }

                    dialogCantidad.IsVisible = false;
                }


            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private bool IsWorking = false;
        private async void AceptarProductos(object sender, EventArgs args)
        {
            try
            {
                if (IsWorking)
                {
                    return;
                }

                IsWorking = true;

                //var item = OfertasMancomunadas.Where(x => !x.OfeIndicadoCompleta).FirstOrDefault();
                //var item2 = OfertasMancomunadas.Where(x => x.OfeIndicadoInCompleta).FirstOrDefault();

                if (cantidadTotal != CantidadAdarOferta)
                {
                    await Functions.DisplayAlert(AppResource.Warning, AppResource.NotAllOffersWereFullyGiven);

                    if (DS_RepresentantesParametros.GetInstance().GetParOfertasFaltantesObligatorias())
                    {
                        IsWorking = false;
                        return;
                    }
                }

                await Navigation.PopModalAsync(false);

                OnProductAccepted?.Invoke();

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

            IsWorking = false;

        }

        private async void CancelarProductos(object sender, EventArgs args)
        {
            onProductsCancel?.Invoke();
            await Navigation.PopModalAsync(true);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AplicarOfertaMancomunadaPorDefault(List<Ofertas> OMancomunadas)
        {
            try
            {
                foreach (var ofe in OMancomunadas.ToList())
                {
                    List<ProductosTemp> listPro = new List<ProductosTemp>();
                    if (ofe.OfeTipo != "14")
                    {
                        listPro = myOfe.GetDetalleProductosOfertaMancomunada(ofe.grpCodigoOferta, ofe.OfeID, (int)Arguments.Values.CurrentModule);

                        if (listPro.Count > 1)
                        {
                            IsOfeMancomunadaCombo = true;
                            continue;
                        }
                        else
                        {
                            OMancomunadas.Remove(ofe);
                            CantidadAdarOferta = 0;
                            ofeCantidadaDar = 0;
                        }
                    }

                    var detalles = myOfe.GetDetalleOfertaById(ofe.OfeID, (int)ofe.CantidadTemp);

                    if ((detalles.Count == 1 || listPro.Count == 1) && !IsOfeMancomunadaCombo)
                    {
                        OfertaAutomatica = true;
                    }

                    var listProModificada = new List<ProductosTemp>();

                    int cantidadAEvaluar = (int)ofe.CantidadTemp;

                    foreach (var detalle in detalles)
                    {
                        if (!string.IsNullOrWhiteSpace(ofe.OfeTipo) && ofe.OfeTipo.Trim() == "14")
                        {
                            if (cantidadAEvaluar >= detalle.OfeCantidad)
                            {
                                var cantidadRegalar = (int)(((int)(cantidadAEvaluar / detalle.OfeCantidad)) * detalle.OfeCantidadOferta);

                                //CantidadAdarOferta += cantidadRegalar;
                                ofeCantidadaDar += cantidadRegalar;

                                var prod = myOfe.CrearProductoParaOfertaMancomunada(detalle.ProID);

                                if (prod != null)
                                {
                                    prod.CantidadMaximaOferta = cantidadRegalar;
                                    listProModificada.Add(prod);
                                }
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(ofe.OfeTipo) && ofe.OfeTipo.Trim() == "13")
                        {
                            if (detalle.OfeCantidadDetalleOferta > 0 && ofe.CantidadTemp >= detalle.OfeCantidadDetalle)
                            {
                                CantidadAdarOferta += (int)(Math.Round(((int)(cantidadAEvaluar / detalle.OfeCantidadDetalle) * detalle.OfeCantidadDetalleOferta)));
                                ofeCantidadaDar = (CantidadAdarOferta <= ofe.OfeCantidadMaximaTransaccion ? CantidadAdarOferta : (int)ofe.OfeCantidadMaximaTransaccion);
                                CantidadAdarOferta = ofeCantidadaDar;
                            }
                        }
                        else
                        {
                            if (cantidadAEvaluar >= detalle.OfeCantidad)
                            {
                                if (detalle.OfePorciento > 0)
                                {
                                    CantidadAdarOferta += (int)(Math.Round((detalle.OfePorciento / 100.0) * cantidadAEvaluar));

                                }
                                else
                                {
                                    CantidadAdarOferta += (int)Math.Round((cantidadAEvaluar / (int)detalle.OfeCantidad) * detalle.OfeCantidadOferta);

                                }
                                ofeCantidadaDar = CantidadAdarOferta <= ofe.OfeCantidadMaximaTransaccion ? CantidadAdarOferta : (int)ofe.OfeCantidadMaximaTransaccion;
                                cantidadAEvaluar -= detalle.OfeCantidadOferta > 0 ? CantidadAdarOferta / (int)detalle.OfeCantidadOferta * (int)detalle.OfeCantidad
                                    : CantidadAdarOferta * (int)detalle.OfeCantidad;

                                if (ofe.OfeCantidadMaximaTransaccion == 0)
                                {
                                    ofeCantidadaDar = CantidadAdarOferta;
                                }
                                else
                                {
                                    CantidadAdarOferta = ofeCantidadaDar;
                                }
                            }
                        }
                    }

                    if (ofe.OfeTipo != "14")
                    {
                        //IsOfeMancomunadaCombo = true;

                        foreach (var ProdTemp in listPro)
                        {
                            if (ofe.OfeTipo.Equals("13"))
                            {
                                ProdTemp.CantidadDetalle = myOfe.GetCantidadInTemp(ProdTemp.ProID);
                            }
                            listProModificada.Add(ProdTemp);
                        }
                    }

                    if (listProModificada.Count > 1)
                    {
                        OfertaAutomatica = false;
                    }

                    foreach (var prod in listProModificada)
                    {
                        CurrentProduct = prod;
                        AceptarCantidad(ofe);
                    }

                }

                IsValidEnabled = (CantidadOfertasShow = OfertasMancomunadas.Count) > 1;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CantidadAdarOferta = 0;
                ofeCantidadaDar = 0;

                var detalles = myOfe.GetDetalleOfertaById(CurrentOferta.OfeID, (int)CurrentOferta.CantidadTemp);//myOfe.GetDetalleOfertaByCantidadMayor(CurrentOferta.OfeID, (int)CurrentOferta.CantidadTemp, CurrentOferta.OfeTipo);

                if (detalles.Count == 1)
                {
                    OfertaAutomatica = true;
                }

                UseLabelCantidad = CurrentOferta.OfeTipo.Trim() == "14";

                var listProModificada = new List<ProductosTemp>();

                int cantidadAEvaluar = (int)CurrentOferta.CantidadTemp;

                foreach (var detalle in detalles)
                {
                    //if (!string.IsNullOrWhiteSpace(CurrentOferta.OfeTipo) && CurrentOferta.OfeTipo.Trim() == "14")
                    //{
                    //    if (cantidadAEvaluar >= detalle.OfeCantidad)
                    //    {
                    //        var cantidadRegalar = (int)(((int)(cantidadAEvaluar / detalle.OfeCantidad)) * detalle.OfeCantidadOferta);

                    //        CantidadAdarOferta += cantidadRegalar;
                    //        ofeCantidadaDar += cantidadRegalar;

                    //        var prod = myOfe.CrearProductoParaOfertaMancomunada(detalle.ProID);

                    //        if (prod != null)
                    //        {
                    //            //prod.Cantidad = cantidadRegalar;
                    //            prod.CantidadMaximaOferta = cantidadRegalar;
                    //            prod.lblOfertaPaqueteCajetilla = "Cantidad seleccionada: ";
                    //            //prod.lblCantidadADarOferta = "Cantidad a dar: " + cantidadRegalar;

                    //            listProModificada.Add(prod);
                    //        }
                    //    }
                    //}
                    //else 
                    if (detalle.OfeCantidadDetalleOferta > 0 && !string.IsNullOrWhiteSpace(CurrentOferta.OfeTipo) && CurrentOferta.OfeTipo.Trim() == "13")
                    {
                        if ( /*&& CurrentOferta.CantidadTemp >= detalle.OfeCantidad*/CurrentOferta.CantidadTemp >= detalle.OfeCantidadDetalle)
                        {
                            //CantidadAdarOferta += (int)((int)(cantidadAEvaluar / detalle.OfeCantidadDetalleOferta) * detalle.OfeCantidadOferta);
                            CantidadAdarOferta += (int)(Math.Round(((int)(cantidadAEvaluar / detalle.OfeCantidadDetalle) * detalle.OfeCantidadDetalleOferta)));
                            ofeCantidadaDar = (CantidadAdarOferta <= CurrentOferta.OfeCantidadMaximaTransaccion ? CantidadAdarOferta : (int)CurrentOferta.OfeCantidadMaximaTransaccion);
                            CantidadAdarOferta = ofeCantidadaDar;
                        }
                    }
                    else
                    {
                        if (cantidadAEvaluar >= detalle.OfeCantidad)
                        {
                            if (detalle.OfePorciento > 0)
                            {
                                CantidadAdarOferta += (int)(Math.Round((detalle.OfePorciento / 100.0) * cantidadAEvaluar));

                            }
                            else
                            {
                                CantidadAdarOferta += (int)Math.Round((cantidadAEvaluar / (int)detalle.OfeCantidad) * detalle.OfeCantidadOferta);

                            }

                            ofeCantidadaDar = CantidadAdarOferta <= CurrentOferta.OfeCantidadMaximaTransaccion ? CantidadAdarOferta : (int)CurrentOferta.OfeCantidadMaximaTransaccion;
                            cantidadAEvaluar -= (int)detalle.OfeCantidadOferta > 0 ? CantidadAdarOferta / (int)detalle.OfeCantidadOferta * (int)detalle.OfeCantidad :
                                                CantidadAdarOferta * (int)detalle.OfeCantidad;

                            if (CurrentOferta.OfeCantidadMaximaTransaccion == 0)
                            {
                                ofeCantidadaDar = CantidadAdarOferta;
                            }
                            else if (CantidadAdarOferta >= CurrentOferta.OfeCantidadMaximaTransaccion)
                            {
                                CantidadAdarOferta = ofeCantidadaDar;
                            }
                        }
                    }
                }

                editCantidadOferta.Text = CantidadAdarOferta.ToString();

                if (CurrentOferta.OfeTipo != "14")
                {
                    List<ProductosTemp> listPro = myOfe.GetDetalleProductosOfertaMancomunada(CurrentOferta.grpCodigoOferta, CurrentOferta.OfeID, (int)Arguments.Values.CurrentModule);

                    foreach (var ProdTemp in listPro)
                    {
                        if (CurrentOferta.OfeTipo.Equals("13"))
                        {
                            ProdTemp.lblOfertaPaqueteCajetilla = AppResource.PacksLabel;
                            ProdTemp.CantidadDetalle = myOfe.GetCantidadInTemp(ProdTemp.ProID);
                            // CurrentOferta.CantidadTemp = ProdTemp.CantidadDetalle;
                        }
                        else if (CurrentOferta.OfeTipo.Equals("3"))
                        {
                            ProdTemp.lblOfertaPaqueteCajetilla = AppResource.PackagesLabel;
                        }
                        else
                        {
                            ProdTemp.lblOfertaPaqueteCajetilla = AppResource.QuantityLabel;
                        }
                        listProModificada.Add(ProdTemp);
                    }
                }

                if (listProModificada.Count > 1)
                {
                    OfertaAutomatica = false;
                }

                ProductosOfertas = new ObservableCollection<ProductosTemp>(listProModificada);
                onchange = false;

            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.ErrorSelectingOffer, ex.Message, AppResource.Aceptar);
            }

        }

        public void AceptarCantidad(Ofertas ofe)
        {

            int cantidad = ofe.OfeTipo == "14" ? CurrentProduct.CantidadMaximaOferta : CantidadAdarOferta;

            var cantidadInTemp = myProd.GetCantidadProductoOferta((int)Arguments.Values.CurrentModule, -1, ofe.OfeID, CurrentProduct.ProID, ofe.OfeTipo == "13");

            var cantidadTotal = cantidad + cantidadInTemp;

            var detalleofeprecio = myOfe.GetDetalleOfertaMancomunadaById(ofe.OfeID, (int)ofe.CantidadTemp);
            var item = new ProductosTemp
            {
                ProID = CurrentProduct.ProID,

                Descripcion = CurrentProduct.Descripcion,
                Itbis = CurrentProduct.Itbis,
                PrecioTemp = CurrentProduct.Precio,
                OfeID = ofe.OfeID,
                IndicadorOferta = true,
                UnmCodigo = CurrentProduct.UnmCodigo,
                Precio = detalleofeprecio != null && !string.IsNullOrWhiteSpace(detalleofeprecio[0].OfePrecio.ToString()) && detalleofeprecio[0].OfePrecio > 0 
                    ? (detalleofeprecio[0].OfePrecio * CurrentProduct.Precio) / 100 
                    : 0
            };

            if (ofe.OfeTipo == "13")
            {
                var cantidadReal = cantidad;

                var proUnidades = CurrentProduct.ProUnidades;

                if (proUnidades < 1)
                {
                    proUnidades = 1;
                }

                var result = (double)cantidadReal / (double)proUnidades;

                var cantidadUn = Math.Truncate(result);
                var detalle = (int)Math.Round((result - Math.Truncate(result)) * proUnidades);

                item.Cantidad = cantidadUn;
                item.CantidadDetalle = detalle;
            }
            else
            {
                item.Cantidad = cantidad;
            }

            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
            {
                myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, ofeId: ofe.OfeID, proId: CurrentProduct.ProID, UnmCodigo: CurrentProduct.UnmCodigo);
            }
            else
            {
                myProd.DeleteOfertaInTemp((int)Arguments.Values.CurrentModule, ofeId: ofe.OfeID, proId: CurrentProduct.ProID, UnmCodigo: "", isFromMancomunada: true, grpCodigo: ofe.GrpCodigo);
            }

            myOfe.InsertOfeIdInProductTemp(ofe);

            myProd.InsertInTemp(item, true);
        }
    }
}

using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AgregarProductosAuditoriaModal : ContentPage, INotifyPropertyChanged
	{
        private DS_AuditoriasMercados myAud;

        public new event PropertyChangedEventHandler PropertyChanged;

        private Action<AuditoriasMercadosTemp, bool> OnAddProduct;

        private AuditoriasMercadosTemp currentproduct = null;
        public AuditoriasMercadosTemp CurrentProduct { get => currentproduct; set { currentproduct = value; OnCurrentProductChanged(); RaiseOnPropertyChanged(); } }

        public ObservableCollection<Categorias1AuditoriasMercado> Categorias { get; set; }
        private ObservableCollection<Categorias2AuditoriasMercado> categorias2 = null;
        public ObservableCollection<Categorias2AuditoriasMercado> Categorias2 { get => categorias2; set { categorias2 = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<MarcasAuditoriasMercado> marcas = null;
        public ObservableCollection<MarcasAuditoriasMercado> Marcas { get => marcas; set { marcas = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<PresentacionesMarcasAuditoriasMercado> presentaciones = null;
        public ObservableCollection<PresentacionesMarcasAuditoriasMercado> Presentaciones { get=> presentaciones; set{ presentaciones = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<MarcasAuditoriasMercado> variedades = null;
        public ObservableCollection<MarcasAuditoriasMercado> Variedades { get => variedades; set { variedades = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<EmpaquesAuditoriasMercados> empaques = null;
        public ObservableCollection<EmpaquesAuditoriasMercados> Empaques { get=> empaques; set{ empaques = value; RaiseOnPropertyChanged(); } }
        private ObservableCollection<UnidadDeMedidasAuditoriasMercados> unidadesMedidas = null;
        public ObservableCollection<UnidadDeMedidasAuditoriasMercados> UnidadesMedidas { get=> unidadesMedidas; set{ unidadesMedidas=value; RaiseOnPropertyChanged(); } }

        private bool IsEditing = false;
        private int CargarCombosDependientes;

        private string ParCamposPermitidos;
       // private bool UseMarcaYVariedad = false;

        public AgregarProductosAuditoriaModal (Action<AuditoriasMercadosTemp, bool> OnAddProduct)
		{
            this.OnAddProduct = OnAddProduct;

            myAud = new DS_AuditoriasMercados();

            CargarCombosDependientes = DS_RepresentantesParametros.GetInstance().GetParAuditoriaMercado();

            InitializeComponent ();

            ParCamposPermitidos = DS_RepresentantesParametros.GetInstance().GetParAuditoriasMercadosCamposAUsar();

            if (!string.IsNullOrEmpty(ParCamposPermitidos))
            {
                ValidarCamposDisponibles();
            }

           // UseMarcaYVariedad = DS_RepresentantesParametros.GetInstance().GetParAuditoriasMercadoUsarMarcayVariedad();

            BindPickers();

            BindingContext = this;
        }

        private void ValidarCamposDisponibles()
        {
            if(ParCamposPermitidos == null)
            {
                return;
            }

            var campos = ParCamposPermitidos.Split(',');

            if(campos == null)
            {
                return;
            }

            if (campos.Contains("CAT"))
            {
                lblCategoria.IsVisible = false;
                comboCategoria1.IsVisible = false;
            }

            if (campos.Contains("DES"))
            {
                lblDescripcion.IsVisible = false;
                comboDescripcion.IsVisible = false;
            }

            if (campos.Contains("MAR"))
            {
                lblMarca.IsVisible = false;
                comboMarca.IsVisible = false;
            }

            if (campos.Contains("PRE"))
            {
                lblPresentacion.IsVisible = false;
                comboPresentacion.IsVisible = false;
            }

            if (campos.Contains("EMP"))
            {
                lblEmpaque.IsVisible = false;
                comboEmpaque.IsVisible = false;
            }

            if (campos.Contains("UME"))
            {
                lblUnidMedida.IsVisible = false;
                comboUnidadesMedidas.IsVisible = false;
            }

            if (campos.Contains("CAP"))
            {
                lblCapacidad.IsVisible = false;
                entryCapacidad.IsVisible = false;
            }

            if (campos.Contains("UDV"))
            {
                //unidad de venta
            }

            if (campos.Contains("PPU"))
            {
                lblPrecioVenta.IsVisible = false;
                entryPrecioVenta.IsVisible = false;
            }

            if (campos.Contains("POF"))
            {
                lblPrecioOferta.IsVisible = false;
                entryPrecioOferta.IsVisible = false;
            }

            if (campos.Contains("GSU"))
            {
                lblGondolaSuelo.IsVisible = false;
                entryGondolaSuelo.IsVisible = false;
            }

            if (campos.Contains("GMA"))
            {
                lblGondolaManos.IsVisible = false;
                entryGondolaManos.IsVisible = false;
            }

            if (campos.Contains("GOJ"))
            {
                lblGondolaOjos.IsVisible = false;
                entryGondolaOjos.IsVisible = false;
            }

            if (campos.Contains("GTE"))
            {
                lblGondolaTecho.IsVisible = false;
                entryGondolaTecho.IsVisible = false;
            }

            if (campos.Contains("ECAB"))
            {
                lblEspacioCabecera.IsVisible = false;
                entryEspacioCabecera.IsVisible = false;
            }

            if (campos.Contains("EIS"))
            {
                lblEspacioIsla.IsVisible = false;
                entryEspacioIsla.IsVisible = false;
            }

            if (campos.Contains("ECAJ"))
            {
                lblEspacioCaja.IsVisible = false;
                entryEspacioCaja.IsVisible = false;
            }

            if (campos.Contains("EFR"))
            {
                lblEspacioFrio.IsVisible = false;
                entryEspacioFrio.IsVisible = false;
            }

            if (campos.Contains("PCOM"))
            {
                lblPrecioCompra.IsVisible = false;
                entryPrecioCompra.IsVisible = false;
            }

        }

        private void BindPickers(bool clear = false)
        {
             if (CargarCombosDependientes == 1)
             {
                lblDescripcion.IsVisible = false;
                comboDescripcion.IsVisible = false;
                lblMarca.IsVisible = false;
                comboMarca.IsVisible = false;
                lblVariedad.IsVisible = false;
                comboVariedad.IsVisible = false;
                Categorias = new ObservableCollection<Categorias1AuditoriasMercado>(myAud.GetCategoriasAuditoriasGenericas());
                Presentaciones = new ObservableCollection<PresentacionesMarcasAuditoriasMercado>(myAud.GetPresentacionesAuditoriasGenericas());
                UnidadesMedidas = new ObservableCollection<UnidadDeMedidasAuditoriasMercados>(myAud.GetUnidadesMedidasAuditoriasGenericas());
                Empaques = new ObservableCollection<EmpaquesAuditoriasMercados>(myAud.GetEmpaquesAuditoriasGenericos());

            }

            else if (CargarCombosDependientes == 3)
            {
                lblVariedad.IsVisible = false;
                comboVariedad.IsVisible = false;
                Categorias = new ObservableCollection<Categorias1AuditoriasMercado>(myAud.GetCategoriasAuditoriasGenericaswithout());
                Presentaciones = new ObservableCollection<PresentacionesMarcasAuditoriasMercado>(myAud.GetPresentacionesAuditoriasGenericaswithout());
                UnidadesMedidas = new ObservableCollection<UnidadDeMedidasAuditoriasMercados>(myAud.GetUnidadesMedidasAuditoriasGenericasWhitOut());
                Empaques = new ObservableCollection<EmpaquesAuditoriasMercados>(myAud.GetEmpaquesAuditoriasGenericosWithOut());
                Categorias2 = new ObservableCollection<Categorias2AuditoriasMercado>(myAud.GetCategorias2());
                Marcas = new ObservableCollection<MarcasAuditoriasMercado>(myAud.GetMarcasAuditoriaswithout());
            }
            else
            {
                if (!clear)
                {
                    Categorias = new ObservableCollection<Categorias1AuditoriasMercado>(myAud.GetCategorias1Auditorias());                    
                }

                Empaques = null;
                Presentaciones = null;
                Variedades = null;
                Marcas = null;
                UnidadesMedidas = null;
                Categorias2 = null;
            }
        }

        private void AceptarProducto(object sender, EventArgs args)
        {
            try
            {
                if (!ValidarDatos())
                {
                    return;
                }

                AuditoriasMercadosTemp temp = new AuditoriasMercadosTemp()
                {
                    AudEmpaque = "",
                    MarCodigo = "",
                    AudCapacidad = 0,
                    Ca1Codigo = "",
                    Ca2Codigo = "",
                    AudPresentacion = "",
                    UnmCodigo = "",
                    AudVariedad = ""
                };

                if (comboCategoria1.SelectedItem != null)
                {
                    temp.Ca1Codigo = (comboCategoria1.SelectedItem as Categorias1AuditoriasMercado).Ca1Codigo;
                    temp.Ca1Descripcion = (comboCategoria1.SelectedItem as Categorias1AuditoriasMercado).Ca1Descripcion;
                }

                if (comboDescripcion.SelectedItem != null)
                {
                    temp.Ca2Codigo = (comboDescripcion.SelectedItem as Categorias2AuditoriasMercado).Ca2Codigo;
                    temp.Ca2Descripcion = (comboDescripcion.SelectedItem as Categorias2AuditoriasMercado).Ca2Descripcion;
                }

                if (comboEmpaque.SelectedItem != null)
                {
                    temp.AudEmpaque = (comboEmpaque.SelectedItem as EmpaquesAuditoriasMercados).EmpCodigo;
                    temp.AudEmpaqueDescripcion = (comboEmpaque.SelectedItem as EmpaquesAuditoriasMercados).EmpDescripcion;
                }

                if (comboUnidadesMedidas.SelectedItem != null)
                {
                    temp.UnmCodigo = (comboUnidadesMedidas.SelectedItem as UnidadDeMedidasAuditoriasMercados).UnidCodigo;
                    temp.UnmCodigoDescripcion = (comboUnidadesMedidas.SelectedItem as UnidadDeMedidasAuditoriasMercados).UnidDescripcion;
                }

                if (comboPresentacion.SelectedItem != null)
                {
                    temp.AudPresentacion = (comboPresentacion.SelectedItem as PresentacionesMarcasAuditoriasMercado).PreCodigo;
                    temp.AudPresentacionDescripcion = (comboPresentacion.SelectedItem as PresentacionesMarcasAuditoriasMercado).PreDescripcion;
                }

                if (comboMarca.SelectedItem != null)
                {
                    temp.MarCodigo = (comboMarca.SelectedItem as MarcasAuditoriasMercado).MarCodigo;
                    temp.MarDescripcion = (comboMarca.SelectedItem as MarcasAuditoriasMercado).MarDescripcion;
                }

                if (comboVariedad.SelectedItem != null)
                {
                    temp.AudVariedad = (comboVariedad.SelectedItem as MarcasAuditoriasMercado).MarFragancia;
                }

                double.TryParse(entryCapacidad.Text, out double capacidad);
                double.TryParse(entryPrecioOferta.Text, out double precioOferta);
                double.TryParse(entryPrecioVenta.Text, out double precioVenta);
                double.TryParse(entryPrecioCompra.Text, out double precioCompra);
                int.TryParse(entryGondolaSuelo.Text, out int gondolaSuelo);
                int.TryParse(entryGondolaManos.Text, out int gondolaManos);
                int.TryParse(entryGondolaOjos.Text, out int gondolaOjos);
                int.TryParse(entryGondolaTecho.Text, out int gondolaTecho);
                int.TryParse(entryEspacioCabecera.Text, out int espacioCabecera);
                int.TryParse(entryEspacioIsla.Text, out int espacioIsla);
                int.TryParse(entryEspacioCaja.Text, out int espacioCajas);
                int.TryParse(entryEspacioFrio.Text, out int espacioFrio);

                temp.AudPrecioCompra = precioCompra;
                temp.AudCapacidad = capacidad;
                temp.AudPrecioOferta = precioOferta;
                temp.AudGondolaSuelo = gondolaSuelo;
                temp.AudGondolaManos = gondolaManos;
                temp.AudGondolaOjos = gondolaOjos;
                temp.AudGondolaTecho = gondolaTecho;
                temp.AudEspacioCabecera = espacioCabecera;
                temp.AudEspacioIsla = espacioIsla;
                temp.AudEspacioCajas = espacioCajas;
                temp.AudEspacioFrio = espacioFrio;
                temp.AudPrecioPublico = precioVenta;

                if (IsEditing)
                {
                    temp.rowguid = CurrentProduct.rowguid;
                }
                else
                {
                    temp.rowguid = Guid.NewGuid().ToString();
                }               

                myAud.InsertTemp(temp, IsEditing);

                OnAddProduct?.Invoke(temp, IsEditing);

                ClearValues();

                Navigation.PopModalAsync(true);

            }
            catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }

        }

        private bool ValidarDatos()
        {
            if(comboCategoria1.IsVisible && comboCategoria1.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustChooseCategory, AppResource.Aceptar);
                return false;
            }


            if(comboDescripcion.IsVisible && comboDescripcion.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustChooseDescription, AppResource.Aceptar);
                return false;
            }

            if(comboMarca.IsVisible && comboMarca.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectBrand, AppResource.Aceptar);
                return false;
            }

            if(comboVariedad.IsVisible && comboVariedad.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectVariety, AppResource.Aceptar);
                return false;
            }

            if(comboEmpaque.IsVisible && comboEmpaque.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectPackaging, AppResource.Aceptar);
                return false;
            }

            if(comboUnidadesMedidas.IsVisible && comboUnidadesMedidas.SelectedIndex == -1)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSelectUnitMeasure, AppResource.Aceptar);
                return false;
            }

           /* double.TryParse(entryCapacidad.Text, out double capacidad);

            if(capacidad == 0)
            {
                DisplayAlert(AppResource.Warning, "Debes de especificar la capacidad", AppResource.Aceptar);
                return false;
            }*/

            return true;

        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Categoria1IndexChanged(object sender, EventArgs e)
        {
            if((comboCategoria1.SelectedIndex == -1 || CargarCombosDependientes == 1|| CargarCombosDependientes == 3))
            {
                return;
            }

            if(comboCategoria1.SelectedItem is Categorias1AuditoriasMercado ca1)
            {
                Categorias2 = new ObservableCollection<Categorias2AuditoriasMercado>(myAud.GetCategorias2ByCa1Codigo(ca1.Ca1Codigo));
            }

        }

        private void Categoria2IndexChanged(object sender, EventArgs args)
        {
            if ((comboDescripcion.SelectedIndex == -1 || CargarCombosDependientes == 1 || CargarCombosDependientes == 3))
            {
                return;
            }

            if (comboDescripcion.SelectedItem is Categorias2AuditoriasMercado ca2)
            {
                Marcas = new ObservableCollection<MarcasAuditoriasMercado>(myAud.GetMarcasAuditorias(ca2.Ca1Codigo, ca2.Ca2Codigo));
            }
        }

        private void MarcasIndexChanged(object sender, EventArgs args)
        {
            if ((comboMarca.SelectedIndex == -1 || CargarCombosDependientes == 1|| CargarCombosDependientes == 3))
            {
                return;
            }

            if (comboMarca.SelectedItem is MarcasAuditoriasMercado mar)
            {
                Presentaciones = new ObservableCollection<PresentacionesMarcasAuditoriasMercado>(myAud.GetPresentacionesAuditorias(mar.Ca1Codigo, mar.Ca2Codigo, mar.MarCodigo));
                Variedades = new ObservableCollection<MarcasAuditoriasMercado>((Marcas.Where(x => x.Ca1Codigo == mar.Ca1Codigo && x.Ca2Codigo == mar.Ca2Codigo && x.MarCodigo == mar.MarCodigo).ToList()));
            }
        }

        private void PresentacionIndexChanged(object sender, EventArgs args)
        {
            if (comboPresentacion.SelectedIndex == -1 || CargarCombosDependientes == 1 || CargarCombosDependientes == 3)
            {
                return;
            }

            if (comboPresentacion.SelectedItem is PresentacionesMarcasAuditoriasMercado pre)
            {
                Empaques = new ObservableCollection<EmpaquesAuditoriasMercados>(myAud.GetEmpaquesAuditorias(pre.Ca1Codigo, pre.Ca2Codigo, pre.MarCodigo, pre.PreCodigo));
            }
        }

        private void EmpaqueIndexChanged(object sender, EventArgs args)
        {
            if (comboEmpaque.SelectedIndex == -1 || CargarCombosDependientes == 1 || CargarCombosDependientes == 3)
            {
                return;
            }

            if (comboEmpaque.SelectedItem is EmpaquesAuditoriasMercados emp)
            {
                UnidadesMedidas = new ObservableCollection<UnidadDeMedidasAuditoriasMercados>(myAud.GetUnidadesMedidasAuditorias(emp.Ca1Codigo, emp.Ca2Codigo, emp.MarCodigo, emp.PreCodigo, emp.EmpCodigo));
            }
        }

        private void OnCurrentProductChanged()
        {
            try
            {
                if (CurrentProduct == null)
                {
                    ClearValues();
                }

                IsEditing = CurrentProduct != null;

                if (!IsEditing)
                {
                    return;
                }

                if (comboCategoria1.IsVisible && Categorias != null && Categorias.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.Ca1Codigo))
                {
                    var cat = Categorias.Where(x => x.Ca1Codigo == CurrentProduct.Ca1Codigo).FirstOrDefault();
                    
                    comboCategoria1.SelectedItem = cat;

                    if(CargarCombosDependientes == 2 && cat != null && comboDescripcion.IsVisible)
                    {
                        Categorias2 = new ObservableCollection<Categorias2AuditoriasMercado>(myAud.GetCategorias2ByCa1Codigo(cat.Ca1Codigo));
                    }
                    
                }

                if (comboDescripcion.IsVisible && Categorias2 != null && Categorias2.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.Ca2Codigo))
                {
                    Categorias2AuditoriasMercado cat = null;

                    if (CargarCombosDependientes == 2)
                    {
                        cat = Categorias2.Where(x => x.Ca1Codigo == CurrentProduct.Ca1Codigo && x.Ca2Codigo == CurrentProduct.Ca2Codigo).FirstOrDefault();
                    }
                    else
                    {
                        cat = Categorias2.Where(x => x.Ca2Codigo == CurrentProduct.Ca2Codigo).FirstOrDefault();
                    }

                    comboDescripcion.SelectedItem = cat;

                    if(CargarCombosDependientes == 2 && cat != null && comboMarca.IsVisible)
                    {
                        Marcas = new ObservableCollection<MarcasAuditoriasMercado>(myAud.GetMarcasAuditorias(cat.Ca1Codigo, cat.Ca2Codigo));
                    }
                }

                if (comboMarca.IsVisible && Marcas != null && Marcas.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.MarCodigo))
                {
                    MarcasAuditoriasMercado mar = null;

                    if (CargarCombosDependientes == 2)
                    {
                        mar = Marcas.Where(x => x.MarCodigo == CurrentProduct.MarCodigo && x.Ca1Codigo == CurrentProduct.Ca1Codigo && x.Ca2Codigo == CurrentProduct.Ca2Codigo).FirstOrDefault(); ;
                    }
                    else
                    {
                        mar = Marcas.Where(x => x.MarCodigo == CurrentProduct.MarCodigo).FirstOrDefault();
                    }

                    comboMarca.SelectedItem = mar;

                    if(mar != null && CargarCombosDependientes == 2)
                    {
                        if (comboPresentacion.IsVisible)
                        {
                            Presentaciones = new ObservableCollection<PresentacionesMarcasAuditoriasMercado>(myAud.GetPresentacionesAuditorias(mar.Ca1Codigo, mar.Ca2Codigo, mar.MarCodigo));
                        }
                        if (comboVariedad.IsVisible)
                        {
                            Variedades = new ObservableCollection<MarcasAuditoriasMercado>((Marcas.Where(x => x.Ca1Codigo == mar.Ca1Codigo && x.Ca2Codigo == mar.Ca2Codigo && x.MarCodigo == mar.MarCodigo).ToList()));
                        }
                    }
                }

                if (comboVariedad.IsVisible && Variedades != null && Variedades.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.AudVariedad))
                {
                    MarcasAuditoriasMercado var = null;

                    if (CargarCombosDependientes == 2)
                    {
                        var = Variedades.Where(x => x.MarFragancia == CurrentProduct.AudVariedad && x.Ca2Codigo == CurrentProduct.Ca2Codigo && x.Ca1Codigo == CurrentProduct.Ca1Codigo && x.MarCodigo == CurrentProduct.MarCodigo).FirstOrDefault();
                    }
                    else
                    {
                        var = Variedades.Where(x => x.MarFragancia == CurrentProduct.AudVariedad).FirstOrDefault();
                    }                    

                    comboVariedad.SelectedItem = var;
                }

                if (comboPresentacion.IsVisible && Presentaciones != null && Presentaciones.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.AudPresentacion))
                {
                    PresentacionesMarcasAuditoriasMercado pre = null;

                    if (CargarCombosDependientes == 2)
                    {
                        pre = Presentaciones.Where(x => (x.PreCodigo ?? "") == (CurrentProduct.AudPresentacion ?? "") && (x.Ca2Codigo ?? "") == (CurrentProduct.Ca2Codigo ?? "") && (x.Ca1Codigo ?? "") == (CurrentProduct.Ca1Codigo ?? "") && (x.MarCodigo ?? "") == (CurrentProduct.MarCodigo ?? "")).FirstOrDefault();
                    }
                    else
                    {
                        pre = Presentaciones.Where(x => x.PreCodigo == CurrentProduct.AudPresentacion).FirstOrDefault();
                    }

                    comboPresentacion.SelectedItem = pre;

                    if(CargarCombosDependientes == 2 && pre != null && comboEmpaque.IsVisible)
                    {
                        Empaques = new ObservableCollection<EmpaquesAuditoriasMercados>(myAud.GetEmpaquesAuditorias(pre.Ca1Codigo, pre.Ca2Codigo, pre.MarCodigo, pre.PreCodigo));
                    }
                }

                if(comboEmpaque.IsVisible && Empaques != null && Empaques.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.AudEmpaque))
                {
                    EmpaquesAuditoriasMercados emp = null;

                    if (CargarCombosDependientes == 2)
                    {
                        emp = Empaques.Where(x => x.EmpCodigo == CurrentProduct.AudEmpaque && x.PreCodigo == CurrentProduct.AudPresentacion && x.Ca1Codigo == CurrentProduct.Ca1Codigo && x.Ca2Codigo == CurrentProduct.Ca2Codigo && x.MarCodigo == CurrentProduct.MarCodigo).FirstOrDefault();
                    }
                    else
                    {
                        emp = Empaques.Where(x => x.EmpCodigo == CurrentProduct.AudEmpaque).FirstOrDefault();
                    }                  

                    comboEmpaque.SelectedItem = emp;

                    if (CargarCombosDependientes == 2 && emp != null && comboUnidadesMedidas.IsVisible)
                    {
                        UnidadesMedidas = new ObservableCollection<UnidadDeMedidasAuditoriasMercados>(myAud.GetUnidadesMedidasAuditorias(emp.Ca1Codigo, emp.Ca2Codigo, emp.MarCodigo, emp.PreCodigo, emp.EmpCodigo));
                    }
                }

                if(comboUnidadesMedidas.IsVisible && UnidadesMedidas != null && UnidadesMedidas.Count > 0 && !string.IsNullOrEmpty(CurrentProduct.UnmCodigo))
                {
                    UnidadDeMedidasAuditoriasMercados unm = null;

                    if (CargarCombosDependientes == 2)
                    {
                        unm = UnidadesMedidas.Where(x => x.Ca1Codigo == CurrentProduct.Ca1Codigo && x.Ca2Codigo == CurrentProduct.Ca2Codigo && x.EmpCodigo == CurrentProduct.AudEmpaque && x.MarCodigo == CurrentProduct.MarCodigo && x.UnidCodigo == CurrentProduct.UnmCodigo && x.PreCodigo == CurrentProduct.AudPresentacion).FirstOrDefault();
                    }
                    else
                    {
                        unm = UnidadesMedidas.Where(x => x.UnidCodigo == CurrentProduct.UnmCodigo).FirstOrDefault();
                    }

                    comboUnidadesMedidas.SelectedItem = unm;

                }

                entryPrecioCompra.Text = CurrentProduct.AudPrecioCompra.ToString();
                entryCapacidad.Text = CurrentProduct.AudCapacidad.ToString();
                entryPrecioOferta.Text = CurrentProduct.AudPrecioOferta.ToString();
                entryPrecioVenta.Text = CurrentProduct.AudPrecioPublico.ToString();
                entryGondolaSuelo.Text = CurrentProduct.AudGondolaSuelo.ToString();
                entryGondolaManos.Text = CurrentProduct.AudGondolaManos.ToString();
                entryGondolaOjos.Text = CurrentProduct.AudGondolaOjos.ToString();
                entryGondolaTecho.Text = CurrentProduct.AudGondolaTecho.ToString();
                entryEspacioCabecera.Text = CurrentProduct.AudEspacioCabecera.ToString();
                entryEspacioIsla.Text = CurrentProduct.AudEspacioIsla.ToString();
                entryEspacioCaja.Text = CurrentProduct.AudEspacioCajas.ToString();
                entryEspacioFrio.Text = CurrentProduct.AudEspacioFrio.ToString();

            }catch(Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message, AppResource.Aceptar);
            }
        }

        private void ClearValues()
        {
            comboCategoria1.SelectedIndex = -1;
            comboDescripcion.SelectedIndex = -1;
            comboMarca.SelectedIndex = -1;
            comboPresentacion.SelectedIndex = -1;
            comboVariedad.SelectedIndex = -1;
            comboEmpaque.SelectedIndex = -1;
            comboUnidadesMedidas.SelectedIndex = -1;

            if (CargarCombosDependientes == 2)
            {
                BindPickers(true);
            }

            entryCapacidad.Text = "";
            entryPrecioCompra.Text = "";
            entryPrecioVenta.Text = "";
            entryPrecioOferta.Text = "";
            entryGondolaSuelo.Text = "";
            entryGondolaManos.Text = "";
            entryGondolaOjos.Text = "";
            entryGondolaTecho.Text = "";
            entryEspacioCabecera.Text = "";
            entryEspacioIsla.Text = "";
            entryEspacioCaja.Text = "";
            entryEspacioFrio.Text = "";
        }
    }
}
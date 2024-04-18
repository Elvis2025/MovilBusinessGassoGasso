using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.ViewModel;
using MovilBusiness.Views.Components.ListItemRows;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EntregasRepartidorDetalleRevisionPage : ContentPage
	{
        public static bool Finish = false;
        private bool FirstTime = true;

        public EntregasRepartidorDetalleRevisionPage(List<EntregasRepartidorTransacciones> entregas)
        {
            BindingContext = new EntregaRepartidorDetalleRevisionViewModel(this, entregas);

            Init(false);
        }

        public EntregasRepartidorDetalleRevisionPage (EntregasRepartidorTransacciones entrega, bool IsDetalle = false, bool IsFromConsultas = false)
		{
            BindingContext = new EntregaRepartidorDetalleRevisionViewModel(this, entrega, IsDetalle);

            Init(IsDetalle, IsFromConsultas);
		}

        private void Init(bool IsDetalle, bool IsFromConsultas = false)
        {
            InitializeComponent();

            if (IsDetalle)
            {
                ToolbarItems.RemoveAt(0);
            }
            if (!IsFromConsultas)
            {
                if(ToolbarItems.Count > 1)
                {
                    ToolbarItems.RemoveAt(1);
                }
                else
                {
                    ToolbarItems.RemoveAt(0);
                }
            }

            

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.CONDUCES:

                    var parNombreConduce = DS_RepresentantesParametros.GetInstance().GetParConducesNombreModulo();

                    if (string.IsNullOrWhiteSpace(parNombreConduce))
                    {
                        Title = "Conduces " + AppResource.Detail;
                    }
                    else
                    {
                        Title = parNombreConduce;
                    }

                    break;
                case Modules.RECEPCIONDEVOLUCION:
                    Title = AppResource.ReturnReceptionDetail;
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParConducesUsarRowSinDialog())
            {
                list.ItemTemplate = new DataTemplate(typeof(RowConducesDetalle2));
            }else if (IsDetalle)
            {
                list.ItemTemplate = new DataTemplate(typeof(RowConsultaEntregasRepartidor));
            }

        }

        protected override void OnAppearing()
        {
            if (Finish)
            {
                Navigation.PopAsync(false);
                Finish = false;
                base.OnAppearing();
                return;
            }

            base.OnAppearing();

            if (FirstTime && BindingContext is EntregaRepartidorDetalleRevisionViewModel vm)
            {
                FirstTime = false;

                vm.CargarProductos();
            }
        }

    }
}
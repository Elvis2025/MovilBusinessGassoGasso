using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DepositosSectorModal : ContentPage
	{
        public List<Sectores> Grupos { get; set; }
        private string CurrentMonCodigo;

        public Action<string, string> OnValueSelected { private get; set; }
        private bool isSociedad;

		public DepositosSectorModal (bool isSociedad = false, string monCodigo = null)
		{
            this.isSociedad = isSociedad;
            CurrentMonCodigo = monCodigo;

            if (isSociedad)
            {
                Grupos = new DS_Recibos().GetSociedadesByRecibosSinDepositar(CurrentMonCodigo);
            }
            else
            {
                Grupos = new DS_Sectores().GetSectores(true);
            }

            InitializeComponent ();

            if (isSociedad)
            {
                lblSubTitle.Text = AppResource.SelectCompanyListToUse;
            }
            else
            {
                lblSubTitle.Text = AppResource.SelectSectorFromListToUse;
            }

            BindingContext = this;
		}

        private void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem == null)
                {
                    return;
                }

                //OnValueSelected?.Invoke((e.SelectedItem as KV).Key);
                ShowAlertConfirm(e.SelectedItem as Sectores);
                //Navigation.PopModalAsync(false);

                list.SelectedItem = null;
            }catch(Exception ex)
            {
                DisplayAlert(AppResource.Warning, ex.Message, AppResource.Aceptar);
            }
        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(false);
        }

        private async void ShowAlertConfirm(Sectores item)
        {
            var result = await DisplayAlert(AppResource.Warning, isSociedad ? AppResource.WantSelectCompanyQuestion : AppResource.WantSelectSectorQuestion, AppResource.Yes, AppResource.No);

            if (!result)
            {
                return;
            }

            await Navigation.PopModalAsync(false);

            OnValueSelected?.Invoke(item.SecCodigo, CurrentMonCodigo);
        }
    }
}


using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NCFModal : ContentPage
	{
        private DS_UsosMultiples myUso;
        //public List<UsosMultiples> TiposComprobante { get; set; }
        public Action<NcfContainer> NcfAceptar { get; set; }

        private NcfContainer editedncf = null;
        public NcfContainer EditedNCF { get => editedncf; set { editedncf = value; ConfigNcfEdited(); } }

        private bool IsEditing = false;

        public NCFModal ()
		{
            // TiposComprobante = new DS_UsosMultiples().GetTiposNCF();

            myUso = new DS_UsosMultiples();

            BindingContext = this;
			InitializeComponent ();

            LoadData();

        }

        private void LoadData()
        {
            comboSerie.ItemsSource = myUso.GetSeriesNCFGastos();

            if(comboSerie.ItemsSource != null && comboSerie.ItemsSource.Count == 1)
            {
                comboSerie.SelectedIndex = 0;
                comboSerie.SelectedItem = comboSerie.ItemsSource[0];
            }

            var anios = new List<int>();

            var anio = DateTime.Now.Year;

            for(int i = anio; i <= anio + 5; i++)
            {
                anios.Add(i);
            }

            pickerVencimiento.ItemsSource = anios;

            var rawTiposNCF = myUso.GetTiposComprobante2018();

            var tiposNCFValidosRaw = DS_RepresentantesParametros.GetInstance().GetParGastosTiposNCFValidos();

            var tiposNCF = new List<UsosMultiples>();

            if (!string.IsNullOrWhiteSpace(tiposNCFValidosRaw))
            {
                var tipos = tiposNCFValidosRaw.Split(',');

                foreach(var tipo in tipos)
                {
                    var item = rawTiposNCF.Where(x => x.CodigoUso.Trim().ToUpper() == tipo.Trim().ToUpper()).FirstOrDefault();

                    if(item != null)
                    {
                        tiposNCF.Add(item);
                    }
                }
            }

            comboTipoNcf.ItemsSource = tiposNCF;

            if(comboTipoNcf.ItemsSource != null && comboTipoNcf.ItemsSource.Count == 1)
            {
                comboTipoNcf.SelectedIndex = 0;
                comboTipoNcf.SelectedItem = comboTipoNcf.ItemsSource[0];
            }

        }

        private void Dismiss(object sender, EventArgs args)
        {
            Navigation.PopModalAsync(true);
        }

        private void AceptarNcf(object sender, EventArgs args)
        {

            if (comboSerie.SelectedItem == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.SerieCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (comboTipoNcf.SelectedItem == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.TypeOfReceiptCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if (string.IsNullOrWhiteSpace(editNcf.Text))
            {
                DisplayAlert(AppResource.Warning, AppResource.NcfCannotBeEmpty, AppResource.Aceptar);
                return;
            }

            if(pickerVencimiento.SelectedItem == null)
            {
                DisplayAlert(AppResource.Warning, AppResource.MustSpecifyExpirationDate, AppResource.Aceptar);
                return;
            }

            var serie = (comboSerie.SelectedItem as UsosMultiples).CodigoUso;
            var tipoNCF = (comboTipoNcf.SelectedItem as UsosMultiples).CodigoUso;
            var anioVencimiento = pickerVencimiento.SelectedItem.ToString();

            string ncf = serie.Trim().ToUpper() + tipoNCF.Trim() + Functions.RellenaCeros(8, editNcf.Text);

            var fechaVenc = DateTime.Parse(anioVencimiento + "/12/31");

            var result = new NcfContainer() { NCF = ncf, FechaVencimiento = fechaVenc, tipoCombrobanteNCF = tipoNCF };

            NcfAceptar?.Invoke(result);

            Reset();

            Dismiss(null, null);
        }


        private void Reset()
        {
            comboSerie.SelectedIndex = -1;
            comboTipoNcf.SelectedIndex = -1;
            editNcf.Text = "";
            pickerVencimiento.SelectedIndex = -1;
            EditedNCF = null;
        }

        private void ConfigNcfEdited()
        {
            try
            {
                IsEditing = EditedNCF != null;

                if (IsEditing)
                {
                    //pickerVencimiento.Date = EditedNCF.FechaVencimiento;


                    var anio = (pickerVencimiento.ItemsSource as List<int>).Where(x => x == EditedNCF.FechaVencimiento.Year).FirstOrDefault();
                    pickerVencimiento.SelectedItem = anio;

                    var serie = (comboSerie.ItemsSource as List<UsosMultiples>).Where(x => x.CodigoUso.Trim().ToUpper() == EditedNCF.NCF.Substring(0, 1).Trim().ToUpper()).FirstOrDefault();
                    comboSerie.SelectedItem = serie;

                    var tipoNCF = (comboTipoNcf.ItemsSource as List<UsosMultiples>).Where(x => x.CodigoUso.Trim().ToUpper() == EditedNCF.NCF.Substring(1, 2).Trim().ToUpper()).FirstOrDefault();
                    comboTipoNcf.SelectedItem = tipoNCF;

                    //editSerie.Text = EditedNCF.NCF.Substring(0, 1);
                    //editTipoNcf.Text = EditedNCF.NCF.Substring(1, 2);
                    editNcf.Text = EditedNCF.NCF.Substring(3, EditedNCF.NCF.Length-3);
                    editNcf.Focus();

                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
	}
}
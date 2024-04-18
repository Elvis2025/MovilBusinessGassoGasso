using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RegistrosBaseDatosPage : ContentPage, INotifyPropertyChanged
    {

        public new event PropertyChangedEventHandler PropertyChanged;

        public List<DbInfo> tablas { get; set; }
        public List<DbInfo> Tablas { get => tablas; set { tablas = value; RaiseOnPropertyChanged(); } }
        public string Cambios { get; set; }

        private int Tappedslnombres;
        private int Tappedslregistros;

        public RegistrosBaseDatosPage ()
		{
             Tappedslnombres = 1;

             Tablas = SqliteManager.GetInstance().GetDatabaseRegistryInfo();

             Cambios = Tablas.Sum(t => t.RegistryCount).ToString("N2").Replace(".00", " ");

             BindingContext = this;

             InitializeComponent();
		}

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Tappedslnombres++;

            lblnombres.Opacity = 0.4;
            lblregistros.Opacity = 1;

            if (Tappedslnombres == 2)
            {
                DependencyService.Get<IAppToast>().ShowToast(AppResource.Descendant);
                Tablas = Tablas.OrderByDescending(o => o.TableName).ToList();
                Tappedslnombres = 0;
                return;
            }

            DependencyService.Get<IAppToast>().ShowToast(AppResource.Ascendant);
            Tablas = Tablas.OrderBy(o => o.TableName).ToList();

        }
        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            lblnombres.Opacity = 1;
            lblregistros.Opacity = 0.4;

            Tappedslregistros++;
            if (Tappedslregistros == 2)
            {
                DependencyService.Get<IAppToast>().ShowToast(AppResource.Descendant);
                Tablas = Tablas.OrderByDescending(o => o.RegistryCount).ToList();
                Tappedslregistros = 0;
                return;
            }
            DependencyService.Get<IAppToast>().ShowToast(AppResource.Ascendant);
            Tablas = Tablas.OrderBy(O => O.RegistryCount).ToList();
        }

        /* private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
         {
             if(e.SelectedItem == null)
             {
                 return;
             }

             list.SelectedItem = null;
         }*/


        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
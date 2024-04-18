using MovilBusiness.DataAccess;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class AsignacionRutasDetalleViewModel : BaseViewModel
    {

        private ObservableCollection<RutaVisitasFecha> ruta;
        public ObservableCollection<RutaVisitasFecha> Ruta { get => ruta; set { ruta = value; RaiseOnPropertyChanged(); } }

        private DS_RutaVisitas myRut;

        public AsignacionRutasDetalleViewModel(Page page) : base(page)
        {
            myRut = new DS_RutaVisitas();

            SaveCommand = new Command(() =>
            {
                GuardarAsignacionRutas();

            }, () => IsUp);
        }

        public async void CargarRuta()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {

                IsBusy = true;

                await Task.Run(() => 
                {
                    Ruta = new ObservableCollection<RutaVisitasFecha>(myRut.GetClientesSinAsignar(new ClientesArgs(), DateTime.Now, forSave:true));
                });

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private async void GuardarAsignacionRutas()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => {
                    myRut.ActualizarPosicionInTemp(Ruta);
                    myRut.GuardarRutaVisitaFromTemp();
                });

                await DisplayAlert(AppResource.Success, AppResource.RouteStoredSuccessfully);

                AsignacionRutasPage.Finish = true;

                await PopAsync(true);

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }

        public void SubirRow(string rowguid)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var item = Ruta.Where(x => x.rowguid == rowguid).FirstOrDefault();

                var index = Ruta.IndexOf(item);

                if (index == 0)
                {
                    return;
                }

                Ruta.Remove(item);

                Ruta.Insert(index - 1, item);
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            IsBusy = false;
        }

        public void BajarRow(string rowguid)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var item = Ruta.Where(x => x.rowguid == rowguid).FirstOrDefault();

                var index = Ruta.IndexOf(item);

                if (index == Ruta.Count - 1)
                {
                    return;
                }

                Ruta.Remove(item);

                Ruta.Insert(index + 1, item);

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            IsBusy = false;
        }
    }
}

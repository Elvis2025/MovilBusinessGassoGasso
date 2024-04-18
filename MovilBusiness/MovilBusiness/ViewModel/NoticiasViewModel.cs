
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class NoticiasViewModel : BaseViewModel
    {
        private DS_Noticias myNot;

        private ObservableCollection<Noticias> noticiassource;
        public ObservableCollection<Noticias> NoticiasSource { get { return noticiassource; } set { noticiassource = value; RaiseOnPropertyChanged(); } }

        public NoticiasViewModel(Page page) : base(page)
        {
            myNot = new DS_Noticias();
            CargarNoticias();
        }

        private async void CargarNoticias()
        {
            try
            {
                IsBusy = true;

                await Task.Run(() => { NoticiasSource = new ObservableCollection<Noticias>(myNot.GetNoticias()); });             

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void AlertNoticiaSeleccionada(Noticias noticia)
        {
            if(noticia == null || IsBusy)
            {
                return;
            }

            try
            {
                bool result = false;

                if (!noticia.NotIndicadorLeido)
                {
                    result = await DisplayAlert(noticia.NotTitulo, noticia.NotDescripcion, AppResource.MarkRead, AppResource.Aceptar);
                }
                else
                {
                    await DisplayAlert(noticia.NotTitulo, noticia.NotDescripcion, AppResource.Aceptar);
                }

                if (!result || noticia.NotIndicadorLeido)
                {
                    return;
                }

                IsBusy = true;

                TaskLoader task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { myNot.MarcarNoticiaLeida(/*noticia.NotID*/noticia.rowguid); });

                CargarNoticias();

            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorMarkingNewsAsRead, e.Message, AppResource.Aceptar);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

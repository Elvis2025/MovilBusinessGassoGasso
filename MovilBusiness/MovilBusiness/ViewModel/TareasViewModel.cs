using MovilBusiness.DataAccess;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class TareasViewModel : BaseViewModel
    {
        private DS_Tareas mytar;

        private ObservableCollection<Tareas> tasksource;
        public ObservableCollection<Tareas> TaskSource { get { return tasksource; } set { tasksource = value; RaiseOnPropertyChanged(); } }
        public Tareas tareaDetalle { get; set; }
        public Tareas TareaDetalle { get { return tareaDetalle; } set { tareaDetalle = value; RaiseOnPropertyChanged(); } }
        public int CurrentSecuencia { get; set;}
        public TareasViewModel(Page page) : base(page)
        {
          
            mytar = new DS_Tareas();
            LoadPendingTaskAsync();
        }

        private async void LoadPendingTaskAsync()
        {
            try
            {
                IsBusy = true;

                await Task.Run(() => { TaskSource = new ObservableCollection<Tareas>(mytar.GetPendingTask()); });
            

            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void DetalleTarea(Tareas tarea)
        {
            if (tarea == null || IsBusy)
            {
                return;
            }
            IsBusy = true;
            try
            {
                
                CurrentSecuencia = tarea.TarSecuencia;
                await PushModalAsync(new DetalleTareasModal(this));
            }
            catch (Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
            finally
            {
                IsBusy = false;
            }
        }



        public void DetalleTarea()
        {
            try
            {
                TareaDetalle = mytar.GetPendingTaskBySecuencia(CurrentSecuencia);
                
            }
            catch (Exception e)
            {
                DisplayAlert(AppResource.Warning, e.Message);
            }
        }



        public void ActualizarTarea(string status)
        {
            try
            {
                mytar.ActualizarTarea(CurrentSecuencia, status);
                LoadPendingTaskAsync();
                PopModalAsync(true);
            }
            catch (Exception e)
            {

                DisplayAlert(AppResource.Warning, e.Message);
            }

        }
    }
}

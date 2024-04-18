using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class QuejasServicioViewModel : BaseViewModel
    {

        private DS_QuejasServicio myQue;

        private ObservableCollection<TiposMensaje> motivosquejas;
        public ObservableCollection<TiposMensaje> MotivosQuejas { get => motivosquejas; set { motivosquejas = value; RaiseOnPropertyChanged(); } }

        private TiposMensaje currentmotivo;
        public TiposMensaje CurrentMotivo { get => currentmotivo; set { currentmotivo = value; RaiseOnPropertyChanged(); } }

        private List<Representantes> vendedores;
        public List<Representantes> Vendedores { get => vendedores; set { vendedores = value; RaiseOnPropertyChanged(); } }

        private Representantes currentvendedor;
        public Representantes CurrentVendedor { get => currentvendedor; set { currentvendedor = value; RaiseOnPropertyChanged(); } }

        private string comentario;
        public string Comentario { get => comentario; set { comentario = value; RaiseOnPropertyChanged(); } }

        private int editedQueSecuencia;

        public bool ControlsEnabled { get; private set; }

        public QuejasServicioViewModel(Page page, int editedQueSecuencia = -1, bool IsDetail = false) : base(page)
        {
            this.editedQueSecuencia = editedQueSecuencia;
            ControlsEnabled = !IsDetail;
            myQue = new DS_QuejasServicio();
            SaveCommand = new Command(() =>
            {
                GuardarQueja();

            }, () => IsUp);

            MotivosQuejas = new ObservableCollection<TiposMensaje>(new DS_TiposMensaje().GetTipoMensaje(54));            

            if(editedQueSecuencia != -1)
            {
                var edited = myQue.GetQuejaBySecuencia(editedQueSecuencia);

                if(edited != null)
                {
                    Vendedores = new DS_Representantes().GetAllRepresentantes(edited.CliID);
                    CurrentVendedor = Vendedores.Where(x=>x.RepCodigo.Trim().Equals(edited.RepCodigoVendedor.Trim())).FirstOrDefault();
                    CurrentMotivo = MotivosQuejas.Where(x => x.MenID == edited.QueIDMotivo).FirstOrDefault();
                    Comentario = edited.QueComentario;
                }
            }
            else
            {
                Vendedores = new DS_Representantes().GetAllRepresentantes(Arguments.Values.CurrentClient.CliID);
            }
        }

        private async void GuardarQueja()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrEmpty(Comentario) && CurrentMotivo.MenDescripcion == "Otros")
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.EnterCommentWarning);
                return;
            }


            if (CurrentVendedor == null)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectSeller);
                return;
            }

            if (CurrentMotivo == null)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectComplaintReason);
                return;
            }

            IsBusy = true;

            try
            {
                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => {
                    if (editedQueSecuencia == -1)
                    {
                        myQue.GuardarQueja(CurrentMotivo, CurrentVendedor.RepCodigo, Comentario);
                    }
                    else
                    {
                        myQue.EditarQueja(editedQueSecuencia, CurrentMotivo, CurrentVendedor.RepCodigo, Comentario);
                    }
                });

                await DisplayAlert(AppResource.Success, AppResource.ComplaintSavedSuccessfully);

                await PopAsync(false);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }
    }
}

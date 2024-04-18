using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal.Structs.services;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.views;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class ComentariosViewModel : BaseViewModel
    {

        private DS_Mensajes myMen;
        public List<TiposMensaje> Predeterminados { get; set; }

        private TiposMensaje currentmensaje = null;
        public TiposMensaje CurrentMensaje { get => currentmensaje; set { currentmensaje = value; IsDetail = true; SetDetalle(); RaiseOnPropertyChanged(); } }

        public List<Departamentos> Departamentos { get; set; }

        private Departamentos currentdepartamento;
        public Departamentos CurrentDepartamento { get => currentdepartamento; set { currentdepartamento = value; RaiseOnPropertyChanged(); } }

        private string currentdetalle;
        public string CurrentDetalle { get => currentdetalle; set { currentdetalle = value; CalculateCharacters(); RaiseOnPropertyChanged(); } }
        public int ComentLenght { get; set; } = 500;
        public bool IsDetail { get => CurrentMensaje != null && !string.IsNullOrEmpty(CurrentMensaje.MenDescripcion) && CurrentMensaje.MenDescripcion.Trim().ToUpper().Replace("S", "") == "OTRO"; set { RaiseOnPropertyChanged(); } }

        private string charactersinfo = "0 / ";
        public string CharactersInfo { get => charactersinfo; set { charactersinfo = value; RaiseOnPropertyChanged(); } }

        private int TraId = 1, CurrentTraSecuencia;

        public bool ShowDepartamento { get; private set; }

        public ComentariosViewModel(Page page, int CurrentTraSecuencia) : base(page)
        {
            this.CurrentTraSecuencia = CurrentTraSecuencia;
            
            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    TraId = 1;
                    break;
                case Modules.DEVOLUCIONES:
                    TraId = 2;
                    break;
                case Modules.COBROS:
                    TraId = 3;
                    break;
                case Modules.INVFISICO:
                    TraId = 7;
                    break;
                case Modules.DEPOSITOS:
                    TraId = 9;
                    break;
                case Modules.ENTREGADOCUMENTOS:
                    TraId = 10;
                    break;
                case Modules.VISITAS:
                    TraId = 13;
                    break;
                case Modules.VENTAS:
                    TraId = 4;
                    break;
                case Modules.COMPRAS:
                    TraId = 11;
                    break;
                case Modules.COTIZACIONES:
                    TraId = 5;
                    break;
                case Modules.ENTREGASREPARTIDOR:
                    TraId = 27;
                    break;
                case Modules.CONDUCES:
                    TraId = 51;
                    break;
                case Modules.SAC:
                    TraId = 15;
                    break;
            }
            
            Predeterminados = new DS_TiposMensaje().GetTipoMensaje(TraId);

            SaveCommand = new Command(() =>
            {
                SaveMessage();

            }, () => IsUp);

            myMen = new DS_Mensajes();
           if(myParametro.GetParCerrarVisitaDespuesTransaccion())
            {
                OperacionesPage.CloseVisit = true;
            }

            ShowDepartamento = myParametro.GetParVisitasComentarioUsarDepartamento();

            if (ShowDepartamento)
            {
                Departamentos = new DS_Departamentos().GetDepartamentos();
            }

            // < summary > Limitar cantidad de caracteres por parametro </ summary >
            //Por default ComentLenght =500
            var comentarioLength = myParametro.GetParLenghtComent();
            if (comentarioLength != -1 && comentarioLength > 0)
            {
                ComentLenght = comentarioLength;             
            }
        }

        private async void SaveMessage()
        {
            IsUp = false;

            if(CurrentMensaje == null)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectTheDefaultComment);
                return;
            }

            if(IsDetail && string.IsNullOrWhiteSpace(CurrentDetalle))
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSpecifyCommentDetail);
                return;
            }

            if (ShowDepartamento && CurrentDepartamento == null)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSelectDepartment);
                return;
            }

            try
            {

                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                var depId = ShowDepartamento && CurrentDepartamento != null ? CurrentDepartamento.DepID : 0;            

                await task.Execute(
                    () =>
                    { myMen.CrearMensaje(Arguments.Values.CurrentClient != null
                        ? Arguments.Values.CurrentClient.CliID :
                        -1, CurrentDetalle, Arguments.Values.CurrentVisSecuencia,
                        CurrentTraSecuencia, TraId, depId); });

                await DisplayAlert(AppResource.Success, AppResource.CommentSavedSuccessfully);

                await PopAsync(false);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message);
            }

            IsUp = true;
            IsBusy = false;
        }

        private void CalculateCharacters()
        {
            if (!IsDetail)
            {
                return;
            }

            if (string.IsNullOrEmpty(CurrentDetalle))
            {
                CharactersInfo = "0 / "+ ComentLenght.ToString() + "";
            }

            CharactersInfo = CurrentDetalle.Length + " / "+ ComentLenght.ToString() + "";
        }

        private void SetDetalle()
        {
            if(CurrentMensaje != null && !IsDetail)
            {
                CurrentDetalle = CurrentMensaje.MenDescripcion;

            }
            else
            {
                CurrentDetalle = "";
            }
        }


 
    }
}


using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using MovilBusiness.Views.Components.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class AuditoriaMercadoViewModel : BaseViewModel
    {
        private DS_AuditoriasMercados myAud;
        public ICommand ShowAddDialogCommand { get; private set; }
        public int ParAuditoria { get; set; } //se usa para saber que row usar

        public List<UsosMultiples> Tamanos { get; set; }
        public UsosMultiples CurrentTamano { get; set; }

        private ObservableCollection<AuditoriasMercadosTemp> productosagregados;
        public ObservableCollection<AuditoriasMercadosTemp> ProductosAgregados { get => productosagregados; set { productosagregados = value; RaiseOnPropertyChanged(); } }

        private bool showingadddialog = false;
        public bool ShowingAddDialog { get => showingadddialog; set { showingadddialog = value; RaiseOnPropertyChanged(); } }

        public string CantCajasRegistradoras { get; set; }

        private AgregarProductosAuditoriaModal DialogAgregarProducto;

        public AuditoriaMercadoViewModel(Page page) : base(page)
        {
            ParAuditoria = myParametro.GetParAuditoriaMercado();

            myAud = new DS_AuditoriasMercados();

            Tamanos = myAud.GetAuditoriaTamanos();

            SaveCommand = new Command(() =>
            {
                PrepareSave();

            }, () => IsUp);

            ProductosAgregados = new ObservableCollection<AuditoriasMercadosTemp>(myAud.GetTemp());

            DialogAgregarProducto = new AgregarProductosAuditoriaModal((s, editing)=> 
            {
                if (!editing)
                {
                    ProductosAgregados.Add(s);
                }
                else
                {
                    var item = ProductosAgregados.Where(x => x.rowguid == s.rowguid).FirstOrDefault();

                    ProductosAgregados[ProductosAgregados.IndexOf(item)] = s;
                }
                
            });

            ShowAddDialogCommand = new Command(()=> { PushModalAsync(DialogAgregarProducto); });
            
        }

        private async void PrepareSave()
        {
            IsUp = false;

            if (IsBusy)
            {
                return;
            }

            if(ProductosAgregados == null || ProductosAgregados.Count == 0)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.YouHaveNotAddAnyArticleWarning);
                return;
            }

            if(CurrentTamano == null)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.MustSpecifySizeInConfigurationTab);
                return;
            }

            int.TryParse(CantCajasRegistradoras, out int cajas);

            if(cajas == 0)
            {
                IsUp = true;
                await DisplayAlert(AppResource.Warning, AppResource.NumberOfCashRegistersMustBeGreaterThanZero);
                return;
            }

            try
            {
                IsBusy = true;

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                await task.Execute(() => { myAud.SaveAuditoria(ProductosAgregados, CurrentTamano.CodigoUso, cajas); });

                await DisplayAlert(AppResource.Success, AppResource.MarketAuditSavedSuccessfully);

                await PopAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorSavingAudit, e.Message);
            }

            IsBusy = false;
            IsUp = true;
        }

        public async void OnListItemSelected(AuditoriasMercadosTemp item)
        {
            try
            {
                var result = await DisplayActionSheet(AppResource.SelectAnOption, buttons: new string[] { AppResource.Edit, AppResource.Remove });


                if(result == AppResource.Edit)
                {
                    DialogAgregarProducto.CurrentProduct = item;
                    await PushModalAsync(DialogAgregarProducto);
                }
                else if(result == AppResource.Remove)
                {
                    myAud.DeleteTemp(item);
                    ProductosAgregados = new ObservableCollection<AuditoriasMercadosTemp>(myAud.GetTemp());
                }
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }
        }

        public void ClearTemp()
        {
            myAud.DeleteTemp();
        }
    }
}

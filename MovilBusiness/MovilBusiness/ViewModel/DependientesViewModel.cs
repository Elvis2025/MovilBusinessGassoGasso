using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using MovilBusiness.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MovilBusiness.ViewModel
{
    public class DependientesViewModel : BaseViewModel
    {
        public ICommand EditarDependienteCommand { get; private set; }
        public ICommand GuardarCambiosCommand { get; private set; }

        public string DependienteIcon { get => EnableDependienteControls ? "ic_close_black_24dp" : "ic_edit_black_24dp"; }
        public bool IsNewDependiente { get => CurrentDependiente != null && CurrentDependiente.Cliid == -2; set { RaiseOnPropertyChanged(); } }

        private ObservableCollection<ClientesDependientes> dependientes;
        public ObservableCollection<ClientesDependientes> Dependientes { get => dependientes; set { dependientes = value; RaiseOnPropertyChanged(); } }

        private ClientesDependientes currentdependiente;
        public ClientesDependientes CurrentDependiente { get => currentdependiente; set { currentdependiente = value; OnCurrentDependienteChanged(); RaiseOnPropertyChanged(); } }

        private bool enabledependientecontrols = false;
        public bool EnableDependienteControls { get => enabledependientecontrols; set { enabledependientecontrols = value; RaiseOnPropertyChanged(); } }

        private bool iseditingdependiente = false;
        public bool IsEditingDependiente { get => iseditingdependiente; set { iseditingdependiente = value; RaiseOnPropertyChanged(); } }

        private bool showediticon;
        public bool ShowEditIcon { get => showediticon; set { showediticon = value; RaiseOnPropertyChanged(); } }

        private bool showsavebutton = false;
        public bool ShowSaveButton { get => showsavebutton; set { showsavebutton = value; RaiseOnPropertyChanged(); } }

        private string currentnocuenta;
        public string CurrentNoCuenta { get => currentnocuenta; set { currentnocuenta = value; RaiseOnPropertyChanged(); } }

        private string dependientecedula;
        private string dependientenombre;
        private string dependientetelefono;

        public string DependienteCedula { get => dependientecedula; set { dependientecedula = value; RaiseOnPropertyChanged(); } }
        public string DependienteNombre { get => dependientenombre; set { dependientenombre = value; RaiseOnPropertyChanged(); } }
        public string DependienteTelefono { get => dependientetelefono; set { dependientetelefono = value; RaiseOnPropertyChanged(); } }

        public List<FormasPago> FormasPago { get; private set; }
        public List<Bancos> Bancos { get; private set; }
        public List<UsosMultiples> TiposCuentaBancarias { get; private set; }
        
        private UsosMultiples currenttipocuentabancaria;
        public UsosMultiples CurrentTipoCuentaBancaria { get => currenttipocuentabancaria; set { currenttipocuentabancaria = value; RaiseOnPropertyChanged(); } }

        private Bancos currentbanco;
        public Bancos CurrentBanco { get => currentbanco; set { currentbanco = value; RaiseOnPropertyChanged(); } }

        private FormasPago currentformapago;
        public FormasPago CurrentFormaPago { get => currentformapago; set { currentformapago = value; OnCurrentFormaPagoChanged(); RaiseOnPropertyChanged(); } }
        public FormasPago FormaPagoEfectivo { get; set; }
        public bool ShowInfoBanco { get { return CurrentFormaPago != null && CurrentFormaPago.FopID == 2; } set { RaiseOnPropertyChanged(); } }

        private DS_UsosMultiples myUso;
        private DS_Clientes myCli;

        public DependientesViewModel(Page page, DS_UsosMultiples myUso, ClientesDependientes editedDependiente) : base(page)
        {
            this.myUso = myUso;
            myCli = new DS_Clientes();

            FormasPago = new DS_FormasPago().GetFormasPago();
            TiposCuentaBancarias = myUso.GetTiposCuentasBancarias();
            Bancos = new DS_Bancos().GetBancos();

            EditarDependienteCommand = new Command(() =>
            {
                if (CurrentDependiente == null || CurrentDependiente.Cliid == -1 || CurrentDependiente.Cliid == -2)
                {
                    return;
                }

                if (EnableDependienteControls)
                {
                    ShowSaveButton = false;
                    IsEditingDependiente = false;
                    EnableDependienteControls = false;
                    OnCurrentDependienteChanged();
                }
                else
                {
                    ShowSaveButton = true;
                    IsEditingDependiente = true;
                    EnableDependienteControls = true;
                }

                RaiseOnPropertyChanged(nameof(DependienteIcon));
            });

            GuardarCambiosCommand = new Command(GuardarCambios);

            LoadDependientesCompras();

            if(editedDependiente != null)
            {
                CurrentDependiente = editedDependiente;
            }
            if (FormasPago != null && FormasPago.Count > 0)
            {
                 FormaPagoEfectivo  = FormasPago.FirstOrDefault(f => f.FopID == 1);               
            }
        }

        private void LoadDependientesCompras()
        {
            Dependientes = new ObservableCollection<ClientesDependientes>(myCli.GetClientesDependientes(Arguments.Values.CurrentClient.CliID));
        }

        private void OnCurrentFormaPagoChanged()
        {
            RaiseOnPropertyChanged(nameof(ShowInfoBanco));

            LoadDataFormaPago();
        }

        private async void GuardarCambios()
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                if (string.IsNullOrWhiteSpace(DependienteCedula))
                {
                    throw new Exception(AppResource.DependentIdCannotBeEmpty);
                }
                
                if(DependienteCedula.Length < 11)
                {
                    throw new Exception(AppResource.MustCompleteIdNumber);
                }

                if (string.IsNullOrWhiteSpace(DependienteNombre))
                {
                    throw new Exception(AppResource.DependentNameCannotBeEmpty);
                }

                if (CurrentFormaPago == null)
                {
                    throw new Exception(AppResource.MustSelectPaymentway);
                }

                if(IsNewDependiente && myCli.ExistsDependiente(Arguments.Values.CurrentClient.CliID, DependienteCedula))
                {
                    throw new Exception(AppResource.DependentAlreadyExistsWarning);
                }

                var task = new TaskLoader() { SqlTransactionWhenRun = true };

                var newDependiente = new ClientesDependientes();
                newDependiente.ClDCedula = DependienteCedula;
                newDependiente.CldTelefono = DependienteTelefono;
                newDependiente.ClDNombre = DependienteNombre;
                newDependiente.FopID = CurrentFormaPago.FopID;
                newDependiente.CldTipoCuentaBancaria = 0;
                newDependiente.CldCuentaBancaria = "0";
                newDependiente.Cliid = Arguments.Values.CurrentClient.CliID;

                if (CurrentFormaPago.FopID == 2)
                {
                    if (CurrentTipoCuentaBancaria != null)
                    {
                        int.TryParse(CurrentTipoCuentaBancaria.CodigoUso, out int tipo);
                        newDependiente.CldTipoCuentaBancaria = tipo;
                    }

                    if (!string.IsNullOrWhiteSpace(CurrentNoCuenta))
                    {
                        newDependiente.CldCuentaBancaria = CurrentNoCuenta;
                    }

                    if(CurrentBanco != null)
                    {
                        newDependiente.BanID = CurrentBanco.BanID;
                    }
                }

                await task.Execute(() => 
                {                    
                    if (IsEditingDependiente && CurrentDependiente != null)
                    {
                        myCli.EditarDependiente(CurrentDependiente, newDependiente);
                    }else if (IsNewDependiente)
                    {
                        myCli.CrearDependiente(newDependiente);
                    }
                });

                await DisplayAlert(AppResource.Success, IsEditingDependiente ? AppResource.DependentEditedSuccessfully : AppResource.DependentCreatedSuccessFully);

                LoadDependientesCompras();

                var temp = Dependientes.Where(x => x.ClDCedula == newDependiente.ClDCedula && x.Cliid == newDependiente.Cliid).FirstOrDefault();

                CurrentDependiente = temp;

                RaiseOnPropertyChanged(nameof(CurrentDependiente));

                IsEditingDependiente = false;

                IsNewDependiente = false;
                EnableDependienteControls = false;
                ShowSaveButton = false;
            }
            catch(Exception e)
            {
                await DisplayAlert(AppResource.Warning, e.Message);
            }

            IsBusy = false;
        }

        private void OnCurrentDependienteChanged()
        {
            IsNewDependiente = false;
            EnableDependienteControls = false;
            ShowSaveButton = false;

            if (CurrentDependiente == null || CurrentDependiente.Cliid == -1)
            {
                ClearComponents();
                return;
            }

            if (CurrentDependiente.Cliid == -2)
            {
                ClearComponents();
                ShowSaveButton = true;
                EnableDependienteControls = true;
                if (FormaPagoEfectivo != null )
                {
                    CurrentFormaPago = FormaPagoEfectivo;
                }
                return;
            }
            
            ShowEditIcon = true;
            DependienteCedula = CurrentDependiente.ClDCedula;
            DependienteTelefono = CurrentDependiente.CldTelefono;
            DependienteNombre = CurrentDependiente.ClDNombre;

            if (FormasPago != null && FormasPago.Count > 0)
            {
                CurrentFormaPago = FormasPago.Where(x => x.FopID == CurrentDependiente.FopID).FirstOrDefault();

                LoadDataFormaPago();
            }
        }

        private void ClearComponents()
        {
            EnableDependienteControls = false;
            ShowSaveButton = false;
            ShowEditIcon = false;
            DependienteCedula = null;
            DependienteTelefono = null;
            DependienteNombre = null;
            CurrentFormaPago = null;
            CurrentNoCuenta = null;
            CurrentTipoCuentaBancaria = null;
            CurrentBanco = null;
          
        }

        private void LoadDataFormaPago()
        {
            if (!ShowInfoBanco || CurrentDependiente == null || CurrentFormaPago == null)
            {
                if(CurrentDependiente == null && CurrentFormaPago != null)
                {
                     CurrentFormaPago = null;                  
                }
              

                CurrentNoCuenta = null;
                CurrentTipoCuentaBancaria = null;
                CurrentBanco = null;
                return;
            }

            CurrentNoCuenta = CurrentDependiente.CldCuentaBancaria;
            if(TiposCuentaBancarias != null && TiposCuentaBancarias.Count > 0)
            {
                CurrentTipoCuentaBancaria = TiposCuentaBancarias.Where(x => x.CodigoUso == CurrentDependiente.CldTipoCuentaBancaria.ToString()).FirstOrDefault();
            }
            
            if(Bancos != null && Bancos.Count > 0)
            {
                CurrentBanco = Bancos.Where(x => x.BanID == CurrentDependiente.BanID).FirstOrDefault();
            }
            
        }
    }

    
}

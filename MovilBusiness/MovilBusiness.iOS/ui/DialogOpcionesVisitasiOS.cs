using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(IDialogOpcionesVisita))]
namespace MovilBusiness.iOS.ui
{
    public class DialogOpcionesVisitasiOS : IDialogOpcionesVisita
    {
        private UIAlertController alert;
        private Action<OpcionesClientes> OptionButtonSelected;

        public DialogOpcionesVisitasiOS()
        {
         
            alert = UIAlertController.Create("titulo", "Seleccione la opción deseada", UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? UIAlertControllerStyle.Alert : UIAlertControllerStyle.ActionSheet);
            alert.AddAction(UIAlertAction.Create("Crear visita", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.CrearVisita); }));

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasFallidas())
            {
                alert.AddAction(UIAlertAction.Create("Visita fallida", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.VisitaFallida); }));
            }
            alert.AddAction(UIAlertAction.Create("Consultar cliente", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.ConsultarCliente); }));
            
            alert.AddAction(UIAlertAction.Create("Información cliente", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.InformacionCliente); }));
            alert.AddAction(UIAlertAction.Create("Ubicación cliente", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.UbicacionCliente); }));
            alert.AddAction(UIAlertAction.Create("Consultar ultimas visitas", UIAlertActionStyle.Default, (s) => { OptionButtonSelected?.Invoke(OpcionesClientes.UltimasVisitas); }));

            alert.AddAction(UIAlertAction.Create("Cerrar", UIAlertActionStyle.Cancel, null));
        }

        public void Dismiss()
        {
            alert.DismissViewController(true, null);
        }

        public void SetEventHandler(Action<OpcionesClientes> eventHandler)
        {
            OptionButtonSelected = eventHandler;
        }

        public void Show(string clinombre, string clicodigo)
        {
            alert.Title = clinombre;
            
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }
    }
}
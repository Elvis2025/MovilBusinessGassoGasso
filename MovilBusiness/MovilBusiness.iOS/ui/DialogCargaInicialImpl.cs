using System;
using MovilBusiness.Abstraction;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(IDialogOpcionesVisita))]
namespace MovilBusiness.iOS.ui
{
    public class DialogCargaInicialImpl : IDialogCargaInicial
    {
        private Action<string, string, string> OnAceptar;
        private UIAlertController Alert;

        public DialogCargaInicialImpl()
        {
            Alert = UIAlertController.Create("Carga inicial", "Complete todos los datos para proceder a la carga inicial", UIAlertControllerStyle.Alert);
            Alert.AddTextField(textField => { textField.Placeholder = "Usuario"; textField.ReturnKeyType = UIReturnKeyType.Next; });
            Alert.AddTextField(textField => { textField.Placeholder = "Contraseña"; textField.SecureTextEntry = true; textField.ReturnKeyType = UIReturnKeyType.Next; });
            Alert.AddTextField(textField => { textField.Placeholder = "Key"; textField.ReturnKeyType = UIReturnKeyType.Done; });
            Alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Default, action => { }));
            Alert.AddAction(UIAlertAction.Create("Aceptar", UIAlertActionStyle.Default, action => { OnAceptar?.Invoke(Alert.TextFields[0].Text.Trim(), Alert.TextFields[1].Text.Trim(), Alert.TextFields[2].Text.Trim()); }));
        }

        public void Show(Action<string, string, string> OnAceptar)
        {
            this.OnAceptar = OnAceptar;

            var controller = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if(controller == null)
            {
                return;
            }

            controller.PresentViewController(Alert, true, null);
        }
    }
}
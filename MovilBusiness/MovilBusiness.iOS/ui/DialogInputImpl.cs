using System;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.ui;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DialogInputImpl))]
namespace MovilBusiness.iOS.ui
{
    public class DialogInputImpl : IDialogInput
    {
        public void Show(string title, string message, Action<string> result, Keyboard keyboard, string defaultText = "", bool isPassword = false)
        {
            var dialog = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            dialog.AddTextField((txt) => { if (keyboard == Keyboard.Numeric) { OnlyAllowNumber(txt, false); } txt.Placeholder = defaultText; });
            dialog.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, null));
            dialog.AddAction(UIAlertAction.Create("Aceptar", UIAlertActionStyle.Default, (a) => { result?.Invoke(dialog.TextFields[0].Text.ToString()); }));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(dialog, true, null);
        }

        private void OnlyAllowNumber(UITextField edit, bool allowDecimals)
        {
            edit.ShouldChangeCharacters = (a, b, c) =>
            {

                bool valid = true;

                if (allowDecimals)
                {
                    valid = (c.EndsWith(".") && !a.Text.Contains(".") && a.Text.Trim().Length > 0);
                }


                return double.TryParse(c, out double value) || c.Length < b.Length || valid;
            };
        }
    }
}
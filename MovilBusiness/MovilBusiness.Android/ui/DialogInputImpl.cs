using System;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.ui;
using Xamarin.Forms;

[assembly: Dependency(typeof(DialogInputImpl))]
namespace MovilBusiness.Droid.ui
{
    public class DialogInputImpl : IDialogInput
    {
        private Action<string> onAceptar;

        public void Show(string title, string message, Action<string> result, Keyboard keyboard, string defaultText = "", bool isPassword = false)
        {
            var input = new EditText(MainActivity.Instance)
            {
                Gravity = Android.Views.GravityFlags.Center,
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
                Text = defaultText
            };

            if (isPassword)
            {
                input.TransformationMethod = PasswordTransformationMethod.Instance;
            }

            var alert = new AlertDialog.Builder(MainActivity.Instance)
                .SetPositiveButton("Aceptar", (s, a) => { onAceptar?.Invoke(input.Text.ToString()); })
                .SetNegativeButton("Cancelar", (s, a) => { })
                .SetView(input);

            if (keyboard == Keyboard.Numeric)
            {
                input.InputType = Android.Text.InputTypes.ClassNumber;
            }

            alert.SetTitle(title);
            alert.SetMessage(message);
            onAceptar = result;
            alert.Show();
        }
    }
}
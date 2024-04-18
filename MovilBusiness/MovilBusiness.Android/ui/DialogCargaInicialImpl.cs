using System;

using Android.App;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.ui;
using Xamarin.Forms;

[assembly: Dependency(typeof(DialogCargaInicialImpl))]
namespace MovilBusiness.Droid.ui
{
    public class DialogCargaInicialImpl : Dialog, IDialogCargaInicial
    {
        private EditText editUser, editPass, editKey;
        private Action<string, string, string> OnAceptar;

        public DialogCargaInicialImpl() : base(MainActivity.Instance)
        {
            Window.RequestFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.dialog_carga_inicial);
            SetCancelable(false);
            Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            editUser = FindViewById<EditText>(Resource.Id.editUsuario);
            editPass = FindViewById<EditText>(Resource.Id.editClave);
            editKey = FindViewById<EditText>(Resource.Id.editSuscriptor);

            FindViewById(Resource.Id.btnAceptar).Click += delegate { OnAceptar?.Invoke(editUser.Text.Trim(), editPass.Text.Trim(), editKey.Text.Trim()); Dismiss(); };
            FindViewById(Resource.Id.btnCancelar).Click += delegate { Dismiss(); };

        }

        public void Show(Action<string, string, string> OnAceptar)
        {
            this.OnAceptar = OnAceptar;

            base.Show();
        }
    }
}
using System;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Droid.ui;
using MovilBusiness.Droid.utils;
using MovilBusiness.Enums;

[assembly: Xamarin.Forms.Dependency(typeof(DialogOpcionesVisitasDroid))]
//[assembly: ExportRenderer(typeof(DialogOpcionesVisita), typeof(DialogOpcionesVisitasDroid))]
namespace MovilBusiness.Droid.ui
{
    public class DialogOpcionesVisitasDroid : Java.Lang.Object, IDialogOpcionesVisita//, Android.Views.View.IOnClickListener
    {
        private Action<OpcionesClientes> OptionButtonSelected;
        //private TextView lblNombre, lblCodigo;
        //private BottomSheetDialog dialog;

        public DialogOpcionesVisitasDroid()
        {
            

        }
        /*
        public void OnClick(Android.Views.View v)
        {
            try
            {

                if(v.Id == Resource.Id.btnCrearVisita)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.CrearVisita);
                }
                else if(v.Id == Resource.Id.btnVisitaFallida)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.VisitaFallida);
                }
                else if(v.Id == Resource.Id.btnConsultarCliente)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.ConsultarCliente);
                }
                else if(v.Id == Resource.Id.btnInfoCliente)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.InformacionCliente);
                }
                else if(v.Id == Resource.Id.btnClienteUbicacion)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.UbicacionCliente);
                }
                else if(v.Id == Resource.Id.btnConsultarVisitas)
                {
                    OptionButtonSelected?.Invoke(OpcionesClientes.UltimasVisitas);
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            Dismiss();
        }*/

        public void SetEventHandler(Action<OpcionesClientes> eventHandler)
        {
            OptionButtonSelected = eventHandler;
        }

        public void Show(string clinombre, string cliCodigo)
        {
            var dialog = new BottomSheetDialog(MainActivity.Instance);

            Android.Views.View v = Android.Views.View.Inflate(MainActivity.Instance, Resource.Layout.dialog_opciones_visita, null);

            dialog.SetContentView(v);

            BottomSheetBehavior behavior = BottomSheetBehavior.From((Android.Views.View)v.Parent);

            if (behavior == null)
            {
                return;
            }

            behavior.SetBottomSheetCallback(new BottomSheetCallBack(() => { Dismiss(); }));

            var lblNombre = dialog.FindViewById<TextView>(Resource.Id.lblNombre);
            var lblCodigo = dialog.FindViewById<TextView>(Resource.Id.lblCodigo);

            if (!DS_RepresentantesParametros.GetInstance().GetParVisitasFallidas())
            {
                dialog.FindViewById(Resource.Id.btnVisitaFallida).Visibility = ViewStates.Gone;
            }

            dialog.FindViewById(Resource.Id.btnCrearVisita).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.CrearVisita); dialog.Dismiss(); };//.SetOnClickListener(this);
            dialog.FindViewById(Resource.Id.btnVisitaFallida).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.VisitaFallida); dialog.Dismiss(); };//.SetOnClickListener(this);
            dialog.FindViewById(Resource.Id.btnConsultarCliente).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.ConsultarCliente); dialog.Dismiss(); };//.SetOnClickListener(this);
            dialog.FindViewById(Resource.Id.btnInfoCliente).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.InformacionCliente); dialog.Dismiss(); };//.SetOnClickListener(this);
            dialog.FindViewById(Resource.Id.btnClienteUbicacion).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.UbicacionCliente); dialog.Dismiss(); };//.SetOnClickListener(this);
            dialog.FindViewById(Resource.Id.btnConsultarVisitas).Click += delegate { OptionButtonSelected?.Invoke(OpcionesClientes.UltimasVisitas); dialog.Dismiss(); };//.SetOnClickListener(this);

            lblNombre.Text = clinombre;
            lblCodigo.Text = cliCodigo;

            dialog.Show();
        }
        
        public void Dismiss()
        {
            /*if(MainActivity.Instance != null && !MainActivity.Instance.IsFinishing)
            {
                dialog.Dismiss();
            }*/         
        }
    }
}
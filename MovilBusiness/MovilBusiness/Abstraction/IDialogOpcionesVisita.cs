using MovilBusiness.Enums;
using System;

namespace MovilBusiness.Abstraction
{
    public interface IDialogOpcionesVisita
    {
        void Show(string clinombre, string clicodigo);
        void Dismiss();
        /*al tocar algun boton de las opciones se ejecuta este evento con el id del boton seleccionado*/
        void SetEventHandler(Action<OpcionesClientes> eventHandler);
    }
}

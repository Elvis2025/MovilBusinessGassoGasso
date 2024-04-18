using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Abstraction
{
    public interface IDialogCargaInicial
    {
        void Show(Action<string, string, string> OnAceptar);
    }
}

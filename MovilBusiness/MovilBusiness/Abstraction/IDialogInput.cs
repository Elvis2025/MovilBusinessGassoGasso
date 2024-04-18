using System;
using Xamarin.Forms;

namespace MovilBusiness.Abstraction
{
    public interface IDialogInput
    {
        void Show(string title, string message, Action<string> result, Keyboard keyboard, string defaultText = "", bool isPassword = false);
    }
}

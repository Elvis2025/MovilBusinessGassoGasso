using System;
using Xamarin.Forms;

namespace MovilBusiness.Controls
{
    public class CheckBoxs : View
    {
        public event EventHandler Checked;
        protected virtual void OnChecked(EventArgs e)
        { 
           if(Checked != null)
           {
             Checked(this, e);
           }
        
        }

        public static readonly BindableProperty IsCheckedProperty =
            BindableProperty.Create("IsChecked",
                typeof(bool),
                typeof(CheckBoxs),
                false,
                propertyChanged: (bindable, oldvalue, newvalue) =>
                {
                    (bindable as CheckBoxs).OnChecked(new EventArgs());
                });

        public bool IsChecked { get { return (bool)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }


    }
}

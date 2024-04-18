using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MovilBusiness.Controls
{
    public class MyDatePicker: DatePicker
    {
        public static readonly BindableProperty EnterTextProperty = BindableProperty.Create(propertyName: "Placeholder", returnType: typeof(string), 
            declaringType: typeof(MyDatePicker), defaultValue: default(string));
        public string Placeholder { get; set; }


    }
}

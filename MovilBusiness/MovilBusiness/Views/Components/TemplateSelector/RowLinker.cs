
using MovilBusiness.model.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace MovilBusiness.Views.Components.TemplateSelector
{
    public class RowLinker
    {
        public bool Bold { get; set; }
        public bool IsHeader { get; set; }
       
        public FontAttributes FontAttribute { get => Bold ? FontAttributes.Bold : FontAttributes.None; }

    }
}

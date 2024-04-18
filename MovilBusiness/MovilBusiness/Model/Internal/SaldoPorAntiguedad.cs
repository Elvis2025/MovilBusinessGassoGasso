using MovilBusiness.Views.Components.TemplateSelector;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class SaldoPorAntiguedad: RowLinker
    {
        public string Desde { get; set; }
        public string Hasta { get; set; }
        public double Balance { get; set; }
    }
}

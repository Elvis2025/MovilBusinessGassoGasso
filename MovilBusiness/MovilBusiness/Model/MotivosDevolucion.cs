using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class MotivosDevolucion
    {
        public int MotID { get; set; }
        public string MotDescripcion { get; set; }
        public string MotReferencia { get; set; }
        public string MotCamposObligatorios { get; set; }
        //public int AlmID { get; set; }
        public string MotCaracteristicas { get; set; }
        public string rowguid { get; set; }
        public string MotEstatus { get; set; }

        public override string ToString()
        {
            return MotDescripcion;
        }
    }
}

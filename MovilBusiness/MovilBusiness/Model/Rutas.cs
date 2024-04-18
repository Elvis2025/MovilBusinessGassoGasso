using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Rutas
    {
        public int RutID { get; set; }
        public string RutDescripcion { get; set; }
        public int TerID { get; set; }
        public double RutPeaje { get; set; }
        public double RutDieta { get; set; }


        public override string ToString()
        {
            return RutDescripcion;
        }
    }
}

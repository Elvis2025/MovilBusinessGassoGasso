using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class TransaccionesTracking
    {
        public string rowguid { get; set; }
        public string RepCodigo { get; set; }
        public int TraID { get; set; }
        public string TraEstado { get; set; }
        public string TraKey { get; set; }
        public string TraMensaje { get; set; }
        public string TraFecha { get; set; }

        public string Title
        {
            get
            {
                DateTime.TryParse(TraFecha, out DateTime fecha);

                return TraEstado + " - " + fecha.ToString("dd-MM-yyyy hh:mm tt");
            }
        }
    }
}

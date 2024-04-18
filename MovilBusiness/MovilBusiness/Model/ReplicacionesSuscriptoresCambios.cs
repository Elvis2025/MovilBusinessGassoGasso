using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class ReplicacionesSuscriptoresCambios
    {
        public string RscKey { get; set; }
        public int RepID { get; set; }
        public string RepSuscriptor { get; set; }
        public string RscTabla { get; set; }
        public string RscTablarowguid { get; set; }
        public string RscTipTran { get; set; }
        public string UsuInicioSesion { get; set; }
        public string RscScript { get; set; }
    }
}

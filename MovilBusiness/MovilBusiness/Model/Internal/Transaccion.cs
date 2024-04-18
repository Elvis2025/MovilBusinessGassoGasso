using System;

namespace MovilBusiness.model.Internal
{
    public class Transaccion
    {
        public int TransaccionID { get; set; }
        public string DataField { get; set; }
        public string RepCodigo { get; set; }
        public string TransacionDescripcion { get; set; }
        public int CantidadDiasDesdeCreacion { get; set; }
        public int CliID { get; set; }
        public string Seccodigo { get; set; }
        public string SecDescripcion { get; set; }
        public bool UseSector { get; set; }
        public string Fecha { get; set; }
        public string TraKey { get; set; }
        public string Estatus { get; set; }
        public string EstatusDescripcion { get; set; }
        public bool ShowEstatus { get; set; }
        public bool IsPedConfirmados { get; set; }
        public string NumeroERP { get; set; }

        public int TransaccionIDNCDPP { get; set; }
        public string FechaFormateada
        {
            get {
                if (DateTime.TryParse(Fecha, out DateTime fecha))
                {
                    return fecha.ToString("dd/MM/yyyy hh:mm tt");
                }
                return Fecha;
            }
        } 
    }
}

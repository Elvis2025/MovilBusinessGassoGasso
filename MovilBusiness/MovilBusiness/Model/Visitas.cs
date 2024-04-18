using MovilBusiness.Utils;
using SQLite;

namespace MovilBusiness.model
{
    public class Visitas
    {
        public string RepCodigo { get; set; }
        public int VisSecuencia { get; set; }
        public int CliID { get; set; }
        public string VisFechaEntrada { get; set; }
        public string VisFechaSalida { get; set; }
        public int VisTipoVisita { get; set; }
        public float VisLatitud { get; set; }
        public float VisLongitud { get; set; }
        public int VisEstatus { get; set; }
        public string rowguid { get; set; }
        public string Comentario { get; set; }
        public int VisIndicadorFueraRuta { get; set; }
        public int VisIndicadorFueraOrden { get; set; }

        public string VisEstatusIcon { get => Functions.GetEstatusVisitaIcon(VisEstatus); }

        [Ignore] public string FechaEntrada { get => Functions.FormatDate(VisFechaEntrada, "dd/MM/yyyy hh:mm tt"); }
        [Ignore] public string FechaSalida { get => Functions.FormatDate(VisFechaSalida, "dd/MM/yyyy hh:mm tt"); }

        //se llena cuando se entra a una visita virtual desde una visita normal, es la secuencia de la visita normal
        public int VisSecuenciaOrigen { get; set; }
        public double VisPorcientoBateria { get; set; }
    }
}

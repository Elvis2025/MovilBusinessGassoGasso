using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class Ofertas
    {
        public int OfeID { get; set; }
        public int CliID { get; set; }
        public int CanID { get; set; }
        public int TinID { get; set; }
        public int LinID { get; set; }
        public int CatID { get; set; }
        public int ProID { get; set; }
        public int ConID { get; set; }
        public int OfeCantidadMax { get; set; }
        public int OfeIndicadorGrupo { get; set; }
        public string GrpCodigo { get; set; }
        public string GrcCodigo { get; set; }
        public string OfeDescripcion { get; set; }
        public string OfeFechainicio { get; set; }
        public string OfeFechaFin { get; set; }
        public string OfeTipo { get; set; }
        public int ProNoVendido { get; set; }
        public bool OfeIndicadorRebajaVenta { get; set; }
        public string grpCodigoOferta { get; set; }
        public int OfeNivel { get; set; }
        public string OfeFuente { get; set; }
        public int OfeTipoMezcla { get; set; }
        public int OfeCantidadMezcla { get; set; }
        public double OfeCantidadMaximaTransaccion { get; set; }

        public string ConIdDescripcion { get; set; }
        public string OfeTipoDescripcion { get; set; }
        public double CantidadTemp { get; set; }

        public string UnmCodigo { get; set; }
        public bool IsMancomunada { get => OfeTipo == "3" || OfeTipo == "13" || OfeTipo == "14";}
        public string OfeCaracteristicas { get; set; }
        [Ignore][JsonIgnore]public bool OfeIndicadoCompleta { get; set; }
        [Ignore][JsonIgnore]public bool OfeIndicadoInCompleta { get; set; }

        public Ofertas Copy()
        {
            return (Ofertas)MemberwiseClone();
        }

        [Ignore]
        public string FechaDesde
        {
            get
            {
                DateTime.TryParse(OfeFechainicio, out DateTime fecha);

                return fecha.ToString("dd-MM-yyyy \nHH:mm:ss");
            }
        }
        [Ignore]
        public string FechaHasta
        {
            get
            {
                DateTime.TryParse(OfeFechaFin, out DateTime fecha);
                return fecha.ToString("dd-MM-yyyy \nHH:mm:ss");
            }
        }
        public int CantidadVentasAcumulada { get; set; }
        public int CantidadOfertasAcumulada { get; set; }
        public bool IsOfertaAcumulada { get; set; }

        public bool isConsultaGeneral { get; set; }
    }
}

using MovilBusiness.Configuration;
using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class RutaVisitasFecha
    {
        public string RepCodigo { get; set; }
        public string RutFecha { get; set; }
        public int CliID { get; set; }
        public int RutPosicion { get; set; }
        public string CliNombre { get; set; }
        public string CliCodigo { get; set; }
        public string CliDatosOtros { get; set; }
        public string CliCalle { get; set; }
        public string RutEstado { get; set; }
        public string rowguid { get; set; }

        public bool IsAsignado { get; set; }
        public int SemNumeroSemana { get; set; }
        [Ignore]public bool ShowDireccion { get => !string.IsNullOrWhiteSpace(CliCalle); }
        [Ignore]public bool ShowDatosOtros { get => !string.IsNullOrWhiteSpace(CliDatosOtros); }
        [Ignore] public string CliNombreCompleto { get { return CliNombre + " - " + CliCodigo; } }
        [Ignore] public bool ShowCreated { get => !string.IsNullOrWhiteSpace(RutEstado) && RutEstado.Trim() == "4"; }
        [Ignore] public string RutEstadoDescripcion { get => !string.IsNullOrWhiteSpace(RutEstado) ? RutEstado.Trim() == "2" ? "Transmitido" : RutEstado.Trim() == "4" ? "Creado" : "" : ""; }
       
        [Ignore] public string FechaLabel
        {
            get
            {
                if(DateTime.TryParse(RutFecha, out DateTime fecha))
                {
                    return fecha.ToString("dd-MM-yyyy");
                }
                else
                {
                    return RutFecha;
                }
            }
        }

        public bool DeleteReal { get; set; }

        public string Background { get => DeleteReal ? "#EF5350" : "White"; }

        public string CliDatosOtrosLabel
        {
            get => Functions.GetCliDatosOtrosLabel(CliDatosOtros);
        }

        public RutaVisitasFecha Copy()
        {
            return (RutaVisitasFecha)MemberwiseClone();
        }
    }
}

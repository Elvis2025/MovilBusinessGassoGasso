using MovilBusiness.DataAccess;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Vehiculos
    {
        public int VehID { get; set; }
        public int MarID { get; set; }
        public int ModID { get; set; }
        public int VehAnio { get; set; }
        public string VehChasis { get; set; }
        public string VehFicha { get; set; }
        public string VehReferencia { get; set; }
        public int VehKilometraje { get; set; }
        public int VehContador { get; set; }
        public double VehVolumenMaximo { get; set; }
        public double VehPesoMaximo { get; set; }
        public string VehFechaActualizacion { get; set; }
        public string UsuInicioSesion { get; set; }
        public string rowguid { get; set; }
        public string  ModDescripcion { get; set; }
        public string MarDescripcion { get; set; }

        public double VehRecorridoPromXdia { get; set; }



        //[Ignore] public string VehiculoDetalle { get => VehFicha + " " + MarDescripcion + " " + ModDescripcion;}
        [Ignore] public string VehiculoDetalle { get { return (DS_RepresentantesParametros.GetInstance().GetParVehiculoDescripcion() == 1 ? VehFicha + " " + VehReferencia : DS_RepresentantesParametros.GetInstance().GetParVehiculoDescripcion() == 2 ? VehChasis : VehFicha + " " + MarDescripcion + " " + ModDescripcion); } }
    }
}

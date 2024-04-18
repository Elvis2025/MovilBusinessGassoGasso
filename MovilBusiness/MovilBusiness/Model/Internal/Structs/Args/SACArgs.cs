using MovilBusiness.Model.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.Structs.Args
{
    public class SACArgs
    {
        public string Nombre { get; set; }
        public string Propietario { get; set; }
        public string Cedula { get; set; }
        public string SitioWeb { get; set; }
        public string Email { get; set; }
        public string Contacto { get; set; }
        public int ProvinciaId { get; set; }
        public int MunicipioId { get; set; }
        public string Sector { get; set; }
        public string CliSector { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaNacContacto { get; set; }
        public string Telefono { get; set; }
        public string Fax { get; set; }
        public string EncargadoPago { get; set; }
        public string CliCasa { get; set; }
        public bool Depositos { get; set; }
        public bool DepositaFactura { get; set; }
        public bool OrdenCompra { get; set; }
        public double Longitud { get; set; }
        public double Latitud { get; set; }
        public int ClaID { get; set; }
        public string CliRegMercantil { get; set; }
        public int TinID { get; set; }
        public int CanID { get; set; }
        public int CliTipoLocal { get; set; }
        public int CliTipoCliente { get; set; }
        public string CliTipoComprobanteFAC { get; set; }
        public int CliRutPosicion { get; set; }
        public string CliFrecuenciaVisita { get; set; }
        public string CliRutSemana1 { get; set; }
        public string CliRutSemana2 { get; set; }
        public string CliRutSemana3 { get; set; }
        public string CliRutSemana4 { get; set; }
        public int CliOrdenRuta { get; set; }
        public string CliUrbanizacion { get; set; }

        public List<SolicitudActualizacionClientesHorarios> Horarios { get; set; }
    }
}

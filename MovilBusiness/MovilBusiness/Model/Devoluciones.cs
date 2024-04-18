using MovilBusiness.Utils;
using MovilBusiness.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Devoluciones
    {
        public string RepCodigo { get; set; }
        public string DevTipo { get; set; }
        public int DevSecuencia { get; set; }
        public int CliID { get; set; }
        public string DevFecha { get; set; }
        public string DevFechaFormatted { get; set; }
        public int DevEstatus { get; set; }
        public int DevTotal { get; set; }
        public string DevReferencia { get; set; }
        public string DevNCF { get; set; }
        public int DevAccion { get; set; }
        public int CuaSecuencia { get; set; }
        public int VisSecuencia { get; set; }
        public string OrvCodigo { get; set; }
        public string OfvCodigo { get; set; }
        public string MonCodigo { get; set; }
        public string SecCodigo { get; set; }
        public string DevCintillo { get; set; }
        public string rowguid { get; set; }
        public int DevCantBultos { get; set; }
        public string Motivo { get; set; }
        public int DevCantidadImpresion { get; set; }

        public string CliCodigo { get; set; }
        public string CliNombre { get; set; }
        public string DevOtrosDatos { get; set; }
        public string Descripcion
        {
            get => "Dev No." + DevSecuencia + ", Fact: "+ DevReferencia + ",  Fecha: " + Functions.FormatDate(DevFecha, "dd/MM/yyyy");
        }
        public double DevMontoTotal { get; set; }
        public double DevMontoSinITBIS { get; set; }
        public double DevMontoITBIS { get; set; }
        public double DevSubTotal { get; set; }
        public double DevPorCientoDsctoGlobal { get; set; }
        public double DevMontoDsctoGlobal { get; set; }
    }
}

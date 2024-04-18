using MovilBusiness.DataAccess;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class RecibosDocumentosTemp
    {
        public string RepCodigo { get; set; }
        public string Fecha { get; set; }
        public string FechaSinFormatear { get; set; }
        public string FechaEntrega { get; set; }
        public string Documento { get; set; }
        public string Referencia { get; set; }
        public string Sigla { get; set; }
        public double Aplicado { get; set; }
        public double Descuento { get; set; }
        public double MontoTotal { get; set; }
        public double Balance { get; set; }
        public double Pendiente { get; set; }
        public string Estado { get; set; }
        public double Credito { get; set; }
        public double CreditoSinItbis { get; set; }
        public string FechaIngles { get; set; }
        public int Origen { get; set; }
        public double MontoSinItbis { get; set; }
        public double DescPorciento { get; set; } //cuando se le pone el porciento manual a la factura es decir, se selecciona del combobox se guarda aqui
        public int AutSecuencia { get; set; }
        public string FechaDescuento { get; set; }
        public string FechaChequeDif { get; set; } //cuando se agrega un chk diferido este campo se actualiza con la fecha de dicho chk
        public int Dias { get; set; }
        public int DiasVencido { get; set; }
        public double DescuentoFactura { get; set; }
        //public double MontoADescuento { get; set; }
        public string Clasificacion { get; set; }
        public string FechaVencimiento { get; set; }
        public bool CalcularDesc { get; set; }
        public double Retencion { get; set; }
        public bool IndicadorNotaCreditoAplicada { get; set; } //si se le aplica una NC al documento este indicador se activa
        public double CxcNotaCredito { get; set; } //el monto de nota de credito que viene desde la empresa
        public bool DefIndicadorItbis { get; set; }
        public string CXCNCF { get; set; }
        public string cxcComentario { get; set; }
        public bool EsReconciliacion { get; set; }
        public bool RecVerificarDesc { get; set; }
        public string MonCodigo { get; set; }

        //public bool IndicadorDescuentoQuitado { get; set; } //en recibos al ver el detalle de la factura y darle al btn de quitar desc. se activa

        public override string ToString()
        {
            return Documento;
        }

        public RecibosDocumentosTemp Copy()
        {
            return (RecibosDocumentosTemp)MemberwiseClone();
        }

        //public string DocumentoMonto { get { return DS_RepresentantesParametros.GetInstance().GetParAplicarNotaCreditoMontoTotal() ? Documento + " - " + Aplicado.ToString("N2") : Documento + " - " + Balance.ToString("N2"); } }
        public string DocumentoMonto { get; set; }
        [Ignore] public double MontoItbis { get => MontoTotal - MontoSinItbis; }
        [Ignore] public string PorcientoDescuento { get => DescPorciento + "%"; }
        [Ignore] public string RowColor { get { return Estado == "Saldo" || Estado == "Abono" || Estado == "Aplicada" || IndicadorNotaCreditoAplicada ? "#90CAF9"/*"#64B5F6"*/ : "White"; ; } }
        [Ignore]
        public int DiasChequeDif
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FechaChequeDif))
                {
                    return Dias;
                }
                else
                {
                    return (DateTime.Parse(FechaChequeDif) - DateTime.ParseExact(Fecha, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Days;
                }
            }
        }

        

        [Ignore]
        public string FechaReciboDif
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FechaChequeDif))
                {
                    return FechaDescuento;
                }
                else
                {
                    DateTime.TryParse(FechaChequeDif, out DateTime fecha);
                    return fecha.ToString("dd-MM-yyyy");
                }
            }
        }
        [Ignore] public int RecTipo { get; set; } = 2;
        public string CXCNCFAfectado { get; set; }
        public double Tasa { get; set; }
        public double TasaDocumento { get; set; }
        public string CxcColor { get; set; }
        public double RecMontoNcDevolver { get; set; }
        public string CxcColorToshow { get => !string.IsNullOrEmpty(CxcColor) && CxcColor.Length == 7 && RowColor == "White" ? CxcColor : RowColor; }
        public bool DefIndicadorNoRestantes { get; set; }
        public string AplicaDescuento { get; set; }
        public double Desmonte { get; set; }
        public bool CalcularDesmonte { get; set; }
        public bool RecVerificarDesmonte { get; set; }
        public string Descripcion { get; set; }
        public double AplicacionesMonto { get; set; }
    }
}

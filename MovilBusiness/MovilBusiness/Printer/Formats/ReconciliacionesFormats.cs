using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class ReconciliacionesFormats : IPrinterFormatter
    {
        private DS_Reconciliaciones myRen;

    public ReconciliacionesFormats(DS_Reconciliaciones myRen)
        {
            this.myRen = myRen;
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var reconciliacion = myRen.GetReconciliacionBySecuencia(traSecuencia, "1", confirmado);

            var detalles = myRen.GetReconciliacionDetalleBySecuencia(traSecuencia, "1", confirmado);

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                if (reconciliacion.RecCantidadImpresion > 1)
                {
                    printer.DrawText("Copia #" + reconciliacion.RecCantidadImpresion);
                }
            }

            printer.Font = PrinterFont.TITLE;
            printer.DrawText("RECONCILIACION: " + reconciliacion.RepCodigo.Trim() + " - " + reconciliacion.RecSecuencia);
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (reconciliacion.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D A");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("Fecha: " + Functions.FormatDate(reconciliacion.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + reconciliacion.CliCodigo);
            printer.DrawText("Cliente: " + reconciliacion.CliNombre, 48);

            printer.DrawText("Sector: " + reconciliacion.CliSector);
            printer.DrawText("");
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawLine();

            //foreach documentos

            double valorTotal = 0.0;
           
            foreach (var app in detalles)
            {
                string sigla = app.CxcSigla;
                
                printer.DrawText(sigla.PadRight(11) + app.CxcDocumentoAplica.PadRight(21) + (app.RefValor).ToString("N2").PadLeft(13));

                valorTotal += app.RefValor;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor total conciliado:    " + valorTotal.ToString("N2").PadLeft(29));
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato reconciliaciones 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRen.ActualizarCantidadImpresion(traSecuencia, confirmado, reconciliacion.RecTipo, reconciliacion.RecCantidadImpresion);

            printer.Print();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            Print(traSecuencia, confirmado, printer);
        }
    }
}

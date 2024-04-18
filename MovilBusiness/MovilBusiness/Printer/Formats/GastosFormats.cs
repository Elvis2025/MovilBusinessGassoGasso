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
  public  class GastosFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Gastos mygas;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public GastosFormats(DS_Gastos myGas)
        {
            this.mygas = myGas;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int GasSecuencia, PrinterManager printer, string rowguid = "") { Print(GasSecuencia, false, printer, rowguid); }
        public void Print(int GasSecuencia, bool Confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            ///Copias = 1;
            this.printer = printer;
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionGastos())
            {
                case 1:
                default:                
                    Formato1(GasSecuencia, Confirmado);
                    break;
                case 2:
                    Formato2(GasSecuencia, Confirmado);
                    break;
            }
        }

        private void Formato1(int gasSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Gastos gastos = mygas.GetGastoBysecuencia(gasSecuencia, confirmado);

            if (gastos == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("R E S U M E N  D E  G A S T O S ");
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("Gasto: " + Arguments.CurrentUser.RepCodigo + " - " + gasSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(gastos.GasFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Rnc:" +gastos.GasRNC);


           // printer.DrawText(gastos.GasNombreProveedor);
            printer.DrawText("NCF: "+gastos.GasNCF);
            printer.DrawText("Tipo Gasto:" + gastos.GasTipoDescripcion);
            printer.DrawLine();
            printer.DrawText("Factura      Monto       Itbis   Propina");
            printer.DrawLine();
            printer.DrawText(gastos.GasNoDocumento.PadRight(13) + gastos.GasMontoTotal.ToString("C2").PadRight(12)
                + gastos.GasItebis.ToString("C2").PadRight(8) + gastos.GasPropina.ToString("C2"));
            printer.DrawLine();

            printer.DrawText("");
            printer.DrawText("Sub-Total     : "+(gastos.GasMontoTotal-gastos.GasItebis - gastos.GasPropina).ToString("C2").PadLeft(12) );
            printer.DrawText("Total Itbis   : "+ gastos.GasItebis.ToString("C2").PadLeft(12));
            printer.DrawText("Total Propinas: "+ gastos.GasPropina.ToString("C2").PadLeft(12));
            printer.DrawText("Total         : "+ gastos.GasMontoTotal.ToString("C2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(16, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionGastos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(16, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionGastos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Formato Gastos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato2(int gasSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Gastos gastos = mygas.GetGastoBysecuencia(gasSecuencia, confirmado);

            if (gastos == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("R E S U M E N  D E  G A S T O S ");
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("Gasto: " + Arguments.CurrentUser.RepCodigo + " - " + gasSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(gastos.GasFecha, "dd-MM-yyyy"));
            printer.DrawText("Proveedor: " + gastos.GasNombreProveedor);
            printer.DrawText("Rnc:" + gastos.GasRNC);


            
            printer.DrawText("NCF: " + gastos.GasNCF);
            printer.DrawText("Fecha Factura: " + Functions.FormatDate(gastos.GasFechaDocumento, "dd-MM-yyyy"));
            printer.DrawText("Tipo Gasto:" + gastos.GasTipoDescripcion);
            printer.DrawLine();
            printer.DrawText("Factura      Monto       Itbis   Propina");
            printer.DrawLine();
            printer.DrawText(gastos.GasNoDocumento.PadRight(13) + gastos.GasMontoTotal.ToString("C2").PadRight(12)
                + gastos.GasItebis.ToString("C2").PadRight(8) + gastos.GasPropina.ToString("C2"));
            printer.DrawLine();

            printer.DrawText("");
            printer.DrawText("Sub-Total             : " + (gastos.GasMontoTotal - gastos.GasItebis - gastos.GasPropina).ToString("C2").PadLeft(8));
            printer.DrawText("Total Monto Suj. Itbis: " + gastos.GasBaseImponible.ToString("C2").PadLeft(8));
            printer.DrawText("Total Itbis           : " + gastos.GasItebis.ToString("C2").PadLeft(8));
            printer.DrawText("Total Propinas        : " + gastos.GasPropina.ToString("C2").PadLeft(8));
            printer.DrawText("Total                 : " + gastos.GasMontoTotal.ToString("C2").PadLeft(8));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Comentario: " + gastos.GasComentario);
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(16, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionGastos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(16, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionGastos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Formato Gastos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
    }
}

using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Linq;
namespace MovilBusiness.Printer.Formats
{
    public class DepositoGastosFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_DepositosGastos myDep;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public DepositoGastosFormats(DS_DepositosGastos myDep)
        {
            this.myDep = myDep;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public void Print(int depSecuencia,  bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositoGastos())
            {
                case 1:
                default:
                    Formato1(depSecuencia);
                    break;
                case 2:
                    Formato2(depSecuencia);
                    break;
            }
        }

        private void Formato1(int depSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var deposito = myDep.GetBySecuencia(depSecuencia);

            if(deposito == null)
            {
                throw new Exception("Error cargando datos del deposito!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
           
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("DEPOSITO DE GASTOS");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Deposito        : " + Arguments.CurrentUser.RepCodigo + "-" + depSecuencia);
            printer.DrawText("Fecha deposito  : " + deposito.DepFecha);
            printer.DrawText("Cant. gastos    : " + deposito.DepCantidadGastos);
            printer.DrawText("Desde           : " + deposito.DepGastoDesde);
            printer.DrawText("Hasta           : " + deposito.DepGastoHasta);
            printer.DrawText("Fecha impresion : " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawLine();

            var gastos = new DS_Gastos().GetByDepSecuencia(depSecuencia);

            foreach(var gasto in gastos)
            {
                printer.DrawText("#      :" + gasto.GasSecuencia);
                printer.DrawText("Desc.  :" + gasto.GasComentario);
                printer.DrawText("RNC    :" + gasto.GasRNC);
                printer.DrawText("Prov.  :" + gasto.GasNombreProveedor);
                printer.DrawText("NCF    :" + gasto.GasNCF);
                printer.DrawText("Fecha  :" + gasto.GasFecha);
                printer.DrawText("Monto  :" + gasto.GasMontoTotal);

                printer.DrawText("");
            }
            printer.DrawLine();

            printer.DrawText("Monto a reponer: " + gastos.Sum(x => x.GasMontoTotal).ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(25, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositoGastos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(25, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositoGastos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato deposito gastos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato2(int depSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var deposito = myDep.GetBySecuencia(depSecuencia);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("REPORTE DE GASTOS");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Deposito        : " + Arguments.CurrentUser.RepCodigo + "-" + depSecuencia);
            printer.DrawText("Fecha deposito  : " + deposito.DepFecha);
            printer.DrawText("Cant. gastos    : " + deposito.DepCantidadGastos);
            printer.DrawText("Desde           : " + deposito.DepGastoDesde);
            printer.DrawText("Hasta           : " + deposito.DepGastoHasta);
            printer.DrawText("Fecha impresion : " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawLine();

            var gastos = new DS_Gastos().GetByDepSecuenciaFormato2(depSecuencia);

            foreach (var gasto in gastos)
            {
                printer.DrawText("No. Secuencia    : " + gasto.GasSecuencia);
                printer.DrawText("Fecha            : " + gasto.GasFecha);
                printer.DrawText("Comentario       : " + gasto.GasComentario);
                printer.DrawText("Proveedor        : " + gasto.GasNombreProveedor);
                printer.DrawText("RNC              : " + gasto.GasRNC);
                printer.DrawText("NCF              : " + gasto.GasNCF);
                printer.DrawText("No. Factura      : " + gasto.GasNoDocumento);
                printer.DrawText("Fecha Factura    : " + Functions.FormatDate(gasto.GasFechaDocumento, "dd-MM-yyyy") );
                printer.DrawText("Tipo Gasto       : " + gasto.GasTipoDescripcion);
                printer.DrawText("Monto Suj. Itbis : " + gasto.GasBaseImponible);
                printer.DrawText("Monto Itbis      : " + gasto.GasItebis);
                printer.DrawText("Propina          : " + gasto.GasPropina);
                printer.DrawText("Monto Sin Itbis  : " + gasto.GasMontoItebis);
                printer.DrawText("Monto Total      : " + gasto.GasMontoTotal);
                printer.DrawLine();
                printer.DrawText("");
            }
            printer.DrawLine();

            printer.DrawText("Total Itbis          : " + gastos.Sum(x => x.GasItebis).ToString("N2"));
            printer.DrawText("Total Propina        : " + gastos.Sum(x => x.GasPropina).ToString("N2"));
            printer.DrawText("Monto Total a reponer: " + gastos.Sum(x => x.GasMontoTotal).ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(25, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositoGastos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(25, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositoGastos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato deposito gastos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
    }
}

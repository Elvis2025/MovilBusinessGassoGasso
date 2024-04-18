using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class TransaccionesCanastosFormats : IPrinterFormatter
    {
        public PrinterManager printer;

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas())
            {
                default:
                case 2:
                    Formato2(traSecuencia, confirmado);
                    break;

            }
        }

        private void Formato2(int traSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var dsTran = new DS_TransaccionesCanastos();

            var transaccion = dsTran.GetBySecuencia(traSecuencia);

            if (transaccion == null)
            {
                throw new Exception("No se encontraron los datos");
            }

            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText((transaccion.TitOrigen == -1 ? "E N T R E G A" : "R E C E P C I O N") + "  D E  C A N A S T O S");
            // printer.DrawText(cambio.CamCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + "-" + traSecuencia);
            printer.DrawText("Fecha  : " + Functions.FormatDate(transaccion.TraFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Cliente: " + transaccion.CliNombre);
            printer.DrawText("Código : " + transaccion.CliCodigo);
            printer.DrawText("Calle  : " + transaccion.CliCalle);
            printer.DrawText("Urb    : " + transaccion.CliUrbanizacion);
            printer.Bold = true;
            printer.DrawLine();            

            if (DS_RepresentantesParametros.GetInstance().GetParCanastosNoDetalle())
            {
                printer.DrawText("CANTIDAD CANASTOS: " + transaccion.TraCantidadCanastos);
                printer.DrawLine();
            }
            else
            {
                printer.DrawText("CODIGO CANASTO");
                printer.DrawLine();
                printer.Bold = false;

                var detalles = dsTran.GetDetalleBySecuencia(traSecuencia);

                foreach(var det in detalles)
                {
                    printer.DrawText(det.RecSerie);
                }
                printer.DrawLine();
            }

            printer.Bold = false;
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del " + (transaccion.TitOrigen == -1 ? "vendedor" : "cliente"));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + transaccion.TraCantidadCanastos.ToString());
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato canastos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();
        }
    }
}

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
    class CambiosFormats : IPrinterFormatter
    {
        public PrinterManager printer;
        public DS_Cambios myCamb;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public CambiosFormats(DS_Cambios myCamb)
        {
            this.myCamb = myCamb;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int CamSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas())
            {
                default:
                case 2:
                    Formato2(CamSecuencia, confirmado);
                    break;

            }
        }

        private void Formato2(int CamSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var cambio = myCamb.GetBySecuencia(CamSecuencia, confirmado);

            if (cambio == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("C A M B I O  D E  M E R C A N C I A");
           // printer.DrawText(cambio.CamCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cambio: " + Arguments.CurrentUser.RepCodigo + "-" + CamSecuencia, 48);
            printer.DrawText("Fecha Cambio: " + cambio.CamFecha);
            printer.DrawText("Cuadre: " + Arguments.Values.CurrentCuaSecuencia);
            printer.DrawText("Cantidad Item: " + cambio.CamTotal);
            if (!String.IsNullOrEmpty(cambio.CliNombreComercial))
            {
                printer.DrawText("Cliente: " + cambio.CliNombreComercial, 48);
                printer.DrawText("Sucursal: " + cambio.CliNombre, 48);
            }
            else
            {
                printer.DrawText("Cliente: " + cambio.CliNombre, 48);
            }
            printer.DrawText("Codigo: " + cambio.CliCodigo);
            printer.DrawText("Calle: " + cambio.CliCalle, 46);
            printer.DrawText("Urb: " + cambio.CliUrbanizacion);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion        Cantidad    Monto");
            printer.DrawLine();
            double montoItbis = 0.0;
            double precioConItbis = 0.0;
            double TotalTotal = 0.0;
            int camtotal = 0;

            foreach (var det in myCamb.GetDetalleBySecuencia(CamSecuencia, confirmado))
            {
                if (DS_RepresentantesParametros.GetInstance().GetParCambiosImprimirLoteEntregado())
                {
                    if (det.CamTipoTransaccion == 0)
                    {
                        continue;
                    }
                }
                var desc = (det.ProCodigo + "-" + det.ProDescripcion);
                if (desc.Length>33)
                {
                    desc = desc.Substring(0, 33);
                }
                montoItbis = det.CamPrecio * (det.CamItbis / 100);
                precioConItbis = det.CamPrecio + Math.Round(montoItbis, 2);
                TotalTotal = (det.CamPrecio * det.CamCantidad) + montoItbis;
                camtotal++;

                printer.DrawText(desc.ToString().PadRight(27) + (det.CamCantidad+"/"+det.CamCantidadDetalle).ToString().PadLeft(6) + TotalTotal.ToString().PadLeft(8));
               
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + camtotal);
            printer.DrawText("");
            
            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(6, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(6, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato cambios mercancia 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

          //  myCamb.ActualizarCantidadImpresion(CamSecuencia);

            printer.Print();

        }

    }
}

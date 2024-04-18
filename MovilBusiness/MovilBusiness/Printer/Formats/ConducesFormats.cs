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
    public class ConducesFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Conduces myCon;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        public ConducesFormats(DS_Conduces myCon)
        {
            this.myCon = myCon;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionConduces())
            {
                default:
                    Formato1(traSecuencia, confirmado);
                    break;
            }
        }


        private void Formato1(int conSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var conduce = myCon.GetBySecuencia(conSecuencia, confirmado);

            if (conduce == null)
            {
                throw new Exception("No se encontraron los datos del conduce");
            }

            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;

            var title = "CONDUCE";

            var parTitle = DS_RepresentantesParametros.GetInstance().GetParConducesNombreModulo();

            if (!string.IsNullOrWhiteSpace(parTitle))
            {
                title = parTitle;
            }

            printer.DrawText(title.ToUpper());
            printer.DrawText("");

            if(!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(conduce.ConCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Conduce: " + Arguments.CurrentUser.RepCodigo + "-" + conSecuencia + " (" + conduce.ConDescripcion + ")", 48);
            printer.DrawText("Fecha conduce: " + conduce.ConFecha);
            printer.DrawText("Cliente: " + conduce.CliNombre, 48);
            printer.DrawText("Codigo: " + conduce.CliCodigo);
            printer.DrawText("Calle: " + conduce.CliCalle, 46);
            printer.DrawText("Urb: " + conduce.CliUrbanizacion);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Descuento MontoItbis  Importe");
            printer.DrawLine();

            double importebruto = 0, subtotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            //foreach (var det in myCon.GetDetalleBySecuencia(conSecuencia, confirmado))
            //{
            //    printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

            //    var cantidad = det.ConCantidad.ToString();
            //    var precioLista = det.ConPrecio + det.ConAdValorem + det.ConSelectivo;
            //    var montoItbis = (precioLista - det.ConDescuento) * (det.ConItbis / 100.0);

            //    var precioConItbis = precioLista + montoItbis;

            //    var montoItbisTotal = montoItbis * det.ConCantidad;
            //    var subTotal = (precioLista - det.ConDescuento + montoItbis) * det.ConCantidad;

            //    itbisTotal += montoItbisTotal;
            //    total += subTotal;
            //    descuentoTotal += (det.ConDescuento * det.ConCantidad);
            //    subTotalTotal += (precioLista * det.ConCantidad);

            //    printer.DrawText(cantidad.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
            //        montoItbisTotal.ToString("N2").PadRight(13) + det.ConDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadRight(9));
            //}

            foreach (var det in myCon.GetDetalleBySecuencia(conSecuencia, confirmado))
            {
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                var cantidad = det.ConCantidad.ToString();
                var precioLista = det.ConPrecio + det.ConAdValorem + det.ConSelectivo;
               

              
                var montoItbis = (precioLista - det.ConDescuento) * (det.ConItbis / 100.0);

                var precioConItbis = precioLista + montoItbis;

                var montoItbisTotal = montoItbis * det.ConCantidad;
                var subTotal = (precioLista - det.ConDescuento + montoItbis) * det.ConCantidad;
                
                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.ConDescuento * det.ConCantidad);
                importebruto += (precioLista * det.ConCantidad);


                var montoDescuento = Math.Round(det.ConDescuento * det.ConCantidad, 2, MidpointRounding.AwayFromZero);
                var subTotalLinea = (precioLista * det.ConCantidad) + montoItbisTotal - montoDescuento;

                precioLista = Math.Round(precioLista, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(9) +                      //Cantidad
                       (precioLista).ToString("N2").PadRight(9) +            //Precio
                       montoDescuento.ToString("N2").PadRight(9) +           //Descuento
                           montoItbisTotal.ToString("N2").PadRight(10) +     //Monto itbis
                                subTotalLinea.ToString("N2").PadRight(9));   //Total
            }


            importebruto = Math.Round(importebruto, 2, MidpointRounding.AwayFromZero);
            descuentoTotal = Math.Round(descuentoTotal, 2, MidpointRounding.AwayFromZero);
            subtotalTotal = importebruto - descuentoTotal;
            subtotalTotal = Math.Round(subtotalTotal, 2, MidpointRounding.AwayFromZero);
            itbisTotal = Math.Round(itbisTotal, 2, MidpointRounding.AwayFromZero);
            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            var bultosYunidades = myCon.GetDetalleBySecuenciaBultosYUnidades(conSecuencia,confirmado);
            int bultos = 0;
            double unidad = 0;
            foreach (var item in bultosYunidades)
            {
                bultos = item.ProUnidades;
                unidad = item.ConCantidad;
            }

            printer.DrawLine();
            printer.DrawText("Total items        : " + conduce.ConTotal);
            printer.DrawText("Bultos Completados : " + bultos.ToString());
            printer.DrawText("Unidades Sueltas   : " + ((int)unidad).ToString());
            printer.DrawText("");
            printer.DrawText("Importe Bruto:" + importebruto.ToString("N2").PadLeft(29));
            printer.DrawText("Descuento:    " + descuentoTotal.ToString("N2").PadLeft(29));
            printer.DrawText("SubTotal:     " + subtotalTotal.ToString("N2").PadLeft(29));
            printer.DrawText("Total Itbis:  " + itbisTotal.ToString("N2").PadLeft(29));
            printer.Bold = true;
            printer.DrawText("Total:        " + total.ToString("N2").PadLeft(29));
            printer.Bold = false;
            printer.DrawText("");

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(51, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionConduces()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(51, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionConduces()));
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
            printer.DrawText("Formato conduces 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myCon.ActualizarCantidadImpresion(conSecuencia, confirmado);

            printer.Print();

        }

    }
}

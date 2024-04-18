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
    public class CotizacionesFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Cotizaciones myCot;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public CotizacionesFormats(DS_Cotizaciones myCot)
        {
            this.myCot = myCot;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int pedSecuencia, bool Confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            // Copias = copias;
            this.printer = printer;
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCotizaciones())
            {
                default:
                case 1: //sanofi
                    Formato1(pedSecuencia, Confirmado);
                    break;
                case 2: //leBetances
                    Formato2(pedSecuencia, Confirmado);
                    break;
                case 3://SUED
                    Formato3(pedSecuencia,Confirmado);
                    break;
                case 8://Fraga Industrial
                    Formato8(pedSecuencia, Confirmado);
                    break;
                case 13: //Cano Ind
                    Formato13(pedSecuencia, Confirmado);
                    break;
                case 14:// Cano Descuento
                    Formato14(pedSecuencia, Confirmado);
                    break;
                case 19:// Grupo Armenteros
                    Formato19(pedSecuencia, Confirmado);
                    break;
                case 35:
                    Formato35(pedSecuencia, Confirmado);
                    break;
                case 36:
                    Formato36(pedSecuencia, Confirmado);
                    break;
               
            }
        }

        
        private void Formato1(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

           // printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;          
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
          //  printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
          //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.CotCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.CotPrecio.ToString("C2").PadRight(12));
            }

            printer.DrawLine();
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
           // printer.DrawText("");
         //   printer.DrawText("");
         //   printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;           
            printer.DrawText("Firma del cliente: ");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
           // printer.DrawText("");
           // printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato cotizaciones 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        private void Formato3(int pedSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuenciaSued(pedSecuencia, confirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            // printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.CotCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("C O P I A  No. " + ped.CotCantidadImpresion);
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
      
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
            {
                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.CotCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.CotPrecio.ToString("C2").PadRight(12)+(det.CotIndicadorOferta?"*":null));
            }

            printer.DrawLine();
            printer.DrawText("Nota: Los precios mostrados son precios de ");
            printer.DrawText("      Referencia.");
            printer.DrawText("Los productos marcados con (*) son");
            printer.DrawText("      ofertas");
            // printer.DrawText("");
            //   printer.DrawText("");
            //   printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente: ");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Items: " + myCot.GetDetalleBySecuencia(pedSecuencia, confirmado).Count);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato cotizaciones 3: Movilbusiness " + Functions.AppVersion);
            printer.Print();
            Hash CotizacionUpdate = new Hash(confirmado? "CotizacionesConfirmados" : "Cotizaciones");
            CotizacionUpdate.Add("PedCantidadImpresion",ped.CotCantidadImpresion + 1);
            CotizacionUpdate.ExecuteUpdate("PedSecuencia = "+ped.CotSecuencia+" And RepCodigo = '"+Arguments.CurrentUser.RepCodigo+"' ");
        }

        private void Formato2(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencias(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.CotCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("C O P I A  No. " + ped.CotCantidadImpresion);
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado);
            foreach (var det in list)
            {
                var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.CotCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.CotPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(10));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);

                subtotal += det.CotPrecio * cantidad;

                total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = false;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 2: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            Hash CotizacionUpdate = new Hash(pedidoConfirmado ? "CotizacionesConfirmados" : "Cotizaciones");
            CotizacionUpdate.Add("CotCantidadImpresion", ped.CotCantidadImpresion + 1);
            CotizacionUpdate.ExecuteUpdate("CotSecuencia = " + ped.CotSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
            printer.Print();

        }

        private void Formato13(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("O R D E N  DE  C O T I Z A C I O N");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado);
            foreach (var det in list)
            {
                var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);

                printer.DrawText(det.ProDescripcion, 48);
                printer.DrawText(det.CotCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.CotPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(10));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);

                subtotal += det.CotPrecio * cantidad;

                total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = false;
            //printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(5, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCotizaciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(5, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCotizaciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 13: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        #region Formato 14
        private void Formato14(int pedSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, confirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("C O T I Z A C I O N");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");

            var visipres = new DS_Visitas().GetClientePresentacion(ped.VisSecuencia);

            if(visipres != null)
            {
                printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                printer.DrawText("Propietario: " + visipres.VisPropietario);
                printer.DrawText("Contact: " + visipres.VisContacto, 45);
                printer.DrawText("Email: " + visipres.VisEmail, 45);
                printer.DrawText("Calle: " + visipres.VisCalle, 45);
                printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                printer.DrawText("RNC: " + visipres.VisRNC, 45);
            }
            else
            {
                printer.DrawText("Cliente: " + ped.CliNombre, 48);
                printer.DrawText("Codigo: " + ped.CliCodigo);
                printer.DrawText("Calle: " + ped.CliCalle, 45);
                printer.DrawText("Urb: " + ped.CliUrbanizacion);
            }           

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));            
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion-Codigo");
            printer.DrawText("Cantidad    Precio          Itbis       Total");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, totaldescuento =0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, confirmado);
            foreach (var det in list)
            {
                var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);
                double descuento = det.CotDescuento * cantidad;

                double totaltogive = (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad; 

                printer.DrawText(det.ProDescripcion+ "-"+det.ProCodigo.Trim());
                printer.DrawText(det.CotCantidad.ToString() + det.CotPrecio.ToString("N2").PadLeft(17) + itbis.ToString("N2").PadLeft(15) + totaltogive.ToString("N2").PadLeft(12));

                totaldescuento += descuento;
                subtotal += det.CotPrecio * cantidad;
                total += totaltogive;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal        :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis     :" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total Descuentos:" + totaldescuento.ToString("N2").PadLeft(15));
            printer.DrawText("Total           :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = false;
            //printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 14: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        #endregion





        #region Grupo Armenteros
        private void Formato19(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            // printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            //  printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.CotCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.CotPrecio.ToString("C2").PadRight(12));
            }

            printer.DrawLine();

            string nota = DS_RepresentantesParametros.GetInstance().GetParCotizacionesNota();
            if (String.IsNullOrEmpty(nota))
            {
                printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            }
            else
            {
                printer.DrawText(nota, 45);
            }
            // printer.DrawText("");
            //   printer.DrawText("");
            //   printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente: ");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            // printer.DrawText("");
            // printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato cotizaciones 19: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }
        #endregion
        private void Formato35(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }



            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawText("--------------------------------");
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawText("--------------------------------");

            
            double total = 0, subtotal = 0, totalItbis = 0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado);
            foreach (var det in list)
            {

            
                
                var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.CotCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16)  + det.CotPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(10));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }



                var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);

                subtotal += det.CotPrecio * cantidad;

                total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawText("--------------------------------");
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("--------------------------------");
            printer.Bold = false;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 35: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato36(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

         

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE COTIZACION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.CotSecuencia);
            printer.DrawText("--------------------------------");
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawText("Codigo de barra");
            printer.DrawText("--------------------------------");

            double total = 0, subtotal = 0, totalItbis = 0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado);
            foreach (var det in list)
            {


                if (det.RepCodigo == null)
                {
                    det.RepCodigo = "--";
                }

                var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);

                printer.DrawText(det.ProDescripcion);

                printer.DrawText(det.CotCantidad.ToString().PadRight(10)  +  det.ProCodigo.Trim().PadRight(16)  +  det.CotPrecio.ToString("N2").PadRight(13)  +  itbis.ToString("N2").PadRight(8));

                printer.DrawText(det.RepCodigo);
                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }



                var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);

                subtotal += det.CotPrecio * cantidad;

                total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawText("--------------------------------");
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("--------------------------------");
            printer.Bold = false;
            printer.DrawText("Nota: los precios mostrados son precios de referencia, validado por 7 dias", 60);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 36: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }


        private void Formato8(int pedSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Cotizaciones ped = myCot.GetBySecuencia(pedSecuencia, confirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos de la cotizacion");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("C O T I Z A C I O N");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");

            var fechaValida = DateTime.TryParse(ped.CotFecha, out DateTime fecha);
            printer.DrawText("Cotizacion: " + Arguments.CurrentUser.RepCodigo + "-" + ped.CotSecuencia, 48);
            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.CotFecha));
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("RNC: " + ped.CliRnc, 48);
            printer.DrawText("Telefono: " + ped.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
            var list = myCot.GetDetalleBySecuencia(pedSecuencia, confirmado);
            foreach (var det in list)
            {
                var producto = det.ProCodigo + "-" + det.ProDescripcion.Trim();

                if (producto.Length >= 35)
                {
                    producto = producto.Substring(0, 35);
                }
                else
                {
                    producto = producto.PadRight(35);
                }
                printer.DrawText(producto);

                var cantidad = det.CotCantidad.ToString("N2");
                var precioLista = det.CotPrecio + det.CotAdValorem + det.CotSelectivo;
                var montoItbis = (precioLista - det.CotDescuento) * (det.CotItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.CotCantidadDetalle.ToString()) / det.ProUnidades) + det.CotCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.CotDescuento + montoItbis) * cantidadTotal;

                if (det.CotCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.CotCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.CotDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(11) + det.CotDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
            }

            printer.DrawLine();
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = false;
            //printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  " + list.Count);
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato cotizaciones 8: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
    }



}

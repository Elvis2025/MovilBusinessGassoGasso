using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfCotizaciones : IPdfGenerator
    {
        private DS_Cotizaciones myCot;
        private DS_Pedidos myPed;
        private string SectorID = "";
        public PdfCotizaciones(DS_Cotizaciones myCot = null, string SecCodigo="")
        {
            if(myCot == null)
            {
                myCot = new DS_Cotizaciones();
            }
            DS_Pedidos myPed = new DS_Pedidos();
            this.myPed = myPed;
            SectorID = SecCodigo;
            this.myCot = myCot;
        }

        public Task<string> GeneratePdf(int traSecuencia, bool confirmado = false)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCotizacionesPDF() == 0 ? DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidosPDF() == 0 ? DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidos()[0] : DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidosPDF() : DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCotizacionesPDF();

            switch (formato)
            {
                case 3:
                    return Formato3(traSecuencia, confirmado);
                case 4:
                    return Formato4(traSecuencia, confirmado);
                case 5:
                    return Formato5(traSecuencia, confirmado);
                case 8://Fraga Industrial
                    return Formato8(traSecuencia, confirmado);
                case 31: //Cano
                    return formato25(traSecuencia, confirmado);
                case 25: //Cano
                    return formato25(traSecuencia, confirmado);
                case 19: //Grupo Armenteros
                    return Formato19(traSecuencia, confirmado);
                case 26://DEINSA
                    return Formato26(traSecuencia, confirmado);
                default:
                    return Formato1(traSecuencia, confirmado);

                case 20:
                    return Formato20(traSecuencia, confirmado);

            }
        }


        private Task<string> Formato1(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE COTIZACION No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow(new List<string>() { "Código", "Descripción", "Cantidad", "Precio", "Itbis", "Total" }, true, numtocalular:2);
                    manager.Bold = false;

                    double total = 0, subtotal = 0, totalItbis = 0, totaldescuento = 0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        if (det.ProUnidades == 0)
                        {
                            det.ProUnidades = 1;
                        }

                        var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);
                        double descuento = det.CotDescuento * cantidad;

                        totaldescuento += descuento;
                        subtotal += det.CotPrecio * cantidad;

                        double totaltosum = (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                        total += totaltosum;

                        totalItbis += (itbis * cantidad);

                        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo())
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(),
                            det.CotCantidad.ToString("N2").PadRight(32), det.CotPrecio.ToString("N4"),
                            det.CotItbis.ToString("N2"), totaltosum.ToString("N2") }, numtocalular: 2);
                        }
                        else
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(),
                            det.CotCantidad.ToString("N2").PadRight(32), det.CotPrecio.ToString("N2"),
                            det.CotItbis.ToString("N2"), totaltosum.ToString("N2") }, numtocalular: 2);
                        }

                        
                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia, valido por 7 dias.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText("        Subtotal:" + subtotal.ToString("N2").PadLeft(20));
                    manager.DrawText("     Total Itbis:" + totalItbis.ToString("N2").PadLeft(20));
                    manager.DrawText("Total Descuentos:" + totaldescuento.ToString("N2").PadLeft(20));
                    manager.DrawText("           Total:" + total.ToString("N2").PadLeft(20));
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf cotizaciones 1: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato26(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE COTIZACION No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow(new List<string>() { "Código", "Descripción", "Cantidad", "Precio", "Itbis", "Total" }, true, numtocalular: 2);
                    manager.Bold = false;
                    double total = 0, subtotal = 0, totalItbis = 0, totaldescuento = 0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        if (det.ProUnidades == 0)
                        {
                            det.ProUnidades = 1;
                        }

                        var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);
                        double descuento = det.CotDescuento * cantidad;

                        totaldescuento += descuento;
                        subtotal += det.CotPrecio * cantidad;

                        double totaltosum = (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                        total += totaltosum;

                        totalItbis += (itbis * cantidad);

                        manager.DrawText("");
                        manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(),
                           ("   " + det.CotCantidad.ToString("N2").PadRight(35)), det.CotPrecio.ToString("N2"),
                            det.CotItbis.ToString("N2"), totaltosum.ToString("N2") }, numtocalular: 2);
                        manager.DrawText("");
                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia, valido por 7 dias.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText("        Subtotal:" + subtotal.ToString("N2").PadLeft(20));
                    manager.DrawText("     Total Itbis:" + totalItbis.ToString("N2").PadLeft(20));
                    manager.DrawText("Total Descuentos:" + totaldescuento.ToString("N2").PadLeft(20));
                    manager.DrawText("           Total:" + total.ToString("N2").PadLeft(20));
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf cotizaciones 26: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato19(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa(true);
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE COTIZACION No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow2(new List<string>() { "Código", "Descripción", "Cantidad", "Precio", "Descuento", "Itbis", "Total" }, true);
                    manager.Bold = false;

                    double total = 0, subtotal = 0, totalItbis = 0, totaldescuento = 0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        if (det.ProUnidades == 0)
                        {
                            det.ProUnidades = 1;
                        }

                        var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);
                        double descuento = det.CotDescuento * cantidad;

                        totaldescuento += descuento;
                        subtotal += det.CotPrecio * cantidad;

                        double totaltosum = (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                        total += totaltosum;

                        totalItbis += (itbis * cantidad);

                        int CotCantidad = Convert.ToInt32(det.CotCantidad);
                        int CotItbis = Convert.ToInt32(det.CotItbis);

                        manager.DrawText(CotCantidad.ToString().PadLeft(114 - CotCantidad.ToString().Length), noline: true);
                        manager.DrawText(det.CotPrecio.ToString("N2").PadLeft(150 - det.CotPrecio.ToString("N2").Length), noline: true);
                        manager.DrawText(det.CotDescuento.ToString("N2").PadLeft(186 - det.CotDescuento.ToString("N2").Length), noline: true);
                        manager.DrawText(("% " + CotItbis.ToString()).PadLeft(212 - ("% " + CotItbis.ToString()).Length), noline: true);
                        manager.DrawText(totaltosum.ToString("N2").PadLeft(254 - totaltosum.ToString("N2").Length), noline: true);
                        manager.DrawTableRow2(new List<string>() { det.ProCodigo, det.ProDescripcion });

                        /*
                        var align = 254 - totaltosum.ToString("N2").Length;
                        manager.DrawText(totaltosum.ToString("N2").PadLeft(align), noline: true);
                        manager.DrawTableRow2(new List<string>() { det.ProCodigo, det.ProDescripcion,
                            CotCantidad.ToString(), det.CotPrecio.ToString("N2"),
                            det.CotDescuento.ToString("N2"),
                            "% " + CotItbis.ToString() });
                        */
                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: Los precios mostrados son precios de referencia, valido por 7 dias.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText(("Subtotal:    " + subtotal.ToString("N2").PadLeft(20)).PadLeft(230));
                    manager.DrawText(("Total Itbis:    " + totalItbis.ToString("N2").PadLeft(24)).PadLeft(234));
                    manager.DrawText(("Total Descuentos:    " + totaldescuento.ToString("N2").PadLeft(22)).PadLeft(227 - totaldescuento.ToString().Length));
                    manager.DrawText(("Total:    " + total.ToString("N2").PadLeft(20)).PadLeft(233));
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf cotizaciones 19: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> formato24(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("C O T I Z A C I O N");
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.DrawText("Orden # : " + Arguments.CurrentUser.RepCodigo + " - " + pedido.CotSecuencia);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawLine();
                    manager.DrawTableRow(new System.Collections.Generic.List<string>() { "Descripción", "Código" });
                    manager.DrawLine();
                    manager.DrawTableRow(new System.Collections.Generic.List<string>() { "Cantidad", "Precio", "Itbis", "Total" });
                    manager.DrawLine();
                    manager.Bold = false;

                    double total = 0, subtotal = 0, totalItbis = 0, totaldescuento = 0;
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

                        manager.DrawTableRow(new System.Collections.Generic.List<string>() { det.ProDescripcion.Trim(), det.ProCodigo });
                        manager.DrawTableRow(new System.Collections.Generic.List<string>() { cantidad.ToString("N2"), det.CotPrecio.ToString("N2"), itbis.ToString("N2"), totaltogive.ToString("N2") });


                        //printer.DrawText(det.ProDescripcion, 48);
                        //printer.DrawText(det.CotCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.CotPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(10));

                        //printer.DrawText((det.CotDesPorciento + "%").ToString().PadRight(12) + det.CotDescuento.ToString().PadRight(26) + descuento.ToString("N2").PadRight(15));


                        totaldescuento += descuento;
                        subtotal += det.CotPrecio * cantidad;

                        total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                        totalItbis += (itbis * cantidad);

                    }
                    manager.NewLine();
                    manager.DrawLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText("        Subtotal:" + subtotal.ToString("N2").PadLeft(20));
                    manager.DrawText("     Total Itbis:" + totalItbis.ToString("N2").PadLeft(20));
                    manager.DrawText("Total Descuentos:" + totaldescuento.ToString("N2").PadLeft(20));
                    manager.DrawText("           Total:" + total.ToString("N2").PadLeft(20));
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawLine();
                    manager.Bold = false;
                    //printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
                    manager.NewLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.DrawText("SKU:  " + list.Count);
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.Font = PrinterFont.FOOTER;
                    manager.NewLine();
                    manager.DrawText("Formato cotizaciones 14: movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }

        private Task<string> formato25(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawTextNew("C O T I Z A C I O N", textSegundaPagina: "COTIZACION No." + pedido.CotSecuencia);
                    manager.DrawText("No." + pedido.CotSecuencia);
                    manager.Font = PrinterFont.BODY;


                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("Cotizar a: ");
                    manager.DrawText(pedido.CliNombre.ToString().ToUpper());
                    manager.Bold = false;
                    manager.DrawText(pedido.CliCalle.ToString());
                    manager.DrawText("Telefono:  " + pedido.CliTelefono.ToString());
                    manager.DrawText("RNC:  " + pedido.CliRnc.ToString());
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha   : " + date, ytoload: true);
                    manager.Bold = true;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawTableRow(new List<string>() { "Descripción", "Cantidad", "Descuento", "Precio", "MontoItbis", "Total" }, true, numtocalular: 1);                    
                    manager.Bold = false;

                    double total = 0, subtotal = 0, totalItbis = 0, totaldescuento = 0;
                    var list = myCot.GetDetalleBySecuencia(pedSecuencia, confirmado);
                    foreach (var det in list)
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        var descripcion = det.ProDescripcion.Trim();
                        if (det.ProUnidades == 0)
                        {
                            det.ProUnidades = 1;
                        }

                        if (descripcion.Length >= 38)
                        {
                            descripcion = det.ProDescripcion.Substring(0, 38);
                        }

                        var cantidad = ((det.CotCantidadDetalle / det.ProUnidades) + det.CotCantidad);
                        double descuento = det.CotDescuento * cantidad;
                        double totaltogive = (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;
                        manager.DrawTableRow(new System.Collections.Generic.List<string>() { descripcion, cantidad.ToString("N2"), ("% " + det.CotDesPorciento.ToString()), det.CotPrecio.ToString("N2"), itbis.ToString("N2"), totaltogive.ToString("N2") }, numtocalular: 1);
                        manager.Bold = true;
                        manager.DrawText("Item Code:", noline: true);
                        manager.Bold = false;
                        manager.DrawText("                                                   " + det.ProCodigo);
                        totaldescuento += descuento;
                        subtotal += det.CotPrecio * cantidad;

                        total += (itbis + (det.CotPrecio - det.CotDescuento)) * cantidad;

                        totalItbis += (itbis * cantidad);

                    }
                    manager.DrawLine(true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("*Cotización valida por 48 horas*");
                    manager.DrawText("*Sujeto a disponibilidad al momento de recibir la orden*");
                    manager.DrawText("*Precio Sujeto a Variacion de la prima en US$ y de los precios del petroleo*");
                    manager.DrawText("*Ordenes de compras para despachos parciales se anulan");
                    manager.DrawText("automaticamente si no sonretiradas en la fecha establecida*");
                    manager.DrawText("Todos los precios, montos y totales estan en RD$ Pesos Dominicanos");
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Subtotal:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(subtotal.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Total Monto Itbis:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(totalItbis.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Total Descuentos:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(totaldescuento.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawTextNew("Total:", true, 3);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(total.ToString("N2"));
                    manager.DrawText("_________________________________________________________", isline: true, noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.DrawText("SKU:  " + list.Count);
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.Font = PrinterFont.FOOTER;
                    manager.NewLine();
                    manager.DrawText("Formato cotizaciones 25: movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }


        private Task<string> Formato3(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia2(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("COTIZACION");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.DrawText("Original");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Página", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText("1 de 1");
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Punto de Emisión:");
                    manager.Bold = true;
                    manager.DrawText("Número", noline: true);
                    manager.Bold = false;
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(Arguments.CurrentUser.RepCodigo + " - " + pedido.CotSecuencia.ToString());
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Fecha", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(date);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Vencimiento", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(date);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTextNew2(" ", true, 30);
                    manager.DrawText("Cliente:  " + pedido.CliNombre, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Vendedor", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(Arguments.CurrentUser.RepNombre);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("RNC:  " + pedido.CliRnc, noline: true);
                    manager.DrawText(("Codigo:    " + pedido.CliCodigo).PadLeft(100), noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Condiciones Pago", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(pedido.RepCodigo);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Direccion:    " + pedido.CliCalle);

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Descripción".PadRight(214), noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Código", noline: true);
                    manager.DrawText("Cantidad".PadLeft(110), noline: true);
                    manager.DrawText("Código Barra".PadLeft(140), noline: true);
                    manager.DrawText("U/M".PadLeft(170), noline: true);
                    manager.DrawText("Precio".PadLeft(190), noline: true);
                    manager.DrawText("Desc.".PadLeft(210), noline: true);
                    manager.DrawText("ITBIS".PadLeft(230), noline: true);
                    manager.DrawText("Monto".PadLeft(250));
                    manager.DrawLine();
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        var subtotalpro = det.CotCantidad * det.CotPrecio - det.CotDescuento;
                        subtotal += det.CotCantidad * det.CotPrecio;

                        //int descporciento = det.CotDescuento != 0 ? (int)((det.CotDescuento * 100) / det.CotPrecio) : 0;
                        double descporciento = det.CotDesPorciento;
                        var cantidad = ((double.Parse(det.CotCantidadDetalle.ToString()) / det.ProUnidades) + det.CotCantidad);
                        totalitbis += (itbis * cantidad);
                        var desproc = det.CotDescuento * cantidad;
                        desc += desproc;

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(det.CotCantidad.ToString().PadLeft(114), noline: true);
                        manager.DrawText(det.RepCodigo.PadLeft(140), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(170), noline: true);
                        manager.DrawText(det.CotPrecio.ToString("N2").PadLeft(198 - det.CotPrecio.ToString("N2").Length), noline: true);
                        manager.DrawText(desproc.ToString("N2").PadLeft(215 - desproc.ToString("N2").Length), noline: true);
                        manager.DrawText(itbis.ToString("N2").PadLeft(235 - itbis.ToString("N2").Length), noline: true);
                        manager.DrawText(subtotalpro.ToString("N2").PadLeft(256 - subtotalpro.ToString("N2").Length), noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (det.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(det.ProDescripcion.Substring(0, 38), alignCustom: 550);
                            manager.DrawText(det.ProDescripcion.Substring(38, det.ProDescripcion.Length - 38), alignCustom: 550);
                        }
                        else
                        {
                            manager.DrawText(det.ProDescripcion, alignCustom: 550);
                        }
                        manager.TextAlign = Justification.LEFT;
                    }
                    total = subtotal + totalitbis - desc;

                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Moneda:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(pedido.MonCodigo.ToString());
                    manager.DrawText("_______________________________________________________");
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Subtotal:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(subtotal.ToString("N2"));
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Desc.:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(desc.ToString("N2"));
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Itbis:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(totalitbis.ToString("N2"));
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText($"Total {pedido.MonCodigo}:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(total.ToString("N2"));
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("________________________________");
                    manager.DrawText("FIRMA");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER; 
                    manager.DrawText("Formato pdf cotizaciones 3: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato4(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia2(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa2();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("COTIZACION PARA:       " + pedido.CliCodigo.PadRight(80));
                    manager.Bold = true;
                    manager.DrawText(pedido.CliNombre.ToUpper());
                    manager.Bold = false;
                    manager.DrawText("RNC: " + pedido.CliRnc + "    TELEFONO: " + pedido.CliTelefono, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT3;
                    manager.DrawText("ENVIADO A: ".PadRight(80));
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText(pedido.CliCalle, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT3;
                    manager.DrawText(pedido.CliCalle);
                    manager.TextAlign = Justification.RIGHT;
                    manager.NewLine();
                    manager.DrawText("PAGINA: 1/1");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = true;
                    manager.DrawText("_______________________________________________________________________________________________________________________________________________", ispointtoline: true);
                    manager.Bold = false;

                    //manager.DrawText("NO.:  " + (Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString()) + "   FECHA:  " + date + "   MONEDA:  " + pedido.MonCodigo.ToString() + 
                    //    "  CONDIC. PAGO: " + pedido.ConDescripcion + "   REFERENCIA:   " + pedido.PedFechaEntrega + "  VEND.: " + Arguments.CurrentUser.RepCodigo + "  " + Arguments.CurrentUser.RepTelefono1);
                    manager.Bold = true;
                    manager.DrawText("NO.:", noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + " - " + pedido.CotSecuencia.ToString()).PadLeft(17), noline: true);

                    manager.Bold = true;
                    manager.DrawText("FECHA:".PadLeft(36), noline: true);
                    manager.Bold = false;
                    manager.DrawText(date.PadLeft(56), noline: true);

                    manager.Bold = true;
                    manager.DrawText("MONEDA:".PadLeft(76), noline: true);
                    manager.Bold = false;
                    manager.DrawText(pedido.MonCodigo.ToString().PadLeft(91), noline: true);

                    manager.Bold = true;
                    manager.DrawText("CONDIC. PAGO:".PadLeft(112), noline: true);
                    manager.Bold = false;
                    manager.DrawText(pedido.RepCodigo.PadLeft(148), noline: true);

                    //manager.Bold = true;
                    //manager.DrawText("REFERENCIA:".PadLeft(180), noline: true);
                    //pedido.CotFechaEntrega = pedido.CotFechaEntrega == null ? "NINGUNA" : pedido.CotFechaEntrega;
                    //manager.Bold = false;
                    //manager.DrawText(pedido.CotFechaEntrega.PadLeft(202), noline: true);

                    manager.Bold = true;
                    manager.DrawText("VEND.:".PadLeft(222), noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + "  " + Arguments.CurrentUser.RepTelefono1).PadLeft(246));

                    manager.Bold = true;
                    manager.DrawLine(true);
                    //manager.DrawTableRow(new List<string>() { "ARTICULO", "EMP. U/E", "CANT.", "U/M", "DESCRIPCION", "PRECIO", "DESC.","TOTAL","ITBIS" }, true, numtocalular: 5);

                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("DESCRIPCION".PadRight(149), noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("ARTICULO", noline: true);
                    manager.DrawText("EMP. U/E".PadLeft(32), noline: true);
                    manager.DrawText("CANT.".PadLeft(52), noline: true);
                    manager.DrawText("U/M".PadLeft(68), noline: true);
                    //manager.DrawText("DESCRIPCION".PadLeft(92), noline: true);
                    manager.DrawText("PRECIO".PadLeft(180), noline: true);
                    manager.DrawText("DESC.".PadLeft(200), noline: true);
                    manager.DrawText("TOTAL".PadLeft(221), noline: true);
                    manager.DrawText("ITBIS".PadLeft(250));
                    manager.DrawLine(true);

                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myCot.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        //int descporciento = det.CotDescuento != 0 ? (int)((det.CotDescuento * 100) / det.CotPrecio) : 0;

                        double descporciento = det.CotDesPorciento;
                        var cantidad = ((double.Parse(det.CotCantidadDetalle.ToString()) / det.ProUnidades) + det.CotCantidad);
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.CotDescuento * cantidad;
                        var subtotalpro = det.CotCantidad * det.CotPrecio;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;
                        string empaque = (det.CotCantidad / det.ProID).ToString() + " " + det.RepCodigo;
                        //manager.DrawTableRow(new List<string>() {det.ProCodigo, empaque.PadRight(5 - empaque.Length), det.PedCantidad.ToString(), det.UnmCodigo, det.ProDescripcion,
                        //                                         " $" + det.PedPrecio.ToString("N2"), descporciento.ToString() + "%", " $" + subtotalpro.ToString("N2"), " $" + itbis.ToString("N2")}, true, numtocalular: 5);

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(empaque.PadLeft(34), noline: true);
                        manager.DrawText(det.CotCantidad.ToString().PadLeft(52), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(68), noline: true);
                        //manager.DrawText(det.ProDescripcion.PadLeft(110).PadRight(120 - det.ProDescripcion.Length), noline: true);
                        manager.DrawText((" $" + det.CotPrecio.ToString("N2")).PadLeft(189 - (" $" + det.CotPrecio.ToString("N2")).Length), noline: true);
                        manager.DrawText((descporciento.ToString() + "%").PadLeft(200), noline: true);
                        manager.DrawText((" $" + subtotalpro.ToString("N2")).PadLeft(230 - (" $" + subtotalpro.ToString("N2")).Length), noline: true);
                        manager.DrawText((" $" + itbis1.ToString("N2")).PadLeft(256 - (" $" + itbis1.ToString("N2")).Length), noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (det.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(det.ProDescripcion.Substring(0, 38), alignCustom: 418);
                            manager.DrawText(det.ProDescripcion.Substring(38, det.ProDescripcion.Length - 38), alignCustom: 418);
                        }
                        else
                        {
                            manager.DrawText(det.ProDescripcion, alignCustom: 418);
                        }
                        manager.TextAlign = Justification.LEFT;
                        manager.DrawLine(true);
                    }
                    total += subtotal + totalitbis;

                    manager.NewLine(true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("TOTAL GRAVADO:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + (subtotal).ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("TOTAL ITBIS:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + totalitbis.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("TOTAL A PAGAR:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(pedido.MonCodigo + " $" + total.ToString("N2"));
                    manager.DrawText("_______________________________________________________", ispointtoline: true, noline: true);
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
                    manager.NewLine();
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.NewLine();
                    manager.DrawText("________________________________", isline: true);
                    manager.DrawText("FIRMA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("*Cotizacion valida por 48 horas*");
                    manager.DrawText("*Sujeto a disponibilidad al momento de recibir la orden*");
                    manager.DrawText("*Precio Sujeto a Variacion de la prima en US$ y de los precios del petroleo*");
                    manager.DrawText("*Ordernes de compras para despachos parciales se anulan automaticamente si");
                    manager.DrawText("no son retiradas en la fecha establecida");
                    manager.DrawText("Todos lo precios, montos y total estan en RD$ Pesos Dominicanos");
                    manager.NewLine();
                    manager.DrawText("Formato pdf cotizaciones 4: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
        private Task<string> Formato5(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia2(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa2();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("COTIZACION PARA:       " + pedido.CliCodigo.PadRight(80));
                    manager.Bold = true;
                    manager.DrawText(pedido.CliNombre.ToUpper());
                    manager.Bold = false;
                    manager.DrawText("RNC: " + pedido.CliRnc + "    TELEFONO: " + pedido.CliTelefono, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT3;
                    manager.DrawText("ENVIADO A: ".PadRight(80));
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText(pedido.CliCalle, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT3;
                    manager.DrawText(pedido.CliCalle);
                    manager.TextAlign = Justification.RIGHT;
                    manager.NewLine();
                    manager.DrawText("PAGINA: 1/1");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = true;
                    manager.DrawText("_______________________________________________________________________________________________________________________________________________", ispointtoline: true);
                    manager.Bold = false;

                    //manager.DrawText("NO.:  " + (Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString()) + "   FECHA:  " + date + "   MONEDA:  " + pedido.MonCodigo.ToString() + 
                    //    "  CONDIC. PAGO: " + pedido.ConDescripcion + "   REFERENCIA:   " + pedido.PedFechaEntrega + "  VEND.: " + Arguments.CurrentUser.RepCodigo + "  " + Arguments.CurrentUser.RepTelefono1);
                    manager.Bold = true;
                    manager.DrawText("NO.:", noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + " - " + pedido.CotSecuencia.ToString()).PadLeft(17), noline: true);

                    manager.Bold = true;
                    manager.DrawText("FECHA:".PadLeft(36), noline: true);
                    manager.Bold = false;
                    manager.DrawText(date.PadLeft(56), noline: true);

                    manager.Bold = true;
                    manager.DrawText("MONEDA:".PadLeft(76), noline: true);
                    manager.Bold = false;
                    manager.DrawText(pedido.MonCodigo.ToString().PadLeft(91), noline: true);

                    manager.Bold = true;
                    manager.DrawText("CONDIC. PAGO:".PadLeft(112), noline: true);
                    manager.Bold = false;
                    manager.DrawText(pedido.RepCodigo.PadLeft(148), noline: true);

                    //manager.Bold = true;
                    //manager.DrawText("REFERENCIA:".PadLeft(180), noline: true);
                    //pedido.CotFechaEntrega = pedido.CotFechaEntrega == null ? "NINGUNA" : pedido.CotFechaEntrega;
                    //manager.Bold = false;
                    //manager.DrawText(pedido.CotFechaEntrega.PadLeft(202), noline: true);

                    manager.Bold = true;
                    manager.DrawText("VEND.:".PadLeft(222), noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + "  " + Arguments.CurrentUser.RepTelefono1).PadLeft(246));

                    manager.Bold = true;
                    manager.DrawLine(true);
                    //manager.DrawTableRow(new List<string>() { "ARTICULO", "EMP. U/E", "CANT.", "U/M", "DESCRIPCION", "PRECIO", "DESC.","TOTAL","ITBIS" }, true, numtocalular: 5);

                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("DESCRIPCION".PadRight(149), noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("ARTICULO", noline: true);
                    manager.DrawText("EMP. U/E".PadLeft(32), noline: true);
                    manager.DrawText("CANT.".PadLeft(52), noline: true);
                    manager.DrawText("U/M".PadLeft(68), noline: true);
                    //manager.DrawText("DESCRIPCION".PadLeft(92), noline: true);
                    manager.DrawText("PRECIO".PadLeft(180), noline: true);
                    manager.DrawText("DESC.".PadLeft(200), noline: true);
                    manager.DrawText("TOTAL".PadLeft(221), noline: true);
                    manager.DrawText("ITBIS".PadLeft(250));
                    manager.DrawLine(true);

                    manager.Bold = false;
                    double subtotal = 0.0;
                    double subtotalSinDescuento = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myCot.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        //int descporciento = det.CotDescuento != 0 ? (int)((det.CotDescuento * 100) / det.CotPrecio) : 0;
                        double descporciento = det.CotDesPorciento;
                        var cantidad = ((double.Parse(det.CotCantidadDetalle.ToString()) / det.ProUnidades) + det.CotCantidad);
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.CotDescuento * cantidad;
                        var subtotalpro = det.CotCantidad * det.CotPrecio;
                        subtotalSinDescuento += subtotalpro;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;
                        string empaque = (det.CotCantidad / det.ProID).ToString() + " " + det.RepCodigo;
                        //manager.DrawTableRow(new List<string>() {det.ProCodigo, empaque.PadRight(5 - empaque.Length), det.PedCantidad.ToString(), det.UnmCodigo, det.ProDescripcion,
                        //                                         " $" + det.PedPrecio.ToString("N2"), descporciento.ToString() + "%", " $" + subtotalpro.ToString("N2"), " $" + itbis.ToString("N2")}, true, numtocalular: 5);

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(empaque.PadLeft(34), noline: true);
                        manager.DrawText(det.CotCantidad.ToString().PadLeft(52), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(68), noline: true);
                        //manager.DrawText(det.ProDescripcion.PadLeft(110).PadRight(120 - det.ProDescripcion.Length), noline: true);
                        manager.DrawText((" $" + det.CotPrecio.ToString("N2")).PadLeft(189 - (" $" + det.CotPrecio.ToString("N2")).Length), noline: true);
                        manager.DrawText((descporciento.ToString() + "%").PadLeft(200), noline: true);
                        manager.DrawText((" $" + subtotalpro.ToString("N2")).PadLeft(230 - (" $" + subtotalpro.ToString("N2")).Length), noline: true);
                        manager.DrawText((" $" + itbis1.ToString("N2")).PadLeft(256 - (" $" + itbis1.ToString("N2")).Length), noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (det.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(det.ProDescripcion.Substring(0, 38), alignCustom: 418);
                            manager.DrawText(det.ProDescripcion.Substring(38, det.ProDescripcion.Length - 38), alignCustom: 418);
                        }
                        else
                        {
                            manager.DrawText(det.ProDescripcion, alignCustom: 418);
                        }
                        manager.TextAlign = Justification.LEFT;
                        manager.DrawLine(true);
                    }
                    total += subtotal + totalitbis;

                    manager.NewLine(true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("SUBTOTAL:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + (subtotalSinDescuento).ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true); manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("DESCUENTO:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + (desc).ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("TOTAL GRAVADO:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + (subtotal).ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("TOTAL ITBIS:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(" $" + totalitbis.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("TOTAL A PAGAR:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(pedido.MonCodigo + " $" + total.ToString("N2"));
                    manager.DrawText("_______________________________________________________", ispointtoline: true, noline: true);
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
                    manager.NewLine();
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.NewLine();
                    manager.DrawText("________________________________", isline: true);
                    manager.DrawText("FIRMA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("*Cotizacion valida por 48 horas*");
                    manager.DrawText("*Sujeto a disponibilidad al momento de recibir la orden*");
                    manager.DrawText("*Precio Sujeto a Variacion de la prima en US$ y de los precios del petroleo*");
                    manager.DrawText("*Ordernes de compras para despachos parciales se anulan automaticamente si");
                    manager.DrawText("no son retiradas en la fecha establecida");
                    manager.DrawText("Todos lo precios, montos y total estan en RD$ Pesos Dominicanos");
                    manager.NewLine();
                    manager.DrawText("Formato pdf cotizaciones 5: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
        private Task<string> Formato20(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia2(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("COTIZACION");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.DrawText("Original");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Página", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText("1 de 1");
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Punto de Emisión:");
                    manager.Bold = true;
                    manager.DrawText("Número", noline: true);
                    manager.Bold = false;
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(Arguments.CurrentUser.RepCodigo + " - " + pedido.CotSecuencia.ToString());
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Fecha", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(date);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Vencimiento", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(date);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTextNew2(" ", true, 30);
                    manager.DrawText("Cliente:  " + pedido.CliNombre, noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Vendedor", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(Arguments.CurrentUser.RepNombre);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("RNC:  " + pedido.CliRnc, noline: true);
                    manager.DrawText(("Codigo:    " + pedido.CliCodigo).PadLeft(100), noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Condiciones Pago", noline: true);
                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText(pedido.RepCodigo);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Direccion:    " + pedido.CliCalle);

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Descripción".PadRight(214), noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Código Barra".PadLeft(140), noline: true);
                    manager.DrawText("Código", noline: true);
                    manager.DrawText("Cantidad".PadLeft(110), noline: true);
                    
                    manager.DrawText("Precio".PadLeft(170), noline: true);
                    manager.DrawText("Desc.".PadLeft(190), noline: true);
                    manager.DrawText("ITBIS".PadLeft(215), noline: true);
                    manager.DrawText("Monto".PadLeft(245));
                    manager.DrawLine();
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.CotPrecio - det.CotDescuento) * (det.CotItbis / 100);
                        var subtotalpro = det.CotCantidad * det.CotPrecio - det.CotDescuento;
                        subtotal += subtotalpro;

                        //int descporciento = det.CotDescuento != 0 ? (int)((det.CotDescuento * 100) / det.CotPrecio) : 0;
                        double descporciento = det.CotDesPorciento;
                        var cantidad = ((double.Parse(det.CotCantidadDetalle.ToString()) / det.ProUnidades) + det.CotCantidad);
                        totalitbis += (itbis * cantidad);
                        var desproc = det.CotDescuento * cantidad;
                        desc += desproc;

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(det.RepCodigo.PadLeft(140), noline: true);
                        manager.DrawText(det.CotCantidad.ToString().PadLeft(114), noline: true);
                        manager.DrawText(det.CotPrecio.ToString("N2").PadLeft(180 - det.CotPrecio.ToString("N2").Length), noline: true);
                        manager.DrawText(desproc.ToString("N2").PadLeft(195 - desproc.ToString("N2").Length), noline: true);
                        manager.DrawText(itbis.ToString("N2").PadLeft(223 - itbis.ToString("N2").Length), noline: true);
                        manager.DrawText(subtotalpro.ToString("N2").PadLeft(255 - subtotalpro.ToString("N2").Length), noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (det.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(det.ProDescripcion.Substring(0, 38), alignCustom: 550);
                            manager.DrawText(det.ProDescripcion.Substring(38, det.ProDescripcion.Length - 38), alignCustom: 550);
                        }
                        else
                        {
                            manager.DrawText(det.ProDescripcion, alignCustom: 550);
                        }
                        manager.TextAlign = Justification.LEFT;
                    }
                    total = subtotal + totalitbis - desc;

                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Moneda:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(pedido.MonCodigo.ToString());
                    manager.DrawText("_______________________________________________________");
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Subtotal:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(subtotal.ToString("N2"));
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Desc.:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(desc.ToString("N2"));
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Itbis:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(totalitbis.ToString("N2"));
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText($"Total {pedido.MonCodigo}:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(total.ToString("N2"));
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("________________________________");
                    manager.DrawText("FIRMA");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Formato pdf cotizaciones 20: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });
          }

        private Task<string> Formato8(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myCot.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de la cotizacion");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE COTIZACION No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.CotFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow(new List<string>() { "Código", "Descripción", "Cantidad", "Precio", "Itbis", "Total" }, true, numtocalular: 2);
                    manager.Bold = false;

                    double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
                    foreach (var det in myCot.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {

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

                        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo())
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(),
                            cantidad.PadRight(32), precioConItbis.ToString("N4"),
                            montoItbisTotal.ToString("N2"), subTotal.ToString("N2") }, numtocalular: 2);
                        }
                        else
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(),
                            cantidad.PadRight(32), precioConItbis.ToString("N2"),
                            montoItbisTotal.ToString("N2"), subTotal.ToString("N2") }, numtocalular: 2);
                        }


                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText("        Subtotal:" + subTotalTotal.ToString("N2").PadLeft(20));
                    manager.DrawText("     Total Itbis:" + itbisTotal.ToString("N2").PadLeft(20));
                    manager.DrawText("Total Descuentos:" + descuentoTotal.ToString("N2").PadLeft(20));
                    manager.DrawText("           Total:" + total.ToString("N2").PadLeft(20));
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf cotizaciones 8: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
    }
    }

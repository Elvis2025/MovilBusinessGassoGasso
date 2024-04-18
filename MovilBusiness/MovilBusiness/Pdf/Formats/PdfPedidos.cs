using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System.Collections.Generic;
using SkiaSharp;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfPedidos : IPdfGenerator
    {
        private DS_Pedidos myPed;
        private string SectorID = "";
        public PdfPedidos(string SecCodigo = "")
        {
            myPed = new DS_Pedidos();
            SectorID = SecCodigo;
        }

        public Task<string> GeneratePdf(int pedSecuencia, bool confirmado)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidosPDF() == 0 ? DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidos()[0]: DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidosPDF();

            switch (formato)
            {
                case 3:
                    return Formato3(pedSecuencia, confirmado);
                case 4:
                    return Formato4(pedSecuencia, confirmado);
                case 5:
                    return Formato5(pedSecuencia, confirmado);
                case 8:
                    return Formato8(pedSecuencia, confirmado);
                case 9:
                    return Formato9(pedSecuencia, confirmado);
                case 10://MGonzales Formato SAP
                    return Formato10(pedSecuencia, confirmado);
                case 11://MGonzales Formato Normal
                    return Formato11(pedSecuencia, confirmado);
                case 38:
                    return Formato38(pedSecuencia, confirmado);
                case 2:
                    return Formato2(pedSecuencia, confirmado);
                case 31:
                    return Formato31(pedSecuencia, confirmado);
                case 19: //Grupo Armenteros
                    return Formato19(pedSecuencia, confirmado);
                case 41: // Formato SAP con Descuento General - No Cambiar
                    return Formato41(pedSecuencia, confirmado);
                case 43: // ANDOSA
                    return Formato43(pedSecuencia, confirmado);
                default:
                    return Formato1(pedSecuencia, confirmado);
            }
        }
        private Task<string> Formato2(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Descripción"}, true);
                    manager.DrawTableRow(new List<string>() { "Código", "Cantidad", "Precio", "Descuento", "Itbis" }, true);
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                        subtotal += det.PedCantidad * det.PedPrecio;
                        var cantidad = ((det.PedCantidadDetalle / (det.ProUnidades == 0 ? 1 : det.ProUnidades)) + det.PedCantidad);
                        totalitbis += (itbis * cantidad);
                        desc += det.PedDescuento * cantidad;
                        manager.DrawTableRow(new List<string>() {det.ProDescripcion.Trim()});
                        manager.DrawTableRow(new List<string>() {det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N2"),
                                                                 det.PedDescuento.ToString("N2").PadLeft(13), itbis.ToString("N2").PadRight(5)});
                    }
                    total = subtotal + totalitbis - desc;
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Subtotal         : ".PadRight(10) + subtotal.ToString("N2").PadLeft(15));
                    manager.DrawText("Desc.              : ".PadRight(10) + desc.ToString("N2").PadLeft(15));
                    manager.DrawText("Total itbis      : ".PadRight(10) + totalitbis.ToString("N2").PadLeft(15));
                    manager.DrawText("Total Pedido : ".PadRight(10) + total.ToString("N2").PadLeft(15));
                   // manager.DrawLine();

                   // manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 2: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato1(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow(new List<string>() { "Descripción", "Código", "Cantidad", "Precio", "Itbis" }, true);
                    manager.Bold = false;

                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo())
                        {
                            manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N4"), det.PedItbis.ToString("N2") });
                        }
                        else
                        {
                            manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N2"), det.PedItbis.ToString("N2") });
                        }
                        
                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if(firma!= null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen,100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 1: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato9(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", "")))
                {
                    manager.PrintEmpresasWhitoutDir(SectorID);
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cuenta : " + pedido.CliCodigo);
                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";
                    var DateEnt = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy hh:mm tt");
                    }
                    if (DateTime.TryParse(pedido.PedFecha, out DateTime date1))
                    {
                        DateEnt = date1.ToString("dd/MM/yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Fecha Entrega    : " + DateEnt);
                    manager.NewLine();

                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Descripción"}, true);
                    manager.DrawTableRow(new List<string>() { "Código", "Cantidad", "Precio", "Importe" }, true);
                    manager.Bold = false;

                    double descfactura = 0.0;
                    double subtotal = 0.0;
                    double totalitbis = 0.0;
                    double totalfactura = 0.0;

                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        double Importe = det.PedCantidad * det.PedPrecio;
                        subtotal += Importe;
                        totalitbis += Importe * (det.PedItbis / 100);
                        var cantidad = (det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad;
                        descfactura += det.PedDescuento * cantidad;

                        manager.DrawTableRow(new List<string>() {det.ProDescripcion.Trim()});
                        manager.DrawTableRow(new List<string>() {det.ProCodigo, det.PedCantidad.ToString("N2"), det.PedPrecio.ToString("N2"), Importe.ToString("N2") });
                    }

                    totalfactura = subtotal + totalitbis - descfactura;

                    manager.DrawLine();
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText("Total: " + totalfactura.ToString("N2"));
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Nota: los precios mostrados son precios de referencia. " );
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.MAXTITLE;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.Font = PrinterFont.MINTITLE;
                    manager.DrawText("Formato pdf pedidos 9: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato8(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawTableRow(new List<string>() { "Descripción", "Código", "Cantidad", "Precio", "Itbis" }, true);
                    manager.Bold = false;

                    double descfactura = 0.0;
                    double subtotal = 0.0;
                    double totalitbis = 0.0;
                    double totalfactura = 0.0;

                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        double Importe = det.PedCantidad * det.PedPrecio;
                        subtotal += Importe;
                        totalitbis += Importe * (det.PedItbis / 100);
                        var cantidad = (det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad;
                        descfactura += det.PedDescuento * cantidad;

                        manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.ProCodigo.PadLeft(20), det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N2"), det.PedItbis.ToString("N2") });
                    }

                    totalfactura = subtotal + totalitbis - descfactura;


                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.               " + "Total: " + totalfactura.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 1: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato19(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos de el Pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa(true);
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Codigo : " + pedido.CliCodigo);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
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
                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        if (det.ProUnidades == 0)
                        {
                            det.ProUnidades = 1;
                        }

                        var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);
                        double descuento = det.PedDescuento * cantidad;

                        totaldescuento += descuento;
                        subtotal += det.PedPrecio * cantidad;

                        double totaltosum = (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;

                        total += totaltosum;

                        totalItbis += (itbis * cantidad);

                        int PedCantidad = Convert.ToInt32(det.PedCantidad);
                        int PedItbis = Convert.ToInt32(det.PedItbis);

                        manager.DrawText(PedCantidad.ToString().PadLeft(114 - PedCantidad.ToString().Length), noline: true);
                        manager.DrawText(det.PedPrecio.ToString("N2").PadLeft(150 - det.PedPrecio.ToString("N2").Length), noline: true);
                        manager.DrawText(det.PedDescuento.ToString("N2").PadLeft(186 - det.PedDescuento.ToString("N2").Length), noline: true);
                        manager.DrawText(("% " + PedItbis.ToString()).PadLeft(212 - ("% " + PedItbis.ToString()).Length), noline: true);
                        manager.DrawText(totaltosum.ToString("N2").PadLeft(254 - totaltosum.ToString("N2").Length), noline: true);
                        manager.DrawTableRow2(new List<string>() { det.ProCodigo, det.ProDescripcion});

                        /*

                        var align = 254 - totaltosum.ToString("N2").Length;
                        manager.DrawText(totaltosum.ToString("N2").PadLeft(align), noline: true);
                        manager.DrawTableRow2(new List<string>() { det.ProCodigo, det.ProDescripcion,
                            PedCantidad.ToString(), det.PedPrecio.ToString("N2"),
                            det.PedDescuento.ToString("N2"),
                            "% " + PedItbis.ToString() });
                        */
                    }
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawText(("Subtotal:    " + subtotal.ToString("N2").PadLeft(20)).PadLeft(230));
                    manager.DrawText(("Total Itbis:    " + totalItbis.ToString("N2").PadLeft(21)).PadLeft(234));
                    manager.DrawText(("Total Descuentos:    " + totaldescuento.ToString("N2").PadLeft(22)).PadLeft(234));
                    manager.DrawText(("Total:    " + total.ToString("N2").PadLeft(20)).PadLeft(233));
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 19: MovilBusiness" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
        private Task<string> Formato31(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy");
                    }
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawTextNew("PEDIDO", true);
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("No. Documento                          Fecha                           Página      ");
                    manager.Bold = true;
                    manager.DrawText(pedido.PedSecuencia.ToString().PadRight(35) + date.PadRight(35) + "1/1".PadRight(12));
                    manager.Bold = false;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("RNC".PadRight(50));
                    manager.Bold = true;
                    manager.DrawText(pedido.CliRnc.PadRight(50));
                    manager.Bold = false;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("Referencia                             Contacto                                          ");
                    manager.Bold = true;
                    manager.DrawText(Arguments.CurrentUser.RepCodigo + "-" + pedido.PedSecuencia.ToString().PadRight(40) + Arguments.CurrentUser.RepNombre.PadRight(45));
                    manager.Bold = false;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.Bold = false;
                    manager.DrawText("Entrega:          " + date + "Moneda:  ".PadLeft(194) + pedido.MonCodigo.ToString());
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Descripción", "Cantidad", "UdM", "Empaque", "Precio", "Total" }, true);
                    manager.Bold = false;

                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;

                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = ((det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100) * det.PedCantidad);

                        totalitbis += itbis;

                        desc += det.PedDescuento * det.PedCantidad;

                        double totaltosum = det.PedCantidad * det.PedPrecio;

                        subtotal += totaltosum;

                        manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.PedCantidad.ToString("N2"), det.UnmCodigo, " ", det.PedPrecio.ToString("N2"), totaltosum.ToString("N2") });
                        manager.DrawText("Item Code:           " + det.ProCodigo);
                    }
                    total = subtotal + totalitbis - desc;

                    manager.DrawLine(true);
                    manager.Bold = true;
                    manager.DrawTextNew($"Términos de pago:  {pedido.ConDescripcion.PadLeft(88)}", true, 2);
                    manager.DrawText("_________________________________________________________________________", isline: true, noline: true);
                    manager.NewLine(true);
                    manager.NewLine(true);
                    manager.DrawTextNew("MB5:", true, 12);
                    manager.Bold = false;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Subtotal:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(subtotal.ToString("N2"));                    
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Descuento:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(desc.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Total antes de impuestos:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText((subtotal - desc).ToString("N2"));                   
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Impuestos:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(totalitbis.ToString("N2"));                    
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("Total:" , noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(total.ToString("N2"));
                    manager.DrawText("_________________________________________________________", isline: true, noline: true);
                    manager.TextAlign = Justification.LEFT;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("Firma:", noline: true);
                    manager.DrawText("                ________________________________", isline: true);
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 31: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato38(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("Sales Order details not found.");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa(english: true);
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.DrawTextNew("SALES ORDER              ", true);
                    manager.Bold = false;
                    manager.Font = PrinterFont.FOOTER;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.DrawTextNew2("Doc. Date         Page         Sales Ord No.         Created Date         Cust. No.               ", body: 16);
                    manager.Bold = false;
                    //manager.DrawText(date.PadRight(25) + "1".PadRight(25) + pedido.PedSecuencia.ToString().PadRight(20) + DateTime.Now.ToString("dd-MM-yyyy").PadRight(30) +  pedido.CliCodigo.PadLeft(15));
                    manager.NewLine();
                    manager.DrawText(date.PadRight(113), noline: true);
                    manager.DrawText("1".PadRight(90), noline: true);
                    manager.DrawText(pedido.PedSecuencia.ToString().PadRight(66), noline: true);
                    manager.DrawText(DateTime.Now.ToString("dd-MM-yyyy").PadRight(32), noline: true);
                    manager.DrawText(pedido.CliCodigo);
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.CENTERRIGHT3;
                    manager.DrawText("Customer : " + pedido.CliNombre);
                    manager.DrawText("Street   : " + pedido.CliCalle);
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = true;
                    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.Bold = false;
                    manager.DrawText("Delivery:         " + date + "Currency:  ".PadLeft(194) + pedido.MonCodigo.ToString());
                    manager.Bold = true;
                    //manager.DrawTableRow(new List<string>() { "Description", "Quantity", "UdM", "Packing", "Price", "Total" }, true);
                    manager.DrawTableRow2(new List<string>() {"Item Code", "Description", "Packing", "UdM", "Quantity", "Price", "Total" }, true);

                    manager.Bold = false;

                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    int cantidadtotal = 0;

                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                        totalitbis += itbis;

                        desc += det.PedDescuento;

                        double totaltosum = det.PedCantidad * det.PedPrecio;
                        cantidadtotal += (int)det.PedCantidad;
                        subtotal += totaltosum;
                        manager.DrawTableRow2(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(), " ", det.UnmCodigo, ((int)det.PedCantidad).ToString(), det.PedPrecio.ToString("N2"), totaltosum.ToString("N2") });
                    }

                    total = subtotal + totalitbis - desc;
                    manager.DrawLine(true);
                    manager.Bold = true;
                    manager.DrawTextNew($"Payment terms:  {pedido.ConDescripcion.PadLeft(88)}", true, 2);
                    manager.DrawText("_________________________________________________________________________", isline: true, noline: true);
                    manager.NewLine(true);
                    manager.NewLine(true);
                    manager.Bold = false;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Total Quantity:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText(cantidadtotal.ToString());
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("SubTotal:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("RD$" + subtotal.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Total Before Tax:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("RD$" + (subtotal - desc).ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.DrawText("Taxes:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("RD$" + totalitbis.ToString("N2"));
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("Total:", noline: true);
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("RD$" + total.ToString("N2"));
                    manager.DrawText("_________________________________________________________", isline: true, noline: true);
                    manager.TextAlign = Justification.LEFT;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());

                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }

                    manager.DrawText("Signature:", noline: true);
                    manager.DrawText("                   ________________________________", isline: true);
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Seller : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Cel Phone number  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Phone number : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Sales Order Format PDF 38: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
        private Task<string> Formato3(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTERRIGHT;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO");
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
                    manager.DrawText(Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString());
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
                    manager.DrawText(pedido.ConDescripcion);
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
                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        var subtotalpro = det.PedCantidad * det.PedPrecio - det.PedDescuento;
                        subtotal += det.PedCantidad * det.PedPrecio;

                        //int descporciento = det.PedDescuento != 0 ? (int)((det.PedDescuento * 100) / det.PedPrecio) : 0;
                        double descporciento = det.PedDesPorciento;
                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        totalitbis += (itbis * cantidad);
                        var desproc = det.PedDescuento * cantidad;
                        desc += desproc;

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(det.PedCantidad.ToString().PadLeft(114), noline: true);
                        manager.DrawText(det.RepSupervidor.ToString().PadLeft(140), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(170), noline: true);
                        manager.DrawText(det.PedPrecio.ToString("N2").PadLeft(198 - det.PedPrecio.ToString("N2").Length), noline: true);
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
                    manager.DrawText("Formato pdf pedidos 3: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        //private Task<string> Formato3(int pedSecuencia, bool confirmado)
        //{
        //    return Task.Run(() =>
        //    {
        //        var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

        //        if (pedido == null)
        //        {
        //            throw new Exception("No se encontraron los datos del pedido");
        //        }

        //        using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
        //        {

        //            var date = "";

        //            if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
        //            {
        //                date = fecha.ToString("dd-MM-yyyy");
        //            }
        //            manager.PrintEmpresa();
        //            manager.NewLine();
        //            manager.Font = PrinterFont.TITLE;
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.Bold = true;
        //            manager.DrawTextNew("PEDIDO", true);
        //            manager.Bold = false;
        //            manager.Font = PrinterFont.BODY;
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.DrawText("No. Documento                          Fecha                           Página      ");
        //            manager.Bold = true;
        //            manager.DrawText(pedido.PedSecuencia.ToString().PadRight(35) + date.PadRight(35) + "1/1".PadRight(12));
        //            manager.Bold = false;
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.DrawText("RNC".PadRight(50));
        //            manager.Bold = true;
        //            manager.DrawText(pedido.CliRnc.PadRight(50));
        //            manager.Bold = false;

        //            manager.TextAlign = Justification.LEFT;
        //            manager.DrawText("", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.LEFT;
        //            manager.DrawText("Cliente : " + pedido.CliNombre, noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText("Referencia                                                          Contacto             ");
        //            manager.TextAlign = Justification.LEFT;
        //            manager.DrawText("Calle   : " + pedido.CliCalle, noline: true);
        //            manager.TextAlign = Justification.RIGHT;

        //            manager.Bold = true;
        //            manager.DrawText(("      " + Arguments.CurrentUser.RepCodigo + "-" + pedido.PedSecuencia.ToString()).PadLeft(12).PadRight(68) + Arguments.CurrentUser.RepNombre);
        //            manager.Bold = false;
        //            manager.TextAlign = Justification.LEFT;
        //            manager.NewLine();
        //            manager.Bold = true;
        //            //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.Bold = false;
        //            manager.DrawText("Entrega:          " + date + "Moneda:  ".PadLeft(194) + pedido.MonCodigo.ToString());
        //            manager.Bold = true;
        //            manager.DrawTableRow(new List<string>() { "Descripción", "Cantidad", "UdM", "Precio", "Total", "Cod. Barra" }, true);
        //            manager.Bold = false;

        //            double subtotal = 0.0;
        //            double desc = 0.0;
        //            double totalitbis = 0.0;
        //            double total = 0.0;

        //            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
        //            {
        //                var itbis = ((det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100) * det.PedCantidad);

        //                totalitbis += itbis;

        //                desc += det.PedDescuento * det.PedCantidad;

        //                double totaltosum = det.PedCantidad * det.PedPrecio;

        //                subtotal += totaltosum;

        //                manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.PedCantidad.ToString("N2"), det.UnmCodigo, det.PedPrecio.ToString("N2"), totaltosum.ToString("N2"), det.AutSecuencia.ToString() });

        //                manager.Bold = true;
        //                manager.DrawText("Item Code:", noline: true);
        //                manager.Bold = false;
        //                manager.DrawText("                      " + det.ProCodigo);
        //            }
        //            total = subtotal + totalitbis - desc;

        //            manager.DrawLine(true);
        //            manager.Bold = true;
        //            manager.DrawTextNew($"Términos de pago:  {pedido.ConDescripcion.PadLeft(88)}", true, 2);
        //            manager.DrawText("_________________________________________________________________________", isline: true, noline: true);
        //            manager.NewLine(true);
        //            manager.NewLine(true);
        //            manager.DrawTextNew("MB5:", true, 12);
        //            manager.Bold = false;
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("Subtotal:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(subtotal.ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("Descuento:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(desc.ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("Total antes de impuestos:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText((subtotal - desc).ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("Impuestos:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(totalitbis.ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.Bold = true;
        //            manager.DrawText("Total:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(total.ToString("N2"));
        //            manager.DrawText("_________________________________________________________", isline: true, noline: true);
        //            manager.TextAlign = Justification.LEFT;
        //            var myTranImg = new DS_TransaccionesImagenes();
        //            var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
        //            if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
        //            {
        //                manager.DrawImageForFirma(firma.TraImagen, 100);
        //            }
        //            manager.DrawText("Firma:", noline: true);
        //            manager.DrawText("                ________________________________", isline: true);
        //            manager.Bold = false;
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.Font = PrinterFont.FOOTER;
        //            manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
        //            manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
        //            manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
        //            manager.DrawText("Formato pdf pedidos 3: MovilBusiness v" + Functions.AppVersion);

        //            return manager.FilePath;
        //        }

        //    });

        //}


        //private Task<string> Formato4(int pedSecuencia, bool confirmado)
        //{
        //    return Task.Run(() =>
        //    {
        //        var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

        //        if (pedido == null)
        //        {
        //            throw new Exception("No se encontraron los datos del pedido");
        //        }

        //        using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
        //        {
        //            var date = "";

        //            if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
        //            {
        //                date = fecha.ToString("dd-MM-yyyy");
        //            }

        //            manager.Font = PrinterFont.MINTITLE;
        //            manager.PrintEmpresa();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.TextAlign = Justification.CENTERRIGHT3;
        //            manager.DrawText("PEDIDO PARA:       " + pedido.CliCodigo.PadRight(80));
        //            manager.Bold = true;
        //            manager.DrawText(pedido.CliNombre.ToUpper());
        //            manager.Bold = false;
        //            manager.DrawText("RNC: " + pedido.CliRnc + "    TELEFONO: " + pedido.CliTelefono);
        //            manager.DrawText(pedido.CliCalle);
        //            manager.NewLine();
        //            manager.DrawText("ENVIADO A: ".PadRight(80));
        //            manager.DrawText(pedido.CliCalle);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.NewLine();
        //            manager.DrawText("PAGINA: 1/1");
        //            manager.TextAlign = Justification.LEFT;
        //            manager.Bold = true;
        //            manager.DrawText("______________________________________________________________________________________________________________________", ispointtoline: true);
        //            //manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.Bold = false;
        //            manager.Font = PrinterFont.BODY;
        //            manager.DrawText("NO.:  " + (Arguments.CurrentUser.RepCodigo + "-" + pedido.PedSecuencia.ToString()) + "   FECHA:  " + date + "   MONEDA:  " + pedido.MonCodigo.ToString() +
        //                "  CONDIC. PAGO: " + pedido.ConDescripcion + "   REFERENCIA:   " + "" + "  VEND.:" + Arguments.CurrentUser.RepCodigo + "  " + Arguments.CurrentUser.RepTelefono1);
        //            manager.Bold = true;
        //            manager.Font = PrinterFont.MINTITLE;
        //            //manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.DrawText("______________________________________________________________________________________________________________________", ispointtoline: true);
        //            manager.DrawTableRow(new List<string>() { "ARTICULO", "EMP. U/E", "CANT.", "U/M", "PRECIO", "DESC.", "TOTAL", "ITBIS" }, numtocalular: 0);
        //            manager.Bold = false;
        //            double subtotal = 0.0;
        //            double desc = 0.0;
        //            double totalitbis = 0.0;
        //            double total = 0.0;
        //            foreach (var det in myPed.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
        //            {
        //                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
        //                var subtotalpro = det.PedCantidad * det.PedPrecio - det.PedDescuento;
        //                subtotal += subtotalpro;

        //                int descporciento = (int)((det.PedDescuento * 100) / det.PedPrecio);

        //                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
        //                totalitbis += (itbis * cantidad);
        //                var desproc = det.PedDescuento * cantidad;
        //                desc += desproc;
        //                string empaque = (det.PedCantidad / det.PedTipoOferta).ToString() + " " + det.CedCodigo;
        //                manager.Bold = true;
        //                manager.DrawText("DESCRIPCION:", noline: true);
        //                manager.Bold = false;
        //                manager.DrawText("                                  " + det.ProDescripcion);
        //                manager.DrawTableRow(new List<string>() {det.ProCodigo, empaque, det.PedCantidad.ToString(), det.UnmCodigo,
        //                                                         " $" + det.PedPrecio.ToString("N2"), descporciento.ToString() + "%", " $" + subtotalpro.ToString("N2"), " $" + itbis.ToString("N2")}, true, numtocalular: 0);
        //            }
        //            total += subtotal + totalitbis - desc;

        //            manager.DrawLine(true);
        //            manager.NewLine(true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("TOTAL GRAVADO:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(" $" + (subtotal - desc).ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.DrawText("TOTAL ITBIS:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(" $" + totalitbis.ToString("N2"));
        //            manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);

        //            manager.TextAlign = Justification.CENTERRIGHT;
        //            manager.Bold = true;
        //            manager.DrawText("TOTAL A PAGAR:", noline: true);
        //            manager.TextAlign = Justification.RIGHT;
        //            manager.DrawText(pedido.MonCodigo + " $" + total.ToString("N2"));
        //            manager.DrawText("__________________________________________", isline: true, noline: true);
        //            manager.TextAlign = Justification.CENTER;
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.DrawText("Firma:");
        //            var myTranImg = new DS_TransaccionesImagenes();
        //            var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
        //            if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
        //            {
        //                manager.DrawImageForFirma(firma.TraImagen, 100);
        //            }
        //            manager.NewLine();
        //            manager.DrawText("________________________________", isline: true);
        //            manager.TextAlign = Justification.LEFT;
        //            manager.Bold = false;
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.NewLine();
        //            manager.Font = PrinterFont.FOOTER;
        //            manager.DrawText("Formato pdf pedidos 4: MovilBusiness v" + Functions.AppVersion);

        //            return manager.FilePath;
        //        }

        //    });

        //}
        private Task<string> Formato4(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
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
                    manager.DrawText("PEDIDO PARA:       " + pedido.CliCodigo.PadRight(80));
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
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString()).PadLeft(17), noline: true);

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
                    manager.DrawText(pedido.ConDescripcion.PadLeft(148), noline: true);

                    manager.Bold = true;
                    manager.DrawText("REFERENCIA:".PadLeft(180), noline: true);
                    pedido.PedFechaEntrega = pedido.PedFechaEntrega == null ? "NINGUNA" : pedido.PedFechaEntrega;
                    manager.Bold = false;
                    manager.DrawText(pedido.PedFechaEntrega.PadLeft(202), noline: true);

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
                    foreach (var det in myPed.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        int descporciento = det.PedDescuento != 0 ? (int)((det.PedDescuento * 100) / det.PedPrecio) : 0;

                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.PedDescuento * cantidad;
                        var subtotalpro = det.PedCantidad * det.PedPrecio;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;
                        string empaque = (det.PedCantidad / det.PedTipoOferta).ToString() + " " + det.CedCodigo;
                        //manager.DrawTableRow(new List<string>() {det.ProCodigo, empaque.PadRight(5 - empaque.Length), det.PedCantidad.ToString(), det.UnmCodigo, det.ProDescripcion,
                        //                                         " $" + det.PedPrecio.ToString("N2"), descporciento.ToString() + "%", " $" + subtotalpro.ToString("N2"), " $" + itbis.ToString("N2")}, true, numtocalular: 5);
                        
                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(empaque.PadLeft(34), noline: true);
                        manager.DrawText(det.PedCantidad.ToString().PadLeft(52), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(68), noline: true);
                        //manager.DrawText(det.ProDescripcion.PadLeft(110).PadRight(120 - det.ProDescripcion.Length), noline: true);
                        manager.DrawText((" $" + det.PedPrecio.ToString("N2")).PadLeft(189 - (" $" + det.PedPrecio.ToString("N2")).Length), noline: true);
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
                    manager.DrawText("Formato pdf pedidos 4: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }
        private Task<string> Formato5(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
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
                    manager.DrawText("PEDIDO PARA:       " + pedido.CliCodigo.PadRight(80));
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
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString()).PadLeft(17), noline: true);

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
                    manager.DrawText(pedido.ConDescripcion.PadLeft(148), noline: true);

                    manager.Bold = true;
                    manager.DrawText("REFERENCIA:".PadLeft(180), noline: true);
                    pedido.PedFechaEntrega = pedido.PedFechaEntrega == null ? "NINGUNA" : pedido.PedFechaEntrega;
                    manager.Bold = false;
                    manager.DrawText(pedido.PedFechaEntrega.PadLeft(202), noline: true);

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
                    foreach (var det in myPed.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        int descporciento = det.PedDescuento != 0 ? (int)((det.PedDescuento * 100) / det.PedPrecio) : 0;

                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.PedDescuento * cantidad;
                        var subtotalpro = det.PedCantidad * det.PedPrecio;
                        subtotalSinDescuento += subtotalpro;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;
                        string empaque = (det.PedCantidad / det.PedTipoOferta).ToString() + " " + det.CedCodigo;
                        //manager.DrawTableRow(new List<string>() {det.ProCodigo, empaque.PadRight(5 - empaque.Length), det.PedCantidad.ToString(), det.UnmCodigo, det.ProDescripcion,
                        //                                         " $" + det.PedPrecio.ToString("N2"), descporciento.ToString() + "%", " $" + subtotalpro.ToString("N2"), " $" + itbis.ToString("N2")}, true, numtocalular: 5);

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(empaque.PadLeft(34), noline: true);
                        manager.DrawText(det.PedCantidad.ToString().PadLeft(52), noline: true);
                        manager.DrawText(det.UnmCodigo.PadLeft(68), noline: true);
                        //manager.DrawText(det.ProDescripcion.PadLeft(110).PadRight(120 - det.ProDescripcion.Length), noline: true);
                        manager.DrawText((" $" + det.PedPrecio.ToString("N2")).PadLeft(189 - (" $" + det.PedPrecio.ToString("N2")).Length), noline: true);
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
                    manager.DrawText("Formato pdf pedidos 5: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato41(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuenciaConTotales(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Descripción" }, true);
                    manager.DrawTableRow(new List<string>() { "Código", "Cantidad", "Precio", "Total" }, true);
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    double totalFlete = 0.0;
                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbisConDescuentoGeneral = ((det.PedPrecio - det.PedDescuento) - ((det.PedPrecio - det.PedDescuento) * (pedido.PedPorCientoDsctoGlobal / 100))) * (det.PedItbis / 100);
                        var precioConDescuento = (det.PedPrecio - det.PedDescuento);
                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var totalLinea = precioConDescuento * cantidad;

                        manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim() });
                        manager.DrawTableRow(new List<string>() {det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), precioConDescuento.ToString("N2"),
                                                                 totalLinea.ToString("N2").PadLeft(13)});

                        subtotal += totalLinea;

                        if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                        {
                            totalFlete += det.PedFlete;
                        }
                    }
                    total = pedido.PedMontoTotal;
                    totalitbis = pedido.PedMontoITBIS;
                    desc = pedido.PedMontoDsctoGlobal;

                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Subtotal         : ".PadRight(10) + subtotal.ToString("N2").PadLeft(15));
                    manager.DrawText("Desc.              : ".PadRight(10) + desc.ToString("N2").PadLeft(15));
                    manager.DrawText("Total itbis      : ".PadRight(10) + totalitbis.ToString("N2").PadLeft(15));
                    if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                    {
                    manager.DrawText("Total flete      : ".PadRight(10) + totalFlete.ToString("N2").PadLeft(15));
                    }
                    manager.DrawText("Total Pedido : ".PadRight(10) + total.ToString("N2").PadLeft(15));
                    // manager.DrawLine();

                    // manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 41: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato43(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuenciaConTotales(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle   : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha   : " + date);
                    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Descripción" }, true);
                    manager.DrawTableRow(new List<string>() { "Código", "Cantidad", "Precio", "%Desc.", "Total Linea" }, true);
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    double totalFlete = 0.0;
                    double descuentoUnitario = 0.0;
                    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                    {
                        var itbisConDescuentoGeneral = ((det.PedPrecio - det.PedDescuento) - ((det.PedPrecio - det.PedDescuento) * (pedido.PedPorCientoDsctoGlobal / 100))) * (det.PedItbis / 100);
                        var precioConDescuento = (det.PedPrecio - det.PedDescuento);
                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var totalLinea = Math.Round(precioConDescuento * cantidad, 2);

                        manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim() });
                        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo() && pedido.MonCodigo == "USD")
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N4"), (det.PedDesPorciento.ToString() + "%").ToString(), totalLinea.ToString("N2").PadLeft(13) });
                        }
                        else
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N2"), (det.PedDesPorciento.ToString() + "%").ToString(), totalLinea.ToString("N2").PadLeft(13) });
                        }

                        subtotal += Math.Round(det.PedPrecio * cantidad, 2);
                        descuentoUnitario += Math.Round(det.PedDescuento * cantidad, 2);
                        if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                        {
                            totalFlete += det.PedFlete;
                        }
                    }
                    totalitbis = pedido.PedMontoITBIS;
                    desc = pedido.PedMontoDsctoGlobal + Math.Round(descuentoUnitario, 2);
                    total = subtotal - desc + totalitbis;

                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Subtotal         : ".PadRight(10) + subtotal.ToString("N2").PadLeft(15));
                    manager.DrawText("Desc.              : ".PadRight(10) + desc.ToString("N2").PadLeft(15));
                    manager.DrawText("Total itbis      : ".PadRight(10) + totalitbis.ToString("N2").PadLeft(15));
                    if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                    {
                        manager.DrawText("Total flete      : ".PadRight(10) + totalFlete.ToString("N2").PadLeft(15));
                    }
                    manager.DrawText("Total Pedido : ".PadRight(10) + total.ToString("N2").PadLeft(15));
                    // manager.DrawLine();

                    // manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 43: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }


        private Task<string> Formato10(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
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
                    manager.DrawText("PEDIDO PARA:       " + pedido.CliCodigo.PadRight(80));
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
                    manager.DrawText("NO.: ", noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepCodigo + " - " + pedido.PedSecuencia.ToString()).PadLeft(17), noline: true);

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
                    manager.DrawText(pedido.ConDescripcion.PadLeft(148), noline: true);

                    manager.Bold = true;
                    manager.DrawText("REFERENCIA:".PadLeft(180), noline: true);
                    pedido.PedFechaEntrega = pedido.PedFechaEntrega == null ? "NINGUNA" : pedido.PedFechaEntrega;
                    manager.Bold = false;
                    manager.DrawText(pedido.PedFechaEntrega.PadLeft(202), noline: true);

                    manager.Bold = true;
                    manager.DrawText("VEND.:".PadLeft(222), noline: true);
                    manager.Bold = false;
                    manager.DrawText((Arguments.CurrentUser.RepNombre ).PadLeft(242));

                    manager.Bold = true;
                    manager.DrawLine(true);

                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("DESCRIPCION".PadRight(136), noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("CODIGO", noline: true);
                    manager.DrawText("COD. BARRA".PadLeft(40), noline: true);
                    manager.DrawText("CANT.".PadLeft(68), noline: true);
                    manager.DrawText("PRECIO".PadLeft(188), noline: true);
                    manager.DrawText("ITBIS".PadLeft(240));
                    manager.DrawLine(true);

                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myPed.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        int descporciento = det.PedDescuento != 0 ? (int)((det.PedDescuento * 100) / det.PedPrecio) : 0;

                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.PedDescuento * cantidad;
                        var subtotalpro = det.PedCantidad * det.PedPrecio;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;

                        manager.DrawText(det.ProCodigo, noline: true);
                        manager.DrawText(det.ProReferencia.PadLeft(40), noline: true);
                        manager.DrawText(det.PedCantidad.ToString("N2").PadLeft(68), noline: true);
                        manager.DrawText((" $" + det.PedPrecio.ToString("N2")).PadLeft(197 - (" $" + det.PedPrecio.ToString("N2")).Length), noline: true);
                        manager.DrawText((det.PedItbis.ToString("N2")).PadLeft(246 - (det.PedItbis.ToString("N2")).Length), noline: true);
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
                    manager.NewLine(true);
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
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
                    manager.DrawText("Formato pdf pedidos 10: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato11(int pedSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var pedido = myPed.GetBySecuencia(pedSecuencia, confirmado);

                if (pedido == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                using (var manager = PdfManagerPrue.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    manager.PrintEmpresaLogoCenter();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.MINTITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + pedido.CliNombre);
                    manager.DrawText("Calle    : " + pedido.CliCalle);

                    var date = "";

                    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                    }

                    manager.DrawText("Fecha  : " + date);
                    manager.DrawText("Urb.     : " + pedido.CliUrbanizacion);
                    manager.NewLine();

                    manager.DrawLine(false);
                    manager.Bold = true;
                    manager.DrawText("Descripción", noline: true);
                    manager.DrawText("Código Interno".PadLeft(95), noline: true);
                    manager.DrawText("Código EAN".PadLeft(130), noline: true);
                    manager.DrawText("Cantidad".PadLeft(170), noline: true);
                    manager.DrawText("Precio".PadLeft(200), noline: true);
                    manager.DrawText("Itbis".PadLeft(230));
                    manager.Bold = false;
                    manager.DrawLine(false);

                    
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    foreach (var det in myPed.GetDetalleBySecuenciaANDEMP(pedSecuencia, confirmado))
                    {
                        int descporciento = det.PedDescuento != 0 ? (int)((det.PedDescuento * 100) / det.PedPrecio) : 0;

                        var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                        var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                        var itbis1 = itbis * cantidad;
                        var desproc = det.PedDescuento * cantidad;
                        var subtotalpro = det.PedCantidad * det.PedPrecio;
                        subtotalpro -= desproc;
                        subtotal += subtotalpro;

                        totalitbis += (itbis * cantidad);
                        desc += desproc;

                        manager.DrawText(det.ProCodigo.PadLeft(90), noline: true);
                        manager.DrawText(det.ProReferencia.PadLeft(130), noline: true);
                        manager.DrawText(det.PedCantidad.ToString("N2").PadLeft(173), noline: true);
                        manager.DrawText((" $" + det.PedPrecio.ToString("N2")).PadLeft(208 - (" $" + det.PedPrecio.ToString("N2")).Length), noline: true);
                        manager.DrawText((det.PedItbis.ToString("N2")).PadLeft(234 - (det.PedItbis.ToString("N2")).Length), noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (det.ProDescripcion.Length >= 28)
                        {
                            manager.DrawText(det.ProDescripcion.Substring(0, 28), alignCustom: 595);
                            manager.DrawText(det.ProDescripcion.Substring(28, det.ProDescripcion.Length - 28), alignCustom: 595);
                        }
                        else
                        {
                            manager.DrawText(det.ProDescripcion, alignCustom: 595);
                        }
                        manager.TextAlign = Justification.LEFT;
                        
                    }
                    total += subtotal + totalitbis;

                    manager.NewLine(true);
                    manager.NewLine(true);
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
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
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf pedidos 11: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

                //using (var manager = PdfManager.NewDocument((pedido.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia).Replace("/", ""), SectorID))
                //{
                //    manager.PrintEmpresa();
                //    manager.NewLine();
                //    manager.Font = PrinterFont.TITLE;
                //    manager.TextAlign = Justification.CENTER;
                //    manager.Bold = true;
                //    manager.DrawText("ORDEN DE PEDIDO No. " + pedSecuencia);
                //    manager.Bold = false;
                //    manager.TextAlign = Justification.LEFT;
                //    manager.NewLine();
                //    manager.Font = PrinterFont.BODY;

                //    manager.DrawText("Cliente : " + pedido.CliNombre);
                //    manager.DrawText("Calle   : " + pedido.CliCalle);

                //    var date = "";

                //    if (DateTime.TryParse(pedido.PedFecha, out DateTime fecha))
                //    {
                //        date = fecha.ToString("dd-MM-yyyy hh:mm tt");
                //    }

                //    manager.DrawText("Fecha   : " + date);
                //    manager.DrawText("Urb:    : " + pedido.CliUrbanizacion);
                //    manager.NewLine();

                //    manager.Bold = true;
                //    //manager.DrawText("Descripción                          Cantidad                          Código                          Precio", true);
                //    manager.DrawTableRow(new List<string>() { "Descripción", "Código Interno", "Código EAN", "Cantidad", "Precio", "Itbis" }, true, corteManual: 15);
                //    manager.Bold = false;

                //    foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
                //    {
                //        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo())
                //        {
                //            manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.ProCodigo,det.ProReferencia, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N4"), det.PedItbis.ToString("N2") }, corteManual:15);
                //        }
                //        else
                //        {
                //            manager.DrawTableRow(new List<string>() { det.ProDescripcion.Trim(), det.ProCodigo,det.ProReferencia, det.PedCantidad.ToString("N2").PadRight(32), det.PedPrecio.ToString("N2"), det.PedItbis.ToString("N2") }, corteManual: 15);
                //        }

                //    }
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.DrawLine();
                //    manager.DrawText("Nota: los precios mostrados son precios de referencia.");
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.TextAlign = Justification.CENTER;
                //    var myTranImg = new DS_TransaccionesImagenes();
                //    var firma = myTranImg.GetFirmaByTransaccion(1, pedSecuencia.ToString());
                //    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                //    {
                //        manager.DrawImageForFirma(firma.TraImagen, 100);
                //    }
                //    manager.DrawText("_____________________________________________");
                //    manager.DrawText("Firma del cliente");
                //    manager.TextAlign = Justification.LEFT;
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.NewLine();
                //    manager.Font = PrinterFont.FOOTER;
                //    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                //    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                //    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                //    manager.DrawText("Formato pdf pedidos 11: MovilBusiness v" + Functions.AppVersion);

                //    return manager.FilePath;
                //}

            });

        }
    }

}


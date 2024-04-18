using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfVentas : IPdfGenerator
    {
        private DS_Ventas myVen;
        private string SectorID = "";
        public PdfVentas(string SecCodigo="")
        {
            myVen = new DS_Ventas();
            SectorID = SecCodigo;
        }

        public Task<string> GeneratePdf(int venSecuencia, bool confirmado = false)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas();

            switch (formato)
            {
                case 41: // Formato SAP con Descuento General - No Cambiar
                    return Formato41(venSecuencia, confirmado);
                case 42: // La Libanesa
                    return Formato42(venSecuencia, confirmado);
                case 43:
                    return Formato43(venSecuencia, confirmado);
                default:
                    return Formato1(venSecuencia, confirmado);
                

            }
        }

        private Task<string> Formato1(int venSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

                if (venta == null)
                {
                    throw new Exception("No se encontraron los datos de la venta");
                }

                using (var manager = PdfManager.NewDocument((venta.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();

                    manager.Font = PrinterFont.TITLE;

                    if (venta.CliTipoComprobanteFAC != "99")
                    {
                        manager.DrawText("NCF: " + venta.VenNCF);

                        if (venta.CliTipoComprobanteFAC == "01")
                        {
                            manager.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                        }
                    }

                    manager.Font = PrinterFont.BODY;

                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
                    manager.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
                    manager.DrawText("Fecha venta: " + venta.VenFecha);
                    manager.DrawText("Cliente: " + venta.CliNombre);
                    manager.DrawText("Codigo: " + venta.CliCodigo);
                    manager.DrawText("Calle: " + venta.CliCalle);
                    manager.DrawText("Urb: " + venta.CliUrbanizacion);

                    manager.DrawLine();
                    manager.Bold = true;

                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string> { "Cant.", "Precio", "Monto Itbis", "Descuento", "Importe" });
                    manager.Bold = false;

                    manager.DrawLine();

                    double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

                    foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
                    {
                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                        var cantidad = det.VenCantidad.ToString();
                        var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                        var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                        var precioConItbis = precioLista + montoItbis;
                        var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);

                        var montoItbisTotal = montoItbis * cantidadTotal;
                        var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                        itbisTotal += montoItbisTotal;
                        total += subTotal;
                        descuentoTotal += (det.VenDescuento * cantidadTotal);
                        subTotalTotal += (precioLista * cantidadTotal);

                        if (det.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow(new List<string>() { cantidad, precioLista.ToString("N2"), montoItbisTotal.ToString("N2"), det.VenDescuento.ToString("N2"), subTotal.ToString("N2") });
                    }

                    manager.DrawLine();
                    manager.DrawText("SKU: " + venta.VenTotal);
                    manager.NewLine();
                    manager.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(14));
                    manager.Bold = true;
                    manager.DrawText("Total:       " + total.ToString("N2").PadLeft(14));
                    manager.Bold = false;
                    manager.NewLine();

                    if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
                    {
                        manager.TextAlign = Justification.CENTER;
                        manager.Bold = true;
                        manager.DrawText("FORMA DE PAGO");
                        manager.Bold = false;
                        manager.TextAlign = Justification.LEFT;

                        var controller = new DS_Recibos();

                        var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                        if (recibo != null)
                        {
                            var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                            var TotalPago = 0.0;

                            if (formasPago != null)
                            {
                                foreach (var rec in formasPago)
                                {
                                    TotalPago += rec.RefValor;

                                    switch (rec.ForID)
                                    {
                                        case 1:
                                            manager.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 2:
                                            manager.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                            if (rec.RefIndicadorDiferido)
                                            {
                                                manager.DrawText("Fecha: " + rec.RefFecha);
                                            }
                                            break;
                                        case 4:
                                            manager.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                            manager.DrawText("Fecha   : " + rec.RefFecha);
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 5:
                                            manager.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                            break;
                                        case 6:
                                            manager.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                            break;
                                    }
                                }
                                manager.DrawLine();
                                manager.DrawText("Total pago: " + TotalPago.ToString("N2"));
                                manager.NewLine();
                            }
                        }
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(4, venSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImage(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato venta 1: Movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;

                }

            });

        }

        private Task<string> Formato41(int venSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var venta = myVen.GetBySecuenciaConTotales(venSecuencia, confirmado);

                if (venta == null)
                {
                    throw new Exception("No se encontraron los datos de la venta");
                }

                using (var manager = PdfManager.NewDocument((venta.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();

                    manager.Font = PrinterFont.TITLE;

                    if (venta.CliTipoComprobanteFAC != "99")
                    {
                        manager.DrawText("NCF: " + venta.VenNCF);

                        if (venta.CliTipoComprobanteFAC == "01")
                        {
                            manager.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                        }
                    }

                    manager.Font = PrinterFont.BODY;

                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
                    manager.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
                    manager.DrawText("Fecha venta: " + venta.VenFecha);
                    manager.DrawText("Cliente: " + venta.CliNombre);
                    manager.DrawText("Codigo: " + venta.CliCodigo);
                    manager.DrawText("Calle: " + venta.CliCalle);
                    manager.DrawText("Urb: " + venta.CliUrbanizacion);

                    manager.DrawLine();
                    manager.Bold = true;

                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string> { "Cant.", "Precio", "Monto Itbis", "% Descuento", "Importe" });
                    manager.Bold = false;

                    manager.DrawLine();

                    double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

                    foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
                    {
                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                        var cantidad = det.VenCantidad.ToString();
                        var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                        var precioConDescuento = (det.VenPrecio - det.VenDescuento);

                        var montoItbis = ((precioLista - det.VenDescuento) - ((precioLista - det.VenDescuento) * (venta.VenPorCientoDsctoGlobal / 100))) * (det.VenItbis / 100);
                        var totalLinea = precioConDescuento * Double.Parse(cantidad);
                        var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);
                        var montoItbisTotalLinea = montoItbis * cantidadTotal;


                        subTotalTotal += Math.Round(totalLinea, 2, MidpointRounding.AwayFromZero);

                        if (det.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow(new List<string>() { cantidad, precioConDescuento.ToString("N2"), montoItbisTotalLinea.ToString("N2"), det.VenDescPorciento.ToString("N2"), totalLinea.ToString("N2") });
                    }

                    total = venta.VenMontoTotal;
                    itbisTotal = venta.VenMontoItbis;
                    descuentoTotal = venta.VenMontoDsctoGlobal;

                    manager.DrawLine();
                    manager.DrawText("SKU: " + venta.VenTotal);
                    manager.NewLine();
                    manager.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(14));
                    manager.Bold = true;
                    manager.DrawText("Total:       " + total.ToString("N2").PadLeft(14));
                    manager.Bold = false;
                    manager.NewLine();

                    if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
                    {
                        manager.TextAlign = Justification.CENTER;
                        manager.Bold = true;
                        manager.DrawText("FORMA DE PAGO");
                        manager.Bold = false;
                        manager.TextAlign = Justification.LEFT;

                        var controller = new DS_Recibos();

                        var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                        if (recibo != null)
                        {
                            var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                            var TotalPago = 0.0;

                            if (formasPago != null)
                            {
                                foreach (var rec in formasPago)
                                {
                                    TotalPago += rec.RefValor;

                                    switch (rec.ForID)
                                    {
                                        case 1:
                                            manager.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 2:
                                            manager.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                            if (rec.RefIndicadorDiferido)
                                            {
                                                manager.DrawText("Fecha: " + rec.RefFecha);
                                            }
                                            break;
                                        case 4:
                                            manager.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                            manager.DrawText("Fecha   : " + rec.RefFecha);
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 5:
                                            manager.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                            break;
                                        case 6:
                                            manager.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                            break;
                                    }
                                }
                                manager.DrawLine();
                                manager.DrawText("Total pago: " + TotalPago.ToString("N2"));
                                manager.NewLine();
                            }
                        }
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(4, venSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImage(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato venta 41: Movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;

                }

            });

        }

        private Task<string> Formato42(int venSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var venta = myVen.GetBySecuenciaConTotales(venSecuencia, confirmado);

                if (venta == null)
                {
                    throw new Exception("No se encontraron los datos de la venta");
                }

                using (var manager = PdfManager.NewDocument((venta.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();

                    manager.Font = PrinterFont.TITLE;

                    if (venta.CliTipoComprobanteFAC != "99")
                    {
                        manager.DrawText("NCF: " + venta.VenNCF);

                        if (venta.CliTipoComprobanteFAC == "01")
                        {
                            manager.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                        }
                    }

                    manager.Font = PrinterFont.BODY;

                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
                    manager.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
                    manager.DrawText("Fecha venta: " + venta.VenFecha);
                    manager.DrawText("Cliente: " + venta.CliNombre);
                    manager.DrawText("Codigo: " + venta.CliCodigo);
                    manager.DrawText("Calle: " + venta.CliCalle);
                    manager.DrawText("Urb: " + venta.CliUrbanizacion);

                    manager.DrawLine();
                    manager.Bold = true;

                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string> { "Cant.", "Precio", "Monto Itbis", "% Descuento", "Importe" });
                    manager.Bold = false;

                    manager.DrawLine();

                    double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

                    foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
                    {
                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                        var cantidad = det.VenCantidad.ToString();
                        var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                        var precioConDescuento = (det.VenPrecio - det.VenDescuento);

                        var montoItbis = ((precioLista - det.VenDescuento) - ((precioLista - det.VenDescuento) * (venta.VenPorCientoDsctoGlobal / 100))) * (det.VenItbis / 100);
                        var totalLinea = precioConDescuento * Double.Parse(cantidad);
                        var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);
                        var montoItbisTotalLinea = montoItbis * cantidadTotal;


                        subTotalTotal += Math.Round(totalLinea, 2, MidpointRounding.AwayFromZero);

                        if (det.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow(new List<string>() { cantidad, precioConDescuento.ToString("N2"), montoItbisTotalLinea.ToString("N2"), det.VenDescPorciento.ToString("N2"), totalLinea.ToString("N2") });
                    }

                    total = venta.VenMontoTotal;
                    itbisTotal = venta.VenMontoItbis;
                    descuentoTotal = venta.VenMontoDsctoGlobal;

                    manager.DrawLine();
                    manager.DrawText("SKU: " + venta.VenTotal);
                    manager.NewLine();
                    manager.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(14));
                    manager.Bold = true;
                    manager.DrawText("Total:       " + total.ToString("N2").PadLeft(14));
                    manager.Bold = false;
                    manager.NewLine();

                    if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
                    {
                        manager.TextAlign = Justification.CENTER;
                        manager.Bold = true;
                        manager.DrawText("FORMA DE PAGO");
                        manager.Bold = false;
                        manager.TextAlign = Justification.LEFT;

                        var controller = new DS_Recibos();

                        var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                        if (recibo != null)
                        {
                            var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                            var TotalPago = 0.0;

                            if (formasPago != null)
                            {
                                foreach (var rec in formasPago)
                                {
                                    TotalPago += rec.RefValor;

                                    switch (rec.ForID)
                                    {
                                        case 1:
                                            manager.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 2:
                                            manager.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                            if (rec.RefIndicadorDiferido)
                                            {
                                                manager.DrawText("Fecha: " + rec.RefFecha);
                                            }
                                            break;
                                        case 4:
                                            manager.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                            manager.DrawText("Fecha   : " + rec.RefFecha);
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 5:
                                            manager.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                            break;
                                        case 6:
                                            manager.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                            break;
                                    }
                                }
                                manager.DrawLine();
                                manager.DrawText("Total pago: " + TotalPago.ToString("N2"));
                                manager.NewLine();
                            }
                        }
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(4, venSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImage(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato venta 41: Movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;

                }

            });

        }


        private Task<string> Formato43(int venSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

                if (venta == null)
                {
                    throw new Exception("No se encontraron los datos de la venta");
                }

                using (var manager = PdfManager.NewDocument((venta.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia).Replace("/", ""), SectorID))
                {
                    //manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();
                     

                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTO");
                    manager.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.DrawText("Documento: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
                    manager.DrawText("Fecha Doc: " + venta.VenFecha);
                    manager.DrawText("Cliente: " + venta.CliNombre);
                    manager.DrawText("Codigo: " + venta.CliCodigo);
                    manager.DrawText("Calle: " + venta.CliCalle);
                    manager.DrawText("Urb: " + venta.CliUrbanizacion);

                    manager.DrawLine();
                    manager.Bold = true;

                    manager.DrawText("Codigo - Descripcion");
                    //manager.DrawTableRow(new List<string> { "Cant.", "Precio", "Monto Itbis", "Descuento", "Importe" });
                    manager.DrawTableRow(new List<string> { "Cant.", "Precio", "Descuento", "Importe" });
                    manager.Bold = false;

                    manager.DrawLine();

                    double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

                    foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
                    {
                        manager.DrawText(det.ProCodigo  + " - " + det.ProDescripcion);

                        var cantidad = det.VenCantidad.ToString();
                        var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                        var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                        var precioConItbis = precioLista + montoItbis;
                        var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);

                        var montoItbisTotal = montoItbis * cantidadTotal;
                        var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                        itbisTotal += montoItbisTotal;
                        total += subTotal;
                        descuentoTotal += (det.VenDescuento * cantidadTotal);
                        subTotalTotal += (precioLista * cantidadTotal);

                        if (det.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                        }

                        //manager.DrawTableRow(new List<string>() { cantidad, precioLista.ToString("N2"), montoItbisTotal.ToString("N2"), det.VenDescuento.ToString("N2"), subTotal.ToString("N2") });
                        manager.DrawTableRow(new List<string>() { cantidad, precioLista.ToString("N2"),   det.VenDescuento.ToString("N2"), subTotal.ToString("N2") });

                    }

                    manager.DrawLine();
                    manager.DrawText("SKU: " + venta.VenTotal);
                    manager.NewLine();
                    manager.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(12));
                    manager.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(12));
                    //manager.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(14));
                    manager.Bold = true;
                    manager.DrawText("Total:       " + total.ToString("N2").PadLeft(14));
                    manager.Bold = false;
                    manager.NewLine();

                    if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
                    {
                        manager.TextAlign = Justification.CENTER;
                        manager.Bold = true;
                        manager.DrawText("FORMA DE PAGO");
                        manager.Bold = false;
                        manager.TextAlign = Justification.LEFT;

                        var controller = new DS_Recibos();

                        var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                        if (recibo != null)
                        {
                            var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                            var TotalPago = 0.0;

                            if (formasPago != null)
                            {
                                foreach (var rec in formasPago)
                                {
                                    TotalPago += rec.RefValor;

                                    switch (rec.ForID)
                                    {
                                        case 1:
                                            manager.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 2:
                                            manager.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                            if (rec.RefIndicadorDiferido)
                                            {
                                                manager.DrawText("Fecha: " + rec.RefFecha);
                                            }
                                            break;
                                        case 4:
                                            manager.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                            manager.DrawText("Fecha   : " + rec.RefFecha);
                                            manager.DrawText("Banco   : " + rec.BanNombre);
                                            manager.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                            break;
                                        case 5:
                                            manager.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                            break;
                                        case 6:
                                            manager.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                            break;
                                    }
                                }
                                manager.DrawLine();
                                manager.DrawText("Total pago: " + TotalPago.ToString("N2"));
                                manager.NewLine();
                            }
                        }
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(4, venSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImage(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);                  
                    manager.DrawText("Formato venta 43: Movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;

                }

            });

        }
    }
}

using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class VentasFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Ventas myVen;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public VentasFormats(DS_Ventas myVen)
        {
            this.myVen = myVen;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int venSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas())
            {
                default:
                case 2:
                case 37: //guaraguano
                    Formato2(venSecuencia, confirmado);
                    break;

                case 3: //Foodsmart
                    Formato3(venSecuencia, confirmado);
                    break;

                case 4: //LA TABACALERA
                    Formato4(venSecuencia, confirmado);
                    break;

                case 5: // FRUTOS FERRER
                    Formato5(venSecuencia, confirmado);
                    break;

                case 6:
                    Formato6(venSecuencia, confirmado);
                    break;
                case 7:  //Nutriciosa
                    Formato7(venSecuencia, confirmado);
                    break;
                case 8:  //Fraga Industrial
                    Formato8(venSecuencia, confirmado);
                    break;
                case 9:  //Molino del Sol
                    Formato9(venSecuencia, confirmado);
                    break;

                case 10: //Jomisardys
                    Formato10(venSecuencia, confirmado);
                    break;


                case 11: //CASA RODRIG
                    Formato11(venSecuencia, confirmado);
                    break;
                case 14: //Espalsa
                    Formato14(venSecuencia, confirmado);
                    break;

                case 35:
                    Formato35(venSecuencia, confirmado);
                    break;
                case 36:
                    Formato36(venSecuencia, confirmado);
                    break;
                case 38: //Hermanos
                    Formato38(venSecuencia, confirmado);
                    break;
                case 40: //Hermanos
                    Formato40(venSecuencia, confirmado);
                    break;
                case 41: // Formato SAP con Descuento General - No Cambiar
                    Formato41(venSecuencia, confirmado);
                    break;
                case 42: //  LIBANESA
                    Formato42(venSecuencia, confirmado);
                    break;
                case 43: //  MACIER INFORMAL 
                    Formato43(venSecuencia, confirmado);
                    break;

            }
        }


        //Guaraguanó
        private void Formato2(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("COMPROBANTE FISCAL");
            printer.Font = PrinterFont.TITLE;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURAS CON VALOR FISCAL");
            }

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                printer.Font = PrinterFont.TITLE;
                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                //printer.DrawText("");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            var fechaVenta = DateTime.TryParse(venta.VenFecha, out DateTime fecha1);
            printer.DrawText("Fecha venta: " + (fechaVenta ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                var cantidad = det.VenCantidad.ToString();
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = Math.Round(precioLista + montoItbis, 2, MidpointRounding.AwayFromZero);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = Math.Round(montoItbis, 2, MidpointRounding.AwayFromZero) * cantidadTotal;
                var subTotal = (precioConItbis - det.VenDescuento) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText(cantidad.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadLeft(10));
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal.ToString().PadLeft(38));
            printer.DrawText("Cant Canastos: " + venta.VenCantidadCanastos.ToString().PadLeft(28));
            printer.DrawText("");
            printer.DrawText("SubTotal:       " + subTotalTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Descuento:      " + descuentoTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Total Itbis:    " + itbisTotal.ToString("N2").PadLeft(30));
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("Total:          " + total.ToString("N2").PadLeft(30));
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                double TotalVenta = 0;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo:       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 2:
                                    printer.DrawText((rec.RefIndicadorDiferido ? "Cheque diferido:" : "Cheque normal:  ") + "  Numero: " + rec.RefNumeroCheque.ToString().PadLeft(20));
                                    printer.DrawText("Banco   :       " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(35));
                                    TotalVenta += rec.RefValor;
                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia:  " + rec.RefNumeroCheque.ToString().PadLeft(30));
                                    printer.DrawText("Fecha   :       " + rec.RefFecha.ToString().PadLeft(30));
                                    printer.DrawText("Banco   :       " + rec.BanNombre.PadLeft(30), 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 5:
                                    printer.DrawText("Retencion:      " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito:" + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                            }
                        }
                        printer.DrawLine();
                        if (formasPago.Count > 1)
                        {
                            printer.DrawText("Total pago:     " + TotalVenta.ToString().PadLeft(30));
                        }
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            /*     if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
                 {
                     printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                     printer.DrawText("");
                 }*/
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato venta 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }

        //Foodsmart
        private void Formato3(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(NoSpaces: true, NoDireccion: true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            //printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                printer.Font = PrinterFont.BODY;
                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                //printer.DrawText("");
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }

            //printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            //printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            var fechaVenta = DateTime.TryParse(venta.VenFecha, out DateTime fecha1);
            printer.DrawText("Fecha venta: " + (fechaVenta ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
           // printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("RNC: " + venta.CliRnc);
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                var cantidad = det.VenCantidad.ToString();
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = Math.Round(precioLista + montoItbis, 2, MidpointRounding.AwayFromZero);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = Math.Round(montoItbis, 2, MidpointRounding.AwayFromZero) * cantidadTotal;
                var subTotal = (precioConItbis - det.VenDescuento) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadRight(9));
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            //printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            //printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: ".PadRight(33) + rec.RefValor.ToString("N2").PadLeft(9));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: ");
                        printer.DrawText("");
                    }
                }
            }

            //printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            // printer.DrawText("");
           /* if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                printer.DrawText("");
            }*/
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato venta 3: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            if (DS_RepresentantesParametros.GetInstance().GetParImpresionLogoSize() != null)
            {
                string[] Size = DS_RepresentantesParametros.GetInstance().GetParImpresionLogoSize();
                try { 
                printer.Print( W: Convert.ToInt32(Size[0]), H: Convert.ToInt32(Size[1]) );
                    }
                catch
                {
                    printer.Print();
                }
            }
            else
            {
                printer.Print();
            }

        }

        //La Tabacalera
        private void Formato4(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }
            bool putfecha = true;
            printer.PrintEmpresa(venSecuencia, putfecha);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + venta.VenNCF);
                printer.DrawText("");

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.Font = PrinterFont.BODY;
                    printer.Bold = false;
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento.Replace("-","/"));
                }

                printer.DrawText("");
            }
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
          


            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Factura    : " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
            var fechaValida = DateTime.TryParse(venta.VenFecha, out DateTime fecha);
            printer.DrawText("Fecha venta: " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + venta.CliCodigo);
            printer.DrawText("Cliente    : " + venta.CliNombre);
            printer.DrawText("");
            printer.DrawText("Propietario: " + venta.CliPropietario, 46);
            printer.DrawText("Calle      : " + venta.CliCalle, 46);
            printer.DrawText("Urb        : " + venta.CliUrbanizacion);
            printer.DrawText("RNC/Cedula : " + venta.CliRnc);
            printer.DrawText("Telefono   : " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cantidad       Precio            Descuento");
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();

            

            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;

            foreach (var det in myVen.GetDetalleBySecuenciaTabacalera(venSecuencia, confirmado))
            {
                double Descuentos;
                double AdValorem = det.VenAdValorem;
                double Selectivo = det.VenSelectivo;
                double PrecioLista = (det.VenindicadorOferta ? 0.0 : det.VenPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.VenCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.VenCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                if (!DS_RepresentantesParametros.GetInstance().GetParCantidadRealSinRedondeo())
                {
                    CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);
                }

                PrecioTotal += (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.VenDescPorciento / 100) * det.VenPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.VenDescuento;
                }

                double descTotalUnitario = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                double descTotalUnitario1 = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario1 = Math.Round(descTotalUnitario1, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.VenindicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.VenItbis;
                var montoItbis = (PrecioLista - det.VenDescuento) * (det.VenItbis / 100);
                double MontoItbis = (det.VenindicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                //(precio - Descuento + Itbis) * cantidad = subtotal
                
                ItebisTotal += (montoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.VenCantidad.ToString();

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad += "/" + det.VenCantidadDetalle;
                }

                double subTotal = (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
               
                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                        (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$"+descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                    printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                        (montoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                    "$"+subTotal.ToString("N2").PadRight(12)); //total
            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + ("$"+SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$"+DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$"+ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$"+Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.DrawText("Fecha Vencimiento: " + DateTime.Parse
                (venta.VenFecha).AddDays(myVen.GetConDiasVencimiento(venta.ConID)).ToString("dd/MM/yyyy"));
            printer.Bold = false;
            printer.DrawText("");
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.Font = PrinterFont.BODY;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawLine();
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(37));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + Total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }
                    
            printer.DrawText("");
        /*    if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                printer.DrawText("");
            }*/
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Por el cliente: -------------------------------");
            printer.DrawText("Aceptado y recibido conforme");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Por el Vendedor: ------------------------------");
            printer.DrawText("("+ Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText(Arguments.CurrentUser.RepTelefono1.FormatTextToPhone().CenterText(48));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORIGINAL: CLIENTE, COPIA: CONTABILIDAD");
            printer.DrawText("");
            printer.DrawText("GRACIAS!!!");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Formato venta 4");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();
        }

        private void Formato5(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);            
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);           
            printer.DrawText("RNC/Cedula : " + venta.CliRnc);
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                var cantidad = Math.Round(det.VenCantidad, 2, MidpointRounding.AwayFromZero);

                var precioLista = Math.Round(det.VenPrecio, 2) + Math.Round(det.VenAdValorem, 2) + Math.Round(det.VenSelectivo, 2);
                var montoItbis = (precioLista - Math.Round(det.VenDescuento, 2)) * (det.VenItbis / 100);
                montoItbis = Math.Round(montoItbis, 2);

                var precioConItbis = precioLista + Math.Round(montoItbis, 2);
                var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                string canttogive = det.VenCantidadDetalle > 0 ? cantidad.ToString() + "/" + det.VenCantidadDetalle.ToString() : cantidad.ToString();

                printer.DrawText(canttogive.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadRight(9));
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + subTotalTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Descuento     :" + descuentoTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Total Itbis   :" + itbisTotal.ToString("N2").PadLeft(30));
            printer.Bold = true;
            printer.DrawText("Total         :" + total.ToString("N2").PadLeft(30));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        if (formasPago.Count > 1)
                        {
                            printer.DrawText("Total pago: ");
                        }
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
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
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 5: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/venta.rowguid);

            printer.Print();
        }

        //Miss Key
        private void Formato6(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }
            bool putfecha = false;
            printer.PrintEmpresa(venSecuencia, putfecha, true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.TextAlign = Justification.LEFT;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.Font = PrinterFont.BODY;
                    printer.Bold = false;
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }
            printer.Bold = true;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");



            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Factura    : " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
            var fechaValida = DateTime.TryParse(venta.VenFecha, out DateTime fecha);
            printer.DrawText("Fecha venta: " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + venta.CliCodigo);
            printer.DrawText("Cliente    : " + venta.CliNombre);
            printer.DrawText("Propietario: " + venta.CliPropietario, 46);
            printer.DrawText("Calle      : " + venta.CliCalle, 46);
            printer.DrawText("Urb        : " + venta.CliUrbanizacion);
            printer.DrawText("Telefono   : " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cantidad       Precio            Descuento");
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();



            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;

            foreach (var det in myVen.GetDetalleBySecuenciaTabacalera(venSecuencia, confirmado))
            {
                double Descuentos;
                double Descuentos1;
                double AdValorem = det.VenAdValorem;
                double Selectivo = det.VenSelectivo;
                double PrecioLista = (det.VenindicadorOferta ? 0.0 : det.VenPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.VenCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.VenCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.VenPrecio * det.VenDescuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.VenDescPorciento / 100) * det.VenPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.VenDescuento;
                }

                double descTotalUnitario = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.VenindicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.VenItbis;

                double MontoItbis = (det.VenindicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                //(precio - Descuento + Itbis) * cantidad = subtotal

                ItebisTotal += (MontoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion, 48);

                string cantidad = det.VenCantidad.ToString();

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad += "/" + det.VenCantidadDetalle;
                }

                if (tasaItbis != 0)
                {
                    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                }

                double subTotal = (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                       (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total


            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal + "    Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            /*    if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
                {
                    printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                    printer.DrawText("");
                }*/
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("(" + Arguments.CurrentUser.RepCodigo + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1.FormatTextToPhone());
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Formato venta 6");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();
        }

        //Nutriciosa
        private void Formato7(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold:true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");
            

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.DrawText("");
            printer.DrawText(venta.ConDescripcion);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("Telefono: " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(11) + det.VenDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);
            var day = "";
            switch (Convert.ToInt32(DateTime.Today.DayOfWeek))
            {

                case 1:
                    day = "Lunes";
                    break;
                case 2: //guaraguano
                    day = "Martes";
                    break;

                case 3: //Foodsmart
                    day = "Miercoles";
                    break;

                case 4: //LA TABACALERA - FRUTOS FERRER
                    day = "Jueves";
                    break;

                case 5:
                    day = "Viernes";
                    break;

                case 6:
                    day = "Sabado";
                    break;
                case 7:
                    day = "Domingo";
                    break;
            }


            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
      
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if(venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto,40);
                    printer.DrawText("");
                }
                    
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("-------------------------------------");
            printer.DrawText("Cedula");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Vendedor: " + RepVendedor);
            printer.DrawText("Secuencia: " + venta.VisSecuencia);
            printer.DrawText("Dia de Visita: " + day);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 7: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }

        private void Formato35(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.MAXTITLE;


            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;

            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.DrawText("");
            printer.DrawText(venta.ConDescripcion);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("Telefono: " + venta.CliTelefono);

            printer.DrawText("--------------------------------");
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis");
            printer.DrawText("Descuento  Importe");
            printer.DrawText("--------------------------------");

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(10) + det.VenDescuento.ToString("N2").PadLeft(9)+subTotal.ToString("N2").PadLeft(13));
                printer.DrawText("");
            }

            printer.DrawText("--------------------------------");
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(18));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(18));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(18));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(18));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(20));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(20));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(20));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(20));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(20));
                                    break;
                            }
                        }
                        printer.DrawText("--------------------------------");
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(18));
                        printer.DrawText("");
                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);
           /* var day = "";
            switch (Convert.ToInt32(DateTime.Today.DayOfWeek))
            {

                case 1:
                    day = "Lunes";
                    break;
                case 2: //guaraguano
                    day = "Martes";
                    break;

                case 3: //Foodsmart
                    day = "Miercoles";
                    break;

                case 4: //LA TABACALERA - FRUTOS FERRER
                    day = "Jueves";
                    break;

                case 5:
                    day = "Viernes";
                    break;

                case 6:
                    day = "Sabado";
                    break;
                case 7:
                    day = "Domingo";
                    break;
            }
           */

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if (venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto, 47);
                    printer.DrawText("");
                }

            }

            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("--------------------------------");
            printer.DrawText("Cedula");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre.Substring(0, 22));
            printer.DrawText("Secuencia Visita: " + venta.VisSecuencia);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 35: Movilbusiness ");
            printer.DrawText(Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }
        private void Formato36(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("RNC/Cedula : " + venta.CliRnc);
            printer.DrawLine();
            printer.DrawText("Codigo Barra - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleByPosicion(venSecuencia, confirmado))
            {
                printer.DrawText(det.ProReferencia + " - " + det.ProDescripcion);

                var cantidad = Math.Round(det.VenCantidad, 2, MidpointRounding.AwayFromZero);

                var precioLista = Math.Round(det.VenPrecio, 2) + Math.Round(det.VenAdValorem, 2) + Math.Round(det.VenSelectivo, 2);
                var montoItbis = (precioLista - Math.Round(det.VenDescuento, 2)) * (det.VenItbis / 100);
                montoItbis = Math.Round(montoItbis, 2);

                var precioConItbis = precioLista + Math.Round(montoItbis, 2);
                var cantidadTotal = ((det.VenCantidadDetalle / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                string canttogive = det.VenCantidadDetalle > 0 ? cantidad.ToString() + "/" + det.VenCantidadDetalle.ToString() : cantidad.ToString();

                printer.DrawText(canttogive.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadRight(9));

                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + subTotalTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Descuento     :" + descuentoTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Total Itbis   :" + itbisTotal.ToString("N2").PadLeft(30));
            printer.Bold = true;
            printer.DrawText("Total         :" + total.ToString("N2").PadLeft(30));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        if (formasPago.Count > 1)
                        {
                            printer.DrawText("Total pago: ");
                        }
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
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
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 5: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/venta.rowguid);

            printer.Print();
        }

        private void Formato38(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("COMPROBANTE FISCAL");
            printer.Font = PrinterFont.TITLE;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURAS CON VALOR FISCAL");
            }

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                printer.Font = PrinterFont.TITLE;
                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                //printer.DrawText("");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            var fechaVenta = DateTime.TryParse(venta.VenFecha, out DateTime fecha1);
            printer.DrawText("Fecha venta: " + (fechaVenta ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, descuentoTotal = 0, total = 0, itbisTotal18 = 0, itbisTotal16 = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                printer.DrawText("Precio Caja:   " + det.PrecioCajas);
                printer.DrawText("Precio Unid:   " + det.PrecioUnidades);

                var cantidad = det.VenCantidad.ToString();
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = Math.Round(precioLista + montoItbis, 2, MidpointRounding.AwayFromZero);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = Math.Round(montoItbis, 2, MidpointRounding.AwayFromZero) * cantidadTotal;
                var subTotal = (precioConItbis - det.VenDescuento) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                }

                if (det.VenItbis == 18)
                {
                    itbisTotal18 += montoItbisTotal;
                }
                else
                {
                    itbisTotal16 += montoItbisTotal;
                }

                total += subTotal;
                descuentoTotal = (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal) + descuentoTotal;

                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText(cantidad.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadLeft(10));
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal.ToString().PadLeft(38));
            printer.DrawText("Cant Canastos: " + venta.VenCantidadCanastos.ToString().PadLeft(28));
            printer.DrawText("");
            printer.DrawText("SubTotal:       " + subTotalTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Itbis 18%:    " + itbisTotal18.ToString("N2").PadLeft(30));
            printer.DrawText("Itbis 16%:    " + itbisTotal16.ToString("N2").PadLeft(30));
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("Total:          " + total.ToString("N2").PadLeft(30));
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                double TotalVenta = 0;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo:       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 2:
                                    printer.DrawText((rec.RefIndicadorDiferido ? "Cheque diferido:" : "Cheque normal:  ") + "  Numero: " + rec.RefNumeroCheque.ToString().PadLeft(20));
                                    printer.DrawText("Banco   :       " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(35));
                                    TotalVenta += rec.RefValor;
                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia:  " + rec.RefNumeroCheque.ToString().PadLeft(30));
                                    printer.DrawText("Fecha   :       " + rec.RefFecha.ToString().PadLeft(30));
                                    printer.DrawText("Banco   :       " + rec.BanNombre.PadLeft(30), 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 5:
                                    printer.DrawText("Retencion:      " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito:" + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                            }
                        }
                        printer.DrawLine();
                        if (formasPago.Count > 1)
                        {
                            printer.DrawText("Total pago:     " + TotalVenta.ToString().PadLeft(30));
                        }
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            /*     if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
                 {
                     printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                     printer.DrawText("");
                 }*/
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato venta 38: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }

        private void Formato40(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(NoSpaces: true, NoDireccion: true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            //printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                printer.Font = PrinterFont.BODY;
                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                //printer.DrawText("");
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }

            //printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            //printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            var fechaVenta = DateTime.TryParse(venta.VenFecha, out DateTime fecha1);
            printer.DrawText("Fecha venta: " + (fechaVenta ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            // printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("RNC: " + venta.CliRnc);
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(10) + det.VenDescuento.ToString("N2").PadLeft(9) + subTotal.ToString("N2").PadLeft(13));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            //printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            //printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: ".PadRight(33) + rec.RefValor.ToString("N2").PadLeft(9));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: ");
                        printer.DrawText("");
                    }
                }
            }

            //printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            // printer.DrawText("");
            /* if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
             {
                 printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                 printer.DrawText("");
             }*/
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato venta 40: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            if (DS_RepresentantesParametros.GetInstance().GetParImpresionLogoSize() != null)
            {
                string[] Size = DS_RepresentantesParametros.GetInstance().GetParImpresionLogoSize();
                try
                {
                    printer.Print(W: Convert.ToInt32(Size[0]), H: Convert.ToInt32(Size[1]));
                }
                catch
                {
                    printer.Print();
                }
            }
            else
            {
                printer.Print();
            }

        }

        //Fraga
        private void Formato8(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: false, NoSpaces:true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Telefono: " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(11) + det.VenDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.FOOTER;
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));

                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);


            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if (venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto, 40);
                }

            }

            printer.DrawText("Version #8: Movilbusiness " + Functions.AppVersion);

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }

        //Molino del Sol
        private void Formato9(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");


            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.DrawText("");
            printer.DrawText(venta.ConDescripcion);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("No. Orden Compra: " + venta.VenOrdenCompra);
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            if (!String.IsNullOrEmpty(venta.CliNombreComercial))
            {
                printer.DrawText("Cliente: " + venta.CliNombreComercial, 48);
                printer.DrawText("Sucursal: " + venta.CliNombre, 48);
            }
            else
            {
                printer.DrawText("Cliente: " + venta.CliNombre, 48);
            }
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("Telefono: " + venta.CliTelefono);
            

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
            int ventotal = 0;

            foreach (var det in myVen.GetDetalleBySecuenciaSinLote(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);
                ventotal += 1;

                printer.DrawText(cantidad.PadRight(8) + precioLista.ToString("N2").PadRight(9) +
                    montoItbis.ToString("N2").PadRight(11) + det.VenDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + ventotal.ToString());
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                
                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + "-" + recibo.RecSecuencia, 48);
                    printer.DrawText("");
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);
            var day = "";
            switch (Convert.ToInt32(DateTime.Today.DayOfWeek))
            {

                case 1:
                    day = "Lunes";
                    break;
                case 2: //guaraguano
                    day = "Martes";
                    break;

                case 3: //Foodsmart
                    day = "Miercoles";
                    break;

                case 4: //LA TABACALERA - FRUTOS FERRER
                    day = "Jueves";
                    break;

                case 5:
                    day = "Viernes";
                    break;

                case 6:
                    day = "Sabado";
                    break;
                case 7:
                    day = "Domingo";
                    break;
            }


            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if (venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto, 40);
                    printer.DrawText("");
                }

            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("-------------------------------------");
            printer.DrawText("Cedula");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Vendedor: " + RepVendedor);
            printer.DrawText("Secuencia: " + venta.VisSecuencia);
            printer.DrawText("Dia de Visita: " + day);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 9: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();
            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

        }

        // Jomisadys
        private void Formato10(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }
            bool putfecha = true;
            printer.PrintEmpresa(venSecuencia, putfecha);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + venta.VenNCF);
                printer.DrawText("");

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.Font = PrinterFont.BODY;
                    printer.Bold = false;
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento.Replace("-", "/"));
                }

                printer.DrawText("");
            }
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");



            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Factura    : " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
            var fechaValida = DateTime.TryParse(venta.VenFecha, out DateTime fecha);
            printer.DrawText("Fecha venta: " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + venta.CliCodigo);
            printer.DrawText("Cliente    : " + venta.CliNombre);
            printer.DrawText("");
            printer.DrawText("Propietario: " + venta.CliPropietario, 46);
            printer.DrawText("Calle      : " + venta.CliCalle, 46);
            printer.DrawText("Urb        : " + venta.CliUrbanizacion);
            printer.DrawText("RNC/Cedula : " + venta.CliRnc);
            printer.DrawText("Telefono   : " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo Barra - Descripcion");
            printer.DrawText("Cantidad       Precio            Importe");
            printer.DrawText("Itbis          Monto Itbis       Descuento ");
            printer.DrawLine();



            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;

            foreach (var det in myVen.GetDetalleWithCodigoBarraByVenPosicion(venSecuencia, confirmado))
            {
                double Descuentos;
                double AdValorem = det.VenAdValorem;
                double Selectivo = det.VenSelectivo;
                double PrecioLista = (det.VenindicadorOferta ? 0.0 : det.VenPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.VenCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.VenCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.VenDescPorciento / 100) * det.VenPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.VenDescuento;
                }

                double descTotalUnitario = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                double descTotalUnitario1 = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario1 = Math.Round(descTotalUnitario1, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.VenindicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.VenItbis;

                //double MontoItbis = (det.VenindicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                //MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                var montoItbis = (PrecioLista - det.VenDescuento) * (det.VenItbis / 100);
                //(precio - Descuento + Itbis) * cantidad = subtotal

                ItebisTotal += (montoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProReferencia + " - " + det.ProDescripcion);

                string cantidad = det.VenCantidad.ToString();

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad += "/" + det.VenCantidadDetalle;
                }

                double subTotal = (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                        (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + subTotal.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (montoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" +  descTotalUnitario.ToString("N2").PadRight(12)); //total
            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.DrawText("Fecha Vencimiento: " + DateTime.Parse
                (venta.VenFecha).AddDays(myVen.GetConDiasVencimiento(venta.ConID)).ToString("dd/MM/yyyy"));
            printer.Bold = false;
            printer.DrawText("");
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.Font = PrinterFont.BODY;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawLine();
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(37));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + Total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            /*    if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
                {
                    printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                    printer.DrawText("");
                }*/
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Por el cliente: -------------------------------");
            printer.DrawText("Aceptado y recibido conforme");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Por el Vendedor: ------------------------------");
            printer.DrawText("(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText(Arguments.CurrentUser.RepTelefono1.FormatTextToPhone().CenterText(48));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORIGINAL: CLIENTE, COPIA: CONTABILIDAD");
            printer.DrawText("");
            printer.DrawText("GRACIAS!!!");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Formato venta 10");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();
        }

        private void Formato11(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }
            bool putfecha = true;
            printer.PrintEmpresa(venSecuencia, putfecha);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText("");

            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + venta.VenNCF);
                printer.DrawText("");

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.Font = PrinterFont.BODY;
                    printer.Bold = false;
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento.Replace("-", "/"));
                }

                printer.DrawText("");
            }
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");



            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Factura    : " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")");
            var fechaValida = DateTime.TryParse(venta.VenFecha, out DateTime fecha);
            printer.DrawText("Fecha venta: " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + venta.CliCodigo);
            printer.DrawText("Cliente    : " + venta.CliNombre);
            printer.DrawText("");
            printer.DrawText("Propietario: " + venta.CliPropietario, 46);
            printer.DrawText("Calle      : " + venta.CliCalle, 46);
            printer.DrawText("Urb        : " + venta.CliUrbanizacion);
            printer.DrawText("RNC/Cedula : " + venta.CliRnc);
            printer.DrawText("Telefono   : " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cantidad       Precio            Descuento");
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();



            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;

            foreach (var det in myVen.GetDetalleBySecuenciaTabacalera(venSecuencia, confirmado))
            {
                double Descuentos;
                double AdValorem = det.VenAdValorem;
                double Selectivo = det.VenSelectivo;
                double PrecioLista = (det.VenindicadorOferta ? 0.0 : det.VenPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.VenCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.VenCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.VenDescPorciento / 100) * det.VenPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.VenDescuento;
                }

                double descTotalUnitario = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                double descTotalUnitario1 = (det.VenindicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario1 = Math.Round(descTotalUnitario1, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.VenindicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.VenItbis;

                double MontoItbis = (det.VenindicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                //(precio - Descuento + Itbis) * cantidad = subtotal

                ItebisTotal += (MontoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.VenCantidad.ToString();

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad += "/" + det.VenCantidadDetalle;
                }

                double subTotal = (det.VenindicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                        (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total
            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.DrawText("Fecha Vencimiento: " + DateTime.Parse
                (venta.VenFecha).AddDays(myVen.GetConDiasVencimiento(venta.ConID)).ToString("dd/MM/yyyy"));
            printer.Bold = false;
            printer.DrawText("");
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.Font = PrinterFont.BODY;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawLine();
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(37));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + Total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Formato venta 11");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();
        }

        private void Formato41(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: false, NoSpaces: true);

            var venta = myVen.GetBySecuenciaConTotales(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }
            }

            printer.Font = PrinterFont.BODY;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Telefono: " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant.   Precio    Monto Itbis   %Desc  Importe");
            //printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var precioConDescuento = (det.VenPrecio - det.VenDescuento);

                var montoItbis = ((precioLista - det.VenDescuento) - ((precioLista - det.VenDescuento) * (venta.VenPorCientoDsctoGlobal / 100))) * (det.VenItbis / 100);
                var totalLinea = precioConDescuento * Double.Parse(cantidad);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);
                var montoItbisTotalLinea = montoItbis * cantidadTotal;


                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                
                subTotalTotal += Math.Round(totalLinea, 2, MidpointRounding.AwayFromZero);  
                //descuentoTotal += descuentoLinea;
                //total += (totalLinea - descuentoLinea) + montoItbisTotalLinea;
                //itbisTotal += montoItbisTotalLinea;

                printer.DrawText(cantidad.PadRight(8) + precioConDescuento.ToString("N2").PadRight(11) +
                    montoItbisTotalLinea.ToString("N2").PadRight(13) + det.VenDescPorciento.ToString("N2").PadRight(7) + totalLinea.ToString("N2").PadLeft(4));
                printer.DrawText("");
            }

            total = venta.VenMontoTotal;
            itbisTotal = venta.VenMontoItbis;
            descuentoTotal = venta.VenMontoDsctoGlobal;


            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.FOOTER;
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));

                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);


            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if (venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto, 40);
                }

            }

            printer.DrawText("Version #41: Movilbusiness " + Functions.AppVersion);

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }

        private void Formato42(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: true);

            var venta = myVen.GetBySecuenciaConTotales(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.Bold = true;
            printer.DrawText("COMPROBANTE FISCAL");
            printer.DrawText("FACTURA CON VALOR FISCAL");
            printer.DrawText("NCF: " + venta.VenNCF);
            printer.DrawText("VALIDO HASTA: " + venta.VenNCFFechaVencimiento);
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("VENTA: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("FECHA: " + venta.VenFecha);
            printer.DrawText("COD. CLIENTE: " + venta.CliCodigo);
            printer.DrawText("CLIENTE: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("TELEFONO: " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("CODIGO - DESCRIPCION");
            printer.DrawText("CANT.   PRECIO    MONTO ITBIS   %DESC  IMPORTE");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
            int ventotal = 0;

            /////
            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {

                var producto = det.ProCodigo + "-" + det.ProDescripcion.Trim();

                //if (producto.Length >= 35)
                //{
                //    producto = producto.Substring(0, 35);
                //}
                //else
                //{
                //    producto = producto.PadRight(35);
                //}
                printer.DrawText(producto);

                var cantidad = det.VenCantidad.ToString();
                var cantidadInt = Convert.ToInt32(cantidad.ToString());
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var precioConDescuento = (det.VenPrecio - det.VenDescuento);

                var montoItbis = ((precioLista - det.VenDescuento) - ((precioLista - det.VenDescuento) * (venta.VenPorCientoDsctoGlobal / 100))) * (det.VenItbis / 100);
                var totalLinea = precioConDescuento * Double.Parse(cantidad);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);
                var montoItbisTotalLinea = montoItbis * cantidadTotal;

                
                subTotalTotal += Math.Round(totalLinea, 2, MidpointRounding.AwayFromZero);
                //descuentoTotal += descuentoLinea;
                //total += (totalLinea - descuentoLinea) + montoItbisTotalLinea;
                //itbisTotal += montoItbisTotalLinea;

                printer.DrawText(cantidadInt.ToString().PadRight(8) + precioConDescuento.ToString("N2").PadRight(11) +
                    montoItbisTotalLinea.ToString("N2").PadRight(13) + det.VenDescPorciento.ToString("N2").PadRight(7) + totalLinea.ToString("N2").PadLeft(4));
                printer.DrawText("");
            }

            total = venta.VenMontoTotal;
            itbisTotal = venta.VenMontoItbis;
            descuentoTotal = venta.VenMontoDsctoGlobal;


            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal);
            printer.DrawText("");
            printer.DrawText("SUBTOTAL:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("DESCUENTOS:  " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("IMPUESTO:    " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("TOTAL:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            ////
            

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + "-" + recibo.RecSecuencia, 48);
                    printer.DrawText("");
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);
            var day = "";
            switch (Convert.ToInt32(DateTime.Today.DayOfWeek))
            {

                case 1:
                    day = "Lunes";
                    break;
                case 2: //guaraguano
                    day = "Martes";
                    break;

                case 3: //Foodsmart
                    day = "Miercoles";
                    break;

                case 4: //LA TABACALERA - FRUTOS FERRER
                    day = "Jueves";
                    break;

                case 5:
                    day = "Viernes";
                    break;

                case 6:
                    day = "Sabado";
                    break;
                case 7:
                    day = "Domingo";
                    break;
            }

            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                if (venta.ConID == 14)
                {
                    string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                    printer.DrawText(Texto, 40);
                    printer.DrawText("");
                }

            }
            printer.TextAlign = Justification.CENTER;
            printer.DrawLine();
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "-ORIGINAL-" : "-COPIA-");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Sello del cliente");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            if (DS_RepresentantesParametros.GetInstance().GetParImprimirVendedor())
            {
                printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            }     
            printer.DrawText("Formato venta 42: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print(185,90);
            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

        }

        private void Formato14(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("COMPROBANTE FISCAL");
            printer.Font = PrinterFont.TITLE;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'").FirstOrDefault();
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF.Descripcion);
            }
            else
            {
                printer.DrawText("FACTURAS CON VALOR FISCAL");
            }

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText("NCF: " + venta.VenNCF);

                printer.Font = PrinterFont.TITLE;
                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                //printer.DrawText("");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia + " (" + venta.ConDescripcion + ")", 48);
            var fechaVenta = DateTime.TryParse(venta.VenFecha, out DateTime fecha1);
            printer.DrawText("Fecha venta: " + (fechaVenta ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : venta.VenFecha));
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;

            foreach (var det in myVen.GetDetalleBySecuencia(venSecuencia, confirmado))
            {
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                var cantidad = det.VenCantidad.ToString();
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = Math.Round(precioLista + montoItbis, 2, MidpointRounding.AwayFromZero);
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = Math.Round(montoItbis * cantidadTotal, 2, MidpointRounding.AwayFromZero);
                var subTotal = (precioConItbis - det.VenDescuento) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString();
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);

                printer.Font = PrinterFont.MAXTITLE;
                printer.DrawText(cantidad.PadRight(5) + precioConItbis.ToString("N2").PadRight(9) +
                montoItbisTotal.ToString("N2").PadRight(13) + det.VenDescuento.ToString("N2").PadRight(9) + subTotal.ToString("N2").PadLeft(10));
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawLine();
            printer.DrawText("SKU: " + venta.VenTotal.ToString().PadLeft(38));
            printer.DrawText("Cant Canastos: " + venta.VenCantidadCanastos.ToString().PadLeft(28));
            printer.DrawText("");
            printer.DrawText("SubTotal:       " + subTotalTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Descuento:      " + descuentoTotal.ToString("N2").PadLeft(30));
            printer.DrawText("Total Itbis:    " + itbisTotal.ToString("N2").PadLeft(30));
            printer.Font = PrinterFont.MAXTITLE;
            printer.DrawText("Total:          " + total.ToString("N2").PadLeft(30));
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                double TotalVenta = 0;
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo:       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 2:
                                    printer.DrawText((rec.RefIndicadorDiferido ? "Cheque diferido:" : "Cheque normal:  ") + "  Numero: " + rec.RefNumeroCheque.ToString().PadLeft(20));
                                    printer.DrawText("Banco   :       " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(35));
                                    TotalVenta += rec.RefValor;
                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia:  " + rec.RefNumeroCheque.ToString().PadLeft(30));
                                    printer.DrawText("Fecha   :       " + rec.RefFecha.ToString().PadLeft(30));
                                    printer.DrawText("Banco   :       " + rec.BanNombre.PadLeft(30), 48);
                                    printer.DrawText("Monto   :       " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 5:
                                    printer.DrawText("Retencion:      " + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito:" + rec.RefValor.ToString("N2").PadLeft(30));
                                    TotalVenta += rec.RefValor;
                                    break;
                            }
                        }
                        printer.DrawLine();
                        if (formasPago.Count > 1)
                        {
                            printer.DrawText("Total pago:     " + TotalVenta.ToString().PadLeft(30));
                        }
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            /*     if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
                 {
                     printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()));
                     printer.DrawText("");
                 }*/
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato venta 14: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

            printer.Print();

        }




        private void Formato43(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            //printer.PrintEmpresa(Notbold: true);

            var venta = myVen.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                throw new Exception("No se encontraron los datos de la venta");
            }

            printer.DrawText("");
            printer.DrawText("");
          

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER; 

            printer.DrawText("DOCUMENTO");
           
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.DrawText("");
            printer.DrawText(venta.ConDescripcion);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");            
            printer.DrawText("Documento: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText("Fecha Doc: " + venta.VenFecha);
            if (!String.IsNullOrEmpty(venta.CliNombreComercial))
            {
                printer.DrawText("Cliente: " + venta.CliNombreComercial, 48);
                printer.DrawText("Sucursal: " + venta.CliNombre, 48);
            }
            else
            {
                printer.DrawText("Cliente: " + venta.CliNombre, 48);
            }
            //printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("Telefono: " + venta.CliTelefono);


            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawText("Cant.      Precio      Descuento      Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
            int ventotal = 0;

            foreach (var det in myVen.GetDetalleBySecuenciaSinLote(venSecuencia, confirmado))
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

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);
                ventotal += 1;

                //printer.DrawText(cantidad.PadRight(8) + precioLista.ToString("N2").PadRight(9) +
                //    montoItbis.ToString("N2").PadRight(11) + det.VenDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
                printer.DrawText(cantidad.PadRight(11) + precioLista.ToString("N2").PadRight(12) +
                    det.VenDescuento.ToString("N2").PadRight(13) + subTotal.ToString("N2").PadLeft(7));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + ventotal.ToString());
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            //printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + "-" + recibo.RecSecuencia, 48);
                    printer.DrawText("");
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

                      
            var RepVendedor = Arguments.CurrentUser.RepNombre;
           


            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas()) != "")
            {
                string Texto = "NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(4, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionVentas());
                printer.DrawText(Texto, 40);
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
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
           
            printer.DrawText("Vendedor: " + RepVendedor);
            
            printer.DrawText("Formato venta 43: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();
            myVen.ActualizarCantidadImpresion(/*venSecuencia*/ venta.rowguid);

        }
    }
}

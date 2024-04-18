using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.Utils;
using System;

namespace MovilBusiness.Printer.Formats
{
    public class ComprasFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Compras myCom;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        bool _isComprasPushMoney;

        public ComprasFormats(DS_Compras myCom, bool isComprasPushMoney = false)
        {
            this.myCom = myCom;
            _isComprasPushMoney = isComprasPushMoney;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public void Print(int comSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var limiteCopias = DS_RepresentantesParametros.GetInstance().GetParComprasLimiteMaximoCopiasImpresion();

            if (limiteCopias > 0)
            {
                validarLimiteCopias(comSecuencia, confirmado, limiteCopias);
            }

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras())
            {
                case 1: //LAM
                    Formato1(comSecuencia, confirmado);
                    break;
                case 2:
                    Formato2(comSecuencia, confirmado);
                    break;
                case 3://ACROMAX
                    Formato3(comSecuencia, confirmado);
                    break;
                case 4: //SUED
                    Formato4(comSecuencia, confirmado);
                    break;
                case 5: //LA TABACALERA
                    Formato5(comSecuencia, confirmado);
                    break;
                case 6: // Disfarmaco
                    Formato6(comSecuencia, confirmado);
                    break;
                case 7: //feltrex
                    Formato7(comSecuencia, confirmado);
                    break;
                case 8: //LAM
                    Formato8(comSecuencia, confirmado);
                    break;
                case 9: //FELTREX - ZEBRA
                    Formato9(comSecuencia, confirmado);
                    break;
                case 10: //
                    Formato10(comSecuencia, confirmado);
                    break;
                default:
                    Formato1(comSecuencia, confirmado);
                    break;
            }

            myCom.ActualizarComCantidadImpresion(rowguid, confirmado);
        }

        private void validarLimiteCopias(int comSecuencia, bool confirmado, int limiteCopias)
        {
            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra.ComCantidadImpresion >= limiteCopias)
            {
                throw new Exception("Ya has impreso este PushMoney el limite de copias: " + limiteCopias.ToString());
            }

        }

        private void Formato1(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;

            printer.TextAlign = Justification.CENTER;

            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("Copia No." + compra.ComCantidadImpresion + 1);
            }

            printer.DrawText("O R D E N  D E  C O M P R A S");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.DrawText("Tipo: " + compra.TipoPagoDescripcion, 48);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;

            if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Descripcion");
                printer.DrawText("Cant. Codigo  Factura    Precio      SubTotal");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {

                    printer.DrawText(det.ProDescripcion);

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                    printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                    total += subTotal;
                }
            }
            else
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Codigo - Descripcion");
                printer.DrawText("Factura            Precio            Cantidad");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {
                    var desc = det.ProCodigo + " - " + det.ProDescripcion;
                    printer.DrawText(desc, 48);

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var subTotal = det.ComPrecio * cantidadReal;

                    total += subTotal;

                    printer.DrawText(det.cxcDocumento.PadRight(19) + det.ComPrecio.ToString("N2").PadRight(18) + cantidad.PadRight(11));
                }
            }

            printer.DrawLine();
            printer.DrawText("Total: " + total.ToString("N2"));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (FormasPagoTemp rec in myCom.GetFormasPago(comSecuencia))
            {
                TotalCobrado += rec.Valor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.Futurista == "Si" ? "diferido" : "normal") + "  Numero: " + rec.NoCheque.ToString());
                        printer.DrawText("Banco   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));

                        if (rec.Futurista == "Si")
                        {
                            printer.DrawText("Fecha: " + rec.Fecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.NoCheque);
                        printer.DrawText("Fecha   : " + rec.Fecha);
                        printer.DrawText("Banco   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + rec.Valor.ToString("N2").PadLeft(34));
                        break;
                    case 6:
                        printer.DrawText("Tarjeta crédito: " + rec.Valor.ToString("N2").PadLeft(28));
                        break;
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.NoCheque);
                        printer.DrawText("Fecha   : " + rec.Fecha);
                        printer.DrawText("COOP.   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 20:
                        printer.DrawText("Bono Denominacion : " + rec.BonoDenominacion);
                        printer.DrawText("Bono Cantidad     : " + rec.BonoCantidad);
                        printer.DrawText("Monto             : " + rec.Valor.ToString("N2"));
                        break;
                }
                printer.DrawLine();
            }
            printer.DrawText("");
            printer.DrawText("Total cobrado: " + ("$" + TotalCobrado.ToString("N2")).PadLeft(29));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            if (!string.IsNullOrWhiteSpace(compra.ComTipoPago) && compra.ComTipoPago.Trim() == "2")
            {
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("-------------------------------------");
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("-------------------------------------");

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del Representante");
                printer.TextAlign = Justification.LEFT;
            }

            if (compra.ComCantidadImpresion > 0)
            {
                printer.DrawText("");
                printer.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                printer.DrawText("");
            }

            printer.DrawText("Items: " + detalles.Count);
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato compras 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato2(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.CENTER;

            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("Copia No." + compra.ComCantidadImpresion + 1);
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;

            printer.DrawText("O R D E N  D E  P U S H  M O N E Y");
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");

            /*PrinterMD.DrawText("Orden #: "+  myCursorPedImpr.getString(4));
			PrinterMD.DrawText("Cliente: " + ClienteLinea1);
			if (clientelinea) {
				PrinterMD.DrawText(ClienteLinea2);
			}
			PrinterMD.DrawText("Codigo:  "+  myCursorPedImpr.getString(1));
			PrinterMD.DrawText("Calle:   "+  CalleLinea1);
			if (Callelinea) {
				PrinterMD.DrawText(CalleLinea2);
			}
			PrinterMD.DrawText("Urb:     "+  myCursorPedImpr.getString(3));*/

            printer.DrawText("Orden #: " + compra.ComSecuencia);
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);
            printer.DrawText("Calle: " + compra.CliCalle);
            printer.DrawText("Urb: " + compra.CliUrbanizacion);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("");
            printer.DrawText("Tipo de pago: " + compra.TipoPagoDescripcion);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));
            printer.DrawText("Forma de pago: " + (dependiente != null ? dependiente.FopDescripcion : ""));

            if (dependiente != null && dependiente.FopID == 2)
            {
                /*PrinterMD.DrawText("Banco: "+ Banco);
				PrinterMD.DrawText("Numero de Cuenta: "+ myDependiente.getString(5));
				PrinterMD.DrawText("Tipo de Cuenta: "+ DS_Dependiente.GetTipoCuenta(myDependiente.getString(6), myConn));*/
                printer.DrawText("Banco: " + dependiente.BanNombre);
                printer.DrawText("Numero de cuenta: " + dependiente.CldCuentaBancaria);
                printer.DrawText("Tipo de cuenta: " + dependiente.TipoCuentaDescripcion);

            }

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;

            // printer.DrawLine();
            printer.Bold = true;
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cant. Codigo  Factura    Precio      SubTotal");
            printer.Bold = false;
            printer.DrawLine();

            foreach (var det in detalles)
            {

                printer.DrawText(det.ProDescripcion);

                var cantidad = det.ComCantidad.ToString();

                if (det.ComCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.ComCantidadDetalle;
                }

                var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                total += subTotal;
            }

            printer.DrawLine();
            printer.DrawText("Total: " + total.ToString("N2"));
            printer.DrawText("Retencion ISR 10%: " + (total * 0.10).ToString("N2"));

            printer.DrawText("Total A Pagar: " + ((total - (total * 0.10)).ToString("N2")));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("SKU: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato compras 2: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato3(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.CENTER;

            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("Copia No." + compra.ComCantidadImpresion + 1);
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P U S H  M O N E Y");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Bold = true;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Tipo: " + compra.TipoPagoDescripcion, 48);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;

            if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Descripcion");
                printer.DrawText("Cant. Codigo             Precio      SubTotal");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {

                    printer.DrawText(det.ProDescripcion);

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                    printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                    total += subTotal;
                }
            }
            else
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Codigo - Descripcion");
                printer.DrawText("Factura            Precio            Cantidad");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {
                    var desc = det.ProCodigo + " - " + det.ProDescripcion;
                    printer.DrawText(desc, 48);

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var subTotal = det.ComPrecio * cantidadReal;

                    total += subTotal;

                    printer.DrawText(det.cxcDocumento.PadRight(19) + det.ComPrecio.ToString("N2").PadRight(18) + cantidad.PadRight(11));
                }
            }

            printer.DrawLine();
            printer.DrawText("Total: " + total.ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            if (!string.IsNullOrWhiteSpace(compra.ComTipoPago) && compra.ComTipoPago.Trim() == "2")
            {
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("-------------------------------------");
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("-------------------------------------");

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del Representante");
                printer.TextAlign = Justification.LEFT;
            }

            if (compra.ComCantidadImpresion > 0)
            {
                printer.DrawText("");
                printer.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                printer.DrawText("");
            }

            printer.DrawText("Items: " + detalles.Count);
            printer.DrawText("");

            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato compras 3: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato4(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("R E C I B O  D E  C A J A S");
            printer.DrawText("");

            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            printer.Bold = false;
            printer.DrawText("");
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + compra.ComCantidadImpresion, "H");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(compra.ComCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A No. " + compra.ComCantidadImpresion);

            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);
            printer.DrawText("Tipo de Pago: " + compra.TipoPagoDescripcion, 48);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd-MM-yyyy HH:mm:ss"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("Ruta: " + Arguments.CurrentUser.RutID.ToString(), 48);
            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);
            printer.DrawText("Calle: " + compra.CliCalle);
            printer.DrawText("Urb: " + compra.CliUrbanizacion);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cant. Codigo             Precio      SubTotal");
            printer.Bold = false;
            printer.DrawLine();

            foreach (var det in detalles)
            {
                printer.DrawText(det.ProDescripcion);

                var cantidad = det.ComCantidad.ToString();

                if (det.ComCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.ComCantidadDetalle;
                }

                var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                total += subTotal;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Total: " + total.ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Items: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato PushMoney 4: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        private void Formato5(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);
            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }
            bool putfecha = true;
            printer.PrintEmpresa(comSecuencia, putfecha);

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.CENTER;

            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                int cantidadImpresion = compra.ComCantidadImpresion + 1;
                printer.DrawText("Copia No." + cantidadImpresion.ToString());
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P U S H  M O N E Y");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd-MM-yyyy HH:mm:ss"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("Pushmoney #  :" + compra.ComSecuencia.ToString(), 48);
            printer.DrawText("Ruta         :" + Arguments.CurrentUser.RutID.ToString(), 48);
            printer.DrawText("Codigo       :" + compra.CliCodigo);
            printer.DrawText("Cliente      :" + compra.CliNombre, 48);
            //CLIENTE
            var cliente = new DS_Clientes().GetClienteById(compra.CliID);
            printer.DrawText("Propietario  :" + (string.IsNullOrWhiteSpace(cliente.CliContacto) ? "" : cliente.CliContacto), 48);
            printer.DrawText("Calle        :" + (string.IsNullOrWhiteSpace(cliente.CliCalle) ? "" : cliente.CliCalle), 48);
            printer.DrawText("Urb          :" + (string.IsNullOrWhiteSpace(cliente.CliUrbanizacion) ? "" : cliente.CliUrbanizacion), 48);
            printer.DrawText("RNC/Cedula   :" + (string.IsNullOrWhiteSpace(cliente.CliRNC) ? "" : cliente.CliRNC));
            printer.DrawText("Telefono     :" + (string.IsNullOrWhiteSpace(cliente.CliTelefono) ? "" : cliente.CliTelefono).FormatTextToPhone());


            //DEPENDIENTE
            //try
            //{
            //    Cursor myDependiente = GetDependiente(GetClDCedula(ComSecuencia));
            //    myDependiente.moveToFirst();
            //    PrinterMD.DrawLine(); ;
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Cedula", 17) + ":" + myDependiente.getString(0));
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Nombre", 17) + ":" + myDependiente.getString(1));
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Telefono", 17) + ":" + myDependiente.getString(2));
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Tipo de pago", 17) + ":" + myDependiente.getString(3));

            //    String Banco = myDependiente.getString(4);

            //    if (myDependiente.getString(3).trim().equalsIgnoreCase("Efectivo"))
            //    {

            //        Banco = "";
            //    }

            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Banco", 17) + ":" + Banco);
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Numero de Cuenta", 17) + ":" + myDependiente.getString(5));
            //    PrinterMD.DrawText(Funciones.ReservarCaracteres("Tipo de Cuenta", 17) + ":" + DS_Dependiente.GetTipoCuenta(myDependiente.getString(6), myConn));

            //}
            //catch (Exception e)
            //{
            //    e.printStackTrace();
            //}
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad Codigo          Precio      SubTotal");
            printer.Bold = false;
            printer.DrawLine();


            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);
            double total = 0.0;

            foreach (var det in detalles)
            {

                printer.DrawText(det.ProDescripcion);

                var cantidad = det.ComCantidad.ToString();

                if (det.ComCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.ComCantidadDetalle;
                }

                var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;
                string cantidadtexto = det.ComCantidad.ToString() + "/" + det.ComCantidadDetalle;
                var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                //printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));
                printer.DrawText(cantidadtexto.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + ("$" + det.ComPrecio.ToString("N2")).PadRight(12) + ("$" + subTotal.ToString("N2")));

                total += subTotal;
            }



            printer.DrawLine();
            printer.DrawText("SKU: " + detalles.Count);
            printer.DrawText("Total:                   $" + total.ToString("N2"));
            printer.DrawText("Total A Pagar:           $" + total.ToString("N2"));//(SubTotal - (SubTotal*0.10))));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente".CenterText(48));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1.FormatTextToPhone());
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2.FormatTextToPhone());
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Formato Push Money 5: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        private void Formato6(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;

            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P U S H  M O N E Y");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Tipo: " + compra.TipoPagoDescripcion, 48);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;

            if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Descripcion");
                printer.DrawText("Cant. Codigo  Factura    Precio      SubTotal");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {

                    printer.DrawText(det.ProDescripcion);

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                    printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                    total += subTotal;
                }
            }
            else
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Codigo - Descripcion");
                printer.DrawText("Factura            Precio            Cantidad");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {
                    var desc = det.ProCodigo + " - " + det.ProDescripcion;
                    printer.DrawText(desc, 48);

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var subTotal = det.ComPrecio * cantidadReal;

                    total += subTotal;

                    printer.DrawText(det.cxcDocumento.PadRight(19) + det.ComPrecio.ToString("N2").PadRight(18) + cantidad.PadRight(11));
                }
            }

            printer.DrawLine();
            printer.DrawText("Total: " + total.ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");

            if (!string.IsNullOrWhiteSpace(compra.ComTipoPago) && compra.ComTipoPago.Trim() == "2")
            {
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("-------------------------------------");
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("-------------------------------------");

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del Representante");
                printer.TextAlign = Justification.LEFT;
            }

            if (compra.ComCantidadImpresion > 0)
            {
                printer.DrawText("");
                printer.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                printer.DrawText("");
            }

            printer.DrawText("Items: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato compras 6: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato7(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("Copia No." + compra.ComCantidadImpresion + 1);
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("O R D E N  D E  P U S H M O N E Y");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("Orden #     : " + compra.ComSecuencia);
            //printer.DrawText("");
            if (compra.CliNombre.Length > 30)
            {
                compra.CliNombre = compra.CliNombre.Substring(0, 30);
            }
            printer.DrawText("Cliente     : " + compra.CliNombre, 48);
            //printer.DrawText("");
            printer.DrawText("Codigo      : " + compra.CliCodigo);
            //printer.DrawText("");
            if (compra.CliCalle.Length > 30)
            {
                compra.CliCalle = compra.CliCalle.Substring(0, 30);
            }
            printer.DrawText("Calle       : " + compra.CliCalle, 48);
            // printer.DrawText("");
            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha   : " + fecha.ToString("dd/MM/yyyy hh:mm tt"));
                //printer.DrawText("");
            }
            else
            {
                printer.DrawText("Fecha   : " + compra.ComFecha);
                //printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Tipo de pago: " + compra.TipoPagoDescripcion);
            //printer.DrawText("");
            string ClienteRNC = new DS_Clientes().GetCliRNC(compra.CliID);
            printer.DrawText("RNC         : " + ClienteRNC);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Cedula        : " + (dependiente != null ? dependiente.ClDCedula : ""), 48);
            //printer.DrawText("");
            printer.DrawText("Nombre        : " + (dependiente != null ? dependiente.ClDNombre : ""), 48);
            //printer.DrawText("");
            printer.DrawText("Telefono      : " + (dependiente != null ? dependiente.CldTelefono : ""), 48);
            //printer.DrawText("");
            printer.DrawText("Tipo de pago  : " + compra.TipoPagoDescripcion, 48);
            //printer.DrawText("");
            printer.DrawText("Banco         : " + (dependiente != null ? dependiente.BanNombre : ""), 48);
            //printer.DrawText("");
            printer.DrawText("No. de cuenta : " + (dependiente != null ? dependiente.CldCuentaBancaria : ""), 48);
            //printer.DrawText("");
            printer.DrawText("Tipo de cuenta: " + (dependiente != null ? dependiente.CldTipoCuentaBancaria.ToString() : ""), 48);
            //printer.DrawText("");
            printer.DrawLine();


            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0;
            printer.Bold = false;
            printer.DrawText("Descripcion");
            //printer.DrawText("");
            printer.DrawText("Cantidad Codigo       Precio           SubTotal");
            //printer.DrawText("");
            printer.Bold = false;
            printer.DrawLine();

            foreach (var det in detalles)
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 46);
                }
                printer.DrawText(det.ProDescripcion);
                //printer.DrawText("");

                var cantidad = det.ComCantidad.ToString();

                if (det.ComCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.ComCantidadDetalle;
                }

                var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                printer.DrawText(cantidad.PadRight(9) + det.ProCodigo.PadRight(16) + det.ComPrecio.ToString("N2").PadLeft(12) + subTotal.ToString("N2").PadLeft(9));
                printer.DrawText("");
                total += subTotal;
            }

            printer.DrawLine();
            //printer.DrawText("Total            :       " + "$" + total.ToString("N2"));
            //printer.DrawText("");
            //printer.DrawText("Retencion ISR 10%:       " + "$" + (total*0.10).ToString("N2"));
            //printer.DrawText("");
            printer.DrawText("Total a pagar    :       " + ("$" + (total).ToString("N2")));
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.RIGHT;
            printer.DrawLine();
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Formato compras 7: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato8(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: true);

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.DrawText("O R D E N  D E  C O M P R A S");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.DrawText("Tipo: " + compra.TipoPagoDescripcion, 48);
            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText(fecha.ToString("dd/MM/yyyy hh:mm tt").PadRight(20) + ("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia).PadLeft(25));
            }
            else
            {
                printer.DrawText((compra.ComFecha).PadRight(20) + ("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia).PadLeft(25));
            }
            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo : " + compra.CliCodigo);
            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);
            printer.DrawText("Dependiente Tel.  :   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));
            printer.DrawText("");
            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);
            double total = 0.0;
            if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Descripcion");
                printer.DrawText("Cant. Codigo  Factura       Precio     SubTotal");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {

                    printer.DrawText(det.ProDescripcion);

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                    printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(8) + det.ComPrecio.ToString("N2").PadLeft(12) + subTotal.ToString("N2").PadLeft(13));

                    total += subTotal;
                }
            }
            else
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Codigo - Descripcion");
                printer.DrawText("Factura            Precio            Cantidad");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {
                    var desc = det.ProCodigo + " - " + det.ProDescripcion;
                    printer.DrawText(desc, 48);

                    var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                    var cantidad = det.ComCantidad.ToString();

                    if (det.ComCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.ComCantidadDetalle;
                    }

                    var subTotal = det.ComPrecio * cantidadReal;

                    total += subTotal;

                    printer.DrawText(det.cxcDocumento.PadRight(19) + det.ComPrecio.ToString("N2").PadRight(18) + cantidad.PadRight(11));
                }
            }
            printer.DrawLine();
            printer.DrawText("Total: " + (total.ToString("N2")).PadLeft(40));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (FormasPagoTemp rec in myCom.GetFormasPago(comSecuencia))
            {
                TotalCobrado += rec.Valor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.Futurista == "Si" ? "diferido" : "normal") + "  Numero: " + rec.NoCheque.ToString());
                        printer.DrawText("Banco   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));

                        if (rec.Futurista == "Si")
                        {
                            printer.DrawText("Fecha: " + rec.Fecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.NoCheque);
                        printer.DrawText("Fecha   : " + rec.Fecha);
                        printer.DrawText("Banco   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + rec.Valor.ToString("N2").PadLeft(34));
                        break;
                    case 6:
                        printer.DrawText("Tarjeta crédito: " + rec.Valor.ToString("N2").PadLeft(28));
                        break;
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.NoCheque);
                        printer.DrawText("Fecha   : " + rec.Fecha);
                        printer.DrawText("COOP.   : " + rec.Banco, 48);
                        printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                    case 20:
                        printer.DrawText("Bono : " + rec.BonoDenominacion + "  Cantidad: " + rec.BonoCantidad + "  Monto : " + rec.Valor.ToString("N2"));
                        // printer.DrawText("Bono Cantidad     : " + rec.BonoCantidad);
                        // printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                        break;
                }
                printer.DrawLine();
            }
            printer.DrawText("");
            printer.DrawText("Total pagado: " + ("$" + TotalCobrado.ToString("N2")).PadLeft(33));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawLine();
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            if (compra.ComCantidadImpresion > 0)
            {
                printer.DrawText("");
                printer.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                printer.DrawText("");
            }
            printer.DrawText("Items: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato compras 8: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato9(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            if (compra.ComCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("Copia No." + compra.ComCantidadImpresion + 1);
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("O R D E N  D E  P U S H M O N E Y");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("Orden #     : " + compra.ComSecuencia);
            if (compra.CliNombre.Length > 30)
            {
                compra.CliNombre = compra.CliNombre.Substring(0, 30);
            }
            compra.CliNombre = compra.CliNombre.TrimEnd(' ');
            printer.DrawText("Cliente     : " + compra.CliNombre, 48);
            printer.DrawText("Codigo      : " + compra.CliCodigo);
            if (compra.CliCalle.Length > 30)
            {
                compra.CliCalle = compra.CliCalle.Substring(0, 30);
            }
            printer.DrawText("Calle       : " + compra.CliCalle, 48);
            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha   : " + fecha.ToString("dd/MM/yyyy hh:mm tt"));
            }
            else
            {
                printer.DrawText("Fecha   : " + compra.ComFecha);
            }
            printer.DrawText("");
            printer.DrawText("Tipo de pago: " + compra.TipoPagoDescripcion);
            string ClienteRNC = new DS_Clientes().GetCliRNC(compra.CliID);
            printer.DrawText("RNC         : " + ClienteRNC);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);
            printer.DrawLine();
            printer.DrawText("Cedula        : " + (dependiente != null ? dependiente.ClDCedula : ""), 48);
            printer.DrawText("Nombre        : " + (dependiente != null ? dependiente.ClDNombre : ""), 48);
            printer.DrawText("Telefono      : " + (dependiente != null ? dependiente.CldTelefono : ""), 48);
            printer.DrawText("Tipo de pago  : " + compra.TipoPagoDescripcion, 48);
            printer.DrawText("Banco         : " + (dependiente != null ? dependiente.BanNombre : ""), 48);
            printer.DrawText("No. de cuenta : " + (dependiente != null ? dependiente.CldCuentaBancaria : ""), 48);
            printer.DrawText("Tipo de cuenta: " + (dependiente != null ? dependiente.CldTipoCuentaBancaria.ToString() : ""), 48);
            printer.DrawText("");
            printer.DrawLine();
            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);
            double total = 0.0;
            printer.Bold = false;
            printer.DrawText("Descripcion");
            //printer.DrawText("");
            printer.DrawText("Cantidad Codigo       Precio           SubTotal");
            //printer.DrawText("");
            printer.Bold = false;
            printer.DrawLine();

            foreach (var det in detalles)
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 46);
                }
                printer.DrawText(det.ProDescripcion);
                //printer.DrawText("");

                var cantidad = det.ComCantidad.ToString();

                if (det.ComCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.ComCantidadDetalle;
                }

                var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                printer.DrawText(cantidad.PadRight(9) + det.ProCodigo.PadRight(16) + det.ComPrecio.ToString("N2").PadLeft(12) + subTotal.ToString("N2").PadLeft(9));
                printer.DrawText("");
                total += subTotal;
            }

            printer.DrawLine();
            //printer.DrawText("Total            :       " + "$" + total.ToString("N2"));
            //printer.DrawText("");
            //printer.DrawText("Retencion ISR 10%:       " + "$" + (total*0.10).ToString("N2"));
            //printer.DrawText("");
            printer.DrawText("Total a pagar    :       " + ("$" + (total).ToString("N2")));
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.RIGHT;
            printer.DrawLine();
            printer.DrawText("Firma del dependiente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU: " + detalles.Count);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato compras 9: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        private void Formato10(int comSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

            if (compra == null)
            {
                throw new Exception("No se encontraron los datos de la compra");
            }

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;

            printer.DrawText(!_isComprasPushMoney ? "O R D E N  D E  C O M P R A S" : "C O M P R A  D E  F A C T U R A");
            printer.DrawText("");
            if (compra.ComEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.DrawText("Tipo: " + compra.TipoPagoDescripcion, 48);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);

            if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
            }
            else
            {
                printer.DrawText("Fecha: " + compra.ComFecha);
            }

            printer.DrawText("");
            printer.DrawText("Cliente: " + compra.CliNombre, 48);
            printer.DrawText("Codigo: " + compra.CliCodigo);

            var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

            printer.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
            printer.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
            printer.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

            var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

            double total = 0.0, subTotal = 0.0;

                if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
                {
                    printer.DrawLine();
                    printer.Bold = true;
                    printer.DrawText("Descripcion");
                    printer.DrawText("Cant. Codigo  Factura    Precio      SubTotal");
                    printer.Bold = false;
                    printer.DrawLine();

                    foreach (var det in detalles)
                    {

                        printer.DrawText(det.ProDescripcion);

                        var cantidad = det.ComCantidad.ToString();

                        if (det.ComCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.ComCantidadDetalle;
                        }

                        var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                        subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                        printer.DrawText(cantidad.PadRight(6) + det.ProCodigo.PadRight(8) + "".PadRight(11) + det.ComPrecio.ToString("N2").PadRight(12) + subTotal.ToString("N2"));

                        total += subTotal;
                    }
                }
                else
                {
                    printer.DrawLine();
                    printer.Bold = true;
                    printer.DrawText("Codigo - Descripcion");
                    printer.DrawText("Factura            Precio            Cantidad");
                    printer.Bold = false;
                    printer.DrawLine();

                    foreach (var det in detalles)
                    {
                        var desc = det.ProCodigo + " - " + det.ProDescripcion.Substring(0,25);
                        printer.DrawText(desc);

                        var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                        var cantidad = det.ComCantidad.ToString();

                        if (det.ComCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.ComCantidadDetalle;
                        }

                        subTotal = det.ComPrecio * cantidadReal;

                        total += subTotal;

                        printer.DrawText(det.cxcDocumento.PadRight(19) + det.ComPrecio.ToString("N2").PadRight(18) + cantidad.PadRight(11));
                    }
                }

                printer.DrawLine();

                printer.DrawText("SubTotal:      " + subTotal.ToString("N2"));
                printer.DrawText("ISR 10%:       " + (total * 0.10).ToString("N2"));
                printer.DrawText("Total: " + (total - (total * 0.10)).ToString("N2"));

                printer.DrawText("");
                printer.DrawText("Formas de pago");
                //foreach formas pago
                double TotalCobrado = 0;
                foreach (FormasPagoTemp rec in myCom.GetFormasPago(comSecuencia))
                {
                    TotalCobrado += rec.Valor;

                    switch (rec.ForID)
                    {
                        case 1:
                            printer.DrawText("Efectivo: " + rec.Valor.ToString("N2").PadLeft(35));
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.Futurista == "Si" ? "diferido" : "normal") + "  Numero: " + rec.NoCheque.ToString());
                            printer.DrawText("Banco   : " + rec.Banco, 48);
                            printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));

                            if (rec.Futurista == "Si")
                            {
                                printer.DrawText("Fecha: " + rec.Fecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.NoCheque);
                            printer.DrawText("Fecha   : " + rec.Fecha);
                            printer.DrawText("Banco   : " + rec.Banco, 48);
                            printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.Valor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("Tarjeta crédito: " + rec.Valor.ToString("N2").PadLeft(28));
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.NoCheque);
                            printer.DrawText("Fecha   : " + rec.Fecha);
                            printer.DrawText("COOP.   : " + rec.Banco, 48);
                            printer.DrawText("Monto   : " + rec.Valor.ToString("N2").PadLeft(35));
                            break;
                        case 20:
                            printer.DrawText("Bono Denominacion : " + rec.BonoDenominacion);
                            printer.DrawText("Bono Cantidad     : " + rec.BonoCantidad);
                            printer.DrawText("Monto             : " + rec.Valor.ToString("N2"));
                            break;
                    }
                    printer.DrawLine();
                }
                printer.DrawText("");
                printer.DrawText("Total cobrado: " + ("$" + TotalCobrado.ToString("N2")).PadLeft(29));
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("-------------------------------------");
                printer.DrawText("Firma del dependiente");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                if (!string.IsNullOrWhiteSpace(compra.ComTipoPago) && compra.ComTipoPago.Trim() == "2")
                {
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("-------------------------------------");
                    printer.DrawText("Firma del cliente");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("-------------------------------------");

                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("Firma del Representante");
                    printer.TextAlign = Justification.LEFT;
                }

                if (compra.ComCantidadImpresion > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                    printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                    printer.DrawText("");
                }

                printer.DrawText("Items: " + detalles.Count);
                printer.DrawText("");
                printer.DrawText("");
                if (myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()) != "")
                {
                    printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(11, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras()));
                    printer.DrawText("");
                }
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                printer.DrawText("Formato compras 10: Movilbusiness " + Functions.AppVersion);
                printer.Print();
        }

    }
}

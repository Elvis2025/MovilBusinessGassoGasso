using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;

namespace MovilBusiness.Printer.Formats
{
    public class PushMoneyPorPagarFormats : IPrinterFormatter
    {
        private DS_PushMoneyPagos myRec;
        private PrinterManager printer;

        public PushMoneyPorPagarFormats()
        {
            myRec = new DS_PushMoneyPagos();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer)
        {
            this.printer = printer;

            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibosPushMoney() == 0 ? DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos() : DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibosPushMoney();

            switch (formato)
            {
                case 1:
                default:
                    Formato1(traSecuencia, confirmado);
                    break;
                case 2:
                    Formato2(traSecuencia, confirmado);
                    break;
                case 10:
                    Formato10(traSecuencia, confirmado);
                    break;
            }
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            Print(traSecuencia, confirmado, printer);
        }

        private void Formato1(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            PushMoneyPagos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            //PushMoneyPagosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);

            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("RECIBO DE PAGO PUSHMONEY");
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.pusEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.pusCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.pusFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("Documento                     Monto");
            printer.DrawLine();

            //foreach documentos
            double totalNeto = 0;

            foreach (PushMoneyPagosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recibo.rowguid, reciboConfirmado))
            {
                printer.DrawText(app.PxpDocumento.PadRight(30) + (app.pxpValor).ToString("N2").PadLeft(13));

                totalNeto += app.pxpValor;
            }

            printer.DrawLine();
            
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (PushMoneyPagosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recibo.rowguid, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 20: //bono
                        printer.DrawText("Bono Denominacion : " + rec.DenDescripcion);
                        printer.DrawText("Bono Cantidad     : " + rec.PusBonoCantidad.ToString("N2"));
                        printer.DrawText("Bono              : " + rec.RefValor.ToString("N2"));
                        break;

                }
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
                printer.DrawText("");
            }

            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(30));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recSecuencia, reciboConfirmado);

            printer.Print();
        }
        private void Formato10(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            PushMoneyPagos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            //PushMoneyPagosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);

            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("RECIBO DE PAGO PUSHMONEY");
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.pusEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.pusCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cedula: " + recibo.ClDCedula);
            printer.DrawText("Nombre: " + recibo.CliNombre, 48);
            printer.DrawText("Dependiente: " + recibo.ClDNombre, 48);

            var detalles = myRec.GetPushMoneyAplicacionBySecuencia(recibo.rowguid, reciboConfirmado);
            double total = 0.0;

            if (detalles != null)
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Descripcion");
                printer.DrawText("Codigo        Cantidad        P/U       Total");
                printer.Bold = false;
                printer.DrawLine();

                foreach (var det in detalles)
                {

                    printer.DrawText(det.ProDescripcion.Substring(0,28));

                    var cantidad = det.PxpCantidad.ToString();

                    if (det.PxpCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + det.PxpCantidadDetalle;
                    }

                    var cantidadReal = det.PxpCantidadDetalle + det.PxpCantidad;

                    var subTotal = (det.PxpPrecio + (det.PxpPrecio * (det.PxpItbis / 100))) * cantidadReal;

                    printer.DrawText(det.ProCodigo + cantidad.PadLeft(16) + det.PxpPrecio.ToString("N2").PadLeft(11) + subTotal.ToString("N2").PadLeft(12));

                    total += subTotal;
                }
                printer.DrawLine();
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("RELACION DE FACTURAS INCLUIDA: ");

                foreach (PushMoneyPagosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recibo.rowguid, reciboConfirmado))
                {
                    printer.DrawText(app.PxpDocumento.PadRight(30));
                }
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("ISR 10%:       " + (total * 0.10).ToString("N2").PadLeft(30));
                printer.DrawText("Total a Pagar:  " + (total - (total * 0.10)).ToString("N2").PadLeft(29));
                printer.DrawText("");
                printer.DrawLine();
            }
            else
            {
                printer.DrawLine();
                printer.DrawText("Documento                     Monto");
                printer.DrawLine();

                foreach (PushMoneyPagosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recibo.rowguid, reciboConfirmado))
                {
                    printer.DrawText(app.PxpDocumento.PadRight(30) + (app.pxpValor).ToString("N2").PadLeft(13));

                    total += app.pxpValor;
                }

                printer.DrawLine();
                printer.DrawText("ISR 10%:       " + (total * 0.10).ToString("N2").PadLeft(30));
                printer.DrawText("Total a Pagar:  " + (total - (total * 0.10)).ToString("N2").PadLeft(29));
                printer.DrawText("");
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
                printer.DrawText("");
            }

            printer.DrawText("");
            printer.DrawText("Recibido Por: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Realizado Por:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 10: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recSecuencia, reciboConfirmado);

            printer.Print();
        }

        private void Formato11(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            PushMoneyPagos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            //PushMoneyPagosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);

            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("RECIBO DE PAGO PUSHMONEY");
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.pusEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.pusCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.pusFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("Documento                     Monto");
            printer.DrawLine();

            //foreach documentos
            double totalNeto = 0;

            foreach (PushMoneyPagosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recibo.rowguid, reciboConfirmado))
            {
                printer.DrawText(app.PxpDocumento.PadRight(30) + (app.pxpValor).ToString("N2").PadLeft(13));

                totalNeto += app.pxpValor;
            }

            printer.DrawLine();

            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            var formapago = myRec.GetRecibosFormasPagoBySecuencia(recibo.rowguid, reciboConfirmado);
            foreach (PushMoneyPagosFormaPago rec in formapago)
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 20: //bono
                        printer.DrawText("Bono: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;

                }
                printer.DrawLine();
            }
            printer.DrawText("Cantidad-Descripcion");
            printer.DrawLine();

            foreach (PushMoneyPagosFormaPago rec in formapago)
            {
                string dendescripcion = new DS_Denominaciones().GetDenominacionesByDenId(rec.DenID);
                printer.DrawText(rec.PusCantidad.ToString() + "-" + dendescripcion);
            }
            printer.DrawLine();

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
                printer.DrawText("");
            }

            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(30));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recSecuencia, reciboConfirmado);

            printer.Print();
        }

        private void Formato2(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            PushMoneyPagos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);

            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("RECIBO DE PAGO PUSHMONEY");
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.pusEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;

            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.pusCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.pusFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("");
            printer.DrawText("Documentos aplicados");
            
            double totalNeto = 0;
            foreach (PushMoneyPagosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recibo.rowguid, reciboConfirmado))
            {
                printer.DrawLine();
                printer.Bold = true;
                printer.DrawText("Documento                             Monto");
                printer.Bold = false;
                printer.DrawLine();
                printer.DrawText(app.PxpDocumento.PadRight(30) + (app.pxpValor).ToString("N2").PadLeft(13));


                var detalles = myRec.GetPushMoneyAplicacionByReferencia(app.PxpReferencia, reciboConfirmado);
                double total = 0.0;

                if (detalles != null)
                {
                    printer.DrawText("");
                    printer.Bold = true;
                    printer.DrawText("Descripcion");
                    printer.DrawText("Codigo        Cantidad        P/U       Total");
                    printer.Bold = false;
                    printer.DrawLine();

                    foreach (var det in detalles)
                    {
                        var descr = det.ProDescripcion;
                        if (descr.Length > 48)
                        {
                            descr = descr.Substring(0, 48);
                        }
                        printer.DrawText(descr, 48);
   
                        var cantidad = det.PxpCantidad.ToString();

                        if (det.PxpCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + det.PxpCantidadDetalle;
                        }

                        var cantidadReal = det.PxpCantidadDetalle + det.PxpCantidad;

                        var subTotal = (det.PxpPrecio + (det.PxpPrecio * (det.PxpItbis / 100))) * cantidadReal;

                        printer.DrawText(det.ProCodigo + cantidad.PadLeft(16) + det.PxpPrecio.ToString("N2").PadLeft(11) + subTotal.ToString("N2").PadLeft(12));

                        total += subTotal;
                    }
                    printer.DrawText("");
                }
                

                totalNeto += app.pxpValor;
            }

            printer.DrawLine();

            printer.DrawText("Total:  " + totalNeto.ToString("N2").PadLeft(37));
            printer.DrawText("Retencion 10%:  " + (totalNeto * 0.10).ToString("N2").PadLeft(29));
            printer.DrawText("Total a Pagar:  " + (totalNeto - (totalNeto * 0.10)).ToString("N2").PadLeft(29));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (PushMoneyPagosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recibo.rowguid, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 20: //bono
                        printer.DrawText("Bono Denominacion : " + rec.DenDescripcion);
                        printer.DrawText("Bono Cantidad     : " + rec.PusBonoCantidad.ToString("N2"));
                        printer.DrawText("Bono              : " + rec.RefValor.ToString("N2"));
                        break;

                }
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
                printer.DrawText("");
            }

            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(30));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recSecuencia, reciboConfirmado);

            printer.Print();
        }

    }
}

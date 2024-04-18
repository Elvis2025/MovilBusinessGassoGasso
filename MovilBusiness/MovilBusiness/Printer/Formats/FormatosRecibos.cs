using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;

using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.printers.formats
{
    public class FormatosRecibos : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Recibos myRec;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        private DS_Representantes myrep;
        private DS_Visitas myvisit;
        private DS_Autorizaciones myAut;
        public FormatosRecibos(DS_Recibos myRec)
        {
            this.myRec = myRec;
            myAut = new DS_Autorizaciones();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
            myrep = new DS_Representantes();
            myvisit = new DS_Visitas();
        }

        public void PrintNCDpp(NCDPP nc, PrinterManager printer)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNCDPP())
            {
                case 2:
                    FormatoNCDPP1(nc);
                    break;
                case 3://lam
                    FormatoNCDPP3(nc);
                    break;
                default:
                    FormatoNCDPP1(nc);
                    break;
            }
        }

        private void FormatoNCDPP1(NCDPP nc)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("NOTA CREDITO POR DESCUENTO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            if (!printer.IsEscPos)
            {
               
                printer.DrawText("");
            }
            printer.DrawText("NCF:   " + nc.NCDNCF);
            if (!printer.IsEscPos)
            {
              
                printer.DrawText("");
            }
            
           // printer.DrawText("");
            printer.Bold = false;
            
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            //printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(nc.NCDFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Recibo. No.: " + Arguments.CurrentUser.RepCodigo +"-"+ nc.RecTipo+"-"+ nc.RecSecuencia);
            printer.DrawText("Nota de Credito No.: "+ Arguments.CurrentUser.RepCodigo + "-" + nc.RecTipo + "-" + nc.NCDSecuencia);
            printer.DrawText("Codigo: " + nc.CliCodigo);
            printer.DrawText("Cliente: " + nc.CliNombre, 48);
         //   printer.DrawText("Monto: " + nc.NCDMonto.ToString("N2"));
          //  printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Factura         Monto         NCF MODIFICADO");
            printer.Bold = false;
            printer.DrawLine();
           
            printer.DrawText(nc.CxcDocumento.PadRight(16) + nc.NCDMonto.ToString("N2").PadRight(16) + nc.CxCNCFAfectado.PadRight(10));
        
            printer.DrawLine();
            printer.Bold = true;
            //printer.DrawText("TOTAL: ".PadRight(16) + nc.NCDMonto.ToString("N2").PadRight(16)) ;
            
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
          //  printer.DrawText("____________________________");
            printer.DrawText("Recibido Por:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(48, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNCDPP()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(48, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNCDPP()));
                printer.DrawText("");
            }
            printer.DrawText("");
            //printer.DrawText("NOTA: " + GetNotaXTipoTransaccionReporte(Arguments.Values.));
            //printer.DrawText("");
            //printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
           // printer.DrawText("***Monto Transaccion Incluye ITBIS***");
            printer.TextAlign = Justification.LEFT;
           /* printer.DrawText("");
            printer.DrawText("");*/
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato NCDPP 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
           
        }

        private void FormatoNCDPP3(NCDPP nc)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold:true);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("NOTA CREDITO POR DESCUENTO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            if (!printer.IsEscPos)
            {

                printer.DrawText("");
            }
            printer.DrawText("NCF:   " + nc.NCDNCF);
            if (!printer.IsEscPos)
            {

                printer.DrawText("");
            }

            // printer.DrawText("");
            printer.Bold = false;

            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            //printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(nc.NCDFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Recibo. No.: " + Arguments.CurrentUser.RepCodigo + "-" + nc.RecTipo + "-" + nc.RecSecuencia);
            printer.DrawText("Nota de Credito No.: " + Arguments.CurrentUser.RepCodigo + "-" + nc.RecTipo + "-" + nc.NCDSecuencia);
            printer.DrawText("Codigo: " + nc.CliCodigo);
            printer.DrawText("Cliente: " + nc.CliNombre, 48);
            //   printer.DrawText("Monto: " + nc.NCDMonto.ToString("N2"));
            //  printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Factura         Monto         NCF MODIFICADO");
            printer.Bold = false;
            printer.DrawLine();

            printer.DrawText(nc.CxcDocumento.PadRight(16) + nc.NCDMonto.ToString("N2").PadRight(16) + nc.CxCNCFAfectado.PadRight(10));

            printer.DrawLine();
            printer.Bold = true;
            //printer.DrawText("TOTAL: ".PadRight(16) + nc.NCDMonto.ToString("N2").PadRight(16)) ;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            //  printer.DrawText("____________________________");
            printer.DrawText("Recibido Por:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(48, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNCDPP()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(48, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNCDPP()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            // printer.DrawText("***Monto Transaccion Incluye ITBIS***");
            printer.TextAlign = Justification.LEFT;
            /* printer.DrawText("");
             printer.DrawText("");*/
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato NCDPP 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        public void Print(int RecSecuencia, PrinterManager printer, string rowguid = "") { Print(RecSecuencia, false, printer, rowguid); }
        public void Print(int RecSecuencia, bool Confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            ///Copias = 1;
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos())
            {
                case 1:
                default:
                    Formato1(RecSecuencia, Confirmado);
                    break;
                case 2://MultiMoneda
                    Formato2(RecSecuencia, Confirmado);
                    break;
                case 3: //Disfarmacos
                    Formato3(RecSecuencia, Confirmado);
                    break;
                case 4: //LAM
                    Formato4(RecSecuencia, Confirmado);
                    break;
                case 5: //LA TABACALERA
                    Formato5(RecSecuencia, Confirmado);
                    break;
                case 6: //TUCAN
                    Formato6(RecSecuencia, Confirmado);
                    break;
                case 21: //SUED
                    Formato21(RecSecuencia, Confirmado);
                    break;
                case 22: //INMUEBLECA
                    Formato22(RecSecuencia, Confirmado);
                    break;
                case 7: //PRODUCTOS DEL CERRO
                    Formato7(RecSecuencia, Confirmado);
                    break;
                case 8: //FELTREX
                    Formato8(RecSecuencia, Confirmado);
                    break;
                case 9: //PIMCO
                    Formato9(RecSecuencia, Confirmado);
                    break;
                case 10: //Nutriciosa
                    Formato10(RecSecuencia, Confirmado);
                    break;
                case 11: //Agrifeed
                    Formato11(RecSecuencia, Confirmado);
                    break;
                case 12: //C. Federico Gomez
                    Formato12(RecSecuencia, Confirmado);
                    break;
                case 13: //Phamatech
                    Formato13(RecSecuencia, Confirmado);
                    break;
                case 14: //Fraga Industrial
                    Formato14(RecSecuencia, Confirmado);
                    break;
                case 15: //Importadora La Plaza
                    Formato15(RecSecuencia, Confirmado);
                    break;
                case 16: //
                    Formato16(RecSecuencia, Confirmado);
                    break;
                case 17: // Agroproductores 
                    Formato17(RecSecuencia, Confirmado);
                    break;
                case 18: // ANDOSA 
                    Formato18(RecSecuencia, Confirmado);
                    break;
                case 20: //GRUPO ARMENTEROS
                    Formato20(RecSecuencia, Confirmado);
                    break;
                case 27:
                    Formato27(RecSecuencia, Confirmado);
                    break;
                case 34: //ACROMAX SUPERVISORES
                    Formato34(RecSecuencia, Confirmado);
                    break;
                case 36: //ARIAS MOTORS
                    Formato36(RecSecuencia, Confirmado);
                    break;
                case 37:
                    Formato37(RecSecuencia, Confirmado);
                    break;
                case 38:
                    Formato38(RecSecuencia, Confirmado);
                    break;
                //FORMATO DE 2 PULGADAS
                case 39:
                    Formato39(RecSecuencia, Confirmado);
                    break;
                case 40:
                    Formato40(RecSecuencia, Confirmado);
                    break;
                case 41: //FOODSMART
                    Formato41(RecSecuencia, Confirmado);
                    break;
                case 42:
                    Formato42(RecSecuencia, Confirmado);
                    break;
                case 43:
                    Formato43(RecSecuencia, Confirmado);
                    break;
                case 44:
                    Formato44(RecSecuencia, Confirmado);
                    break;
            }

           // myRec.ActualizarCantidadImpresion(rowguid);
        }

    

        private void Formato1(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            if(!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT ;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;
                
                if(sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

          //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 2:
                        printer.DrawText("Cheque "+(rec.RefIndicadorDiferido?"diferido":"normal")+"  Numero: " + rec.RefNumeroCheque.ToString());
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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;           
            printer.DrawText("Formato recibos 1: Movilbusiness "+ Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #region Multimoneda
        private void Formato2(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null) {
            printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            } else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else 
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;

         

            foreach (var moneda in monedas)
            {
               
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count>1?moneda.MonCodigo:null))
                    {
                        printer.DrawText(moneda.MonNombre +"-"+ moneda.MonCodigo);
                       //TotalCobrado += rec.RecPrima;
                        var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;
                    
                    TotalCobrado  += monedas.Count > 1 ? Conversion: rec.RecPrima;
                    switch (rec.ForID)
                        {
                            case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                            
                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count>1)
                            {
                                printer.DrawText("Monto "+ recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                            case 2:
                                printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                printer.DrawText("Banco    : " + rec.BanNombre, 48);
                                printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                                if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                                }

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto "+ recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                                {
                                    printer.DrawText("Fecha: " + rec.RefFecha);
                                }
                                break;
                            case 4:
                                printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                printer.DrawText("Fecha    : " + rec.RefFecha);
                                printer.DrawText("Banco    : " + rec.BanNombre, 48);
                                printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                                if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                                }  
                            

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto "+ recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                          
                            break;
                            case 5:
                                printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                break;
                            case 6:
                                printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                            case 18:
                                printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                                printer.DrawText("Fecha    : " + rec.RefFecha);
                                printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                                printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10:35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        }
                        printer.DrawLine();
                    }

                    if (!printer.IsEscPos)
                    {
                        printer.DrawText("");
                        printer.DrawText("");
                    }
                }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        #endregion

        #region CambioMoneda


        private void Formato11(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia2(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (recibo.MonCodigo + (valorBruto).ToString("N2")).PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                if ((recibo.MonCodigo != app.MonCodigo) && !string.IsNullOrEmpty(app.MonCodigo))
                {
                    var convert = recibo.RecTasa / app.RecTasa;

                    printer.DrawText(("Moneda: " + app.MonCodigo).PadRight(11) + ("    Tasa: " + (app.RecTasa == 1 ? recibo.RecTasa : app.RecTasa).ToString("N2")).PadRight(17) + ("Valor: " + (valorBruto * convert).ToString("N2")).PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;

            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);///
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 11: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion

        #region Arias Motors
        private void Formato36(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX-Documento");
            printer.DrawText("Balance            Importe          Balance ");
            printer.DrawText("Original           Cobrado          Pendiente");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            var recaplica = myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in recaplica)
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                var balancependiente = Math.Abs(app.CxcBalance - app.RecValor);

                printer.DrawText(sigla + app.CxCDocumento);
                printer.DrawText(app.CxcBalance.ToString("N2") + (valorBruto).ToString("N2").PadLeft(20) + balancependiente.ToString("N2").PadLeft(14));
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla + nc.CxCDocumento.PadRight(14));
                    printer.DrawText((nc.RecValor).ToString("N2").PadLeft(22) + balancependiente.ToString("N2").PadLeft(19));
                }

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 36: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        #endregion

        #region Disfarmacos
        private void Formato3(int recSecuencia, bool reciboConfirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            //printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            //printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            if (monedas.Count > 1)
            {
                printer.DrawText("Moneda:" + recibo.MonCodigo + "   Tasa: " + recibo.RecTasa);
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                if (app.RecDescuento == 0)
                {
                    printer.DrawText("   ".PadRight(24) + (app.RecValor).ToString("N2"));
                }
                else
                {
                    printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));
                }

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            //printer.DrawText("");
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 3: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();

        }
        #endregion

        private void Formato4(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa(Notbold:true);
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "ORIGINAL" : "COPIA #" + recibo.RecCantidadImpresion);
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("RECIBO : " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia); 
            printer.Font = PrinterFont.BODY;
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("");
                printer.DrawText("A N U L A D O");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + recSecuencia, "H");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha  : " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo : " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            if(Arguments.Values.CurrentSector != null)
                printer.DrawText("Sector : " + Arguments.Values.CurrentSector);
            else
                printer.DrawText("Sector : ");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Documentos aplicados");
            printer.DrawText("TX         Documento                   Monto");
            printer.DrawText("Descuento                               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(32) + (app.RecValor).ToString("N2").PadLeft(13));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(29));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                }
                printer.DrawLine();
            }
            printer.DrawText("");
            printer.DrawText("Total cobrado: " + ("$" + TotalCobrado.ToString("N2")).PadLeft(29));
            printer.DrawText("");
            printer.DrawText("Fecha de Impresion: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma Vendedor:________________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma Cliente:_________________________________");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular : " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 4: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #region Tucan
        private void Formato6(int recSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, confirmado);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            /*if (monedas.Count > 1)
            {
                printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
            }*/
            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento  Neto                    Balance");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, confirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, confirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, confirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                double SumatoriaNC = 0;
                double DescuentoNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    SumatoriaNC += nc.RecValor;
                    DescuentoNC += nc.RecDescuento;
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                double cxcBalance = app.CxcBalance - app.RecValor - app.RecDescuento - SumatoriaNC - DescuentoNC;
                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(11) + (app.RecValor).ToString("N2").PadRight(21) + cxcBalance.ToString("N2").PadLeft(13));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
                cxcBalance = 0;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, confirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 6: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        #endregion

        #region La Tabacalera
        private void Formato5(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }
            bool putfecha = true;
            printer.PrintEmpresa(recSecuencia,putfecha);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff");
            if (recibo.RecCantidadImpresion > 0)
            {
                printer.Bold = false;
                printer.DrawText("*Copia** " + fechaRecibo);
                printer.DrawText("");
                printer.Bold = true;
                printer.DrawText("R E C I B O  D E  P A G O");
            }
            else
            {
                printer.DrawText("      ORIGINAL");
                printer.DrawText("");
                printer.DrawText("R E C I B O  D E  P A G O");
            }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            var fechaValida = DateTime.TryParse(recibo.RecFecha, out DateTime fecha);
            printer.DrawText("Fecha Recibo :" + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : recibo.RecFecha));
            printer.DrawText("RECIBO       : " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
           // printer.DrawText("Fecha: " + fechaRecibo);
            printer.DrawText("Codigo       :" + recibo.CliCodigo);
            printer.DrawText("Cliente      :" + recibo.CliNombre, 48);
            printer.DrawText("Ruta         :" + Arguments.CurrentUser.RutID);
            printer.DrawText("Propietario  :" + recibo.CliContacto, 48);
            printer.DrawText("Calle        :" + recibo.CliCalle, 48);
            printer.DrawText("Urb          :" + recibo.CliUrbanizacion,48);
            printer.DrawText("RNC/Cedula   :" + recibo.CliRNC);
            printer.DrawText("Telefono     :" + recibo.CliTelefono.FormatTextToPhone());
            
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();            
            printer.DrawText("Documentos Aplicados:");                      
            printer.DrawText("Tx         Documento         Monto ");
            printer.DrawText("Descuento                              Neto");
            printer.DrawText("");
            printer.DrawLine();

 
            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                {
                    sigla = app.RecIndicadorSaldo ? "FC" : "AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                string cxcNCF = app.CXCReferencia;
                var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                if(CxC != null)
                {
                    if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                    {
                        cxcNCF = CxC.CXCNCF; 
                    }
                }
                printer.DrawText(sigla.PadRight(11) + cxcNCF.PadRight(21) + ("$" + (valorBruto).ToString("N2")).PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("$" + (nc.RecValor).ToString("N2")).PadLeft(13));
                }

                printer.DrawText(("$" + app.RecDescuento.ToString("N2")).PadRight(24) + ("$" + (app.RecValor).ToString("N2")));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

          //  printer.DrawLine();

              printer.DrawText("");
            ValorNC = Math.Abs(ValorNC) * -1;
            printer.DrawText("Valor bruto:    " + ("$" + TotalBruto.ToString("N2")).PadLeft(29));
            printer.DrawText("Valor NC:       " + ("$" + ValorNC.ToString("N2")).PadLeft(29));
            printer.DrawText("Valor Desc:     " + ("$" + TotalDesc.ToString("N2")).PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + ("$" + totalNeto.ToString("N2")).PadLeft(30));

            printer.DrawText("");
            var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
            double TotalCobrado   = recibosFormasPagos.Sum(r => r.RefValor);
            printer.DrawText("Total Cobrado:  " + ("$" + TotalCobrado.ToString("N2")).PadLeft(30));
            //printer.DrawText("Valor Bruto:         " + Funciones.rellenaDerecha(ValorBruto, 20));
            //printer.DrawText("Valor NC:            " + Funciones.rellenaDerecha(ValorNC.replace("$", "$-"), 20));
            //printer.DrawText("Valor Desc:          " + Funciones.rellenaDerecha(ValorDescuento, 20));
            //printer.DrawText("Valor Sobrante:      " + Funciones.rellenaDerecha(ValorMontoSobrante, 20));
            //printer.DrawText("Total a Pagar:       " + Funciones.rellenaDerecha(ValorTotalAPagar, 20));

            printer.DrawText("");
            printer.DrawText("Formas de pago");            
            foreach (RecibosFormaPago rec in recibosFormasPagos)
            {       
               switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + ("$" + rec.RefValor.ToString("N2")));                      
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "Diferido" : "Normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("");
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }                        
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);                       
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));                      
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + ("$" + rec.RefValor.ToString("N2")));
                        break;
                    case 6:
                        printer.DrawText("Tarjeta Credito: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));
                        break;

                }
                printer.DrawText("");
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {                
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            //printer.DrawText("Firma Cliente:____________________________________");            
            printer.DrawText("Firma Cliente:_________________________________");

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            string rutaVendedor = "(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre;
            rutaVendedor = rutaVendedor.CenterText(48);
            printer.DrawText("Firma Vendedor:________________________________");
            printer.DrawText(rutaVendedor);
            printer.DrawText(("Celular: " + Arguments.CurrentUser.RepTelefono1.FormatTextToPhone()).CenterText(48));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las Facturas");
            printer.DrawText("FC: Factura pagada Completa");
            printer.DrawText("AB: Abono a Factura ");
            printer.DrawText("NC: Nota de Credito Aplicada");
            printer.DrawText("CK: Factura por cheque devuelto ");

            //printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 5: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }


        #endregion

        #region SUED
        private void Formato21(int recSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A No. " + recibo.RecCantidadImpresion);
            printer.DrawText("");
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + recibo.RecSecuencia, "H");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            if (!string.IsNullOrWhiteSpace(recibo.SecCodigo))
            {
                printer.DrawText("Sector: " + recibo.SecCodigo.Trim());
            }
           
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Documentos Aplicados: ");
            printer.DrawText("Tx         Documento                  Monto");
            printer.DrawText("Descuento                              Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, confirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, confirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, confirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-FC" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(36) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("FC: factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 21: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();


        }

        #endregion

        #region Productos del Cerro
        private void Formato7(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O  D E  P A G O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 7: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion

        #region Feltrex
        private void Formato8(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }
            printer.PrintEmpresa(Notbold: true);
            printer.Bold = true;
            printer.DrawText("");
            //if (!printer.IsEscPos)
            //{
            //    printer.DrawText("");
            //}
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff");
            printer.DrawText("R E C I B O");
            printer.DrawText("");
            if (recibo.RecCantidadImpresion > 0)
            {
                printer.DrawText("**COPIA** ");
            }
            else
            {
                printer.DrawText("**ORIGINAL** ");
            }
            printer.Font = PrinterFont.BODY;
            //if (!printer.IsEscPos)
            //{
            //    printer.DrawText("");
            //}
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            var fechaValida = DateTime.TryParse(recibo.RecFecha, out DateTime fecha);
            printer.DrawText("RECIBO     : " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha      : " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : recibo.RecFecha));
            // printer.DrawText("Fecha: " + fechaRecibo);
            printer.DrawText("Codigo       :" + recibo.CliCodigo);
            printer.DrawText("Cliente      :" + recibo.CliNombre, 48);           
            //printer.DrawText("");
          
            printer.DrawText("Documentos Aplicados:");
            printer.DrawLine();
            printer.DrawText("Tx       Documento  Vencimiento Dias    Monto ");
            printer.DrawText("Descuento                       Pago   Neto");
            //printer.DrawText("");
            printer.DrawLine();


            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ItbisPagado = 0.0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                ItbisPagado += app.RecItbis;
                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                {
                    sigla = app.RecIndicadorSaldo ? "FC" : "AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                string cxcNCF = app.CXCReferencia;
                var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                if (CxC != null)
                {
                    if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                    {
                        cxcNCF = CxC.CXCNCF;
                    }
                }
                int dias = 0;
                if (recibo.RecFecha != null && CxC.CxcFecha != null)
                {
                    dias = ((TimeSpan)(Convert.ToDateTime(recibo.RecFecha) - Convert.ToDateTime(CxC.CxcFecha))).Days;
                }
                printer.DrawText(sigla.PadRight(9) + app.CxCDocumento.PadRight(11) + Convert.ToDateTime(app.CXCFechaVencimiento).ToString("dd/MM/yyyy").PadRight(12) + ("("+ dias + ")").PadRight(4) +  ("$" + (valorBruto).ToString("N2")).PadLeft(10));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("$" + (nc.RecValor).ToString("N2")).PadLeft(13));
                }

                printer.DrawText(("$" + app.RecDescuento.ToString("N2") + " (" + app.RecPorcDescuento + "%)").PadRight(15) + ("$" + (app.RecValor).ToString("N2")).PadLeft(31));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }


            printer.DrawLine();

            //printer.DrawText("");
            ValorNC = Math.Abs(ValorNC) * -1;
            printer.DrawText("Valor bruto:    " + ("$" + TotalBruto.ToString("N2")).PadLeft(29));
            printer.DrawText("Valor NC:       " + ("$" + ValorNC.ToString("N2")).PadLeft(29));
            printer.DrawText("Valor Desc:     " + ("$" + TotalDesc.ToString("N2")).PadLeft(29));
            printer.DrawText("Itbis Pagado:   " + ("$" + ItbisPagado.ToString("N2")).PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + ("$" + totalNeto.ToString("N2")).PadLeft(29));

            //printer.DrawText("");
            var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
            double TotalCobrado = recibosFormasPagos.Sum(r => r.RefValor);
            //printer.DrawText("Total Cobrado:  " + ("$" + TotalCobrado.ToString("N2")).PadLeft(29));
            //printer.DrawText("Valor Bruto:         " + Funciones.rellenaDerecha(ValorBruto, 20));
            //printer.DrawText("Valor NC:            " + Funciones.rellenaDerecha(ValorNC.replace("$", "$-"), 20));
            //printer.DrawText("Valor Desc:          " + Funciones.rellenaDerecha(ValorDescuento, 20));
            //printer.DrawText("Valor Sobrante:      " + Funciones.rellenaDerecha(ValorMontoSobrante, 20));
            //printer.DrawText("Total a Pagar:       " + Funciones.rellenaDerecha(ValorTotalAPagar, 20));

            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            foreach (RecibosFormaPago rec in recibosFormasPagos)
            {
                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + ("$" + rec.RefValor.ToString("N2")));
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "Futurista" : "Normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                        if (rec.AutSecuencia != 0)
                        {
                            printer.DrawText("Autorizacion: " + rec.AutSecuencia);
                        }
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("");
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + ("$" + rec.RefValor.ToString("N2")));
                        break;

                }
                printer.DrawText("");      
            }
            printer.DrawLine();

            //if (!printer.IsEscPos)
            //{
            //    printer.DrawText("");
            //}
            printer.DrawText("Total Cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha de Impresion: " + DateTime.Now.ToString("dd/MM/yyyy h:mm tt"));
            string comentario = new DS_Recibos().GetComentarioRecibo(recibo.RecSecuencia);
            if(comentario == "")
            {
                comentario = "Sin comentario";
            }
            printer.DrawText("Nota: " + comentario);
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            //printer.DrawText("Firma Cliente:____________________________________");            
            printer.DrawText("Firma Cliente:_________________________________");

           // printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            string rutaVendedor = "(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre;
            rutaVendedor = rutaVendedor.CenterText(48);
            printer.DrawText("Firma Vendedor:________________________________");
            //printer.DrawText(rutaVendedor);
            //printer.DrawText(("Celular: " + Arguments.CurrentUser.RepTelefono1.FormatTextToPhone()).CenterText(48));
            //printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Leyenda explicativa de las Facturas");
            printer.DrawText("FC: Factura pagada Completa");
            printer.DrawText("AB: Abono a Factura ");
            printer.DrawText("NC: Nota de Credito Aplicada");
            printer.DrawText("CK: Factura por cheque devuelto ");

            //printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular : " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 8: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }


        #endregion

        #region PIMCO
        private void Formato9(int recSecuencia, bool reciboConfirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo, reciboConfirmado);
            if (recibo == null)
            {
                return;
            }
            bool putfecha = false;
            printer.PrintEmpresa(0, putfecha,false,true);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff");
            if (recibo.RecCantidadImpresion > 0)
            {
                printer.Bold = false;
                printer.DrawText("*Copia** " + fechaRecibo);
                printer.DrawText("");
                printer.Bold = true;
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O  P R O V I S I O N A L");
                }
                else { printer.DrawText("R E C I B O  P R O V I S I O N A L"); }
            }
            else
            {
                printer.DrawText("      ORIGINAL");
                printer.DrawText("");
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O  P R O V I S I O N A L");
                }
                else { printer.DrawText("R E C I B O  P R O V I S I O N A L"); }
            }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            //printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            var fechaValida = DateTime.TryParse(recibo.RecFecha, out DateTime fecha);
            printer.DrawText("Fecha Recibo :" + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : recibo.RecFecha));
            printer.DrawText("RECIBO       : " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Codigo       :" + recibo.CliCodigo);
            printer.DrawText("Cliente      :" + recibo.CliNombre, 48);
            printer.DrawText("RNC/Cedula   :" + recibo.CliRNC);
            //printer.DrawText("Calle        :" + recibo.CliCalle, 48);
            //printer.DrawText("Telefono     :" + recibo.CliTelefono.FormatTextToPhone());

           // printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Documentos Aplicados:");
            printer.DrawText("TX         Documento                   Monto");
            //printer.DrawText("Descuento                              Neto");
            printer.DrawText("");
            printer.DrawLine();


            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                {
                    sigla = app.RecIndicadorSaldo ? "FC" : "AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                string cxcNCF = app.CXCReferencia;
                var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                if (CxC != null)
                {
                    if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                    {
                        cxcNCF = CxC.CXCNCF;
                    }
                }
                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + ("$" + (valorBruto).ToString("N2")).PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("$" + (nc.RecValor).ToString("N2")).PadLeft(13));
                }

                //printer.DrawText(("$" + app.RecDescuento.ToString("N2")).PadRight(24) + ("$" + (app.RecValor).ToString("N2")));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            //  printer.DrawLine();

            printer.DrawText("");
            ValorNC = Math.Abs(ValorNC) * -1;
            ///printer.DrawText("Valor bruto:    " + ("$" + TotalBruto.ToString("N2")).PadLeft(29));
            //printer.DrawText("Valor NC:       " + ("$" + ValorNC.ToString("N2")).PadLeft(29));
            //printer.DrawText("Valor Desc:     " + ("$" + TotalDesc.ToString("N2")).PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            //printer.DrawText("Total a Pagar:  " + ("$" + totalNeto.ToString("N2")).PadLeft(30));

            //printer.DrawText("");
            var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
            double TotalCobrado = recibosFormasPagos.Sum(r => r.RefValor);
            printer.DrawText("Total Cobrado:  " + ("$" + TotalCobrado.ToString("N2")).PadLeft(30));
            //printer.DrawText("Valor Bruto:         " + Funciones.rellenaDerecha(ValorBruto, 20));
            //printer.DrawText("Valor NC:            " + Funciones.rellenaDerecha(ValorNC.replace("$", "$-"), 20));
            //printer.DrawText("Valor Desc:          " + Funciones.rellenaDerecha(ValorDescuento, 20));
            //printer.DrawText("Valor Sobrante:      " + Funciones.rellenaDerecha(ValorMontoSobrante, 20));
            //printer.DrawText("Total a Pagar:       " + Funciones.rellenaDerecha(ValorTotalAPagar, 20));

            printer.DrawText("");
            printer.DrawText("Formas de pago");
            foreach (RecibosFormaPago rec in recibosFormasPagos)
            {
                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + ("$" + rec.RefValor.ToString("N2")));
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "Diferido" : "Normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("");
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("Monto   : " + ("$" + rec.RefValor.ToString("N2")));
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + ("$" + rec.RefValor.ToString("N2")));
                        break;

                }
                printer.DrawText("");
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            //printer.DrawText("");
            //printer.DrawText("Firma Cliente:____________________________________");            
            printer.DrawText("Firma Cliente:_________________________________");

            printer.DrawText("");
            printer.DrawText("");
            //printer.DrawText("");
            //printer.DrawText("");
            string rutaVendedor = "(" + Arguments.CurrentUser.RepCodigo + ") " + Arguments.CurrentUser.RepNombre;
            rutaVendedor = rutaVendedor.CenterText(48);
            printer.DrawText("Firma Vendedor:________________________________");
            printer.DrawText(rutaVendedor);
            //printer.DrawText(("Celular: " + Arguments.CurrentUser.RepTelefono1.FormatTextToPhone()).CenterText(48));
            //printer.DrawText("");
            //printer.DrawText("");
            //printer.DrawText("Leyenda explicativa de las Facturas");
            //printer.DrawText("FC: Factura pagada Completa");
            //printer.DrawText("AB: Abono a Factura ");
            //printer.DrawText("NC: Nota de Credito Aplicada");
            //printer.DrawText("CK: Factura por cheque devuelto ");

            //printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 9: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion

        #region Nutriciosa
        private void Formato10(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("RNC: " + recibo.CliRNC, 48);
            printer.DrawText("Telefono: " + recibo.CliTelefono);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos
            double BalancePendiente = 0;
            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                
                printer.DrawText(sigla.PadRight(11) + app.CXCReferencia.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
                
            }
            
            
            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(29));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Formas de pago");
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            int cantidadFormaPago = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;
                
                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                        cantidadFormaPago += 1;
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }
                        cantidadFormaPago += 1;
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                        cantidadFormaPago += 1;
                        break;
                    case 5:
                        printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                        cantidadFormaPago += 1;
                        break;
                    case 6:
                        printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                        cantidadFormaPago += 1;
                        break;
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                        cantidadFormaPago += 1;
                        break;
                }
                printer.DrawLine();
            }

            //if (!printer.IsEscPos)
            //{
            //    printer.DrawText("");
            //    printer.DrawText("");
            //}
            if (cantidadFormaPago > 1)
            {
                printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(30));
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(recibo.RepVendedor);

            var myCxc = new DS_CuentasxCobrar();
            BalancePendiente = myCxc.GetBalanceTotalByCliid(recibo.CliID, WithChD: !DS_RepresentantesParametros.GetInstance().GetParNoTomarEnCuentaChequesDiferidos());

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

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Cedula:");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma distribuidor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Vendedor: " + RepVendedor);
            printer.DrawText("Secuencia: " + recibo.VisSecuencia);
            printer.DrawText("Dia de Visita: " + day);
            printer.DrawText("Balance Pendiente: " + BalancePendiente);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 10: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion

        #region Importadora La Plaza
        private void Formato15(int recSecuencia, bool reciboConfirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }
            bool putfecha = false;
            printer.PrintEmpresa(0, putfecha);
            printer.DrawLine();
            printer.Bold = true;            
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd/MM/yyyy hh:mm tt");
            if (recibo.RecCantidadImpresion > 0)
            {
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "COMPROBANTE DE INGRESO");
                }
                else { printer.DrawText("R E C I B O"); }

                printer.Bold = false;

                printer.TextAlign = Justification.LEFT;
                printer.DrawText("*REIMPRESO** " + fechaRecibo);

                printer.TextAlign = Justification.CENTER;
                printer.Bold = true;
            }
            else
            {
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "COMPROBANTE DE INGRESO");
                }
                else { printer.DrawText("COMPROBANTE DE INGRESO"); }
            }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }

            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("CODIGO: " + recibo.CliCodigo);
            // printer.DrawText("Fecha: " + fechaRecibo);
            printer.DrawText("RECIBOS DE: " + recibo.CliNombre, 48);

            double valor = 0.00;

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {
                //var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                //double valorNC = 0;
                //foreach (RecibosAplicacion nc in notasCredito)
                //{
                //    valorNC += nc.RecValor;
                //}

                valor += app.RecValor /*+ valorNC*/;
            }

            printer.DrawText("LA SUMA DE:" + "$" + valor.ToString("N2"), 48);            
            printer.DrawText(Functions.NumberToTextV2(valor));            
            //printer.DrawText(Functions.NumberToText((int)valor) + " Con " + valor.ToString("N2").Substring((valor.ToString("N2").LastIndexOf(".") + 1), 2));            
            printer.DrawText("PUEBLO:" + recibo.CliSector, 48);
            printer.Bold = true;
            string fpStr =  "FORMA PAGO:        ";
            var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
            foreach (RecibosFormaPago rec in recibosFormasPagos)
            {
                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText(fpStr + "EFECTIVO");
                        break;
                    case 2:
                        printer.DrawText(fpStr + "CHEQUE " + (rec.RefIndicadorDiferido ? "FUTURISTA" : "NORMAL"));
                        printer.DrawText("CHEQUE NO.: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("");
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }
                        break;
                    case 4:
                        printer.DrawText(fpStr + "TRANSFERENCIA");
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        break;
                    case 5:
                        printer.DrawText(fpStr + "Retencion ");
                        break;

                }
            }

            printer.DrawText("VENDEDOR " + Arguments.CurrentUser.RepCodigo + "       NO." + recSecuencia);

            printer.Bold = false;
            printer.DrawLine();
            printer.DrawText("Factura             Bruto%D               Neto");
            printer.DrawLine();

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);
            bool isfirsttimefc = true, isfirsttimeab = true;

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado).OrderBy(a => a.RecIndicadorSaldo))
            {
                if (app.RecIndicadorSaldo && isfirsttimefc)
                {
                    printer.DrawText("SALDADAS:");
                    isfirsttimefc = false;
                }
                else if (!app.RecIndicadorSaldo && isfirsttimeab)
                {
                    printer.DrawText("ABONADAS:");
                    isfirsttimeab = false;
                }

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                {
                    sigla = app.RecIndicadorSaldo ? "FC" : "AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                string cxcNCF = app.CXCReferencia;
                var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                if (CxC != null)
                {
                    if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                    {
                        cxcNCF = CxC.CXCNCF;
                    }
                }
                printer.DrawText(app.CxCDocumento + ("$" + (valorBruto).ToString("N2") +" "+ app.RecPorcDescuento.ToString()).PadLeft(17)
                    + ("$" + (app.RecValor).ToString("N2")).PadLeft(19));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxCDocumento + ("$" + (nc.RecValor).ToString("N2")).PadLeft(17) 
                        + ("$" + (app.RecValor).ToString("N2")).PadLeft(17));
                }

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            //  printer.DrawLine();

            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            ValorNC = Math.Abs(ValorNC) * -1;
            printer.DrawText("                                   ___________");
            printer.DrawText("                     Total bruto:    " + "$" + TotalBruto.ToString("N2").PadLeft(4));
            printer.DrawText("                     Total desc:      " + "$" + TotalDesc.ToString("N2").PadLeft(4));
            printer.DrawText("                                   ___________");
            printer.DrawText("                     Total neto:     " + "$" + totalNeto.ToString("N2").PadLeft(4));
            printer.Bold = false;
            printer.DrawText("");            
            //double TotalCobrado = recibosFormasPagos.Sum(r => r.RefValor);
            //printer.DrawText("Total Cobrado:  " + ("$" + TotalCobrado.ToString("N2")).PadLeft(30));
            //printer.DrawText("Valor Bruto:         " + Funciones.rellenaDerecha(ValorBruto, 20));
            //printer.DrawText("Valor NC:            " + Funciones.rellenaDerecha(ValorNC.replace("$", "$-"), 20));
            //printer.DrawText("Valor Desc:          " + Funciones.rellenaDerecha(ValorDescuento, 20));
            //printer.DrawText("Valor Sobrante:      " + Funciones.rellenaDerecha(ValorMontoSobrante, 20));
            //printer.DrawText("Total a Pagar:       " + Funciones.rellenaDerecha(ValorTotalAPagar, 20));

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha de Impresion: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));

            printer.DrawText("");
            printer.DrawText("");
            //printer.DrawText("Firma Cliente:____________________________________");                   
            printer.DrawText("______________________________________________");
            printer.DrawText("FIRMA AUTORIZADA");

            printer.DrawText("");            
            //printer.DrawText("");
            //printer.DrawText("Leyenda explicativa de las Facturas");
            //printer.DrawText("FC: Factura pagada Completa");
            //printer.DrawText("AB: Abono a Factura ");
            //printer.DrawText("NC: Nota de Credito Aplicada");
            //printer.DrawText("CK: Factura por cheque devuelto ");

            //printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));            
            printer.TextAlign = Justification.RIGHT;
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Version " + Functions.AppVersion + " Movilbusiness");
            printer.DrawText("Formato recibos 15");
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        
        private void Formato16(int recSecuencia, bool reciboConfirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }
            bool putfecha = false;
            printer.PrintEmpresa(recSecuencia, putfecha);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff");
            if (recibo.RecCantidadImpresion > 0)
            {
                printer.Bold = false;
                printer.DrawText("*Copia** " + fechaRecibo);
                printer.DrawText("");
                printer.Bold = true;
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "COMPROBANTE DE INGRESO");
                }
                else { printer.DrawText("R E C I B O"); }
            }
            else
            {
                printer.DrawText("");
                if (formaPago != null)
                {
                    printer.DrawText(formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "COMPROBANTE DE INGRESO");
                }
                else { printer.DrawText("COMPROBANTE DE INGRESO"); }
            }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }

            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("CODIGO       :" + recibo.CliCodigo);
            printer.DrawText("RECIBOS DE   :" + recibo.CliNombre);
            printer.DrawText("LA SUMA DE   :" + myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado).Sum(x => x.RecValor), 48);
            printer.Bold = true;
            printer.DrawText("FORMA PAGO   :" + formaPago.FormaPago, 48);
            printer.DrawText("VENDEDOR     :" + Arguments.CurrentUser.RepCodigo + ("No." + recSecuencia.ToString()).PadLeft(10), 48);
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Factura             Bruto               Neto ");
            printer.DrawLine();

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            bool isfirsttimefc = true, isfirsttimeab = true;

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado).OrderBy(a => a.RecIndicadorSaldo))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                {
                    sigla = app.RecIndicadorSaldo ? "FC" : "AB";
                }
                if(app.RecIndicadorSaldo && isfirsttimefc)
                {
                    printer.DrawText("SALDADAS:");
                    isfirsttimefc = false;
                }
                else if(!app.RecIndicadorSaldo && isfirsttimeab)
                {
                    printer.DrawText("ABONADAS:");
                    isfirsttimeab = false;
                }


                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                string cxcNCF = app.CXCReferencia;
                var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                if (CxC != null)
                {
                    if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                    {
                        cxcNCF = CxC.CXCNCF;
                    }
                }
                printer.DrawText(app.CxCDocumento + ("$" + (valorBruto).ToString("N2")).PadLeft(13) + ("$" + (app.RecValor).ToString("N2")).PadLeft(19));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxCDocumento.PadRight(13) + ("$" + (nc.RecValor).ToString("N2")).PadLeft(18));
                }


                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Total bruto:    " + ("$" + TotalBruto.ToString("N2")).PadLeft(29));
            printer.DrawText("Total desc:     " + ("$" + TotalDesc.ToString("N2")).PadLeft(29));
            printer.DrawText("Total neto:  " + ("$" + totalNeto.ToString("N2")).PadLeft(32));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            printer.DrawText("");       
            printer.DrawText("______________________________________________");
            printer.DrawText("FIRMA AUTORIZADA");

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            string rutaVendedor = "(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre;
            rutaVendedor = rutaVendedor.CenterText(48);
            printer.DrawText(rutaVendedor);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 16: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        #endregion

        private void Formato20(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O  " + (recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A"));
            }
            else { printer.DrawText("R E C I B O  " + (recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A")); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            //printer.DrawText("");
            //printer.Bold = false;
            //printer.TextAlign = Justification.CENTER;
            //printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {


                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            //printer.DrawText("");
            //printer.DrawText("Firma cliente: ");
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 20: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato34(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var repcod = myRec.GetClientesForRepcodigo(recibo.CliCodigo);
            var vendedor = myrep.GetRepNombre(repcod.RepCodigo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + repcod.RepCodigo + " - "  + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo del cliente: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            printer.DrawText("Firma representante: ");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("vendedor: " + repcod.RepCodigo + "  - " + vendedor);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 34: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato27(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            var myparm = DS_RepresentantesParametros.GetInstance();
            if (!myparm.GetParEliminarCopiasYOriginal())
            {
                string recivonot = myparm.GetParPrefSec()+ " " + recSecuencia.ToString();
                printer.DrawText((recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A  No. " + recibo.RecCantidadImpresion.ToString())+ " - " +  recivonot);
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            printer.DrawText("TX         Documento                     Fecha");
            printer.DrawText("Monto                 Desc            SubTotal");
            printer.DrawLine();

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                var fechaValida = DateTime.TryParse(app.CXCFecha, out DateTime fecha);

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (fechaValida? fecha.ToString("dd/MM/yyyy").PadLeft(13) : ""));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("-" + (nc.RecValor).ToString("N2")).PadLeft(13));
                }

                printer.DrawText(valorBruto.ToString("N2").PadRight(11) + (app.RecDescuento).ToString("N2").PadLeft(16) + (valorBruto - app.RecDescuento).ToString("N2").PadLeft(19));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 27: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            myRec.ActualizarCantidadImpresion(recibo.rowguid);
        }


        private void Formato37(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 37: Movilbusiness " + Functions.AppVersion);
           
            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        private void Formato38(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            List<Autorizaciones> autorizaciones = myAut.GetAutorizacionesByCxcDocumento(recibo.cxcDocumento, recibo.CliID);

            if (recibo == null)
            {
                return;
            }

            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.PrintEmpresa(TitleNotBold: true);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if(!reciboConfirmado)
                printer.DrawText("ESTADO: " + myRec.getNombreEstatus(recibo.RecEstatus));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("RECIBO DE COBRO   " + (reciboConfirmado ? "CONFIRMADO" : (recibo.RecCantidadImpresion == 0 ? "ORIGINAL" : "COPIA" )));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("RECIBO: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Oficina Venta: " + recibo.ofvCodigo);
            printer.DrawText("Moneda: " + recibo.MonSigla);
            printer.DrawText("");
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Vendedor: " + "(" + Arguments.CurrentUser.RepCodigo + ")" + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Documentos aplicados:");
            printer.DrawText("TX Documento      Monto   Descuento    Neto");
            printer.DrawLine();

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla = app.RecIndicadorSaldo ? "S" : "AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                var RecValorString = "$" + app.RecValor.ToString("N2");
                printer.DrawText(sigla.PadRight(3) + app.CxCDocumento.PadRight(13) + "$" + (valorBruto).ToString("N2").PadRight(9) + "$" + app.RecDescuento.ToString("N2").PadRight(9) + RecValorString.PadLeft(10));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(3) + nc.CxCDocumento.PadRight(13) + (nc.RecValor).ToString("N2").PadLeft(30));
                }

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }


            var TotalBrutoString = "$" + TotalBruto.ToString("N2");
            var ValorNCString = "$" + ValorNC.ToString("N2");
            var TotalDescString = "$" + TotalDesc.ToString("N2");
            var MontoSobranteString = "$" + recibo.RecMontoSobrante.ToString("N2");
            var totalNetoString = "$" + totalNeto.ToString("N2");

            printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBrutoString.PadLeft(30));
            printer.DrawText("Valor NC:       " + ValorNCString.PadLeft(30));
            printer.DrawText("Valor desc:     " + TotalDescString.PadLeft(30));

            double recmontosobrante = recibo.RecMontoSobrante;
            if (recmontosobrante > 0.99)
            {
                    printer.DrawText("Valor Sobrante: " + MontoSobranteString.PadLeft(29));
            }
            else
            {
                    printer.DrawText("Valor Sobrante: " + "$0.00".PadLeft(30));
            }
            printer.DrawText("Total a Pagar:  " + totalNetoString.PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(36));
                        break;
                    case 2:
                        printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(36));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("Fecha: " + rec.RefFecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("Banco   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(36));
                        break;
                    case 5:
                        printer.DrawText("Retencion ISR:   " + rec.RefValor.ToString("N2").PadLeft(29));
                        break;
                    case 6:
                        printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(29));
                        break;
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(36));
                        break;
                }
                printer.DrawText("");
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            var TotalCobradoString = "$" + TotalCobrado.ToString("N2");
            printer.DrawLine();
            printer.DrawText("Total cobrado:  " + TotalCobradoString.PadLeft(30));
            printer.DrawText("");
            printer.DrawText("");
            string pinusados = "";
            foreach (Autorizaciones aut in autorizaciones)
            {
                pinusados = aut.AutPin + ",";
            }
            printer.DrawText("Pin Usados: " + pinusados.ToString().PadLeft(33));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma cliente:________________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:_______________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Leyenda Explicativa de las facturas");
            printer.DrawText("FC: Factura pagada Completa");
            printer.DrawText("AB: Abono a Factura ");
            printer.DrawText("NC: Nota de Credito Aplicada");
            printer.DrawText("CK: Factura por cheque devuelto ");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 38: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        //                          2Inches Format

        private void Formato12(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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

            bool ckdindicador = false;
            List<RecibosAplicacion> ckd = myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado);

            for (int i = 0; i < ckd.Count; i++)
            {
                if (ckd[i].CxcSigla == "CKD" || ckd[i].CxcSigla == "CCK")
                    ckdindicador = true;
            }

            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O " + (ckdindicador ? " C H E Q U E  D E V U E L T O " : ""));
            }
            else { printer.DrawText("R E C I B O " + (ckdindicador ? " C H E Q U E  D E V U E L T O " : "")); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            var myparm = DS_RepresentantesParametros.GetInstance();
            if (!myparm.GetParEliminarCopiasYOriginal())
            {
                string recivonot = myparm.GetParPrefSec() + " " + recSecuencia.ToString();
                printer.DrawText((recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A  No. " + recibo.RecCantidadImpresion.ToString()) + " - " + recivonot);
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            printer.DrawText("TX         Documento                     Fecha");
            printer.DrawText("Monto                 Desc            SubTotal");
            printer.DrawLine();

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                var fechaValida = DateTime.TryParse(app.CXCFecha, out DateTime fecha);

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (fechaValida ? fecha.ToString("dd/MM/yyyy").PadLeft(13) : ""));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("-" + (nc.RecValor).ToString("N2")).PadLeft(13));
                }

                printer.DrawText(valorBruto.ToString("N2").PadRight(11) + (app.RecDescuento).ToString("N2").PadLeft(16) + (valorBruto - app.RecDescuento).ToString("N2").PadLeft(19));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 12: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            myRec.ActualizarCantidadImpresion(recibo.rowguid);
        }

        #region INMUEBLECA
        private void Formato22(int recSecuencia, bool reciboConfirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo, reciboConfirmado);
            RecibosFormaPago reciboDiferido = myRec.GetRecibosIsDiferido(recSecuencia, recibo.RecTipo, reciboConfirmado);
            if (recibo == null)
            {
                return;
            }
            bool putfecha = false;
            printer.PrintEmpresaV2(0, putfecha, false, true);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            string fechaRecibo = Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff");
            string ReferenciaSAP = myRec.CrearNoReferencia(Arguments.CurrentUser.RepCodigo, recSecuencia);
            var recibosFormasPagosTitulo = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);

                if (reciboDiferido.RefIndicadorDiferido)
                {
                    if (recibo.RecCantidadImpresion > 0)
                    {
                        printer.Bold = false;
                        printer.DrawText("*Copia** " + fechaRecibo);
                        printer.DrawText("");
                        printer.Bold = true;
                        printer.Font = PrinterFont.MAXTITLE;
                        printer.DrawText("D O C U M E N T O S  R E C I B I D O S  # " + ReferenciaSAP.ToString());


                    }
                    else
                    {
                        printer.DrawText("      ORIGINAL");
                        printer.DrawText("");
                        printer.Font = PrinterFont.MAXTITLE;
                        printer.DrawText("D O C U M E N T O S  R E C I B I D O S  # " + ReferenciaSAP.ToString());
                    }
                }
                else
                {
                    if (recibo.RecCantidadImpresion > 0)
                    {
                        printer.Bold = false;
                        printer.DrawText("*Copia** " + fechaRecibo);
                        printer.DrawText("");
                        printer.Bold = true;
                        printer.Font = PrinterFont.MAXTITLE;
                        printer.DrawText("R E C I B O  D E  C O B R O  # " + ReferenciaSAP.ToString());


                    }
                    else
                    {
                        printer.DrawText("      ORIGINAL");
                        printer.DrawText("");
                        printer.Font = PrinterFont.MAXTITLE;
                        printer.DrawText("R E C I B O  D E  C O B R O  # " + ReferenciaSAP.ToString());
                    }
                }
    



            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            //printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            var fechaValida = DateTime.TryParse(recibo.RecFecha, out DateTime fecha);
            

            printer.DrawText("Fecha Recibo :" + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : recibo.RecFecha));
            //printer.DrawText("RECIBO #     :" + ReferenciaSAP.ToString());
            printer.DrawText("Cod. Cliente :" + recibo.CliCodigo);
            printer.DrawText("Cliente      :" + recibo.CliNombre, 48);
            printer.DrawText("RNC/Cedula   :" + recibo.CliRNC);

            printer.DrawText("");
            printer.DrawLine();


            if (!reciboDiferido.RefIndicadorDiferido)
            {
                printer.DrawText("Documentos Aplicados:");
                printer.DrawText("Doc.         Documento                   Monto");
                printer.DrawText("");
                printer.DrawLine();
            

                    //foreach documentos

                    double TotalBruto = 0;
                    double totalNeto = 0;
                    double TotalDesc = 0;
                    double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

                    foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
                    {

                        var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                        double valorNC = 0;
                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            valorNC += nc.RecValor;
                        }

                        string sigla = app.CxcSigla;

                        if (sigla == "FT" || sigla == "FAT" || sigla == "FC")
                        {
                            sigla = "FACT";
                        }

                        double valorBruto = app.RecValor + app.RecDescuento + valorNC;
                        string cxcNCF = app.CXCReferencia;
                        var CxC = myRec.GetCxCNCFByReference(app.CXCReferencia).FirstOrDefault();
                        if (CxC != null)
                        {
                            if (!string.IsNullOrWhiteSpace(CxC.CXCNCF))
                            {
                                cxcNCF = CxC.CXCNCF;
                            }
                        }
                        printer.DrawText(sigla.PadRight(13) + (app.CxCDocumento + "/" + (CxC != null ? CxC.cxcComentario : "") + (app.RecIndicadorSaldo ? "" : "  ABONO")).PadRight(21) + ("$" + (valorBruto).ToString("N2")).PadLeft(13));

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + ("$" + (nc.RecValor).ToString("N2")).PadLeft(13));
                        }

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }


                printer.DrawLine();
                printer.DrawText("");
                ValorNC = Math.Abs(ValorNC) * -1;
                var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
                double TotalCobrado = recibosFormasPagos.Sum(r => r.RefValor);
                //printer.DrawText("Diferencia : " + recibo.RecMontoSobrante.ToString("N2").PadLeft(32));
                //printer.DrawText("Total :     " + ("$" + TotalCobrado.ToString("N2")).PadLeft(33));
                //printer.DrawText("");


                printer.DrawText("Formas de pago");
                printer.DrawText("Doc. No.     Banco                      Valor");
                printer.DrawText("");
                printer.DrawLine();
                foreach (RecibosFormaPago rec in recibosFormasPagos)
                {
                        printer.DrawText("CH ".PadRight(5) + rec.RefNumeroCheque.ToString().PadRight(8) + rec.BanNombre.ToString().PadRight(20) + ("$" + (rec.RefValor).ToString("N2")).PadLeft(14));
                }
                printer.DrawLine();
                printer.DrawText("");

                printer.DrawText("Total :     " + ("$" + TotalCobrado.ToString("N2")).PadLeft(33));
            }
            else
            {
                

                var recibosFormasPagos = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
                double TotalCobrado = recibosFormasPagos.Sum(r => r.RefValor);

                printer.DrawText("");


                printer.DrawText("Detalle");
                printer.DrawText("Doc. No.     Banco                      Valor");
                printer.DrawText("");
                printer.DrawLine();
                foreach (RecibosFormaPago rec in recibosFormasPagos)
                {

                        printer.DrawText("CH ".PadRight(5) + rec.RefNumeroCheque.ToString().PadRight(8) + rec.BanNombre.ToString().PadRight(20) + ("$" + (rec.RefValor).ToString("N2")).PadLeft(14));

                    

                }
                printer.DrawLine();
                printer.DrawText("");

                //printer.DrawText("Diferencia : " + recibo.RecMontoSobrante.ToString("N2").PadLeft(34));
                printer.DrawText("Total :     " + ("$" + TotalCobrado.ToString("N2")).PadLeft(35));
            }
            

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            
            printer.DrawText("");
            printer.DrawText("");


            //string rutaVendedor = myrep.GetRepNombre(recibo.CliRepCodigo);  
            string rutaVendedor = Arguments.CurrentUser.RepNombre.ToString();
            rutaVendedor = rutaVendedor.CenterText(48);
            printer.DrawText("Firma Vendedor:________________________________");
            printer.DrawText(rutaVendedor);
            printer.DrawText("");
            printer.Font = PrinterFont.MINTITLE;
            printer.DrawText("Formato recibos 22: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion


        private void Formato39(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "RECIBO");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("      Documentos aplicados      ");
            printer.DrawText("--------------------------------");
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento       Monto");
            printer.DrawText("Descuento                   Neto");
            printer.DrawText("--------------------------------");

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(9) + (valorBruto).ToString("N2").PadLeft(12));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(10) + (nc.RecValor).ToString("N2").PadLeft(5));
                }

                printer.DrawText(app.RecDescuento.ToString("N2") + (app.RecValor).ToString("N2").PadLeft(28));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawText("--------------------------------");

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(16));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(16));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(16));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(16));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(16));
            }
            printer.DrawText("Total pagado:   " + totalNeto.ToString("N2").PadLeft(16));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawText("--------------------------------");
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 24) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(16) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + " Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 24) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(16) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha   : " + rec.RefFecha);
                            printer.DrawText("Banco : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(15));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }
                            break;
                    }
                    printer.DrawText("--------------------------------");
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(17));
            printer.DrawText("--------------------------------");
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawText("--------------------------------");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawText("--------------------------------");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 39: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        } 

        private void Formato40(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("You don't have the printer configured.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                //printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");

                printer.DrawText(formaPago.ForID == 18 ? "D O C U M E N T S  R E C E P T I O N" : "R E C E I P T");
            }
            else { printer.DrawText("R E C E I P T"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("C A N C E L E D");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P Y");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Receipt: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Date: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Code: " + recibo.CliCodigo);
            printer.DrawText("Customer: " + recibo.CliNombre, 48);
            printer.DrawText("Applied documents");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            //printer.DrawText("TX         Documento               Monto");
            //printer.DrawText("Descuento               Neto");
            printer.DrawText("TX         Document                Amount");
            printer.DrawText("Discount                Net worth");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Gross value:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Credit notes value:       " + ValorNC.ToString("N2").PadLeft(19));
            printer.DrawText("Discount value:     " + TotalDesc.ToString("N2").PadLeft(25));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Advance Value: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Residual value: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total to pay:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Payment Methods");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

                switch (rec.ForID)
                {
                    case 1:
                        printer.DrawText("Cash: " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 2:
                        printer.DrawText("Check " + (rec.RefIndicadorDiferido ? "deferred" : "normal") + "  Number: " + rec.RefNumeroCheque.ToString());
                        printer.DrawText("Bank   : " + rec.BanNombre, 48);
                        printer.DrawText("Amount   : " + rec.RefValor.ToString("N2").PadLeft(34));

                        if (rec.RefIndicadorDiferido)
                        {
                            printer.DrawText("Date: " + rec.RefFecha);
                        }
                        break;
                    case 4:
                        printer.DrawText("Transfer: " + rec.RefNumeroCheque);
                        printer.DrawText("Date   : " + rec.RefFecha);
                        printer.DrawText("Bank   : " + rec.BanNombre, 48);
                        printer.DrawText("Amount   : " + rec.RefValor.ToString("N2").PadLeft(35));
                        break;
                    case 5:
                        printer.DrawText("Retention: " + rec.RefValor.ToString("N2").PadLeft(34));
                        break;
                    case 6:
                        printer.DrawText("Credit card: " + rec.RefValor.ToString("N2").PadLeft(28));
                        break;
                    case 18:
                        printer.DrawText("Pay order: " + rec.RefNumeroCheque);
                        printer.DrawText("Date   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Amount   : " + rec.RefValor.ToString("N2").PadLeft(34));
                        break;
                }
                printer.DrawLine();
            }

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
                printer.DrawText("");
            }

            printer.DrawText("Total charged: " + TotalCobrado.ToString("N2").PadLeft(30));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Customer's signature: ");
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Seller's signature:");
            printer.DrawLine();
            printer.DrawText("Printing Date: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTE: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Seller: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Phone: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            //printer.DrawText("Explanatory legend of the invoices");
            //printer.DrawText("S : full paid invoice");
            //printer.DrawText("AB: installment");
            //printer.DrawText("CN: credit note applied");
            //printer.DrawText("CK: invoice for returned check");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Receipts format 40: MovilBusiness v" + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }


        private void Formato13(int recSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, confirmado);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            /*if (monedas.Count > 1)
            {
                printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
            }*/
            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento  Neto                    Balance");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, confirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, confirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, confirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                double SumatoriaNC = 0;
                double DescuentoNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    SumatoriaNC += nc.RecValor;
                    DescuentoNC += nc.RecDescuento;
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                double cxcBalance = app.CxcBalance - app.RecValor - app.RecDescuento - SumatoriaNC - DescuentoNC;
                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(11) + (app.RecValor).ToString("N2").PadRight(21) + cxcBalance.ToString("N2").PadLeft(13));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
                cxcBalance = 0;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;

            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, confirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.Bold = true;
                                printer.Font = PrinterFont.TITLE;
                            }

                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.Bold = false;
                                printer.Font = PrinterFont.BODY;
                                printer.DrawText("");

                            }
                                
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            if (rec.RefIndicadorDiferido)
                            {
                                printer.Bold = true;
                                printer.Font = PrinterFont.TITLE;
                                printer.DrawText("Fecha: " + rec.RefFecha);
                                printer.Bold = false;
                                printer.Font = PrinterFont.BODY;
                                printer.DrawText("");
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 13: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }


        //Fraga Industrial
        private void Formato14(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia2(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = 0.00;
                double valorNeto = 0.00;
                double RecValor = 0.00;
                if (app.MonCodigo == "RD$" && recibo.MonCodigo == "USD" && app.RecIndicadorSaldo)
                {
                    RecValor = (app.RecValor * recibo.RecTasa) / app.RecTasa;
                    valorBruto = RecValor + app.RecDescuento + valorNC;
                    valorNeto = RecValor;
                }
                else
                {
                    RecValor = app.RecValor;
                    valorBruto = RecValor + app.RecDescuento + valorNC;
                    valorNeto = RecValor;
                }
                //double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);
                }

                printer.DrawText(app.RecDescuento.ToString("N2") + " " + recibo.MonSigla.PadRight(17) + (valorNeto).ToString("N2") + " " + recibo.MonSigla);

                totalNeto += RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(26) + " " + recibo.MonSigla);
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(26) + " " + recibo.MonSigla);
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(26) + " " + recibo.MonSigla);
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(26) + " " + recibo.MonSigla);
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(26) + " " + recibo.MonSigla);
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(27) + " " + recibo.MonSigla);
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    //TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 6 : 31) + " " + recibo.MonSigla);
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(29) + " " + recibo.MonSigla);
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 6 : 31) + " " + recibo.MonSigla);
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }


                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + " " + recibo.MonSigla);
                            }

                            break;
                        case 5:
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + " " + recibo.MonSigla);
                            }
                            break;
                        //case 8:
                        //    printer.DrawText("Diferencia Cambiaria    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 16) + " " + recibo.MonSigla);
                        //    if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                        //    {
                        //        printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + " " + recibo.MonSigla);
                        //    }
                        //    break;
                        //case 9:
                        //    printer.DrawText("Redondeo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 28) + " " + recibo.MonSigla);
                        //    if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                        //    {
                        //        printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + " " + recibo.MonSigla);
                        //    }
                        //    break;
                        case 18:
                            TotalCobrado += rec.RefValor;
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(27) + " " + recibo.MonSigla);
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 14: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato17(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("");
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX              Documento               Monto");
            printer.DrawText("Descuento       Descarga                Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double TotalDesmonte = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionConDesmonteBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + app.RecDescuentoDesmonte + valorNC;

                printer.DrawText(sigla.PadRight(16) + app.CxCDocumento.PadRight(16) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(16) + nc.CxCDocumento.PadRight(16) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(16) + (app.RecDescuentoDesmonte).ToString("N2").PadRight(16) + app.RecValor.ToString("N2").PadLeft(13));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
                TotalDesmonte += app.RecDescuentoDesmonte;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            printer.DrawText("Valor descarga: " + TotalDesmonte.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(29));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado))
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 17: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato44(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa();
            printer.Bold = true;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;




            if (formaPago != null)
            {
                string textoImpresion = recibo.RecCantidadImpresion == 0 ? "ORIGINAL" : "COPIA";
                string textoFormaPago = formaPago.ForID == 18 ? "RECEPCION DE DOCUMENTOS" : "RECIBO";
                string textoFinal = $"{textoFormaPago} - {textoImpresion}";
                printer.DrawText(textoFinal);
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    //if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    //{
                    //    printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    //}
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("      Documentos aplicados      ");
            printer.DrawText("--------------------------------");
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento       Monto");
            printer.DrawText("Descuento                   Neto");
            printer.DrawText("--------------------------------");

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(9) + (valorBruto).ToString("N2").PadLeft(12));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(10) + (nc.RecValor).ToString("N2").PadLeft(5));
                }

                printer.DrawText(app.RecDescuento.ToString("N2") + (app.RecValor).ToString("N2").PadLeft(28));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawText("--------------------------------");
            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(16));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(16));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(16));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(16));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(16));
            }
            printer.DrawText("Total pagado:   " + totalNeto.ToString("N2").PadLeft(16));
            printer.DrawText("Formas de pago");
            printer.DrawText("--------------------------------");
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 19) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(16) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + " Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 24) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(16) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha   : " + rec.RefFecha);
                            printer.DrawText("Banco : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(15));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 21) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(15) + "");
                            }
                            break;
                    }
                    printer.DrawText("--------------------------------");
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(17));
            printer.DrawText("--------------------------------");
            printer.DrawText("Firma vendedor:");
            printer.DrawText("--------------------------------");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }

            string vendedor = Arguments.CurrentUser.RepNombre;
            if (vendedor.Length > 18)
            {
                vendedor = vendedor.Substring(0, 18);
            }
            printer.DrawText("Vendedor: " + vendedor);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.Font = PrinterFont.FOOTER;
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 44 ");
            printer.DrawText("Movilbusiness" + Functions.AppVersion);
            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }



        #region FoodSmart

        private void Formato41(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
            {
                printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
            printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
            printer.DrawText("Codigo: " + recibo.CliCodigo);
            printer.DrawText("Cliente: " + recibo.CliNombre, 48);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(13));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total a Pagar:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;
            var formasPago = myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, reciboConfirmado);
            foreach (RecibosFormaPago rec in formasPago)
            {
                TotalCobrado += rec.RefValor;

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
                    case 18:
                        printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                        printer.DrawText("Fecha   : " + rec.RefFecha);
                        printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                        printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
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

            string nota = "";
            bool parNotaPorFormaPago = DS_RepresentantesParametros.GetInstance().GetParRecibosNotasPorFormaPago();
            if (parNotaPorFormaPago)
            {
                var fp = formasPago.FirstOrDefault();
                if(fp != null)
                {
                    nota = myTitRepNot.GetNotaXTipoTransaccionReporte(3, fp.ForID);
                }
            }
            else
            {
                nota = myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos());
            }
            if (!string.IsNullOrWhiteSpace(nota))
            {
                printer.DrawText("NOTA: " + nota, 42);
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            // printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 41: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        #endregion

        private void Formato42(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
            if (recibo == null)
            {
                return;
            }

            printer.PrintEmpresa(length2: 31, length1: 31);
            printer.Bold = true;
            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.TITLE;
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "    RECEPCION DE DOCUMENTOS    " : "            RECIBO            ");
            }
            else { printer.DrawText("          R E C I B O          "); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("         A N U L A D O         ");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.LEFT;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "        O R I G I N A L        " : "           C O P I A           ");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 31);
                    printer.DrawText("Propietario: " + visipres.VisPropietario, 31);
                    printer.DrawText("Contact: " + visipres.VisContacto, 31);
                    printer.DrawText("Email: " + visipres.VisEmail, 31);
                    printer.DrawText("Calle: " + visipres.VisCalle, 31);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 31);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 31);
                    printer.DrawText("RNC: " + visipres.VisRNC, 31);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.LEFT;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "        O R I G I N A L        " : "           C O P I A           ");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 31);
                    break;
            }

            if (DS_RepresentantesParametros.GetInstance().GetParRecibosImpresionTasa())
            {
                printer.DrawText($"Tasa {recibo.MonSigla}: {recibo.RecTasa.ToString("N2")}");
            }
            else
            {
                if (monedas.Count > 1)
                {
                    printer.DrawText("Moneda:" + recibo.MonSigla + "   Tasa: " + recibo.RecTasa);
                }
            }
            printer.DrawText("      Documentos aplicados     ");
            printer.DrawText("-------------------------------");
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento      Monto");
            printer.DrawText("Descuento                  Neto");
            printer.DrawText("-------------------------------");

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(9) + (valorBruto).ToString("N2").PadLeft(11));

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(10) + (nc.RecValor).ToString("N2").PadLeft(5));
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(10) + (app.RecValor).ToString("N2").PadLeft(21));

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawText("--------------------------------");

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(15));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(15));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(15));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(15));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(15));
            }
            printer.DrawText("Total pagado:   " + totalNeto.ToString("N2").PadLeft(15));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawText("--------------------------------");
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:

                            printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(21));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                printer.DrawText("Tasa: " + rec.RecTasa.ToString("N2")) ;
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(16));
                            }
                            break;

                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + " Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco : " + rec.BanNombre, 31);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(23));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)

                                printer.DrawText("Tasa: " + rec.RecTasa.ToString("N2"));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(21));
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;

                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha   : " + rec.RefFecha);
                            printer.DrawText("Banco : " + rec.BanNombre, 31);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(23));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                printer.DrawText("Tasa: " + rec.RecTasa.ToString("N2"));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(21));
                            }
                            break;

                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(20));
                            break;

                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(19));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                printer.DrawText("Tasa: " + rec.RecTasa.ToString("N2"));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(21));
                            }
                            break;

                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque.PadLeft(16));
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 31);
                            printer.DrawText("Monto : " + rec.RefValor.ToString("N2").PadLeft(23));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                                printer.DrawText("Tasa: " + rec.RecTasa.ToString("N2"));

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(21));
                            }
                            break;
                    }
                    printer.DrawText("--------------------------------");
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(16));
            printer.DrawText("--------------------------------");
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawText("--------------------------------");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawText("--------------------------------");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 42: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato43(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var monedaRecibos = myRec.MonedasGetTasaFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O");
            }
            else { printer.DrawText("R E C I B O"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            printer.DrawText("Moneda:" + monedaRecibos.MonCodigo + "   Tasa: " + monedaRecibos.MonTasa);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2") + " " + recibo.MonSigla);

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }


                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 43: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }

        private void Formato18(int recSecuencia, bool reciboConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Recibos recibo = myRec.GetReciboBySecuencia(recSecuencia, reciboConfirmado);
            RecibosFormaPago formaPago = myRec.GetForID(recSecuencia, recibo.RecTipo);
            var monedas = myRec.GetMonedasFromRecibos(recSecuencia, reciboConfirmado);
            var monedaRecibos = myRec.MonedasGetTasaFromRecibos(recSecuencia, reciboConfirmado);
            var visipres = myvisit.GetClientePresentacion(recibo.VisSecuencia);
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
            if (formaPago != null)
            {
                printer.DrawText(formaPago.ForID == 18 ? "R E C E P C I O N  D E  D O C U M E N T O S" : "R E C I B O  P R O V I S I O N A L");
            }
            else { printer.DrawText("R E C I B O  P R O V I S I O N A L"); }
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            if (recibo.RecEstatus == 0)
            {
                printer.DrawText("A N U L A D O");
            }

            switch (recibo.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }

                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Bold = false;
                    printer.TextAlign = Justification.CENTER;
                    if (!DS_RepresentantesParametros.GetInstance().GetParEliminarCopiasYOriginal())
                    {
                        printer.DrawText(recibo.RecCantidadImpresion == 0 ? "O R I G I N A L" : "C O P I A");
                    }
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    printer.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    printer.DrawText("Codigo: " + recibo.CliCodigo);
                    printer.DrawText("Cliente: " + recibo.CliNombre, 48);
                    break;
            }

            printer.DrawText("Moneda:" + monedaRecibos.MonCodigo + "   Tasa: " + monedaRecibos.MonTasa);
            printer.DrawText("Documentos aplicados");
            printer.DrawLine();
            //"       Cantidad       Codigo            Precio"
            printer.DrawText("TX         Documento               Monto");
            printer.DrawText("Descuento               Neto");
            printer.DrawLine();

            //foreach documentos

            double TotalBruto = 0;
            double totalNeto = 0;
            double TotalDesc = 0;
            double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, reciboConfirmado);

            foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionBySecuencia(recSecuencia, reciboConfirmado))
            {

                var notasCredito = myRec.GetNotasCreditosAplicadasByRecibo(app.CXCReferencia, recSecuencia, reciboConfirmado);
                double valorNC = 0;
                foreach (RecibosAplicacion nc in notasCredito)
                {
                    valorNC += nc.RecValor;
                }

                string sigla = app.CxcSigla;

                if (sigla == "FT" || sigla == "FAT")
                {
                    sigla += app.RecIndicadorSaldo ? "-S" : "-AB";
                }

                double valorBruto = app.RecValor + app.RecDescuento + valorNC;

                printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);

                foreach (RecibosAplicacion nc in notasCredito)
                {
                    printer.DrawText(nc.CxcSigla.PadRight(11) + nc.CxCDocumento.PadRight(21) + (nc.RecValor).ToString("N2").PadLeft(10) + " " + recibo.MonSigla);
                }

                printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2") + " " + recibo.MonSigla);

                totalNeto += app.RecValor;
                TotalBruto += valorBruto;
                TotalDesc += app.RecDescuento;
            }

            printer.DrawLine();

            //  printer.DrawText("");
            printer.DrawText("Valor bruto:    " + TotalBruto.ToString("N2").PadLeft(29));
            printer.DrawText("Valor NC:       " + ValorNC.ToString("N2").PadLeft(29));
            printer.DrawText("Valor desc:     " + TotalDesc.ToString("N2").PadLeft(29));
            if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
            {
                printer.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            else
            {
                printer.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
            }
            printer.DrawText("Total pagado:  " + totalNeto.ToString("N2").PadLeft(30));
            printer.DrawText("");
            printer.DrawText("Formas de pago");
            printer.DrawLine();
            //foreach formas pago
            double TotalCobrado = 0;



            foreach (var moneda in monedas)
            {

                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuenciaYmonedas(recSecuencia, reciboConfirmado, monedas.Count > 1 ? moneda.MonCodigo : null))
                {
                    printer.DrawText(moneda.MonNombre + "-" + moneda.MonCodigo);
                    //TotalCobrado += rec.RecPrima;
                    var Conversion = (rec.RecTasa / recibo.RecTasa) * rec.RefValor;

                    TotalCobrado += monedas.Count > 1 ? Conversion : rec.RecPrima;
                    switch (rec.ForID)
                    {
                        case 1:
                            //    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));

                            printer.DrawText("Efectivo    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 31) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 2:
                            printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }

                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            if (rec.RefIndicadorDiferido)
                            {
                                printer.DrawText("Fecha: " + rec.RefFecha);
                            }
                            break;
                        case 4:
                            printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("Banco    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }


                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }

                            break;
                        case 5:
                            printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                            break;
                        case 6:
                            printer.DrawText("T. crédito: " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 0 : 35) + " " + (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1 ? "Tasa: " + rec.RecTasa.ToString("N2") + "" : null));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha    : " + rec.RefFecha);
                            printer.DrawText("COOP.    : " + rec.BanNombre, 48);
                            printer.DrawText("Monto    : " + rec.RefValor.ToString("N2").PadLeft(moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1 ? 10 : 35));
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count == 1)
                            {
                                printer.DrawText("Tasa     : " + rec.RecTasa.ToString("N2"));
                            }
                            if (moneda.MonCodigo != recibo.MonCodigo && monedas.Count > 1)
                            {
                                printer.DrawText("Monto " + recibo.MonCodigo + "   :" + Conversion.ToString("N2").PadLeft(33) + "");
                            }
                            break;
                    }
                    printer.DrawLine();
                }

                if (!printer.IsEscPos)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                }
            }
            printer.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(31));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yy HH:mm"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(3, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos()));
                printer.DrawText("");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativa de las facturas");
            printer.DrawText("S : factura pagada completa");
            printer.DrawText("AB: abono");
            printer.DrawText("NC: nota de credito aplicada");
            printer.DrawText("CK: factura por cheque devuelto");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato recibos 18: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myRec.ActualizarCantidadImpresion(recibo.rowguid);

            printer.Print();
        }
        
    }
}

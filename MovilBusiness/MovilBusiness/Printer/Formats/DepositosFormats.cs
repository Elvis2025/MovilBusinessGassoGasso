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
using System.Globalization;
using System.Linq;

namespace MovilBusiness.printers.formats
{
    public class DepositosFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Depositos myDep;
        private DS_Recibos myRec;
        private DS_Monedas myMon;
        private DS_Ventas myVent;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        public DepositosFormats(DS_Depositos myDep)
        {
            this.myDep = myDep;
            myRec = new DS_Recibos();
            myVent = new DS_Ventas();
            myMon = new DS_Monedas();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public void Print(int depSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            //Copias = copias;
            this.printer = printer;

            myRec.DeletePairValuesInRecibos();

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos())
            {
                case 1:
                case 8:
                default: // UyC / sanofi
                    Formato1(depSecuencia, confirmado);
                    break;
                case 2: //LeBetances
                    Formato2(depSecuencia, confirmado);
                    break;
                case 3: //Disfarmaco
                    Formato3(depSecuencia, confirmado);
                    break;
                case 4: //TUCAN
                    Formato4(depSecuencia, confirmado);
                    break;
                case 5: //NUTRICIOSA
                    Formato5(depSecuencia, confirmado);
                    break;
                case 6: //feltrex
                    Formato6(depSecuencia, confirmado);
                    break;
                case 7: //LAM
                    Formato7(depSecuencia, confirmado);
                    break;
                case 9: //Feltrex - Zebra
                    Formato9(depSecuencia, confirmado);
                    break;
                case 10:
                    Formato10(depSecuencia, confirmado);
                    break;
                case 11: //SUED
                    Formato11(depSecuencia, confirmado);
                    break;
                case 22: //Canon
                    Formato22(depSecuencia, confirmado);
                    break;
                case 23: //GASSO
                    Formato23(depSecuencia, confirmado);
                    break;
            }
        }

        //public void PrintFacturas(int depSecuencia, bool confirmado, PrinterManager printer)
        //{
        //    if (printer == null || !printer.IsConnectionAvailable)
        //    {
        //        throw new Exception("Error conectando con la impresora");
        //    }

        //    Depositos deposito = myDep.GetDepositobySecuenciaOrdenPago(depSecuencia, confirmado);

        //    if (deposito == null)
        //    {
        //        throw new Exception("Error cargando datos del deposito");
        //    }

        //    printer.PrintEmpresa();

        //    printer.DrawText("");
        //    printer.DrawText("");
        //    printer.Bold = true;
        //    printer.Font = PrinterFont.TITLE;
        //    printer.TextAlign = Justification.CENTER;
        //    printer.DrawText("ENTREGA DE FACTURAS A CREDITO");
        //    printer.TextAlign = Justification.LEFT;
        //    printer.DrawText("");
        //    printer.DrawText("");
        //    printer.Bold = false;
        //    printer.Font = PrinterFont.BODY;
        //    printer.DrawText("Fecha entrega: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
        //    printer.DrawText("");
        //    printer.DrawText("Entrega           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
        //    printer.DrawText("");
        //    printer.DrawLine();
        //    printer.DrawText("Factura     Sec.  Cliente   Descuento  Monto");
        //    printer.DrawLine();

        //    double Totalfacturascredito = 0.0;
        //    foreach (Ventas ventas in myVent.GetVentasaCreditoByfecha())
        //    {
        //        double Descuento = 0.0;
        //        string Facturas = "";
        //        Facturas = ventas.VenNCF;


        //        Totalfacturascredito += ventas.VenTotal;
        //        printer.DrawText(Facturas.PadRight(12) + ventas.VenSecuencia.ToString().PadRight(6) +
        //        ventas.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(5) + ventas.VenTotal.ToString("N2").PadLeft(10));

        //    }

        //    if (Totalfacturascredito > 0)
        //    {
        //        printer.DrawText("Total Facturas a Credito: ".PadRight(33) + ("RD$" + Totalfacturascredito.ToString("N", new CultureInfo("en-US"))).PadLeft(9));
        //        printer.DrawText("");
        //    }
        //    else
        //    {
        //        printer.DrawText("-No hay Facturas a credito-".CenterText(48));
        //        printer.DrawText("");
        //    }

        //    printer.DrawText("");
        //    printer.DrawText("");
        //    printer.DrawText("");
        //    printer.TextAlign = Justification.LEFT;
        //    printer.DrawText("Firma vendedor:");
        //    printer.DrawLine();
        //    printer.DrawText("");
        //    printer.DrawText("");
        //    printer.Font = PrinterFont.FOOTER;
        //    printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
        //    printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
        //    printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
        //    printer.DrawText("Formato deposito 5: movilbusiness " + Functions.AppVersion);
        //    printer.DrawText("");

        //    printer.Print();
        //}

        private void Formato1(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Fecha deposito: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
            printer.DrawText("");
            printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero          Cliente                  Monto");
            printer.DrawLine();

            var recibos = myRec.GetRecibosByDeposito(depSecuencia);
            //int num = recibos.Max(c => c.CliNombre.Length);
            //int clinombremax = num > 25 ? 25 : num;

            //foreach
            foreach (Recibos recibo in recibos)
            {
                var codigo_Cliente = recibo.CliCodigo + '-' + recibo.CliNombre;
                var clinombre = codigo_Cliente.Length > 25 ? codigo_Cliente.Substring(0,25) : codigo_Cliente;

                /*while(clinombre.Length < clinombremax)
                {
                    clinombre += " ";
                }*/

                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(29) + recibo.RecTotal.ToString("N2").PadRight(10));
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {
                    var codigo_Cliente = anulados.CliCodigo + '-' + anulados.CliNombre;
                    var clinombre = codigo_Cliente.Length > 28 ? codigo_Cliente.Substring(0, 28) : codigo_Cliente;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(29) + anulados.RecTotal.ToString("N2").PadRight(10));
                }
            }
            
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 1: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }
        private void Formato10(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Fecha deposito: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
            printer.DrawText("");
            printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(6) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero          Cliente                 Monto");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(9));
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + anulados.RecTotal.ToString("N2").PadRight(9));
                }
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 10: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato22(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            var mondep = myMon.GetMonedaByMonCodForDep(deposito.MonCodigo);
            

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;

            printer.DrawText("Fecha deposito: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
            printer.DrawText("");
            if(mondep != null)
            {
                printer.DrawText("Moneda             :".PadRight(42) + mondep.MonSigla);
            }
            printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero          Cliente                Monto");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(10));
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + anulados.RecTotal.ToString("N2").PadRight(10));
                }
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 1: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato2(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Tipo: " + deposito.DepTipoDescripcion);
            printer.DrawText("");
            printer.DrawText("Fecha deposito:      " + deposito.DepFecha);
            printer.DrawText("Secuencia #:         " + Arguments.CurrentUser.RepCodigo + " - " + depSecuencia.ToString());
            printer.DrawText("Vendedor:            " + deposito.RepCodigo);
            printer.DrawText("");
            printer.DrawText("_________________________________________________________");
            printer.DrawText("Cant. recibos:       " + deposito.DepCantidadRecibos.ToString().PadLeft(20));
            printer.DrawText("Cant. cheques:       " + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(20));
            printer.DrawText("Total efectivo:      " + deposito.DepMontoEfectivo.ToString("N2").PadLeft(20));
            printer.DrawText("Total cheques:       " + deposito.DepMontoCheque.ToString("N2").PadLeft(20));
            printer.DrawText("Total cheques fut:   " + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(20));
            printer.DrawText("Total transferencia: " + deposito.DepMontoTransferencia.ToString("N2").PadLeft(20));
            printer.DrawText("Total tarj.credito:  " + deposito.DepMontoTarjeta.ToString("N2").PadLeft(20));

            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoTransferencia + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito:      " + totalDeposito.ToString("N2").PadLeft(20));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero              Monto");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(20) + recibo.RecTotal.ToString("N2").PadRight(14) + (recibo.RecEstatus == 0 ? " (Anulado)" : ""));
            }

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato depositos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato3(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Fecha deposito: " + deposito.DepFecha);
            printer.DrawText("");
            printer.DrawText("Deposito:            " + Arguments.CurrentUser.RepCodigo + " - " + depSecuencia.ToString());
            printer.DrawText("Cant. recibos:       " + deposito.DepCantidadRecibos.ToString());
            printer.DrawText("Cant. cheques:       " + myDep.GetCantidadChequesDepositados(depSecuencia).ToString());
            printer.DrawText("Total efectivo:      " + deposito.DepMontoEfectivo.ToString("N2"));
            printer.DrawText("Total cheques:       " + deposito.DepMontoCheque.ToString("N2"));
            printer.DrawText("Total cheques fut:   " + deposito.DepMontoChequeDiferido.ToString("N2"));
            printer.DrawText("Total transferencia: " + deposito.DepMontoTransferencia.ToString("N2"));
            printer.DrawText("Total tarj.credito:  " + deposito.DepMontoTarjeta.ToString("N2"));
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoTransferencia + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito:      " + totalDeposito.ToString("N2"));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero          Cliente                Monto");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(10));
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + anulados.RecTotal.ToString("N2").PadRight(10));
                }
            }



            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 3: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato4(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuenciaOrdenPago(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            if (deposito.DepTipo == 1)
            {
                printer.DrawText("DEPOSITO A BANCO");
            }
            else
            {
                printer.DrawText("DEPOSITO A CAJA");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Fecha deposito: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
            printer.DrawText("");
            printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            if (DS_RepresentantesParametros.GetInstance().GetParDepositosOrdenPago() && deposito.DepMontoOrdenPago > 0)
            {
                printer.DrawText("Total orden pago   :".PadRight(35) + deposito.DepMontoOrdenPago.ToString("N2").PadLeft(10));
            }
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido + deposito.DepMontoOrdenPago;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Numero          Cliente                Monto");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(10));
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + anulados.RecTotal.ToString("N2").PadRight(10));
                }
            }



            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 4: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato5(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuenciaOrdenPago(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE DEPOSITOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Fecha deposito: ".PadRight(4) + deposito.DepFecha.PadLeft(4));
            printer.DrawText("");
            printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            if (DS_RepresentantesParametros.GetInstance().GetParDepositosOrdenPago() && deposito.DepMontoOrdenPago > 0)
            {
                printer.DrawText("Total orden pago   :".PadRight(35) + deposito.DepMontoOrdenPago.ToString("N2").PadLeft(10));
            }
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido + deposito.DepMontoOrdenPago;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("Factura     Rec.  Cliente   Descuento  Monto");
            printer.DrawLine();

            //foreach
            double Totalreciboscontado = 0.0;
            double Totalreciboscredito = 0.0;
            //double Totalfacturascredito = 0.0;
            printer.DrawText("RECIBOS CONTADO:");
            printer.DrawLine();
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                double Descuento = 0.0;
                //var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                List<RecibosAplicacion> FacturasyDescuentos = myRec.GetRecibosFacturasByDeposito(recibo.RecSecuencia);
                string Facturas = "";
                foreach(var recap in FacturasyDescuentos)
                {
                    if (String.IsNullOrEmpty(Facturas) || String.IsNullOrWhiteSpace(Facturas))
                    {
                        Facturas = recap.CXCReferencia;
                    }
                    else
                    {
                       // Facturas = Facturas;//+ " | " + recap.CXCReferencia;
                    }
                    Descuento += recap.RecDescuento;
                }
                //printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(10));

                if (recibo.RecTipo == "1")
                {
                    Totalreciboscontado += recibo.RecTotal;
                    printer.DrawText(Facturas.PadRight(12) + recibo.RecSecuencia.ToString().PadRight(6) +
                    recibo.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(5) + recibo.RecTotal.ToString("N2").PadLeft(10));
                }

            }
            if(Totalreciboscontado > 0)
            {
                printer.DrawText("Total Recibos Contado: ".PadRight(33) + ("RD$" + Totalreciboscontado.ToString("N", new CultureInfo("en-US"))).PadLeft(10));
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("-No hay recibos a contado-".CenterText(48));
                printer.DrawText("");
            }

            printer.DrawText("RECIBOS CREDITO:");
            printer.DrawLine();
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {    
                double Descuento = 0.0;
                //var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                List<RecibosAplicacion> FacturasyDescuentos = myRec.GetRecibosFacturasByDeposito(recibo.RecSecuencia);
                string Facturas = "";
                foreach (var recap in FacturasyDescuentos)
                {
                    if (String.IsNullOrEmpty(Facturas) || String.IsNullOrWhiteSpace(Facturas))
                    {
                        if (recap.CXCReferencia.Contains("B"))
                        {
                            Facturas = recap.CXCReferencia;
                        }
                        else
                        {
                            Facturas = recap.CxCDocumento;
                        }
                    }
                    else
                    {
                        ///Facturas = Facturas; //+ " | " + recap.CXCReferencia;
                    }
                    Descuento += recap.RecDescuento;
                }
                //printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadRight(10));

                if (recibo.RecTipo == "2")
                {
                    Totalreciboscredito += recibo.RecTotal;
                    printer.DrawText(Facturas.PadRight(12) + recibo.RecSecuencia.ToString().PadRight(6) +
                    recibo.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(5) + recibo.RecTotal.ToString("N2").PadLeft(10));
                }

            }

            if (Totalreciboscredito > 0)
            {
                printer.DrawText("Total Recibos Credito: ".PadRight(33) + ("RD$" + Totalreciboscredito.ToString("N", new CultureInfo("en-US"))).PadLeft(10));
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("-No hay recibos a credito-".CenterText(48));
                printer.DrawText("");
            }

            

            double TotalRecibosAnulados = 0.0;
            printer.DrawText("RECIBOS ANULADOS");
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {
                    double Descuento = 0.0;
                    List<RecibosAplicacion> FacturasyDescuentos = myRec.GetRecibosFacturasByDeposito(anulados.RecSecuencia);
                    string Facturas = "";
                    foreach (var recap in FacturasyDescuentos)
                    {
                        if (String.IsNullOrEmpty(Facturas) || String.IsNullOrWhiteSpace(Facturas))
                        {
                            Facturas = recap.CXCReferencia;
                        }
                        else
                        {
                            Facturas = Facturas + " | " + recap.CXCReferencia;
                        }
                        Descuento += recap.RecDescuento;
                    }

                    TotalRecibosAnulados += anulados.RecTotal;
                    printer.DrawText(Facturas.PadRight(12) + anulados.RecSecuencia.ToString().PadRight(6) +
                    anulados.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(9) + anulados.RecTotal.ToString("N2").PadLeft(10));
                }
            }

            if(TotalRecibosAnulados > 0)
            {
                printer.DrawText("Total Recibos Anulados: ".PadRight(33) + ("RD$" + TotalRecibosAnulados.ToString("N", new CultureInfo("en-US"))).PadLeft(10));
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("-No hay recibos anulados-".CenterText(48));
                printer.DrawText("");
            }

            //printer.DrawText("FACTURAS A CREDITO:");
            //printer.DrawText("Factura     Sec.  Cliente   Descuento  Monto");
            //printer.DrawLine();
            //foreach (Ventas ventas in myVent.GetVentasaCreditoByfecha())
            //{
            //    double Descuento = 0.0;
            //    string Facturas = "";
            //    Facturas = ventas.VenNCF;


            //    Totalfacturascredito += ventas.VenTotal;
            //    printer.DrawText(Facturas.PadRight(12) + ventas.VenSecuencia.ToString().PadRight(6) +
            //    ventas.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(5) + ventas.VenTotal.ToString("N2").PadLeft(10));

            //}

            //if (Totalfacturascredito > 0)
            //{
            //    printer.DrawText("Total Facturas a Credito: ".PadRight(33) + ("RD$" + Totalfacturascredito.ToString("N", new CultureInfo("en-US"))).PadLeft(9));
            //    printer.DrawText("");
            //}
            //else
            //{
            //    printer.DrawText("-No hay Facturas a credito-".CenterText(48));
            //    printer.DrawText("");
            //}

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato depositos 5: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato6(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("E N T R E G A  D E  D E P O S I T O S");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            if (deposito.DepTipo == 1)
            {
                printer.DrawText("Tipo: Banco");
            }
            else
            {
                printer.DrawText("Tipo: Caja General");
            }
            //printer.DrawText("");
            if (DateTime.TryParse(deposito.DepFecha, out DateTime fecha))
            {
            printer.DrawText("Fecha      : " + fecha.ToString("dd/MM/yyyy hh:mm tt"));
            //printer.DrawText("");
            }
            else
            {
            printer.DrawText("Fecha      : " + deposito.DepFecha);
            //printer.DrawText("");
            }
            printer.DrawText("Secuencia #: " + deposito.DepSecuencia);
            //printer.DrawText("");
            printer.DrawText("Vendedor   :" + Arguments.CurrentUser.RepCodigo);
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            
            //printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------------------");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            //printer.DrawText("");
            //printer.DrawText("");
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total Pushmoney    :".PadRight(35) + deposito.DepMontoPushMoney.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            double Retencion = new DS_Depositos().GetRecibosRetencion(depSecuencia);
            printer.DrawText("Total Retencion    :".PadRight(35) + Retencion.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            //printer.DrawText("");
            printer.DrawText("Numero Cliente                            Monto");
            //printer.DrawText("");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadLeft(12));
                //printer.DrawText("");
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(18) + /*clinombre.ToString().PadRight(28) +*/ anulados.RecTotal.ToString("N2").PadLeft(26));
                    //printer.DrawText("");
                }
            }



            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------------------");
            printer.DrawText("Firma vendedor:");       
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            //printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Formato depositos 6: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato7(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }
            printer.PrintEmpresa(Notbold:true);
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("E N T R E G A  D E  D E P O S I T O S");
            printer.DrawText("");
            printer.DrawText("Tipo deposito: " + deposito.DepTipoDescripcion);
            var fechaValida = DateTime.TryParse(deposito.DepFecha, out DateTime fecha);
            printer.DrawText("Fecha      :" + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : deposito.DepFecha));
            printer.DrawText("Secuencia #:"  + deposito.DepSecuencia);
            printer.DrawText("Vendedor   :" + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("");
        
            int cantRecibos = deposito.DepCantidadRecibos;
            int ReciboInicial = deposito.DepReciboInicial;
            int ReciboFinal = deposito.DepReciboFinal;
            int CantidadCheque = 0;
            double TotalEfetivo = deposito.DepMontoEfectivo;
            double TotalCheque = deposito.DepMontoCheque;
            double TotalChequefut = deposito.DepMontoChequeDiferido;
            double TotalDeposito = 0.0;
            double TotalEfetivoTrans = deposito.DepMontoTransferencia;
            double TotalMontoPushMoney = deposito.DepMontoPushMoney;

            string cantRecibos2 = cantRecibos.ToString("N", new CultureInfo("en-US"));
            string ReciboInicial2 = ReciboInicial.ToString("N", new CultureInfo("en-US"));
            string ReciboFinal2 = ReciboFinal.ToString("N", new CultureInfo("en-US"));
            string TotalEfetivo2 = TotalEfetivo.ToString("N", new CultureInfo("en-US"));
            string TotalEfectivoADepositar = (TotalEfetivo - TotalMontoPushMoney).ToString("N", new CultureInfo("en-US"));
            string TotalCheque2 = TotalCheque.ToString("N", new CultureInfo("en-US"));
            string TotalChequefut2 = TotalChequefut.ToString("N", new CultureInfo("en-US"));
            string TotalEfetivoTrans2 = TotalEfetivoTrans.ToString("N", new CultureInfo("en-US"));
            string TotalMontoPushMoney2 = (Math.Abs(TotalMontoPushMoney) * (-1)).ToString("N", new CultureInfo("en-US"));

            DS_CuentasBancarias mycuen = new DS_CuentasBancarias();
            CantidadCheque = mycuen.getCantidadCheque(deposito.DepSecuencia, Arguments.CurrentUser.RepCodigo);

            string CantidadCheque2 = CantidadCheque.ToString("N", new CultureInfo("en-US"));
            TotalDeposito = (TotalEfetivo + TotalCheque + TotalChequefut +
                    TotalEfetivoTrans - TotalMontoPushMoney);

            string TotalDeposito2 = TotalDeposito.ToString("N", new CultureInfo("en-US"));

            printer.DrawLine();
            string cubReferencia = deposito.CuBID.ToString();
            string depReferencia = deposito.DepReferencia;
            printer.DrawText("Cant. Recibos : ".PadRight(30) + cantRecibos.ToString().PadLeft(15));
            printer.DrawText("Recibo Inicial: ".PadRight(30) + deposito.DepReciboInicial.ToString().PadLeft(15));
            printer.DrawText("Recibo Final  : ".PadRight(30) + deposito.DepReciboFinal.ToString().PadLeft(15));
            printer.DrawText("Cant. Cheques : ".PadRight(30) + CantidadCheque2.ToString().PadLeft(15));           
            printer.DrawText("Total Efectivo:   ".PadRight(30) + ("$" + TotalEfetivo2).PadLeft(15));
            printer.DrawText("Total Cheques    :".PadRight(30) + ("$" + TotalCheque2).PadLeft(15));
            printer.DrawText("Total Cheques Fut:".PadRight(30) + ("$" + TotalChequefut2).PadLeft(15));
            printer.DrawText("Total Deposito   :".PadRight(30) + ("$" + TotalDeposito2).PadLeft(15));
            printer.DrawText("Total Transf     :".PadRight(30) + ("$" + TotalEfetivoTrans2).PadLeft(15));
            printer.DrawText("Total tarj.credito:".PadRight(30) + ("$" + deposito.DepMontoTarjeta.ToString("N", new CultureInfo("en-US"))).PadLeft(15));
            printer.DrawText("Total Pushmoney  :".PadRight(30) + ("$" + TotalMontoPushMoney2).PadLeft(15));
            double Retencion = new DS_Depositos().GetRecibosRetencion(depSecuencia);
            printer.DrawText("Total Retencion  :".PadRight(30) + ("$" + Retencion.ToString("N2")).PadLeft(15));
            printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("---------------------------------------------");
            printer.DrawText("Numero    Cliente                       Monto");
            printer.DrawText("---------------------------------------------");
            printer.Bold = false;
            foreach (Recibos recibo in myRec.GetRecibosByDepositoToShowAnulado(depSecuencia))
            {
                string recNumero = "";
                if (recibo.RecEstatus == 0)
                {
                    recNumero = "(*) " + recibo.RecSecuencia;
                }
                else if (recibo.RecEstatus == 5)
                {
                    recNumero = "(R) " + recibo.RecSecuencia;
                }
                else
                {
                    recNumero = recibo.RecSecuencia.ToString();
                }

                if(recibo.CliNombre.Length > 19)
                {
                    recibo.CliNombre = recibo.CliNombre.Substring(0, 19);
                }
                printer.DrawText((String.IsNullOrWhiteSpace(recNumero) ? "Numero" : recNumero).PadRight(10) + recibo.CliNombre.PadRight(20) + recibo.RecTotal.ToString("N", new CultureInfo("en-US")).PadLeft(15));

            }
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" (*) = Recibo anulado");
            printer.DrawText(" (R) = Recibo Rechazado");
            printer.DrawText(" ");
            printer.Bold = true;
            printer.DrawText("SECUENCIA PUSHMONEY DEPOSITADOS:");
            printer.DrawText("---------------------------------------------");
            printer.DrawText("Numero    Cliente                       Monto");
            printer.DrawText("---------------------------------------------");
            printer.Bold = false;
            printer.DrawText(" ");

            double totalPushM = 0.0;
            int cantPushMoney = 0;
            foreach (var pm in myRec.GetPushMoneyDepositados(deposito.DepSecuencia))
            {
                if (pm.CliNombre.Length > 19)
                {
                    pm.CliNombre = pm.CliNombre.Substring(0, 19);
                }
                totalPushM += pm.ComTotal;
                printer.DrawText(pm.ComSecuencia.ToString().PadRight(10) + pm.CliNombre.PadRight(20)  + pm.ComTotal.ToString("N", new CultureInfo("en-US")).PadLeft(15));
                cantPushMoney++;
            }
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("------------------------------------------------");
            printer.DrawText("              Firma del Vendedor");
            printer.DrawText(" ");
            printer.DrawText(" ");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("Vendedor:" + Arguments.CurrentUser.RepCodigo);//+dataRepresentante.get(0).getRepNombre());
            printer.DrawText("Celular :" + Arguments.CurrentUser.RepTelefono2); //+dataRepresentante.get(0).getRepTelefono1());
            printer.DrawText("Telefono :" + Arguments.CurrentUser.RepTelefono1); //+dataRepresentante.get(0).getRepTelefono2());
            printer.DrawText(" ");
            printer.Font = PrinterFont.FOOTER;
            printer.Bold = true;
            printer.DrawText("Formato pedidos 7: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato9(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("E N T R E G A  D E  D E P O S I T O S");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (deposito.DepTipo == 1)
            {
                printer.DrawText("Tipo: Banco");
            }
            else
            {
                printer.DrawText("Tipo: Caja General");
            }
            printer.DrawText("");
            if (DateTime.TryParse(deposito.DepFecha, out DateTime fecha))
            {
                printer.DrawText("Fecha      : " + fecha.ToString("dd/MM/yyyy hh:mm tt"));          
            }
            else
            {
                printer.DrawText("Fecha      : " + deposito.DepFecha);               
            }
            printer.DrawText("Secuencia #: " + deposito.DepSecuencia);
            //printer.DrawText("");
            printer.DrawText("Vendedor   :" + Arguments.CurrentUser.RepCodigo);
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;

            //printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------------------");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("Deposito           :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString(), 47);
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Cant. cheques      :".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total efectivo     :".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total cheques      :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total cheques fut  :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total deposito     :".PadRight(35) + totalDeposito.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawText("Total Pushmoney    :".PadRight(35) + deposito.DepMontoPushMoney.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            double Retencion = new DS_Depositos().GetRecibosRetencion(depSecuencia);
            printer.DrawText("Total Retencion    :".PadRight(35) + Retencion.ToString("N2").PadLeft(10));
            //printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            //printer.DrawText("");
            printer.DrawText("Numero Cliente                            Monto");
            //printer.DrawText("");
            printer.DrawLine();

            //foreach
            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                var clinombre = recibo.CliNombre.Length > 25 ? recibo.CliNombre.Substring(0, 25) : recibo.CliNombre;
                printer.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(28) + recibo.RecTotal.ToString("N2").PadLeft(12));
                //printer.DrawText("");
            }
            if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
            {
                printer.DrawLine();
                printer.DrawText("RECIBOS ANULADOS");
                printer.DrawLine();
                foreach (Recibos anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                {

                    var clinombre = anulados.CliNombre.Length > 28 ? anulados.CliNombre.Substring(0, 28) : anulados.CliNombre;
                    printer.DrawText(anulados.RecSecuencia.ToString().PadRight(18) + /*clinombre.ToString().PadRight(28) +*/ anulados.RecTotal.ToString("N2").PadLeft(26));
                    //printer.DrawText("");
                }
            }



            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------------------");
            printer.DrawText("Firma vendedor:");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            //printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Formato depositos 9: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();

        }

        private void Formato11(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");//Arreglos v2
            var fechaValida = DateTime.TryParse(deposito.DepFecha, out DateTime fecha);
            printer.DrawText("Fecha     :" + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : deposito.DepFecha));
            printer.DrawText("Deposito #:" + Arguments.CurrentUser.RepCodigo + "-" + deposito.DepSecuencia);
            printer.DrawText(" ");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("E N T R E G A  D E  C O B R O S");
            printer.DrawText(""); //Arreglos v2
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;


            int cantRecibos = deposito.DepCantidadRecibos;
            int ReciboInicial = deposito.DepReciboInicial;
            int ReciboFinal = deposito.DepReciboFinal;
            int CantidadCheque = 0;
            double TotalEfetivo = deposito.DepMontoEfectivo;
            double TotalCheque = deposito.DepMontoCheque;
            double TotalChequefut = deposito.DepMontoChequeDiferido;
            double TotalDeposito = 0.0;
            double TotalEfetivoTrans = deposito.DepMontoTransferencia;
            double TotalMontoPushMoney = deposito.DepMontoPushMoney;

            string cantRecibos2 = cantRecibos.ToString("N", new CultureInfo("en-US"));
            string ReciboInicial2 = ReciboInicial.ToString("N", new CultureInfo("en-US"));
            string ReciboFinal2 = ReciboFinal.ToString("N", new CultureInfo("en-US"));
            string TotalEfetivo2 = TotalEfetivo.ToString("N", new CultureInfo("en-US"));
            string TotalEfectivoADepositar = (TotalEfetivo - TotalMontoPushMoney).ToString("N", new CultureInfo("en-US"));
            string TotalCheque2 = TotalCheque.ToString("N", new CultureInfo("en-US"));
            string TotalChequefut2 = TotalChequefut.ToString("N", new CultureInfo("en-US"));
            string TotalEfetivoTrans2 = TotalEfetivoTrans.ToString("N", new CultureInfo("en-US"));
            string TotalMontoPushMoney2 = (Math.Abs(TotalMontoPushMoney) * (-1)).ToString("N", new CultureInfo("en-US"));

            DS_CuentasBancarias mycuen = new DS_CuentasBancarias();
            CantidadCheque = mycuen.getCantidadCheque(deposito.DepSecuencia, Arguments.CurrentUser.RepCodigo);

            string CantidadCheque2 = CantidadCheque.ToString("N", new CultureInfo("en-US"));
            TotalDeposito = (TotalEfetivo + TotalCheque + TotalChequefut +
                    TotalEfetivoTrans - TotalMontoPushMoney);

            string TotalDeposito2 = TotalDeposito.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("Tipo deposito: " + deposito.DepTipoDescripcion);
            printer.DrawText("------------------------------------------------------------");
            string cubReferencia = deposito.CuBID.ToString();
            string depReferencia = deposito.DepReferencia;
            printer.DrawText("Cant. Cheques: " + CantidadCheque2);
            if (cubReferencia != null)
            {
                printer.DrawText(("Banco / Deposito: " + deposito.CuBNombre + "  " + deposito.DepNumero),47);
                printer.DrawText("Cuenta corriente: " + cubReferencia);
            }

            printer.DrawText("Referencia:".PadRight(35) + depReferencia.PadLeft(10));
            printer.DrawText("------------------------------------------------------------");

            printer.DrawText(" ");
            printer.DrawText(" ");
            //TotalEfetivo2
            printer.DrawText("Efectivo Cobrado      :".PadRight(35) + TotalEfetivo2.PadLeft(10));
            printer.DrawText("Total Monto Pushmoney :".PadRight(35) + TotalMontoPushMoney2.PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Efectivo depositar    :".PadRight(35) + TotalEfectivoADepositar.PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total Cheques         :".PadRight(35) + TotalCheque2.PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total Cheques Dif     :".PadRight(35) + TotalChequefut2.PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total Transf          :".PadRight(35) + TotalEfetivoTrans2.PadLeft(10));
            printer.DrawText("");
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N", new CultureInfo("en-US")).PadLeft(10));
            printer.DrawText("");
            printer.Bold = true;
            printer.DrawText("Total Deposito        :".PadRight(35) + TotalDeposito2.PadLeft(10));
            printer.DrawText("");
            printer.Bold = false;
            


            printer.DrawText("------------------------------------------------------------");

            printer.Bold = true;
            printer.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
            printer.DrawText("--------------------------------------------");
            printer.DrawText("Numero                                 Monto");
            printer.DrawText("--------------------------------------------");
            printer.Bold = false;
            printer.DrawText(" ");


            foreach (Recibos recibo in myRec.GetRecibosByDeposito(depSecuencia))
            {
                string recNumero = "";
                if (recibo.RecEstatus == 0)
                {
                    recNumero = "* " + recibo.RecNumero;
                }
                else if (recibo.RecEstatus == 5)
                {
                    recNumero = "(R) " + recibo.RecNumero;
                }
                else if (recibo.RecMontoChequeF > 0)
                {
                    recNumero = "(D) " + recibo.RecNumero;
                }
                else
                {
                    recNumero = recibo.RecNumero;
                }

                printer.DrawText((String.IsNullOrWhiteSpace(recNumero) ? "Número" : recNumero).PadRight(35) + recibo.RecTotal.ToString("N", new CultureInfo("en-US")).PadLeft(10));

            }

            printer.DrawText(" ");
            printer.DrawText("Cant. Recibos:" + cantRecibos2);
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" (*) = Recibo anulado");
            printer.DrawText(" (R) = Recibo Rechazado");
            printer.DrawText(" (D) = Recibo Diferido");
            printer.DrawText(" ");
            printer.Bold = true;
            printer.DrawText("SECUENCIA PUSHMONEY DEPOSITADOS:");
            printer.DrawText("--------------------------------------------");
            printer.DrawText("Numero                                 Monto");
            printer.DrawText("--------------------------------------------");
            printer.Bold = false;
            printer.DrawText(" ");
            
            double totalPushM = 0.0;
            int cantPushMoney = 0;
            foreach (var pm in myRec.GetPushMoneyDepositados(deposito.DepSecuencia))
            {
                totalPushM += pm.ComTotal;
                printer.DrawText(pm.ComSecuencia.ToString().PadRight(35) + pm.ComTotal.ToString("N", new CultureInfo("en-US")).PadLeft(10));
                cantPushMoney++;
            }
            printer.DrawText(" ");
            printer.DrawText("Cant. Pushmoney: " + cantPushMoney);
            printer.DrawLine();
            printer.DrawText(" ");
            printer.DrawText("Total PushMoney Dep:" + totalPushM);
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("------------------------------------------------");
            printer.DrawText("              Firma del Vendedor");
            printer.DrawText(" ");
            printer.DrawText(" ");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                printer.DrawText("");
            }
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("Vendedor:" + Arguments.CurrentUser.RepCodigo);//+dataRepresentante.get(0).getRepNombre());
            printer.DrawText("Celular :" + Arguments.CurrentUser.RepTelefono2); //+dataRepresentante.get(0).getRepTelefono1());
            printer.DrawText("Telefono :" + Arguments.CurrentUser.RepTelefono1); //+dataRepresentante.get(0).getRepTelefono2());
            printer.DrawText(" ");
            printer.Font = PrinterFont.FOOTER;

            printer.Bold = true;
            printer.DrawText("Formato pedidos 11: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato23(int depSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            Depositos deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("E N T R E G A  D E  C O B R O S");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            var fechaValida = DateTime.TryParse(deposito.DepFecha, out DateTime fecha);
            printer.DrawText("Fecha     :" + (fechaValida ? fecha.ToString("dd/MM/yyyy") : deposito.DepFecha));
            printer.DrawText("Hora de Reporte: " + DateTime.Now.ToString("dd/MM/yyyy"));
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + ")" + " " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("No Deposito MB: " + Arguments.CurrentUser.RepCodigo + "-" + deposito.DepSecuencia);
            printer.DrawText("No Deposito: " + deposito.DepNumero);
            if (deposito.DepTipo == 1)
            {
                printer.DrawText("Banco: " + new DS_CuentasBancarias().getNombreBanco(deposito.CuBID));
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Cant. recibos      :".PadRight(35) + deposito.DepCantidadRecibos.ToString().PadLeft(10));
            printer.DrawText("");
            printer.DrawText("");
            double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
            printer.DrawText("Total Cobrado Bruto:".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Total Neto Deposito:".PadRight(35) + (deposito.DepMontoChequeDiferido - totalDeposito).ToString("N2").PadLeft(10));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cantidad de Cheques:".PadRight(35) + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
            printer.DrawText("Valores en Efectivo:".PadRight(35) + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
            printer.DrawText("Valores en Cheques :".PadRight(35) + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
            printer.DrawText("Valores en Cheques fut :".PadRight(35) + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
            printer.DrawText("Total transferencia:".PadRight(35) + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
            printer.DrawText("Total tarj.credito:".PadRight(35) + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
            printer.DrawLine();

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Entregado por:__________________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Recibido por:__________________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Revisado por:__________________________________");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Formato depositos 23: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

    }
}

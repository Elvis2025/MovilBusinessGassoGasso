using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.printers.formats
{
    public class EntregaDocumentosFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_EntregaFactura myEnt;
        private DS_Clientes myCli;
        string municipio = "";
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public EntregaDocumentosFormats(DS_EntregaFactura ent)
        {
            myEnt = ent;
            myCli = new DS_Clientes();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int entSecuencia, PrinterManager printer, string rowguid = "") { Print(entSecuencia, false, printer, rowguid); }
        public void Print(int entSecuencia, bool entConfirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            //Copias = copias;
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasDocumentos())
            {
                case 1:
                case 8:
                default:
                    Formato1(entSecuencia, entConfirmado);
                    break;
                case 2: //Feltrex
                    Formato2(entSecuencia, entConfirmado);
                    break;
                case 3://LAM
                    Formato3(entSecuencia, entConfirmado);
                    break;
                case 4://FELTREX - zebra
                    Formato4(entSecuencia, entConfirmado);
                    break;
                case 5:
                    Formato5(entSecuencia, entConfirmado);
                    break;
                case 6:
                    Formato6(entSecuencia, entConfirmado);
                    break;
            }
        }

        private void Formato1(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if(cliente == null)
            {
                return;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ENTREGA DOCUMENTOS");
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("Entrega: " + Arguments.CurrentUser.RepCodigo + " - " + entSecuencia);
            printer.DrawText("");
            printer.DrawText("Cliente: "+ cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 48);
            printer.DrawText("Urb:" + cliente.CliUrbanizacion, 48);
            printer.DrawText("Fecha entrega: " + entrega.EntFecha);
            printer.DrawLine();
            printer.DrawText("Fecha        Tipo  Num            Valor");
            printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach(EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;

                printer.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(6) + ent.EntDocumento.PadRight(15) + ent.EntMonto.ToString("N2").PadRight(12));
            }
            //foreach
            printer.DrawLine();
            printer.DrawText("Cantidad Documentos: " + CantidadDocumentos);
            printer.DrawText("Monto: " + MontoTotal.ToString("N2"));
            printer.DrawText("Nota: Esto no es un recibo de pago.");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Recibido por: " + entrega.EntRecibidoPor);
            printer.DrawText("");           
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato entrega documentos 1: Movilbusiness "+ Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
        private void Formato2(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if (cliente == null)
            {
                return;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("ENTREGA DOCUMENTOS");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            //printer.DrawText("");
            printer.DrawText("");
            //printer.DrawText("Entrega: " + Arguments.CurrentUser.RepCodigo + " - " + entSecuencia);
            //printer.DrawText("");
            var Fecha = DateTime.TryParse(entrega.EntFecha,out DateTime EntDate);
            printer.DrawText("Fecha  : " + (Fecha ? EntDate.ToString("dd-MM-yyyy") : entrega.EntFecha));
            //printer.DrawText("");
            var Hora = DateTime.TryParse(entrega.EntFecha, out DateTime EntTime);
            printer.DrawText("Hora   : " + (Hora ? EntDate.ToString("hh:mm tt") : entrega.EntFecha));
            //printer.DrawText("");
            printer.DrawText("Numero : " + Arguments.CurrentUser.RepCodigo + " - " + entrega.EntSecuencia, 48);
            //printer.DrawText("");
            if(cliente.CliNombre.Length > 36)
            {
                cliente.CliNombre = cliente.CliNombre.Substring(0, 36);
            }
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            //printer.DrawText("");
            int MunId = new DS_Clientes().GetCliMunID(cliente.CliID);
            if (MunId != 0)
            {
                municipio = new DS_Municipios().GetMunicipioById(MunId).MunDescripcion;

            }
            printer.DrawText("Ciudad : " + municipio, 48);
           
            
            //printer.DrawLine();            
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Fecha        Tipo    Num         Valor Total");
            printer.DrawText("-----------------------------------------------");
            //printer.DrawText("");
            //printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach (EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;

                printer.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(8) + ent.EntDocumento.PadRight(14) + ent.EntMonto.ToString("N2").PadLeft(11));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("-----------------------------------------------");
            //foreach
            //printer.DrawLine();
            //printer.DrawText("");
            printer.DrawText("Total: " + ("$" + MontoTotal.ToString("N2")).PadLeft(39));
            //printer.DrawText("");
            printer.DrawText("Cantidad Documentos: " + CantidadDocumentos);
            //printer.DrawText("");
            printer.DrawText("Nota: Esto no es un recibo de pago.");
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Recibido por: " + entrega.EntRecibidoPor);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Formato entrega docs 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato3(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if (cliente == null)
            {
                return;
            }

            printer.PrintEmpresa(Notbold:true);
            printer.Bold = false;
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("ENTREGA DOCUMENTOS");
            printer.DrawText("");
            var Fecha = DateTime.TryParse(entrega.EntFecha, out DateTime EntDate);
            printer.DrawText("Fecha  : " + (Fecha ? EntDate.ToString("dd-MM-yyyy") : entrega.EntFecha));
            var Hora = DateTime.TryParse(entrega.EntFecha, out DateTime EntTime);
            printer.DrawText("Hora   : " + (Hora ? EntDate.ToString("hh:mm tt") : entrega.EntFecha));
            printer.DrawText("");
            printer.DrawText("Numero Entrega: " + Arguments.CurrentUser.RepCodigo + " - " + entrega.EntSecuencia, 48);
            printer.DrawText("Codigo Cliente: " + cliente.CliCodigo, 48);
            if (cliente.CliNombre.Length > 36)
            {
                cliente.CliNombre = cliente.CliNombre.Substring(0, 36);
            }
            printer.DrawText("Nombre Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("");
            printer.DrawLine();            
            printer.DrawText("Fecha        Tipo     Num        Valor Total");
            printer.DrawLine();
            //printer.DrawText("");
            //printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach (EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;

                printer.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(9) + ent.EntDocumento.PadRight(10) + ent.EntMonto.ToString("N2").PadLeft(12));
            }
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Total: " + ("$" + MontoTotal.ToString("N2")).PadLeft(38));
            printer.DrawText("Nota: Esto no es un recibo de pago.");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(entrega.EntRecibidoPor);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Representante: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Tipos Transacciones: ");
            int FT = 0, NC = 0, ND = 0, CKD = 0, RCA = 0, CCK = 0, ACD = 0, ACC = 0, FTN = 0, PAU = 0;
            foreach (var p in detalles)
            {
                if (p.cxcSigla == "FT" && FT == 0)
                {
                    printer.DrawText("FT: " + "Factura");
                    FT++;
                }

                if (p.cxcSigla == "NC" && NC==0)
                {
                    printer.DrawText("NC: " + "Nota de Credito");
                    NC++;
                }

                if (p.cxcSigla == "ND" && ND == 0)
                {
                    printer.DrawText("ND: " + "Nota de Debito");
                    ND++;
                }

                if (p.cxcSigla == "CKD" && CKD == 0)
                {
                    printer.DrawText("CKD: " + "Cheque Devuelto");
                    CKD++;
                }

                if (p.cxcSigla == "RCA" && RCA == 0)
                {
                    printer.DrawText("RCA: " + "Recibo a favor");
                    RCA++;
                }

                if (p.cxcSigla == "CCK" && CCK == 0)
                {
                    printer.DrawText("CCK: " + "Cargo Cheque Devuelto");
                    CCK++;
                }

                if (p.cxcSigla == "ACD" && ACD == 0)
                {
                    printer.DrawText("ACD: " + "Ajuste Cobro Debito");
                    ACD++;
                }

                if (p.cxcSigla == "ACC" && ACC == 0)
                {
                    printer.DrawText("ACC: " + "Ajuste Cobro Credito");
                    ACC++;
                }

                if (p.cxcSigla == "FTN" && FTN == 0)
                {
                    printer.DrawText("FTN: " + "Factura Negativa");
                    FTN++;
                }

                if (p.cxcSigla == "PAU" && PAU == 0)
                {
                    printer.DrawText("PAU: " + "Pedidos Autorizados");
                    PAU++;
                }
            }
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion);
            printer.DrawText("Formato entrega docs 3");
            printer.DrawText("");
            printer.Print();

        }

        private void Formato4(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if (cliente == null)
            {
                return;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("ENTREGA DOCUMENTOS");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            var Fecha = DateTime.TryParse(entrega.EntFecha, out DateTime EntDate);
            printer.DrawText("Fecha  : " + (Fecha ? EntDate.ToString("dd-MM-yyyy") : entrega.EntFecha));
            var Hora = DateTime.TryParse(entrega.EntFecha, out DateTime EntTime);
            printer.DrawText("Hora   : " + (Hora ? EntDate.ToString("hh:mm tt") : entrega.EntFecha));
            printer.DrawText("Numero : " + Arguments.CurrentUser.RepCodigo + " - " + entrega.EntSecuencia, 48);
            if (cliente.CliNombre.Length > 36)
            {
                cliente.CliNombre = cliente.CliNombre.Substring(0, 36);
            }
            cliente.CliNombre = cliente.CliNombre.TrimEnd(' ');
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);

            int MunId = new DS_Clientes().GetCliMunID(cliente.CliID);
            if (MunId != 0)
            {
                municipio = new DS_Municipios().GetMunicipioById(MunId).MunDescripcion;

            }
            printer.DrawText("Ciudad : " + municipio, 48);

            printer.DrawText("");          
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Fecha        Tipo    Num         Valor Total");
            printer.DrawText("-----------------------------------------------");
            //printer.DrawText("");
            //printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach (EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;

                printer.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(8) + ent.EntDocumento.PadRight(14) + ent.EntMonto.ToString("N2").PadLeft(11));
                //printer.DrawText("");
            }
            //printer.DrawText("");
            printer.DrawText("-----------------------------------------------");

            printer.DrawText("");
            printer.DrawText("Total: " + ("$" + MontoTotal.ToString("N2")).PadLeft(39));
            printer.DrawText("Cantidad Documentos: " + CantidadDocumentos);
            printer.DrawText("Nota: Esto no es un recibo de pago.");
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Recibido por: " + entrega.EntRecibidoPor);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato entrega docs 4: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }


        private void Formato5(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if (cliente == null)
            {
                return;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ENTREGA DOCUMENTOS");
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("Entrega: " + DS_RepresentantesParametros.GetInstance().GetParPrefSec() + " - " + entSecuencia);
            printer.DrawText("");
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 48);
            printer.DrawText("Fecha entrega: " + entrega.EntFecha);
            printer.DrawLine();
            printer.DrawText("Fecha        Tipo  Num            Valor");
            printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach (EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;

                printer.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(6) + ent.EntDocumento.PadRight(15) + ent.EntMonto.ToString("N2").PadRight(12));
            }
            //foreach
            printer.DrawLine();
            printer.DrawText("Cantidad Documentos: " + CantidadDocumentos);
            printer.DrawText("Monto: " + MontoTotal.ToString("N2"));
            printer.DrawText("Nota: Esto no es un recibo de pago.");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Recibido por: " + entrega.EntRecibidoPor);
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato entrega documentos 5: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato6(int entSecuencia, bool entConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            EntregasDocumentos entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);
            List<EntregasDocumentosDetalle> detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

            if (entrega == null)
            {
                return;
            }

            Clientes cliente = myCli.GetClienteById(entrega.CliID);

            if (cliente == null)
            {
                return;
            }

            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.PrintEmpresa(TitleNotBold: true);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("E N T R E G A  D E  D O C U M E N T O S");
            printer.DrawText("");
            printer.DrawText("Fecha entrega: " + Functions.FormatDate(entrega.EntFecha, "dd-MM-yyyy HH:mm:ss"));
            printer.DrawText("");
            printer.DrawText("Numero Entrega: " + Arguments.CurrentUser.RepCodigo + " - " + entSecuencia);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Fecha       Tipo  Num       Div     ValorTotal");
            printer.DrawLine();
            int CantidadDocumentos = 0;
            double MontoTotal = 0;

            foreach (EntregasDocumentosDetalle ent in detalles)
            {
                MontoTotal += ent.EntMonto;
                CantidadDocumentos++;
                var MontoString = "$" + ent.EntMonto.ToString("N2");

                printer.DrawText(ent.cxcFecha.PadRight(12) + ent.cxcSigla.PadRight(4) + ent.EntDocumento.PadRight(12) + ent.AreaCtrlCredit?.Substring(ent.AreaCtrlCredit.Length -2, 2).PadRight(4) +  MontoString.PadLeft(14));
            }

            var MontoTotalString = "$" + MontoTotal.ToString("N2");
            printer.DrawLine();
            printer.DrawText("Total" + MontoTotalString.PadLeft(41));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Recibido por: " + entrega.EntRecibidoPor);
            printer.DrawText("");
            printer.DrawText("Rep. : " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativas:");
            printer.DrawText("FT: Factura");
            printer.DrawText("NC: Nota de Credito");
            printer.DrawText("CK: Cargo por Cheque Devuelto");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato entrega documentos 6: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.Print();

        }

    }
}

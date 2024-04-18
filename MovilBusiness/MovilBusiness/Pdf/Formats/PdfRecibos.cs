using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfRecibos : IPdfGenerator
    {
        private DS_Recibos myRec;
        private string SectorID = "";
        public PdfRecibos(string SecCodigo = "")
        {
            myRec = new DS_Recibos();
            SectorID = SecCodigo;
        }

        public Task<string> GeneratePdf(int recSecuencia, bool confirmado)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos();

            switch (formato)
            {
                default:
                case 1:
                    return Formato1(recSecuencia, confirmado);
                case 11:
                    return Formato11(recSecuencia, confirmado);
                case 17:
                    return Formato17(recSecuencia, confirmado);
                case 18: //ANDOSA
                    return Formato18(recSecuencia, confirmado);
                case 40:
                    return Formato40(recSecuencia, confirmado);
                case 36:
                    return Formato36(recSecuencia, confirmado);
            }
        }

        public Task<string> GenerateNCDPPPdf(int recSecuencia, int ncdSecuencia)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos();

            switch (formato)
            {
                default:
                    return FormatoNCDPP1(recSecuencia, ncdSecuencia);
            }
        }

        private Task<string> Formato1(int recSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);

                if (recibo == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C I B O");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Codigo: " + recibo.CliCodigo);
                    manager.DrawText("Cliente: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTOS APLICADOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Sigla", "Documento", "Monto", "Descuento", "Neto" }, true);
                    manager.Bold = false;

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

                        //printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                        manager.DrawTableRow(new List<string>() { sigla, app.CxCDocumento, valorBruto.ToString("N2"), app.RecDescuento.ToString("N2"), app.RecValor.ToString("N2") });

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            //manager.TextColor = Xamarin.Forms.Color.FromHex("#C62828");
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento, nc.RecValor.ToString("N2"), " ", " " });
                            // manager.TextColor = Xamarin.Forms.Color.Black;
                        }

                        //printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Valor bruto:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor NC:          " + ValorNC.ToString("N2").PadLeft(5));
                    manager.DrawText("Valor desc:        " + TotalDesc.ToString("N2").PadLeft(12));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    else
                    {
                        manager.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    manager.DrawText("Total pagado:      " + totalNeto.ToString("N2").PadLeft(10));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FORMAS DE PAGO", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

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
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma cliente");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Leyenda explicativa de las facturas");
                    manager.DrawText("S : factura pagada completa");
                    manager.DrawText("AB: abono");
                    manager.DrawText("NC: nota de credito aplicada");
                    manager.DrawText("CK: factura por cheque devuelto");
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf recibos 1: Movilbusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });
        }
        private Task<string> Formato11(int recSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);

                if (recibo == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C I B O");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Codigo: " + recibo.CliCodigo);
                    manager.DrawText("Cliente: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTOS APLICADOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Sigla", "Documento", "Monto", "Descuento", "Neto" }, true);
                    manager.Bold = false;

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

                        //printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                        manager.DrawTableRow(new List<string>() { sigla, app.CxCDocumento, valorBruto.ToString("N2"), app.RecDescuento.ToString("N2"), recibo.MonCodigo + app.RecValor.ToString("N2") });

                        if ((recibo.MonCodigo != app.MonCodigo) && !string.IsNullOrEmpty(app.MonCodigo))
                        {
                            var convert = recibo.RecTasa / app.RecTasa;

                            manager.DrawTableRow(new List<string>() { "Moneda: " + app.MonCodigo, "", "Tasa: " + (app.RecTasa == 1 ? recibo.RecTasa : app.RecTasa), "", "Valor: " + (valorBruto * convert).ToString("N2") });
                        }

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            //manager.TextColor = Xamarin.Forms.Color.FromHex("#C62828");
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento, nc.RecValor.ToString("N2"), " ", " " });
                            // manager.TextColor = Xamarin.Forms.Color.Black;
                        }

                        //printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Valor bruto:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor NC:          " + ValorNC.ToString("N2").PadLeft(5));
                    manager.DrawText("Valor desc:        " + TotalDesc.ToString("N2").PadLeft(12));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    else
                    {
                        manager.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    manager.DrawText("Total pagado:      " + totalNeto.ToString("N2").PadLeft(10));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FORMAS DE PAGO", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    manager.DrawText("Moneda: " + recibo.MonCodigo);
                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

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
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma cliente");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Leyenda explicativa de las facturas");
                    manager.DrawText("S : factura pagada completa");
                    manager.DrawText("AB: abono");
                    manager.DrawText("NC: nota de credito aplicada");
                    manager.DrawText("CK: factura por cheque devuelto");
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf recibos 11: Movilbusiness v" + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }

        private Task<string> Formato36(int recSecuencia, bool confirmado)
        {
            return Task.Run(() => 
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);
              
                if (recibo == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C I B O");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Codigo: " + recibo.CliCodigo);
                    manager.DrawText("Cliente: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTOS APLICADOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Sigla - Documento"}, true);
                    manager.DrawTableRow(new List<string>() { "Balance Original", "Balance Pendiente", "Importe Cobrado"}, true);
                    manager.Bold = false;

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
                        var balancependiente = Math.Abs(app.CxcBalance - valorBruto);

                        manager.DrawTableRow(new List<string>() { sigla + "-" + app.CxCDocumento});
                        manager.DrawTableRow(new List<string>() { app.CxcBalance.ToString("N2").PadLeft(20), balancependiente.ToString("N2").PadLeft(20), valorBruto.ToString("N2").PadLeft(20) });

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento});
                            manager.DrawTableRow(new List<string>() { nc.RecValor.ToString("N2"), " ", " " });
                        }

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Valor bruto:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor NC:          " + ValorNC.ToString("N2").PadLeft(5));
                    manager.DrawText("Valor desc:        " + TotalDesc.ToString("N2").PadLeft(12));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    else
                    {
                        manager.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    manager.DrawText("Total pagado:      " + totalNeto.ToString("N2").PadLeft(10));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FORMAS DE PAGO", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

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
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma cliente");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Leyenda explicativa de las facturas");
                    manager.DrawText("S : factura pagada completa");
                    manager.DrawText("AB: abono");
                    manager.DrawText("NC: nota de credito aplicada");
                    manager.DrawText("CK: factura por cheque devuelto");
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf recibos 36: Movilbusiness v" + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }

        private Task<string> Formato40(int recSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);

                if (recibo == null)
                {
                    throw new Exception("Receipt details not found.");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C E I P T");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Receipt: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Date: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Code: " + recibo.CliCodigo);
                    manager.DrawText("Customer: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("APPLIED DOCUMENTS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Initials", "Document", "Amount", "Discount", "Net worth" }, true);
                    manager.Bold = false;

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

                        //printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                        manager.DrawTableRow(new List<string>() { sigla, app.CxCDocumento, valorBruto.ToString("N2"), app.RecDescuento.ToString("N2"), app.RecValor.ToString("N2") });

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            //manager.TextColor = Xamarin.Forms.Color.FromHex("#C62828");
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento, nc.RecValor.ToString("N2"), " ", " " });
                            // manager.TextColor = Xamarin.Forms.Color.Black;
                        }

                        //printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Gross value:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Credit notes value:       " + ValorNC.ToString("N2").PadLeft(5));
                    manager.DrawText("Discount value:    " + TotalDesc.ToString("N2").PadLeft(12));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Advance Value: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(19));
                    }
                    else
                    {
                        manager.DrawText("Residual value: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(19));
                    }
                    manager.DrawText("Total to pay:      " + totalNeto.ToString("N2").PadLeft(11));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("Payment Methods", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

                        switch (rec.ForID)
                        {
                            case 1:
                                manager.DrawText("Cash: " + rec.RefValor.ToString("N2").PadLeft(35));
                                break;
                            case 2:
                                manager.DrawText("Check " + (rec.RefIndicadorDiferido ? "deferred" : "normal") + "  Number: " + rec.RefNumeroCheque.ToString());
                                manager.DrawText("Bank   : " + rec.BanNombre);
                                manager.DrawText("Amount   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                if (rec.RefIndicadorDiferido)
                                {
                                    manager.DrawText("Fecha: " + rec.RefFecha);
                                }
                                break;
                            case 4:
                                manager.DrawText("Transfer: " + rec.RefNumeroCheque);
                                manager.DrawText("Date   : " + rec.RefFecha);
                                manager.DrawText("Bank   : " + rec.BanNombre);
                                manager.DrawText("Amount   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                break;
                            case 5:
                                manager.DrawText("Retention: " + rec.RefValor.ToString("N2").PadLeft(34));
                                break;
                            case 6:
                                manager.DrawText("Credit card: " + rec.RefValor.ToString("N2").PadLeft(28));
                                break;
                        }
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total charged: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Customer's signature");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Seller's signature");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    //manager.NewLine();
                    //manager.NewLine();
                    //manager.DrawText("Leyenda explicativa de las facturas");
                    //manager.DrawText("S : factura pagada completa");
                    //manager.DrawText("AB: abono");
                    //manager.DrawText("NC: nota de credito aplicada");
                    //manager.DrawText("CK: factura por cheque devuelto");
                    //manager.NewLine();
                    //manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Seller: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Phone: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Receipts format 40: MovilBusiness v" + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }


        private Task<string> FormatoNCDPP1(int recSecuencia, int ncdSecuencia)
        {
            return Task.Run(() =>
            {
                var NCDPP = myRec.GetNCDppRecibosPdf(recSecuencia, ncdSecuencia);
               
                if (NCDPP == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((NCDPP.CliNombre + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("NOTA CREDITO POR DESCUENTO");
                    manager.DrawText("NCF:   " + NCDPP.NCDNCF);
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha: " + Functions.FormatDate(NCDPP.NCDFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Recibo. No.: " + Arguments.CurrentUser.RepCodigo + "-" + NCDPP.RecTipo + "-" + NCDPP.RecSecuencia);
                    manager.DrawText("Nota de Credito No.: " + Arguments.CurrentUser.RepCodigo + "-" + NCDPP.RecTipo + "-" + NCDPP.NCDSecuencia);
                    manager.DrawText("Codigo: " + NCDPP.CliCodigo);
                    manager.DrawText("Cliente: " + NCDPP.CliNombre);
                    manager.TextAlign = Justification.LEFT;

                    manager.Bold = true;

                    manager.DrawTableRow(new List<string>() { "Factura", "Monto", "NCF MODIFICADO" }, true);
                    manager.Bold = false;

                    manager.DrawTableRow(new List<string>() { NCDPP.CxcDocumento.PadRight(16), NCDPP.NCDMonto.ToString("N2").PadRight(16), NCDPP.CxCNCFAfectado.PadRight(10) });
                    manager.DrawLine();
                    manager.Bold = true;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Recibido Por:");
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("***Monto Transaccion Incluye ITBIS***");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Formato NCDPP 1: Movilbusiness " + Functions.AppVersion);
                    manager.DrawText("");

                    return manager.FilePath;
                }

            });
        }

        private Task<string> Formato17(int recSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);

                if (recibo == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C I B O");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Codigo: " + recibo.CliCodigo);
                    manager.DrawText("Cliente: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTOS APLICADOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Sigla", "Documento", "Monto", "Descuento", "Descarga", "Neto" }, true);
                    manager.Bold = false;

                    double TotalBruto = 0;
                    double totalNeto = 0;
                    double TotalDesc = 0;
                    double TotalDesmonte = 0;
                    double ValorNC = myRec.GetMontoTotalNCByRecibos(recSecuencia, confirmado);

                    foreach (RecibosAplicacion app in myRec.GetRecibosAplicacionConDesmonteBySecuencia(recSecuencia, confirmado))
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

                        //printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                        manager.DrawTableRow(new List<string>() { sigla, app.CxCDocumento, valorBruto.ToString("N2"), app.RecDescuento.ToString("N2"), app.RecDescuentoDesmonte.ToString("N2"), app.RecValor.ToString("N2") });

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            //manager.TextColor = Xamarin.Forms.Color.FromHex("#C62828");
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento, nc.RecValor.ToString("N2"), " ", " " });
                            // manager.TextColor = Xamarin.Forms.Color.Black;
                        }

                        //printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                        TotalDesmonte += app.RecDescuentoDesmonte;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Valor bruto:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor NC:          " + ValorNC.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor desc:        " + TotalDesc.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor descarga:    " + TotalDesmonte.ToString("N2").PadLeft(14));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(14));
                    }
                    else
                    {
                        manager.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(14));
                    }
                    manager.DrawText("Total pagado:      " + totalNeto.ToString("N2").PadLeft(10));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FORMAS DE PAGO", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

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
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma cliente");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Leyenda explicativa de las facturas");
                    manager.DrawText("S : factura pagada completa");
                    manager.DrawText("AB: abono");
                    manager.DrawText("NC: nota de credito aplicada");
                    manager.DrawText("CK: factura por cheque devuelto");
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf recibos 17: Movilbusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });
        }

        private Task<string> Formato18(int recSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {
                var recibo = myRec.GetReciboBySecuencia(recSecuencia, confirmado);

                if (recibo == null)
                {
                    throw new Exception("No se encontraron los datos del recibo");
                }

                using (var manager = PdfManager.NewDocument((recibo.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + recSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("R E C I B O  P R O V I S I O N A L");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

                    manager.DrawText("Recibo: " + Arguments.CurrentUser.RepCodigo + " - " + recSecuencia.ToString());
                    manager.DrawText("Fecha: " + Functions.FormatDate(recibo.RecFecha, "dd-MM-yyyy HH:mm ff"));
                    manager.DrawText("Codigo: " + recibo.CliCodigo);
                    manager.DrawText("Cliente: " + recibo.CliNombre);
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DOCUMENTOS APLICADOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawTableRow(new List<string>() { "Sigla", "Documento", "Monto", "Descuento", "Neto" }, true);
                    manager.Bold = false;

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

                        //printer.DrawText(sigla.PadRight(11) + app.CxCDocumento.PadRight(21) + (valorBruto).ToString("N2").PadLeft(13));
                        manager.DrawTableRow(new List<string>() { sigla, app.CxCDocumento, valorBruto.ToString("N2"), app.RecDescuento.ToString("N2"), app.RecValor.ToString("N2") });

                        foreach (RecibosAplicacion nc in notasCredito)
                        {
                            //manager.TextColor = Xamarin.Forms.Color.FromHex("#C62828");
                            manager.DrawTableRow(new List<string>() { nc.CxcSigla, nc.CxCDocumento, nc.RecValor.ToString("N2"), " ", " " });
                            // manager.TextColor = Xamarin.Forms.Color.Black;
                        }

                        //printer.DrawText(app.RecDescuento.ToString("N2").PadRight(24) + (app.RecValor).ToString("N2"));

                        totalNeto += app.RecValor;
                        TotalBruto += valorBruto;
                        TotalDesc += app.RecDescuento;
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("Valor bruto:       " + TotalBruto.ToString("N2").PadLeft(10));
                    manager.DrawText("Valor NC:          " + ValorNC.ToString("N2").PadLeft(5));
                    manager.DrawText("Valor desc:        " + TotalDesc.ToString("N2").PadLeft(12));
                    if (DS_RepresentantesParametros.GetInstance().GetParSustituirSobrantePorAdelantoEnFormato() && !myRec.ReciboTieneAplicaciones(recSecuencia))
                    {
                        manager.DrawText("Valor Adelanto: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    else
                    {
                        manager.DrawText("Valor sobrante: " + recibo.RecMontoSobrante.ToString("N2").PadLeft(29));
                    }
                    manager.DrawText("Total pagado:      " + totalNeto.ToString("N2").PadLeft(10));
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("FORMAS DE PAGO", false);
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();

                    double TotalCobrado = 0;
                    foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recSecuencia, confirmado))
                    {
                        TotalCobrado += rec.RefValor;

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
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Total cobrado: " + TotalCobrado.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(3, recSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma cliente");
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Leyenda explicativa de las facturas");
                    manager.DrawText("S : factura pagada completa");
                    manager.DrawText("AB: abono");
                    manager.DrawText("NC: nota de credito aplicada");
                    manager.DrawText("CK: factura por cheque devuelto");
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf recibos 18: Movilbusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });
        }

    }
}

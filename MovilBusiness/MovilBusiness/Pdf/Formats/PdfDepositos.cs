using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfDepositos : IPdfGenerator
    {
        private DS_Depositos myDep;
        private DS_Recibos myRec;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        public PdfDepositos()
        {
            myDep = new DS_Depositos();
            myRec = new DS_Recibos();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public Task<string> GeneratePdf(int traSecuencia, bool confirmado = false)
        {
            return Formato1(traSecuencia, confirmado);
        }

        private Task<string> Formato1(int depSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {

                var deposito = myDep.GetDepositobySecuencia(depSecuencia, confirmado);

                if (deposito == null)
                {
                    throw new Exception("Error cargando datos del deposito");
                }

                using (var manager = PdfManager.NewDocument((Arguments.CurrentUser.RepCodigo + "-Deposito_No." + depSecuencia).Replace("/", "")))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("ENTREGA DE DEPOSITOS");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Fecha deposito     : " + deposito.DepFecha.PadLeft(4));
                    manager.DrawText("Deposito           : " + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + depSecuencia.ToString());
                    manager.DrawText("Cant. recibos      : " + deposito.DepCantidadRecibos.ToString().PadLeft(10));
                    manager.DrawText("Cant. cheques      : " + myDep.GetCantidadChequesDepositados(depSecuencia).ToString().PadLeft(10));
                    manager.DrawText("Total efectivo     : " + deposito.DepMontoEfectivo.ToString("N2").PadLeft(10));
                    manager.DrawText("Total cheques      : " + deposito.DepMontoCheque.ToString("N2").PadLeft(10));
                    manager.DrawText("Total cheques fut  : " + deposito.DepMontoChequeDiferido.ToString("N2").PadLeft(10));
                    double totalDeposito = deposito.DepMontoCheque + deposito.DepMontoEfectivo + deposito.DepMontoChequeDiferido;
                    manager.DrawText("Total deposito     : " + totalDeposito.ToString("N2").PadLeft(10));
                    manager.DrawText("Total transferencia: " + deposito.DepMontoTransferencia.ToString("N2").PadLeft(10));
                    manager.DrawText("Total tarj.credito : " + deposito.DepMontoTarjeta.ToString("N2").PadLeft(10));
                    // manager.DrawLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("SECUENCIA DE LOS RECIBOS DEPOSITADOS:");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Numero", "Cliente", "Monto" }, true);
                    manager.Bold = false;
                    //manager.DrawText("Numero          Cliente                  Monto");
                    //manager.DrawLine();

                    var recibos = myRec.GetRecibosByDeposito(depSecuencia);
                    //int num = recibos.Max(c => c.CliNombre.Length);
                    //int clinombremax = num > 25 ? 25 : num;

                    //foreach
                    foreach (var recibo in recibos)
                    {
                        var clinombre = recibo.CliCodigo +'-'+ recibo.CliNombre;

                        /*while(clinombre.Length < clinombremax)
                        {
                            clinombre += " ";
                        }*/
                        manager.DrawTableRow(new List<string>() { recibo.RecSecuencia.ToString(), clinombre, recibo.RecTotal.ToString("N2") });
                        //manager.DrawText(recibo.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(33) + recibo.RecTotal.ToString("N2").PadRight(10));
                    }

                    if (myRec.GetRecibosByDepositoAnulados(depSecuencia).Count > 0)
                    {
                        manager.DrawLine();
                        manager.DrawText("RECIBOS ANULADOS");
                        manager.DrawLine();
                        foreach (var anulados in myRec.GetRecibosByDepositoAnulados(depSecuencia))
                        {
                            var clinombre = anulados.CliCodigo + '-' + anulados.CliNombre;
                            manager.DrawTableRow(new List<string>() { anulados.RecSecuencia.ToString(), clinombre, anulados.RecTotal.ToString("N2") }, false);
                            //printer.DrawText(anulados.RecSecuencia.ToString().PadRight(7) + clinombre.ToString().PadRight(33) + anulados.RecTotal.ToString("N2").PadRight(10));
                        }
                    }

                    manager.DrawLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("-------------------------------------");
                    manager.DrawText("Firma vendedor:");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    if (myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()) != "")
                    {
                        manager.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(9, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositos()));
                        manager.NewLine();
                    }
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf depositos 1: movilbusiness " + Functions.AppVersion);
                    manager.DrawText("");

                    return manager.FilePath;
                }
            });
        }
    }
}

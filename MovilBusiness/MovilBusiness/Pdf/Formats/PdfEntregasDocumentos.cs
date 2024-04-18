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
    public class PdfEntregasDocumentos : IPdfGenerator
    {
        private DS_EntregaFactura myEnt;

        public PdfEntregasDocumentos(DS_EntregaFactura ds)
        {
            myEnt = ds;
        }

        public Task<string> GeneratePdf(int traSecuencia, bool confirmado = false)
        {
            var formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasDocumentos();

            switch (formato)
            {
                case 1:
                default:
                    return Formato1(traSecuencia, confirmado);
            }
        }

        private Task<string> Formato1(int entSecuencia, bool entConfirmado)
        {
            return Task.Run(() =>
            {
                var entrega = myEnt.GetEntregaBySecuencia(entSecuencia, entConfirmado);

                if (entrega == null)
                {
                    throw new Exception("No se encontraron los datos de la compra");
                }

                var detalles = myEnt.GetEntregasDetalleBySecuencia(entSecuencia, entConfirmado);

                using (var manager = PdfManager.NewDocument(("Entrega factura No. " + entSecuencia.ToString() + " - " + Arguments.CurrentUser.RepCodigo + " : " + entrega.EntFecha).Replace("/", "")))
                {
                    var cliente = new DS_Clientes().GetClienteById(entrega.CliID);

                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.DrawText("ENTREGA DOCUMENTOS");
                    manager.Font = PrinterFont.BODY;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Entrega: " + Arguments.CurrentUser.RepCodigo + " - " + entSecuencia);
                    manager.NewLine();
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Urb:" + cliente.CliUrbanizacion);
                    manager.DrawText("Fecha entrega: " + entrega.EntFecha);

                    manager.DrawLine();
                    manager.DrawTableRow(new List<string>() { "Fecha", "Tipo", "Num", "Valor" }, true);//"Fecha        Tipo      Num         Valor");

                    int CantidadDocumentos = 0;
                    double MontoTotal = 0;

                    foreach (var ent in detalles)
                    {
                        MontoTotal += ent.EntMonto;
                        CantidadDocumentos++;

                        manager.DrawTableRow(new List<string>() { ent.cxcFecha, ent.cxcSigla, ent.EntDocumento, ent.EntMonto.ToString("N2") });
                        //manager.DrawText(ent.cxcFecha.PadRight(13) + ent.cxcSigla.PadRight(10) + ent.EntDocumento.PadRight(12) + ent.EntMonto.ToString("N2").PadRight(12));
                    }
                    //foreach
                    manager.DrawLine();
                    manager.NewLine();
                    manager.DrawText("Cantidad Documentos: " + CantidadDocumentos);
                    manager.DrawText("Monto: " + MontoTotal.ToString("N2"));
                    manager.DrawText("Nota: Esto no es un recibo de pago.");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Recibido por: " + entrega.EntRecibidoPor);
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Formato pdf entrega documentos 1: Movilbusiness " + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }
            });
        }
    }
}

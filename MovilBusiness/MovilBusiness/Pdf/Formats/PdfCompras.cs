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
    public class PdfCompras : IPdfGenerator
    {
        private DS_Compras myCom;
        private string SectorID = "";
        public PdfCompras(DS_Compras myCom = null, string SecCodigo = "")
        {
            if (myCom == null)
            {
                myCom = new DS_Compras();
            }
            this.myCom = myCom;
            SectorID = SecCodigo;
        }

        public Task<string> GeneratePdf(int comSecuencia, bool confirmado)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCompras();

            switch (formato)
            {
                default:
                    return Formato1(comSecuencia, confirmado);
            }
        }

        private Task<string> Formato1(int comSecuencia, bool confirmado)
        {
            return Task.Run(() => 
            {
                var compra = myCom.GetBySecuencia(comSecuencia, confirmado);

                if (compra == null)
                {
                    throw new Exception("No se encontraron los datos de la compra");
                }

                using (var manager = PdfManager.NewDocument((compra.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + comSecuencia).Replace("/", "")))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE COMPRAS No. " + comSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.DrawText("Tipo: " + compra.TipoPagoDescripcion);
                    manager.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + compra.ComSecuencia);

                    if (DateTime.TryParse(compra.ComFecha, out DateTime fecha))
                    {
                        manager.DrawText("Fecha: " + fecha.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        manager.DrawText("Fecha: " + compra.ComFecha);
                    }

                    manager.NewLine();
                    manager.DrawText("Cliente: " + compra.CliNombre);
                    manager.DrawText("Codigo: " + compra.CliCodigo);

                    var dependiente = new DS_Clientes().GetDependienteByCedula(compra.CLDCedula, compra.CliID);

                    manager.DrawText("Dependiente Tel.:   " + (dependiente != null ? dependiente.CldTelefono : ""));
                    manager.DrawText("Dependiente Cedula: " + (dependiente != null ? dependiente.ClDCedula : ""));
                    manager.DrawText("Dependiente Nombre: " + (dependiente != null ? dependiente.ClDNombre : ""));

                    var detalles = myCom.GetDetalleBySecuencia(comSecuencia, confirmado);

                    double total = 0.0;

                    if (string.IsNullOrWhiteSpace(compra.ComTipoPago) || compra.ComTipoPago == "1")
                    {
                        manager.DrawLine();
                        manager.Bold = true;
                        manager.DrawText("Descripcion");
                        manager.DrawTableRow(new List<string>() { "Cant.", "Codigo", "Factura", "Precio", "SubTotal" });
                        manager.Bold = false;
                        manager.DrawLine();

                        foreach (var det in detalles)
                        {
                            manager.DrawText(det.ProDescripcion);

                            var cantidad = det.ComCantidad.ToString();

                            if (det.ComCantidadDetalle > 0)
                            {
                                cantidad = cantidad + "/" + det.ComCantidadDetalle;
                            }

                            var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                            var subTotal = (det.ComPrecio + (det.ComPrecio * (det.ComItbis / 100))) * cantidadReal;

                            manager.DrawTableRow(new List<string>() { cantidad, det.ProCodigo, "", det.ComPrecio.ToString("N2"), subTotal.ToString("N2") });

                            total += subTotal;
                        }
                    }
                    else
                    {
                        manager.DrawLine();
                        manager.Bold = true;
                        manager.DrawText("Codigo - Descripcion");
                        manager.DrawTableRow(new List<string>() { "Factura", "Precio", "Cantidad" });
                        manager.Bold = false;
                        manager.DrawLine();

                        foreach (var det in detalles)
                        {
                            var desc = det.ProCodigo + " - " + det.ProDescripcion;
                            manager.DrawText(desc);

                            var cantidadReal = (det.ComCantidadDetalle / det.ProUnidades) + det.ComCantidad;

                            var cantidad = det.ComCantidad.ToString();

                            if (det.ComCantidadDetalle > 0)
                            {
                                cantidad = cantidad + "/" + det.ComCantidadDetalle;
                            }

                            var subTotal = det.ComPrecio * cantidadReal;

                            total += subTotal;

                            manager.DrawTableRow(new List<string>() { det.cxcDocumento, det.ComPrecio.ToString("N2"), cantidad });
                        }
                    }

                    manager.DrawLine();
                    manager.DrawText("Total: " + total.ToString("N2"));
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("-------------------------------------");
                    manager.DrawText("Firma del dependiente");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    if (!string.IsNullOrWhiteSpace(compra.ComTipoPago) && compra.ComTipoPago.Trim() == "2")
                    {
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("-------------------------------------");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                        manager.NewLine(); ;
                        manager.NewLine();
                        manager.NewLine();
                        manager.DrawText("-------------------------------------");

                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("Firma del Representante");
                        manager.TextAlign = Justification.LEFT;
                    }

                    if (compra.ComCantidadImpresion > 0)
                    {
                        manager.NewLine();
                        manager.DrawText("Fecha reimpresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
                        manager.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + comSecuencia);
                        manager.NewLine();
                    }

                    manager.DrawText("Items: " + detalles.Count);
                    manager.NewLine();
                    manager.NewLine();

                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato compras 1: Movilbusiness " + Functions.AppVersion);

                    return manager.FilePath;
                }
            });
        }
    }
}

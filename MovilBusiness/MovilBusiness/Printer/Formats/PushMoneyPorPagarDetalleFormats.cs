using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class PushMoneyPorPagarDetalleFormats
    {
        private PrinterManager printer;
        private DS_Clientes myCli;
        private DS_PushMoneyPorPagar myPxp;

        public PushMoneyPorPagarDetalleFormats()
        {
            myCli = new DS_Clientes();
            myPxp = new DS_PushMoneyPorPagar();
        }

        public void Print(PushMoneyPorPagar documento, PrinterManager printer)
        {
            this.printer = printer;
            Formato1(documento);
        }

        private void Formato1(PushMoneyPorPagar documento)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cliente = myCli.GetClienteById(documento.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente");
            }

            printer.PrintEmpresa();
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("PUSHMONEY POR PAGAR");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("RNC: " + cliente.CliRNC);
            printer.DrawText("Calle: " + cliente.CliCalle, 48);

            printer.DrawText("No. Documento: " + documento.PxpDocumento);
            printer.DrawText("Referencia: " + documento.PxpReferencia);
            printer.DrawText("NCF: " + documento.PxpNCF);
            printer.DrawText("Fecha doc: " + documento.PxpFecha);
            printer.DrawText("Monto total: " + Math.Abs(documento.PxpMontoTotal));

            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
            printer.Bold = true;
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            printer.DrawText("Cant.     Precio     Desc.    Itbis    Importe");
            printer.DrawLine();
            printer.Bold = false;
            //}

            double subTotal = 0.0;
            double itbisTotal = 0.0;
            double descTotal = 0.0;

            var productos = myPxp.GetProductosFromPushMoneyxPagarDetalle(documento.PxpReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var valorneto = (pro.ProPrecio * pro.ProCantidad);
                    var desc1 = valorneto * (pro.ProDescuentoMaximo / 100);
                    var itbis = (valorneto - desc1) * (pro.ProItbis / 100);
                    var itbis1 = (valorneto) * (pro.ProItbis / 100);
                    var precioNeto = valorneto + itbis1;
                    var desc = (valorneto + itbis1) * (pro.ProDescuentoMaximo / 100);

                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }
                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2") +
                        pro.ProPrecio.ToString("N2").PadLeft(12) +
                        desc.ToString("N2").PadLeft(9) +
                        itbis.ToString("N2").PadLeft(10) +
                        precioNeto.ToString("N2").PadLeft(11));

                    subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis;
                    descTotal += desc;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total   : " + Math.Abs(documento.PxpMontoSinItbis).ToString("N2").PadLeft(10));
            printer.DrawText("Descuento   : " + Math.Abs(descTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Total       : " + Math.Abs(documento.PxpMontoTotal).ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("");
            
            printer.DrawText("Formato PushMoneyxPagar 1: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }
    }
}

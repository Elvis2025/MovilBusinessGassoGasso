using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    class ColocacionProductosFormats : IPrinterFormatter
    {
        private DS_ColocacionProductos myColProd;
        private DS_Clientes clientes;
        private PrinterManager _printer;
        public ColocacionProductosFormats(DS_ColocacionProductos colProd)
        {
            myColProd = colProd;
            clientes = new DS_Clientes();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            _printer = printer;

            var formatos = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionColocacionProductos();

            switch (formatos)
            {
                default:
                    Formato1(traSecuencia, confirmado);
                    break;
            }

        }

        private void Formato1(int invfisSecuencia, bool Confirmado)
        {
            int cont = 0;
            if (_printer == null || !_printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            _printer.PrintEmpresa(Notbold: true);

            var invfisico = myColProd.GetColocacionBySecuencia(invfisSecuencia);

            var cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.Bold = false;
            _printer.DrawText("");
            _printer.DrawText("Fecha: " + invfisico.ColFecha);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.TextAlign = Justification.CENTER;
            _printer.Font = PrinterFont.TITLE;
            _printer.DrawText("COLOCACION DE MERCANCIAS");
            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            _printer.DrawText("Codigo: " + cliente.CliCodigo);
            _printer.DrawText("Orden #: " + invfisSecuencia);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawLine();
            _printer.DrawText("Codigo - Descripcion");
            _printer.DrawText("CantidadGondola  CantidadAlmacen   CantidadTotal");
            _printer.DrawLine();

            int total = 0;

            foreach (var det in myColProd.GetColocacionProductosDetalles(invfisSecuencia))
            {
                bool iscantidadNull = det.ColCantidad == null;
                bool iscantidadDetalleNull = det.ColCantidadDetalle == null;

                total = (int)((iscantidadNull ? 0 : det.ColCantidad)
                + (iscantidadDetalleNull ? 0 : (det.ColCantidadDetalle * det.ProUnidades)));

                cont++;
                _printer.DrawText(det.ProCodigo + " - " + (!string.IsNullOrEmpty(det.ProDescripcion) && det.ProDescripcion.Length > 33 ?
                    det.ProDescripcion.Substring(0, 33) : det.ProDescripcion));
                _printer.DrawText(det.ColCantidad.ToString() + det.ColCantidadDetalle.ToString().PadLeft(29)
                    + total.ToString().PadLeft(16));
                _printer.DrawText("");
            }

            _printer.DrawLine();
            _printer.TextAlign = Justification.LEFT;
            _printer.DrawText("");
            _printer.DrawText("SKU: " + cont);
            _printer.DrawText("Fecha Impresion: " + DateTime.Now.ToString("dd/MM/yyyy"));
            _printer.DrawText("");
            _printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            _printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            _printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            _printer.Font = PrinterFont.FOOTER;
            _printer.DrawText("");
            _printer.DrawText("Formato ColocacionMercancia 1 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }
    }
}

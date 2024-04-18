using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    class InventarioFisicoFormats : IPrinterFormatter
    {

        DS_InventariosFisicos _invfisic;
        DS_Clientes clientes;
        PrinterManager _printer;
        private DS_UsosMultiples myUsosMul;
        private DS_Productos myProd;
        public InventarioFisicoFormats(DS_InventariosFisicos invfisic)
        {
            _invfisic = invfisic;
            clientes = new DS_Clientes();
            myUsosMul = new DS_UsosMultiples();
            myProd = new DS_Productos();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            _printer = printer;

            var formatos = DS_RepresentantesParametros.GetInstance().GetFormatoInventarioFisico();

            switch (formatos)
            {
                default:
                    Formato1(traSecuencia, confirmado);
                    break;
                case 2:
                    Formato2(traSecuencia, confirmado);
                    break;
                case 3: //tabacalera
                    Formato3(traSecuencia, confirmado);
                    break;
                case 4: //Inventario Consignacion Pharmatech
                    Formato4(traSecuencia, confirmado);
                    break;
                case 5: //Inventario Fisico Con Area en Cabecera
                    Formato5(traSecuencia, confirmado);
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

            var invfisico = _invfisic.GetInventarioBySecuencia(invfisSecuencia);

            Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.Bold = false;
            _printer.DrawText("");
            _printer.DrawText("Fecha: " + invfisico.infFecha);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.TextAlign = Justification.CENTER;
            _printer.Font = PrinterFont.TITLE;
            _printer.DrawText("INVENTARIO FISICO");
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

            foreach (var det in _invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
            {
                bool iscantidadNull = det.infCantidad == null;
                bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                total = (int)((iscantidadNull ? 0 : det.infCantidad)
                + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                cont++; 
                _printer.DrawText(det.ProCodigo + " - " + (!string.IsNullOrEmpty(det.ProDescripcion) && det.ProDescripcion.Length > 33 ? 
                    det.ProDescripcion.Substring(0,33) : det.ProDescripcion));
                _printer.DrawText(det.infCantidad.ToString() + det.infCantidadDetalle.ToString().PadLeft(29)
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
            _printer.DrawText("Formato InventarioFisico 1 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }

        private void Formato2(int invfisSecuencia, bool Confirmado)
        {
            int cont = 0;
            if (_printer == null || !_printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            _printer.PrintEmpresa();

            var invfisico = _invfisic.GetInventarioBySecuencia(invfisSecuencia);

            Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.TextAlign = Justification.CENTER;
            _printer.Font = PrinterFont.TITLE;
            _printer.DrawText("INVENTARIO FISICO");
            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.Bold = false;
            _printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            _printer.DrawText("Codigo: " + cliente.CliCodigo);
            _printer.DrawText("Orden #: " + invfisSecuencia);
            _printer.DrawText("Fecha: " + invfisico.infFecha);
            _printer.DrawText("");
            _printer.DrawText("--------------------------------");
            _printer.DrawText("Codigo - Descripcion");
            _printer.DrawText("CantGond    CantAlm    CantTotal");
            _printer.DrawText("--------------------------------");

            int total = 0;

            foreach (var det in _invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
            {
                bool iscantidadNull = det.infCantidad == null;
                bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                total = (int)((iscantidadNull ? 0 : det.infCantidad)
                + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                cont++;
                _printer.DrawText(det.ProCodigo + " - " + (!string.IsNullOrEmpty(det.ProDescripcion) && det.ProDescripcion.Length > 60 ?
                    det.ProDescripcion.Substring(0, 60) : det.ProDescripcion));
                _printer.DrawText(det.infCantidad.ToString().PadRight(12) + det.infCantidadDetalle.ToString().PadRight(10)
                    + total.ToString().PadLeft(10));
                _printer.DrawText("");
            }

            _printer.DrawText("--------------------------------");
            _printer.TextAlign = Justification.LEFT;
            _printer.DrawText("SKU: " + cont);
            _printer.DrawText("Fecha Impresion: " + DateTime.Now.ToString("dd/MM/yyyy"));
            _printer.DrawText("");
            _printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            _printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            _printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            _printer.Font = PrinterFont.FOOTER;
            _printer.DrawText("");
            _printer.DrawText("Formato InventarioFisico 2 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }

        private void Formato3(int invfisSecuencia, bool Confirmado)
        {
            int cont = 0;
            if (_printer == null || !_printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            _printer.PrintEmpresa(Notbold: true);

            var invfisico = _invfisic.GetInventarioBySecuencia(invfisSecuencia);

            Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.Bold = false;
            _printer.DrawText("");
            _printer.DrawText("Fecha: " + invfisico.infFecha);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.TextAlign = Justification.CENTER;
            _printer.Font = PrinterFont.TITLE;
            _printer.DrawText("INVENTARIO FISICO");
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
            _printer.DrawText("Cantidad         Unidades       CantidadTotal");
            _printer.DrawLine();

            int total = 0;

            foreach (var det in _invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
            {
                bool iscantidadNull = det.infCantidad == null;
                bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                total = (int)((iscantidadNull ? 0 : det.infCantidad)
                + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                cont++;
                _printer.DrawText(det.ProCodigo + " - " + (!string.IsNullOrEmpty(det.ProDescripcion) && det.ProDescripcion.Length > 33 ?
                    det.ProDescripcion.Substring(0, 33) : det.ProDescripcion));
                _printer.DrawText(det.infCantidad?.ToString("N2") + det.infCantidadDetalle?.ToString("N2").PadLeft(17)
                    + total.ToString("N2").PadLeft(15));
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
            _printer.DrawText("Formato InventarioFisico 3 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }

        private void Formato4(int invfisSecuencia, bool confirmado)
        {
            if (_printer == null || !_printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var invfisico = _invfisic.GetInventarioBySecuencia(invfisSecuencia);

            Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (invfisico == null)
            {
                throw new Exception("Error cargando los datos del inventario fisico");
            }

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.PrintEmpresa(Notbold: true);
            _printer.DrawText("");

            if (!_printer.IsEscPos)
            {
                _printer.DrawText("");
            }

            _printer.Font = PrinterFont.TITLE;
            _printer.Bold = true;
            _printer.TextAlign = Justification.CENTER;
            _printer.DrawText("INVENTARIO FISICO");
            if (!_printer.IsEscPos)
            {
                _printer.DrawText("");
            }
            _printer.Bold = false;
            _printer.Font = PrinterFont.BODY;
            _printer.TextAlign = Justification.LEFT;
            if (!_printer.IsEscPos)
            {
                _printer.DrawText("");
            }
            _printer.DrawText("");
            _printer.DrawText("Inventario #: " + Arguments.CurrentUser.RepCodigo + "-"+ invfisSecuencia);
            _printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            _printer.DrawText("Codigo: " + cliente.CliCodigo);
            _printer.DrawText("Fecha inventario: " + invfisico.infFecha);
            _printer.DrawText("");
            _printer.TextAlign = Justification.CENTER;
            _printer.Bold = true;
            _printer.DrawText("DETALLES");
            _printer.TextAlign = Justification.LEFT;
            _printer.Bold = false;
            _printer.DrawLine();
            _printer.Bold = true;
            _printer.DrawText("Codigo-Descripcion      ");
            _printer.DrawText("Logica           Fisica          Diferencia");
            _printer.Bold = false;
            _printer.DrawLine();

            List<InventarioFisicoDetalle> productos = _invfisic.GetInventarioFisicoDetalles(invfisSecuencia).GroupBy(x => x.ProID).Select(x => x.FirstOrDefault()).ToList();

            double MontoDiferencia = 0.0;
            double MontoDiferenciaCaja = 0.0;
            double DiferenciaCaja = 0;

            foreach (var prod in productos)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;
                _printer.DrawText(desc);

                var productosDetalle = _invfisic.GetInventarioFisicoDetallesByProID(invfisSecuencia,prod.ProID);

                foreach (var detalle in productosDetalle)
                {
                    var cantidadLogica = detalle.infCantidadLogica.ToString() + "/" + 0;
                    double cantidad = 0;
                    double unidadesFisica = 0;
                    double TotalCaja = 0;
                    double DiferenciaUnidades = 0;

                    if (detalle.infCantidad > 0 || detalle.infCantidadDetalle > 0)
                    {
                        cantidad = myProd.ConvertirUnidadesACajas(
                        myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(detalle.ProID),
                        (double)detalle.infCantidad, (double)detalle.infCantidadDetalle), myProd.GetProUnidades(detalle.ProID));

                        unidadesFisica = Math.Round((cantidad - (int)cantidad) * myProd.GetProUnidades(detalle.ProID), 0);

                        TotalCaja = myProd.ConvertirUnidadesACajas(
                        myProd.ConvertirCajasAunidades((double)detalle.infCantidadLogica, 0, myProd.GetProUnidades(detalle.ProID),
                        (double)detalle.infCantidad, (double)detalle.infCantidadDetalle), myProd.GetProUnidades(detalle.ProID));

                        DiferenciaCaja = TotalCaja;
                        DiferenciaUnidades = Math.Round((DiferenciaCaja - DiferenciaCaja) * myProd.GetProUnidades(detalle.ProID), 0);
                    }
                    else
                    {
                        DiferenciaCaja = -(double)detalle.infCantidadLogica;
                        DiferenciaUnidades = 0;
                    }

                    //double itbis = double.Parse(prod.Itbis.ToString()) / 100;

                    MontoDiferencia += DiferenciaUnidades;
                    MontoDiferenciaCaja += DiferenciaCaja;

                    _printer.DrawText("Lote: " + detalle.infLote);

                    _printer.DrawText(cantidadLogica.PadRight(17) + (cantidad + "/" + unidadesFisica).PadRight(17)
                        + DiferenciaCaja + "/" + DiferenciaUnidades);

                }
            }

            
            if (productos == null || productos.Count == 0)
            {
                _printer.DrawText("---------------- No hay Detalle ----------------");
            }
            _printer.DrawText("------------------------------------------------");

            List<InventarioFisicoDetalle> ProductosSobrantes = _invfisic.GetProductosSobrantes(invfisSecuencia).GroupBy(x => x.ProID).Select(x => x.FirstOrDefault()).ToList();

            if (ProductosSobrantes != null && ProductosSobrantes.Count > 0)
            {
                _printer.DrawText("");
                _printer.DrawText("PRODUCTOS CON SOBRANTES");
                _printer.DrawText("");
                _printer.DrawText("------------------------------------------------");
                _printer.DrawText("Codigo-Descripcion      ");
                _printer.DrawText("Logica           Fisica          Diferencia");
                _printer.DrawText("------------------------------------------------");
                _printer.DrawText(" ");
                var Largo = 35;

                foreach (var myInvFis in ProductosSobrantes)
                {
                    if (myInvFis.ProDescripcion.Length < 35)
                    {
                        Largo = myInvFis.ProDescripcion.Length;
                    }
                    else
                    {
                        Largo = 35;
                    }

                    string codigo = myInvFis.ProCodigo;
                    string nombre = myInvFis.ProDescripcion;

                    _printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));

                    var productosDetalle = _invfisic.GetProductosSobrantesByProId(invfisSecuencia, myInvFis.ProID);
                    foreach (var myInvDetalle in productosDetalle)
                    {
                        string CantidadLogica = myInvDetalle.infCantidadLogica.ToString() + "/" + 0;
                        string CantidadFisica = myInvDetalle.infCantidad + "/" + myInvDetalle.infCantidadDetalle;

                        double cantidadSobrante = (double)myInvDetalle.infCantidadLogica - (double)myInvDetalle.infCantidad;
                        double cantidadSobranteDetalle = (double)myInvDetalle.infCantidadDetalle - 0.00;

                        var TotalCaja = myProd.ConvertirUnidadesACajas(
                            myProd.ConvertirCajasAunidades((double)myInvDetalle.infCantidadLogica, 0, myProd.GetProUnidades(myInvDetalle.ProID),
                            (double)myInvDetalle.infCantidad, (double)myInvDetalle.infCantidadDetalle), myProd.GetProUnidades(myInvDetalle.ProID));

                        DiferenciaCaja = TotalCaja;
                        var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(myInvDetalle.ProID), 2);

                        _printer.DrawText("Lote: " + myInvDetalle.infLote);
                        _printer.DrawText(CantidadLogica.ToString().PadRight(15)
                                + CantidadFisica.ToString().PadRight(15) +
                               (DiferenciaCaja + "/" + DiferenciaUnidades).PadLeft(9));
                    }
                }

            }

            if (ProductosSobrantes == null)
            {
                _printer.DrawText("---------------- No hay Detalle ----------------");
            }

            _printer.DrawLine();
            _printer.TextAlign = Justification.LEFT;
            _printer.DrawText("");
            _printer.DrawText("SKU: " + productos.Count.ToString());
            _printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            _printer.DrawText("");
            _printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            _printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            _printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            _printer.Font = PrinterFont.FOOTER;
            _printer.DrawText("");
            _printer.DrawText("Formato InventarioFisico 4 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }

        private void Formato5(int invfisSecuencia, bool Confirmado)
        {
            int cont = 0;
            if (_printer == null || !_printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            _printer.PrintEmpresa(Notbold: true);

            var invfisico = _invfisic.GetInventarioBySecuencia(invfisSecuencia, Confirmado);

            Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

            if (cliente == null)
            {
                throw new Exception("No se encontraron los datos del cliente");
            }

            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.Bold = false;
            _printer.DrawText("");
            _printer.DrawText("Fecha: " + invfisico.infFecha);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.TextAlign = Justification.CENTER;
            _printer.Font = PrinterFont.TITLE;
            _printer.DrawText("INVENTARIO FISICO");
            _printer.TextAlign = Justification.LEFT;
            _printer.Font = PrinterFont.BODY;
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            _printer.DrawText("Codigo: " + cliente.CliCodigo);
            _printer.DrawText("Orden #: " + invfisSecuencia);
            _printer.DrawText("Area: " + invfisico.InvAreaDescr);
            _printer.DrawText("");
            _printer.DrawText("");
            _printer.DrawLine();
            _printer.DrawText("Codigo - Descripcion");
            _printer.DrawText("Cantidad         Unidades       CantidadTotal");
            _printer.DrawLine();

            int total = 0;

            foreach (var det in _invfisic.GetInventarioFisicoDetalles(invfisSecuencia,Confirmado))
            {
                bool iscantidadNull = det.infCantidad == null;
                bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                total = (int)((iscantidadNull ? 0 : det.infCantidad)
                + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                cont++;
                _printer.DrawText(det.ProCodigo + " - " + (!string.IsNullOrEmpty(det.ProDescripcion) && det.ProDescripcion.Length > 33 ?
                    det.ProDescripcion.Substring(0, 33) : det.ProDescripcion));
                _printer.DrawText(det.infCantidad?.ToString("N2") + det.infCantidadDetalle?.ToString("N2").PadLeft(17)
                    + total.ToString("N2").PadLeft(15));
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
            _printer.DrawText("Formato InventarioFisico 5 : movilbusiness " + Functions.AppVersion);
            _printer.DrawText("");
            _printer.Print();

        }

    }
}

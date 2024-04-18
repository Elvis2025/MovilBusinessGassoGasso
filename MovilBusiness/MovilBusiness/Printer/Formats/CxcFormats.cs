
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MovilBusiness.printers.formats
{
    public class CxcFormatos
    {
        private PrinterManager printer;
        private DS_CuentasxCobrar myCxc;
        private DS_Clientes myCli;

        public CxcFormatos(DS_CuentasxCobrar myCxc) 
        {
            this.myCxc = myCxc;
            myCli = new DS_Clientes();
        }

        public void PrintEstadoCuentas(int cliid, PrinterManager printer)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEstadosCuentas())
            {
                case 1:
                case 2: //disfarmacos
                    FormatoEstadoCuenta2(cliid);
                    break;
                case 3: //feltrex - citizen
                    FormatoEstadoCuenta3(cliid);
                    break;
                case 4: //feltrex - zebra
                    FormatoEstadoCuenta4(cliid);
                    break;
                case 5: //acromax
                    FormatoEstadoCuenta5(cliid);
                    break;
                case 8://lam
                    FormatoEstadoCuenta8(cliid);
                    break;
                case 9://GASSO
                    FormatoEstadoCuenta9(cliid);
                    break;
                default:
                    FormatoEstadoCuenta1(cliid);
                    break;
                
            }
        }

        public void ImprimirDetalleDocumento(CuentasxCobrar documento, PrinterManager printer)
        {
            this.printer = printer;
            int Formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNotaCredito();
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNotaCredito())
            {
                default:
                case 1:
                    FormatoNC1(documento);
                    break;
                case 2://cano
                    FormatoNC2(documento);
                    break;
                case 3: //Feltrex
                    FormatoNC3(documento);
                    break;
                case 4: //Mgomez
                    FormatoNC4(documento);
                    break;
                case 5: //Infalab
                    FormatoNC5(documento);
                    break;
                case 6:// Garmenteros
                    FormatoNC6(documento);
                    break;
                case 7:// LAM
                    FormatoNC7(documento);
                    break;
                case 8:// GASSO
                    FormatoNC8(documento);
                    break;
            }

        }

        private void FormatoNC1(CuentasxCobrar documento)
        {
            if(printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cliente = myCli.GetClienteById(documento.CliID);

            if(cliente == null)
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
            if(documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
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

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + Math.Abs(documento.CxcMontoTotal));

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

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if(productos != null && productos.Count > 0)
            {
                foreach(var pro in productos)
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
            printer.DrawText("Sub Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString("N2").PadLeft(10));
            printer.DrawText("Descuento   : " + Math.Abs(descTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Esta Nota de credito tiene una validez de 180 dias a partir de la fecha de emision ", 45);          
            printer.DrawText("Formato documentos 1: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC2(CuentasxCobrar documento)
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

            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.TITLE;
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawBarcode("128", documento.CxcReferencia, "H");

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("RNC: " + cliente.CliRNC);
            printer.DrawText("Calle: " + cliente.CliCalle, 48);

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + documento.CxcMontoTotal);

            printer.DrawText("");
            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
                printer.Bold = true;
                printer.DrawLine();
                printer.DrawText("Codigo-Descripcion");
                printer.DrawText("Cant.       Precio       Itbis         Importe");
                printer.DrawLine();
                printer.Bold = false;
            //}

            //var subTotal = 0.0;
            var itbisTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                    var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;

                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                        pro.ProPrecio.ToString("N2").PadRight(13)
                        + itbis.ToString("N2").PadRight(14)
                        + precioNeto.ToString("N2").PadRight(10));

                    //subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis * pro.ProCantidad;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
            printer.DrawText("Itbis    : " + itbisTotal.ToString("N2").PadLeft(10));
            printer.DrawText("Total    : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep. : " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("Leyenda Explicativas: ");
            printer.DrawText("AB: ABONO");
            printer.DrawText("FT: FACTURA");
            printer.DrawText("NC: NOTA DE CREDITO");
            printer.DrawText("CK: CARGO POR CHEQUE DEVUELTO");
            printer.DrawText("");
            printer.DrawText("Formato documentos 2: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC3(CuentasxCobrar documento)
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
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
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

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + documento.CxcMontoTotal);

            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
            printer.Bold = true;
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            printer.DrawText("Cant.       Precio       Itbis         Importe");
            printer.DrawLine();
            printer.Bold = false;
            //}

            //var subTotal = 0.0;
            var itbisTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                    var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;
                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }
                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                        pro.ProPrecio.ToString("N2").PadRight(13)
                        + itbis.ToString("N2").PadRight(14)
                        + precioNeto.ToString("N2").PadRight(10));
                    printer.DrawText("");

                    //subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis * pro.ProCantidad;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
            printer.DrawText("Itbis    : " + itbisTotal.ToString("N2").PadLeft(10));
            printer.DrawText("Total    : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");

            printer.DrawText("Formato documentos 3: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC4(CuentasxCobrar documento)
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
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
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

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + Math.Abs(documento.CxcMontoTotal));

            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
            printer.Bold = true;
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            //printer.DrawText("Cant.       Precio       Itbis          Importe");
            printer.DrawText("Cant.     Precio     Desc     Itbis    Importe");
            printer.DrawLine();
            printer.Bold = false;
            //}

            double subTotal = 0.0;
            double itbisTotal = 0.0;
            double descTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var valorneto = (pro.ProPrecio * pro.ProCantidad) - pro.ProDescuentoMaximo;
                    var itbis = valorneto * (pro.ProItbis / 100);
                    var precioNeto = valorneto + itbis;

                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }
                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2") +
                        pro.ProPrecio.ToString("N2").PadLeft(12) +
                        pro.ProDescuentoMaximo.ToString("N2").PadLeft(9) + 
                        itbis.ToString("N2").PadLeft(10) +
                        precioNeto.ToString("N2").PadLeft(11));

                    subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis;
                    descTotal += pro.ProDescuentoMaximo;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total   : " + Math.Abs(subTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Descuento   : " + Math.Abs(descTotal).ToString().PadLeft(10));
            printer.DrawText("Neto S/ITBIS: " + Math.Abs(subTotal - descTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Esta Nota de credito tiene una validez de 180 dias a partir de la fecha de emision ", 45);
            printer.DrawText("Formato documentos 4: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }
        private void FormatoNC5(CuentasxCobrar documento)
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
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
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

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Referencia: " + documento.cxcReferencia2);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + Math.Abs(documento.CxcMontoTotal));

            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
            printer.Bold = true;
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            //printer.DrawText("Cant.       Precio       Itbis          Importe");
            printer.DrawText("Cant.     Precio     Desc     Itbis    Importe");
            printer.DrawLine();
            printer.Bold = false;
            //}

            double subTotal = 0.0;
            double itbisTotal = 0.0;
            double descTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var valorneto = (pro.ProPrecio * pro.ProCantidad) - pro.ProDescuentoMaximo;
                    var itbis = valorneto * (pro.ProItbis / 100);
                    var precioNeto = valorneto + itbis;

                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }
                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2") +
                        pro.ProPrecio.ToString("N2").PadLeft(12) +
                        pro.ProDescuentoMaximo.ToString("N2").PadLeft(9) +
                        itbis.ToString("N2").PadLeft(10) +
                        precioNeto.ToString("N2").PadLeft(11));

                    subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis;
                    descTotal += pro.ProDescuentoMaximo;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString("N2").PadLeft(10));
            printer.DrawText("Descuento   : " + Math.Abs(descTotal).ToString().PadLeft(10));
            printer.DrawText("Imp Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString().PadLeft(10));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Esta Nota de credito tiene una validez de 180 dias a partir de la fecha de emision ", 45);
            printer.DrawText("Formato documentos 5: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC6(CuentasxCobrar documento)
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
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.DrawText("Cliente: " + cliente.CliNombreComercial, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("RNC: " + cliente.CliRNC);
            printer.DrawText("Calle: " + cliente.CliCalle, 48);

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + Math.Abs(documento.CxcMontoTotal));

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

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

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
            printer.DrawText("Sub Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString("N2").PadLeft(10));
            printer.DrawText("Descuento   : " + Math.Abs(descTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(10));
            printer.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Esta Nota de credito tiene una validez de 180 dias a partir de la fecha de emision ", 45);
            printer.DrawText("Formato documentos 6: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC7(CuentasxCobrar documento)
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
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
            }
            else
            {
                printer.Font = PrinterFont.BODY;
                printer.DrawText("COPIA");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("FACTURA CON VALOR FISCAL");
            }
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

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                printer.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
            }
            else
            {
                printer.DrawText("No. Factura: " + documento.CxcDocumento);
                printer.DrawText("Referencia: " + documento.CxcReferencia);
            }
            printer.DrawText("NCF: " + documento.CXCNCF);
            printer.DrawText("Fecha doc: " + documento.CxcFecha);
            printer.DrawText("Monto total: " + documento.CxcMontoTotal);
            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("NOTA: " + documento.cxcComentario, 48);
            }

            //if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
            //{
            printer.Bold = true;
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            printer.DrawText("Cant.       Precio       Itbis         Importe");
            printer.DrawLine();
            printer.Bold = false;
            //}

            //var subTotal = 0.0;
            var itbisTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                    var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;
                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }
                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion, 48);
                    printer.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                        pro.ProPrecio.ToString("N2").PadRight(13)
                        + itbis.ToString("N2").PadRight(14)
                        + precioNeto.ToString("N2").PadRight(10));
                    printer.DrawText("");

                    //subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis * pro.ProCantidad;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
            printer.DrawText("Itbis    : " + itbisTotal.ToString("N2").PadLeft(10));
            printer.DrawText("Total    : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
            printer.DrawText("");

            printer.DrawText("Formato documentos 7: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.Print();
        }

        private void FormatoNC8(CuentasxCobrar documento)
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

            printer.TextAlign = Justification.CENTER;
            printer.PrintEmpresa(TitleNotBold: true);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.DrawText("NOTA DE CREDITO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("No. Nota Credito: " + documento.CxcDocumento);
            printer.DrawText("NCF             : " + documento.CXCNCF);
            printer.DrawText("NCF modificado  : " + documento.CXCNCFAfectado);
            printer.DrawText("Fecha Doc       : " + documento.CxcFecha);
            printer.DrawText("Monto Total     : $" + Math.Abs(documento.CxcMontoTotal).ToString("N2"));

            printer.DrawText("");

            printer.DrawText("DATOS DEL CLIENTE");
            printer.DrawText("Cliente         : " + cliente.CliNombre, 48);
            printer.DrawText("Codigo          : " + cliente.CliCodigo);
            printer.DrawText("RNC             : " + cliente.CliRNC);
            printer.DrawText("Calle           : " + cliente.CliCalle, 48);

            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Descripcion Producto");
            printer.DrawText("Cant.       Precio       Itbis         Total");
            printer.DrawLine();
            printer.Bold = false;
 
            double subTotal = 0.0;
            double itbisTotal = 0.0;
            double descTotal = 0.0;

            var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

            if (productos != null && productos.Count > 0)
            {
                foreach (var pro in productos)
                {

                    var valorneto = (pro.ProPrecio * pro.ProCantidad);
                    var desc1 = valorneto * (pro.ProDescuentoMaximo / 100);
                    var itbis = (valorneto - desc1) * (pro.ProItbis / 100);
                    var itbis1 = (valorneto) * (pro.ProItbis / 100);
                    var precioNeto = valorneto + itbis1;
                    var precioNetoString = "$" + precioNeto.ToString("N2");
                    var desc = (valorneto + itbis1) * (pro.ProDescuentoMaximo / 100);

                    if (pro.ProDescripcion.Length > 37)
                    {
                        pro.ProDescripcion = pro.ProDescripcion.Substring(0, 36);
                    }

                    printer.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion,48);
                    printer.DrawText(pro.ProCantidad.ToString().PadRight(12) + "$" +
                        pro.ProPrecio.ToString("N2").PadRight(12) + "$" +
                        itbis.ToString("N2").PadRight(10) +
                        precioNetoString.PadLeft(11));

                    subTotal += pro.ProPrecio * pro.ProCantidad;
                    itbisTotal += itbis;
                    descTotal += desc;
                }
            }

            printer.DrawLine();
            printer.DrawText("Sub Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString("N2").PadLeft(33));
            printer.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(33));
            printer.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(33));
            printer.DrawText("");

            if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawLine();
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
            }
            printer.DrawText("");
            printer.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.DrawText("Esta Nota de credito tiene una validez de 180 dias a partir de la fecha de emision ", 45);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato documentos 8: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.Print();
        }
        private void FormatoEstadoCuenta1(int cliid)
        {
            if(printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if(cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ESTADO DE CUENTA");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Cliente: "+cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawLine();
            printer.DrawText("Fecha       Sigla Num.           Balance");
            printer.DrawText("Monto");
            printer.DrawLine();

            double BalanceTotal = 0;

            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);
            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2))
                {
                    printer.DrawText(cxc.CxcFecha + cxc.CxcSIGLA.PadLeft(5) + cxc.CxcDocumento.PadLeft(13) + cxc.CxcBalance.ToString("N2").PadLeft(12));


                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                    printer.DrawText(cxc.CxcMontoTotal.ToString("N2"));

                }
                printer.DrawLine();
                printer.TextAlign = Justification.RIGHT;
                printer.DrawText("Total: " + Math.Abs(BalanceTotal).ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 1");
            printer.DrawText("");
            printer.Print();

        }

        // G: Pending
        private void FormatoEstadoCuenta2(int cliid)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ESTADO DE CUENTA");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Nombre comercial: " + cliente.CliNombreComercial, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm:ss"));
            printer.TextAlign = Justification.CENTER;
            printer.DrawLine();
            printer.DrawText("Fecha   Sigla-Num.     Dias        Balance");
            printer.DrawText("Monto");
            printer.DrawLine();

            double BalanceTotal = 0;
            int sourestcu = DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta();
            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);

            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in sourestcu == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, sourestcu == 2))
                {
                    if(cxc.CxcSIGLA.ToUpper() == "NC")
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(7) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }
                    else if(cxc.CxcSIGLA.ToUpper() == "RCB")
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(6) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }
                    else
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(6) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }


                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                    printer.DrawText(cxc.CxcMontoTotal.ToString("N2"));

                }
                printer.DrawLine();
                printer.TextAlign = Justification.RIGHT;
                printer.DrawText("Total: " + BalanceTotal.ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 2");
            printer.DrawText("");
            printer.Print();

        }

        private void FormatoEstadoCuenta3(int cliid)
        {
            int counter = 0;
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa(Notbold:true);

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            //printer.Bold = true;
            printer.DrawText("E S T A D O  D E  C U E N T A");
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Fecha  : " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            //printer.DrawText("");
            if (cliente.CliNombre.Length > 45)
            {
                cliente.CliNombre = cliente.CliNombre.Substring(0, 45);
            }
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            //printer.DrawText("");
            if (cliente.CliCalle.Length > 35)
            {
                cliente.CliCalle = cliente.CliCalle.Substring(0, 35);
            }
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Fecha      Doc Num.          Dias        Monto");
            //printer.DrawText("");
            printer.DrawLine();

            double BalanceTotal = 0;

            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);
            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta()==1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2))
                {
                    counter++; 
                    printer.DrawText(cxc.CxcFecha.PadRight(11) + cxc.CxcSIGLA.PadRight(4) + cxc.CxcDocumento.PadRight(14)  + cxc.CxcBalance.ToString("N2").PadLeft(12));

                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                    printer.DrawText("");
                }

                printer.DrawLine();
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("Cantidad   : " + counter.ToString("N2"));
                //printer.DrawText("");
                printer.DrawText("Monto Total:  $" + BalanceTotal.ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular : " + Arguments.CurrentUser.RepTelefono2);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);

            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 3");
            printer.DrawText("");
            printer.Print();

        }

        private void FormatoEstadoCuenta4(int cliid)
        {
            int counter = 0;
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            //printer.Bold = true;
            printer.DrawText("E S T A D O  D E  C U E N T A");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Fecha  : " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            //printer.DrawText("");
            if (cliente.CliNombre.Length > 45)
            {
                cliente.CliNombre = cliente.CliNombre.Substring(0, 45);
            }
            cliente.CliNombre = cliente.CliNombre.TrimEnd(' ');
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            //printer.DrawText("");
            if (cliente.CliCalle.Length > 35)
            {
                cliente.CliCalle = cliente.CliCalle.Substring(0, 35);
            }
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Fecha      Doc Num.          Dias        Monto");
            printer.DrawLine();

            double BalanceTotal = 0;

            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);
            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta()==1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2))
                {
                    counter++; 
                    printer.DrawText(cxc.CxcFecha.PadRight(11) + cxc.CxcSIGLA.PadRight(4) + cxc.CxcDocumento.PadRight(14) + cxc.CxcDias.ToString().PadLeft(4) + cxc.CxcBalance.ToString("N2").PadLeft(14));

                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                    printer.DrawText("");
                }

                printer.DrawLine();
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("Cantidad   : " + counter.ToString("N2"));
                //printer.DrawText("");
                printer.DrawText("Monto Total:  $" + BalanceTotal.ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular : " + Arguments.CurrentUser.RepTelefono2);
            //printer.DrawText("");
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);

            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 4");
            printer.DrawText("");
            printer.Print();

        }
        private void FormatoEstadoCuenta5(int cliid)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ESTADO DE CUENTA");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Nombre comercial: " + cliente.CliNombreComercial, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm:ss"));
            printer.TextAlign = Justification.CENTER;
            printer.DrawLine();
            printer.DrawText("Fecha   Sigla-Num.     Dias        Balance");
            printer.DrawLine();

            double BalanceTotal = 0;
            int sourestcu = DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta();
            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);

            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in sourestcu == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, sourestcu == 2))
                {
                    if (cxc.CxcSIGLA.ToUpper() == "NC")
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(7) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }
                    else if (cxc.CxcSIGLA.ToUpper() == "RCB")
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(6) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }
                    else
                    {
                        printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA + "-" + cxc.CxcDocumento + cxc.CxcDias.ToString().PadLeft(6) + " " + cxc.CxcBalance.ToString("N2").PadLeft(13));
                    }


                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                }
                printer.DrawLine();
                printer.TextAlign = Justification.RIGHT;
                printer.DrawText("Total: " + BalanceTotal.ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 5");
            printer.DrawText("");
            printer.Print();

        }

        private void FormatoEstadoCuenta8(int cliid)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.PrintEmpresa(Notbold:true);

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("ESTADO DE CUENTA");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawLine();
            printer.DrawText("Fecha       Sigla Num.           Balance");
            printer.DrawLine();

            double BalanceTotal = 0;

            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);
            foreach (var moneda in monedas)
            {
                if (monedas.Count > 1)
                {
                    printer.Bold = true;
                    printer.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                    printer.Bold = false;
                }

                foreach (CuentasxCobrar cxc in DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() ==1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2))
                {
                    printer.DrawText(cxc.CxcFecha.PadRight(12) + cxc.CxcSIGLA.PadRight(6) + cxc.CxcDocumento.PadRight(15) + cxc.CxcBalance.ToString("N2").PadRight(19));

                    if (cxc.Origen == 1)
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                }
                printer.DrawLine();
                printer.TextAlign = Justification.RIGHT;
                printer.DrawText("Total: " + BalanceTotal.ToString("N2"));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato estado de cuentas 8");
            printer.DrawText("");
            printer.Print();

        }

        private void FormatoEstadoCuenta9(int cliid)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Clientes cliente = myCli.GetClienteById(cliid);

            if (cliente == null)
            {
                throw new Exception("Error cargando datos del cliente!");
            }

            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.CENTER;
            printer.Bold = false;
            printer.PrintEmpresa(TitleNotBold: true);
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliid, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2);
            printer.DrawText("E S T A D O  D E  C U E N T A");
            printer.DrawText("Moneda: " + monedas[0].MonSigla);
            printer.DrawText("Fecha: " + Functions.CurrentDate("dd-MM-yyyy HH:mm:ss"));
            printer.DrawText("");
            printer.DrawText("Codigo : " + cliente.CliCodigo);
            printer.DrawText("Cliente: " + cliente.CliNombre, 46);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("FECHA      SG  Documento Aplicado A.    Balance");
            printer.DrawLine();

            double BalanceTotal = 0;

            
            foreach (var moneda in monedas)
            {

                foreach (CuentasxCobrar cxc in DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliid, monedas.Count > 1 ? moneda.MonCodigo : null, DS_RepresentantesParametros.GetInstance().GetSourceEstadoCuenta() == 2))
                {
                    var aplicado = "";
                    if (cxc.CxcSIGLA == "NC")
                    {
                        aplicado = myCxc.GetCxCDocumentoFacturafromAplicacion(cxc.CxcReferencia);
                    }else if (cxc.CxcSIGLA == "AB")
                    {
                        aplicado = cxc.cxcReferencia2;
                    }
                    string BalanceString = "$" + cxc.CxcBalance.ToString("N2");

                    printer.DrawText(cxc.CxcFecha.PadRight(11) + cxc.CxcSIGLA.PadRight(4) + cxc.CxcDocumento.PadRight(10) + aplicado.PadRight(11) + BalanceString.PadLeft(10));

                    if (cxc.Origen == 1) 
                        BalanceTotal += Math.Abs(cxc.CxcBalance);
                    else
                        BalanceTotal -= Math.Abs(cxc.CxcBalance);

                }
                printer.DrawLine();
                printer.DrawText("Balance:  " + Math.Abs(BalanceTotal).ToString("N2").PadLeft(36));
                BalanceTotal = 0;
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Rep.: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.DrawText("Leyenda explicativas:");
            printer.DrawText("AB: Abono");
            printer.DrawText("FT: Factura");
            printer.DrawText("NC: Nota de Credito");
            printer.DrawText("CK: Cargo por Cheque Devuelto");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato Estados de cuenta 9: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.Print();

        }


    }
}

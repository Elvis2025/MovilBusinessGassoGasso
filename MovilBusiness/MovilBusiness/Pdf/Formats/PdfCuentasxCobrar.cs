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
    public class PdfCuentasxCobrar 
    {
        private DS_CuentasxCobrar myCxc;
        private DS_RepresentantesParametros myparam;
        private string sectorID = "";
        public PdfCuentasxCobrar(DS_CuentasxCobrar myCxc, string SecCodigo = "")
        {
            this.myCxc = myCxc;
            myparam = DS_RepresentantesParametros.GetInstance();
            sectorID = Arguments.Values.CurrentSector?.SecCodigo ?? SecCodigo;
        }

        public Task<string> GenerateEstadoDeCuenta(int cliId)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEstadosCuentas())
            {
                default:
                    return Formato1(cliId);
                case 8:// LAM
                    return Formato2(cliId);
            }
        }

        public Task<string> Formato1(int cliId)
        {
            return Task.Run(() => 
            {
                Clientes cliente = new DS_Clientes().GetClienteById(cliId);

                if (cliente == null)
                {
                    throw new Exception("Error cargando datos del cliente!");
                }

                using (var manager = PdfManager.NewDocument("ESTADO DE CUENTA:" + cliente.CliNombre.Trim().Replace("/", "") + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy"), sectorID))
                {
                   
                    manager.PrintEmpresa();
                    
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ESTADO DE CUENTA");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Nombre comercial: " + cliente.CliNombreComercial);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();
                    manager.DrawTableRow(new List<string>() { "Fecha", "Sigla", "Num","Dias", "Monto", "Balance"}, true);                   

                    double BalanceTotal = 0;

                    var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliId, myparam.GetSourceEstadoCuenta() == 2);
                    foreach (var moneda in monedas)
                    {
                        if (monedas.Count > 1)
                        {
                            manager.Bold = true;
                            manager.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                            manager.Bold = false;
                        }
                        foreach (CuentasxCobrar cxc in myparam.GetSourceEstadoCuenta() == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliId, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliId, monedas.Count > 1 ? moneda.MonCodigo : null, myparam.GetSourceEstadoCuenta() == 2))
                        {
                            manager.DrawTableRow(new List<string>() { cxc.CxcFecha, cxc.CxcSIGLA, cxc.CxcDocumento, cxc.CxcDias.ToString(), cxc.CxcMontoTotal.ToString("N2").PadLeft(6), cxc.CxcBalance.ToString("N2").PadLeft(6) });

                            if (cxc.CxcSIGLA.ToUpper().Trim() == "FAT" && myparam.GetParNcf())
                            {
                                if (cxc.CXCNCF != null)
                                {
                                    manager.DrawText("NCF: " + cxc.CXCNCF);
                                }

                            }
                            
                            if (cxc.Origen == 1)                            
                                BalanceTotal += Math.Abs(cxc.CxcBalance);                            
                            else                            
                                BalanceTotal -= Math.Abs(cxc.CxcBalance);
                            
                        }
                        manager.DrawLine();
                        manager.TextAlign = Justification.RIGHT;
                        manager.DrawText("Total: " + BalanceTotal.ToString("N2"));
                        manager.TextAlign = Justification.LEFT;
                        BalanceTotal = 0;
                    }
                        
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf estado de cuenta 1: MovilBusiness v" + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }
        //          LAM
        public Task<string> Formato2(int cliId)
        {
            return Task.Run(() =>
            {
                Clientes cliente = new DS_Clientes().GetClienteById(cliId);

                if (cliente == null)
                {
                    throw new Exception("Error cargando datos del cliente!");
                }

                using (var manager = PdfManager.NewDocument("ESTADO DE CUENTA:" + cliente.CliNombre.Trim().Replace("/", "") + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy"), sectorID))
                {

                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ESTADO DE CUENTA");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    if (!string.IsNullOrEmpty(cliente.CliNombreComercial))
                    {
                        manager.DrawText("Nombre comercial: " + cliente.CliNombreComercial);
                    }
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();
                    manager.DrawTableRow(new List<string>() { "Fecha", "Sigla", "Num", "Dias", "Balance" }, true);

                    double BalanceTotal = 0;

                    var monedas = myCxc.GetMonedasDeLosDocumentosPendientes(cliId,myparam.GetSourceEstadoCuenta() == 2);
                    foreach (var moneda in monedas)
                    {
                        if (monedas.Count > 1)
                        {
                            manager.Bold = true;
                            manager.DrawText(moneda.MonNombre + " - " + moneda.MonSigla);
                            manager.Bold = false;
                        }
                        foreach (CuentasxCobrar cxc in  myparam.GetSourceEstadoCuenta() == 1 ? myCxc.GetAllCuentasPendientesByCliente(cliId, monedas.Count > 1 ? moneda.MonCodigo : null) : myCxc.GetAllCuentasByCliente(cliId, monedas.Count > 1 ? moneda.MonCodigo : null, myparam.GetSourceEstadoCuenta() == 2))
                        {
                            manager.DrawText(cxc.CxcBalance.ToString("N2").PadLeft(162), noline: true);
                            manager.DrawTableRow(new List<string>() { cxc.CxcFecha, cxc.CxcSIGLA, cxc.CxcDocumento, cxc.CxcDias.ToString()});

                            if (cxc.CxcSIGLA.ToUpper().Trim() == "FAT" && myparam.GetParNcf())
                            {
                                if (cxc.CXCNCF != null)
                                {
                                    manager.DrawText("NCF: " + cxc.CXCNCF);
                                }

                            }

                            if (cxc.Origen == 1)
                                BalanceTotal += Math.Abs(cxc.CxcBalance);
                            else
                                BalanceTotal -= Math.Abs(cxc.CxcBalance);

                        }
                        manager.DrawLine();
                        manager.TextAlign = Justification.LEFT;
                        manager.DrawText(("Total: " + BalanceTotal.ToString("N2")).PadLeft(157));
                        BalanceTotal = 0;
                    }

                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Formato pdf estado de cuenta 2: MovilBusiness v" + Functions.AppVersion);
                    manager.NewLine();

                    return manager.FilePath;
                }

            });
        }

        //aqui se generan los formatos pdf de las NC y las facturas
        public Task<string> ReportePdfDocumento(CuentasxCobrar documento)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionNotaCredito())
            {
                default:
                    return FormatoNC1(documento);
                case 2: //cano
                    return FormatoNC2(documento);
                case 3: //garmenteros
                    return FormatoNC3(documento);
                case 4: //Mgomez
                    return FormatoNC4(documento);
                case 7: //LAM
                    return FormatoNC7(documento);
            }
        }

        private Task<string> FormatoNC1(CuentasxCobrar documento)
        {
            return Task.Run(() => 
            {
            var cliente = new DS_Clientes().GetClienteById(documento.CliID);

                using (var manager = PdfManager.NewDocument(("Documento: " + documento.CxcDocumento.Replace("/", "") + ": " + cliente.CliNombre.Trim().Replace("/", "") + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy")).Replace("/", ""), sectorID))
                {
                    if (cliente == null)
                    {
                        throw new Exception("Error cargando datos del cliente");
                    }

                    manager.PrintEmpresa();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
                    }
                    else
                    {
                        manager.Font = PrinterFont.BODY;
                        manager.DrawText("COPIA");
                        manager.Font = PrinterFont.TITLE;
                        manager.DrawText("FACTURA CON VALOR FISCAL");
                    }
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.LEFT;

                    manager.NewLine();

                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("RNC: " + cliente.CliRNC);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                        manager.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
                    }
                    else
                    {
                        manager.DrawText("No. Factura: " + documento.CxcDocumento);
                        manager.DrawText("Referencia: " + documento.CxcReferencia);
                    }
                    manager.Bold = false;
                    manager.DrawText("NCF: " + documento.CXCNCF);
                    manager.DrawText("Fecha doc: " + documento.CxcFecha);


                    if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
                    {
                        manager.DrawText("Monto total: " + documento.CxcMontoTotal.ToString("N2"));
                        manager.Bold = true;
                        manager.DrawLine();
                        manager.DrawText("Codigo-Descripcion");
                        manager.DrawTableRow(new List<string>() { "Cant.", "Precio", "Desc.", "Itbis", "Importe" });
                        manager.DrawLine();
                        manager.Bold = false;
                    }

                    double subTotal = 0.0;
                    double itbisTotal = 0.0;
                    double descTotal = 0.0;
                    double descTotal1 = 0.0;

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
                            manager.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion);

                            manager.DrawTableRow(new List<string>() { pro.ProCantidad.ToString("N2"), pro.ProPrecio.ToString("N2"), desc.ToString("N2"), itbis.ToString("N2"), precioNeto.ToString("N2") });


                            subTotal += pro.ProPrecio * pro.ProCantidad;
                            itbisTotal += itbis;
                            descTotal += desc;
                            descTotal1 += desc1;

                        }
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Sub Total   : " + Math.Abs(documento.CxcMontoSinItbis).ToString("N2").PadLeft(10 - documento.CxcMontoSinItbis.ToString("N2").Length));
                    manager.DrawText("Descuento   : " + Math.Abs(descTotal).ToString("N2").PadLeft(10 - descTotal.ToString("N2").Length));
                    manager.DrawText("Itbis       : " + Math.Abs(itbisTotal).ToString("N2").PadLeft(16 - itbisTotal.ToString("N2").Length));
                    manager.DrawText("Total       : " + Math.Abs(documento.CxcMontoTotal).ToString("N2").PadLeft(10 - documento.CxcMontoTotal.ToString("N2").Length));
                    manager.NewLine();

                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.NewLine();
                        manager.NewLine();
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));

                    manager.DrawText("Formato pdf documentos 1: MovilBusiness " + Functions.AppVersion);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Fecha generacion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();

                    return manager.FilePath;

                }
            });
        }

        private Task<string> FormatoNC2(CuentasxCobrar documento)
        {
            return Task.Run(() => 
            {
                var cliente = new DS_Clientes().GetClienteById(documento.CliID);

                if (cliente == null)
                {
                    throw new Exception("Error cargando datos del cliente");
                }

                using (var manager = PdfManager.NewDocument(("Documento: " + documento.CxcDocumento.Replace("/", "") + ": " + cliente.CliNombre.Trim() + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy")).Replace("/", ""), sectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
                    }
                    else
                    {
                        manager.Font = PrinterFont.BODY;
                        manager.DrawText("COPIA");
                        manager.Font = PrinterFont.TITLE;
                        manager.DrawText("FACTURA CON VALOR FISCAL");
                    }

                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    //manager.DrawBarcode(documento.CxcReferencia);

                    //manager.NewLine();

                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("RNC: " + cliente.CliRNC);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                        manager.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
                    }
                    else
                    {
                        manager.DrawText("No. Factura: " + documento.CxcDocumento);
                        manager.DrawText("Referencia: " + documento.CxcReferencia);
                    }
                    manager.Bold = false;
                    manager.DrawText("NCF: " + documento.CXCNCF);
                    manager.DrawText("Fecha doc: " + documento.CxcFecha);

                    if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
                    {
                        manager.DrawText("Monto total: " + documento.CxcMontoTotal.ToString("N2"));
                        manager.Bold = true;
                        manager.DrawLine();
                        manager.DrawText("Codigo-Descripcion");
                        manager.DrawTableRow(new List<string>() { "Cant.", "Precio", "Itbis", "Importe" });
                        manager.DrawLine();
                        manager.Bold = false;
                    }

                    //var subTotal = 0.0;
                    var itbisTotal = 0.0;

                    var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

                    if (productos != null && productos.Count > 0)
                    {
                        foreach (var pro in productos)
                        {

                            var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                            var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;

                            manager.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion);
                            manager.DrawTableRow(new List<string>() { pro.ProCantidad.ToString("N2"), pro.ProPrecio.ToString("N2"), itbis.ToString("N2"), precioNeto.ToString("N2") });

                            /* manager.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                                 pro.ProPrecio.ToString("N2").PadRight(13)
                                 + itbis.ToString("N2").PadRight(14)
                                 + precioNeto.ToString("N2").PadRight(10));*/

                            //subTotal += pro.ProPrecio * pro.ProCantidad;
                            itbisTotal += itbis * pro.ProCantidad;
                        }
                        manager.DrawLine();
                    }
                    manager.NewLine();
                    manager.DrawText("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
                    manager.DrawText("Itbis        : " + itbisTotal.ToString("N2").PadLeft(10));
                    manager.DrawText("Total       : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();

                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.NewLine();
                        manager.NewLine();
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
                    manager.DrawText("Formato pdf documentos 2: MovilBusiness " + Functions.AppVersion);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Fecha generacion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();

                    return manager.FilePath;
                }
            });
        }
        //                                                                  Nuevo Formato   garmenteros
        private Task<string> FormatoNC3(CuentasxCobrar documento)
        {
            return Task.Run(() =>
            {
                var cliente = new DS_Clientes().GetClienteById(documento.CliID);

                if (cliente == null)
                {
                    throw new Exception("Error cargando datos del cliente");
                }

                using (var manager = PdfManagerPrue.NewDocument(("Documento: " + documento.CxcDocumento.Replace("/", "") + ": " + cliente.CliNombre.Trim() + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy")).Replace("/", ""), sectorID))
                {
                    manager.PrintEmpresa();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
                    }
                    else
                    {
                        manager.Font = PrinterFont.BODY;
                        manager.DrawText("COPIA");
                        manager.Font = PrinterFont.TITLE;
                        manager.DrawText("FACTURA CON VALOR FISCAL");
                    }
                    manager.Font = PrinterFont.BODY;

                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Bold = true;
                    manager.DrawTextNew2(" ", true, 20);
                    manager.DrawText("Comprobantes con Valor Fiscal");
                    manager.Bold = false;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                        manager.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
                    }
                    else
                    {
                        manager.DrawText("No. Factura: " + documento.CxcDocumento);
                        manager.DrawText("Referencia: " + documento.CxcReferencia);
                    }
                    manager.Bold = false;
                    manager.DrawText("NCF: " + documento.CXCNCF);
                    manager.DrawText("Fecha doc: " + documento.CxcFecha);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText(("Cliente: " + cliente.CliNombre) + ("             Calle: " + cliente.CliCalle).PadLeft(30));
                    manager.DrawText(("Codigo:  " + cliente.CliCodigo) + ("RNC:   " + cliente.CliRNC).PadLeft(30));

                    if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
                    {
                        manager.DrawText("Monto total: " + documento.CxcMontoTotal.ToString("N2") + ("Vendedor: " + documento.RepCodigo).PadLeft(200));
                        manager.Bold = true;
                        manager.DrawLine();
                        manager.DrawText("Codigo-Descripcion");
                        manager.DrawTableRow(new List<string>() { "Cant.","Unidad", "Precio","%Desc", "Itbis", "Importe" });
                        manager.DrawLine();
                        manager.Bold = false;
                    }

                    //var subTotal = 0.0;
                    var itbisTotal = 0.0;
                    var descTotal = 0.0;

                    var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

                    if (productos != null && productos.Count > 0)
                    {
                        foreach (var pro in productos)
                        {

                            var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                            var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;

                            manager.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion);
                            manager.DrawTableRow(new List<string>() { pro.ProCantidad.ToString("N2"),pro.UnmCodigo.ToString(), pro.ProPrecio.ToString("N2"),pro.ProDescuentoMaximo.ToString("N2"), itbis.ToString("N2"), precioNeto.ToString("N2") });

                            /* manager.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                                 pro.ProPrecio.ToString("N2").PadRight(13)
                                 + itbis.ToString("N2").PadRight(14)
                                 + precioNeto.ToString("N2").PadRight(10));*/

                            //subTotal += pro.ProPrecio * pro.ProCantidad;
                            itbisTotal += itbis * pro.ProCantidad;
                            descTotal += pro.ProDescuentoMaximo;
                        }
                        manager.DrawLine();
                    }

                    var bultosYunidades = myCxc.GetDetalleBySecuenciaBultosYUnidades(documento.CxcReferencia);
                    int bultos = 0;
                    double unidad = 0;
                    foreach (var item in bultosYunidades)
                    {
                        bultos = (int)item.CxcCantidad;
                        unidad = item.CxcCantidadDetalle;
                    }
                    
                    manager.NewLine();
                    manager.DrawTextNew2((("Total Bultos: " + bultos.ToString("N2").PadRight(100)) + ("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2")) + ("| Total Desc: " + descTotal.ToString("N2").PadRight(25)) + ("Itbis: " + itbisTotal.ToString("N2")) + ("| Total: " + documento.CxcMontoTotal.ToString("N2"))), true, 15);
                    //manager.DrawText("Sub Total   : " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
                    //manager.DrawText("Itbis       : " + itbisTotal.ToString("N2").PadLeft(10));
                    //manager.DrawText("Total       : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();

                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.NewLine();
                        manager.NewLine();
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
                    manager.DrawText("Formato pdf documentos 3: MovilBusiness " + Functions.AppVersion);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Fecha generacion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();

                    return manager.FilePath;
                }
            });
        }
        //                                                                  Nuevo Formato   mgomezp
        private Task<string> FormatoNC4(CuentasxCobrar documento)
        {
            return Task.Run(() =>
            {
                var cliente = new DS_Clientes().GetClienteById(documento.CliID);

                using (var manager = PdfManager.NewDocument(("Documento: " + documento.CxcDocumento.Replace("/", "") + ": " + cliente.CliNombre.Trim().Replace("/", "") + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy")).Replace("/", ""), sectorID))
                {
                    if (cliente == null)
                    {
                        throw new Exception("Error cargando datos del cliente");
                    }

                    manager.PrintEmpresa();
                    manager.NewLine();

                    manager.TextAlign = Justification.CENTER;
                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
                    }
                    else
                    {
                        manager.Font = PrinterFont.BODY;
                        manager.DrawText("COPIA");
                        manager.Font = PrinterFont.TITLE;
                        manager.DrawText("FACTURA CON VALOR FISCAL");
                    }
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.LEFT;

                    manager.NewLine();

                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("RNC: " + cliente.CliRNC);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                        manager.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
                    }
                    else
                    {
                        manager.DrawText("No. Factura: " + documento.CxcDocumento);
                        manager.DrawText("Referencia: " + documento.CxcReferencia);
                    }
                    manager.Bold = false;
                    manager.DrawText("NCF: " + documento.CXCNCF);
                    manager.DrawText("Fecha doc: " + documento.CxcFecha);

                    if (documento.CxcSIGLA.ToUpper().Trim() != "NC")
                    {
                        manager.DrawText("Monto total: " + documento.CxcMontoTotal.ToString("N2"));
                        manager.Bold = true;
                        manager.DrawLine();
                        manager.DrawText("Codigo-Descripcion");
                        manager.DrawTableRow(new List<string>() { "Cant.", "Precio", "Desc", "Itbis", "Importe" });
                        manager.DrawLine();
                        manager.Bold = false;
                    }

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

                            manager.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion);
                            manager.DrawTableRow(new List<string>() { pro.ProCantidad.ToString("N2"), pro.ProPrecio.ToString("N2"), pro.ProDescuentoMaximo.ToString("N2"), itbis.ToString("N2"), precioNeto.ToString("N2") });

                            subTotal += pro.ProPrecio * pro.ProCantidad;
                            itbisTotal += itbis;
                            descTotal += pro.ProDescuentoMaximo;
                        }
                        manager.DrawLine();
                    }

                    manager.NewLine();
                    manager.DrawText("Sub Total   : " + subTotal.ToString().PadLeft(10));
                    manager.DrawText("Descuento   : " + descTotal.ToString().PadLeft(10));
                    manager.DrawText("Imp Total   : " + documento.CxcMontoSinItbis.ToString().PadLeft(10));
                    manager.DrawText("Itbis       : " + itbisTotal.ToString().PadLeft(10));
                    manager.DrawText("Total       : " + documento.CxcMontoTotal.ToString().PadLeft(10));
                    manager.NewLine();

                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.NewLine();
                        manager.NewLine();
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));

                    manager.DrawText("Formato pdf documentos 4: MovilBusiness " + Functions.AppVersion);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Fecha generacion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();

                    return manager.FilePath;

                }
            });
        }

        private Task<string> FormatoNC7(CuentasxCobrar documento)
        {
            return Task.Run(() =>
            {
                var cliente = new DS_Clientes().GetClienteById(documento.CliID);

                if (cliente == null)
                {
                    throw new Exception("Error cargando datos del cliente");
                }

                using (var manager = PdfManagerPrue.NewDocument(("Documento: " + documento.CxcDocumento.Replace("/", "") + ": " + cliente.CliNombre.Trim() + "--Fecha:" + DateTime.Now.ToString("dd-MM-yyyy")).Replace("/", ""), sectorID))
                {
                    manager.PrintEmpresa();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("NOTA DE CREDITO CON VALOR FISCAL");
                    }
                    else
                    {
                        manager.Font = PrinterFont.BODY;
                        manager.DrawText("COPIA");
                        manager.Font = PrinterFont.TITLE;
                        manager.DrawText("FACTURA CON VALOR FISCAL");
                    }
                    manager.Font = PrinterFont.BODY;

                    manager.TextAlign = Justification.CENTERRIGHT2;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Bold = true;
                    manager.DrawTextNew2(" ", true, 20);
                    manager.DrawText("Comprobantes con Valor Fiscal");
                    manager.Bold = false;
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("No. Nota Credito: " + documento.CxcDocumento);
                        manager.DrawText("NCF Afectado: " + documento.CXCNCFAfectado);
                    }
                    else
                    {
                        manager.DrawText("No. Factura: " + documento.CxcDocumento);
                        manager.DrawText("Referencia: " + documento.CxcReferencia);
                    }
                    manager.Bold = false;
                    manager.DrawText("NCF: " + documento.CXCNCF);
                    manager.DrawText("Fecha doc: " + documento.CxcFecha);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText(("Cliente: " + cliente.CliNombre) + ("             Calle: " + cliente.CliCalle).PadLeft(30));
                    manager.DrawText(("Codigo:  " + cliente.CliCodigo) + ("RNC:   " + cliente.CliRNC).PadLeft(30));


                    manager.DrawText("Monto total: " + documento.CxcMontoTotal.ToString("N2") + ("Vendedor: " + documento.RepCodigo).PadLeft(200));
                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.DrawText("Nota: " + documento.cxcComentario);
                    }
                    manager.Bold = true;
                    manager.DrawLine();
                    manager.DrawText("Codigo-Descripcion");
                    manager.DrawTableRow(new List<string>() { "Cant.", "Unidad", "Precio", "%Desc", "Itbis", "Importe" });
                    manager.DrawLine();
                    manager.Bold = false;
                 

                    //var subTotal = 0.0;
                    var itbisTotal = 0.0;
                    var descTotal = 0.0;

                    var productos = myCxc.GetProductosFromCuentasxCobrarDetalle(documento.CxcReferencia);

                    if (productos != null && productos.Count > 0)
                    {
                        foreach (var pro in productos)
                        {

                            var itbis = pro.ProPrecio * (pro.ProItbis / 100);
                            var precioNeto = (pro.ProPrecio + itbis) * pro.ProCantidad;

                            manager.DrawText(pro.ProCodigo + "-" + pro.ProDescripcion);
                            manager.DrawTableRow(new List<string>() { pro.ProCantidad.ToString("N2"), pro.UnmCodigo.ToString(), pro.ProPrecio.ToString("N2"), pro.ProDescuentoMaximo.ToString("N2"), itbis.ToString("N2"), precioNeto.ToString("N2") });

                            /* manager.DrawText(pro.ProCantidad.ToString("N2").PadRight(12) +
                                 pro.ProPrecio.ToString("N2").PadRight(13)
                                 + itbis.ToString("N2").PadRight(14)
                                 + precioNeto.ToString("N2").PadRight(10));*/

                            //subTotal += pro.ProPrecio * pro.ProCantidad;
                            itbisTotal += itbis * pro.ProCantidad;
                            descTotal += pro.ProDescuentoMaximo;
                        }
                        manager.DrawLine();
                    }

                    var bultosYunidades = myCxc.GetDetalleBySecuenciaBultosYUnidades(documento.CxcReferencia);
                    int bultos = 0;
                    double unidad = 0;
                    foreach (var item in bultosYunidades)
                    {
                        bultos = (int)item.CxcCantidad;
                        unidad = item.CxcCantidadDetalle;
                    }

                    manager.NewLine();
                    manager.DrawTextNew2((("Total Bultos: " + bultos.ToString("N2").PadRight(100)) + ("Sub Total: " + documento.CxcMontoSinItbis.ToString("N2")) + ("| Total Desc: " + descTotal.ToString("N2").PadRight(25)) + ("Itbis: " + itbisTotal.ToString("N2")) + ("| Total: " + documento.CxcMontoTotal.ToString("N2"))), true, 15);
                    //manager.DrawText("Sub Total   : " + documento.CxcMontoSinItbis.ToString("N2").PadLeft(10));
                    //manager.DrawText("Itbis       : " + itbisTotal.ToString("N2").PadLeft(10));
                    //manager.DrawText("Total       : " + documento.CxcMontoTotal.ToString("N2").PadLeft(10));
                    manager.NewLine();
                    manager.NewLine();

                    if (documento.CxcSIGLA.ToUpper().Trim() == "NC")
                    {
                        manager.NewLine();
                        manager.NewLine();
                        manager.NewLine();
                        manager.TextAlign = Justification.CENTER;
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma del cliente");
                        manager.TextAlign = Justification.LEFT;
                    }

                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Rep.Venta: " + documento.RepCodigo + "-" + new DS_Representantes().GetRepNombre(documento.RepCodigo));
                    manager.DrawText("Formato pdf documentos 7: MovilBusiness " + Functions.AppVersion);
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Fecha generacion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();

                    return manager.FilePath;
                }
            });
        }

    }
}

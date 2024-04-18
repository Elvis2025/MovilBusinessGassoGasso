using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class CuadresFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Cuadres myCuadre;
        private DS_Vehiculos myVehiculo;
        private DS_Cargas myCarga;
        private DS_Ventas myVentas;
        private DS_Pedidos myPedidos;
        private DS_Productos myProd;
        private DS_Cambios myCambios;
        private DS_UsosMultiples myUsosMul;
        private DS_Almacenes myAlm;
        private DS_Recibos myRec;

        public CuadresFormats(DS_Cuadres myCua)
        {
            myCuadre = new DS_Cuadres();
            myVehiculo = new DS_Vehiculos();
            myCarga = new DS_Cargas();
            myVentas = new DS_Ventas();
            myPedidos = new DS_Pedidos();
            myProd = new DS_Productos();
            myCambios = new DS_Cambios();
            myUsosMul = new DS_UsosMultiples();
            myAlm = new DS_Almacenes();
            myRec = new DS_Recibos();
        }


        public void Print(int CuaSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCuadres())
            {
                case 1: //TABACALERA
                    Formato1(CuaSecuencia);
                    break;

                default:
                case 2: //DEFAULT
                    ImprimirFormato3(CuaSecuencia);
                    break;

                case 3: //FRUTOS FERRER
                    Formato3(CuaSecuencia);
                    break;

                case 4: //NUTRICIOSA
                    Formato4(CuaSecuencia);
                    break;

                case 5: // La Tabacalera
                    Formato5(CuaSecuencia);
                    break;

                case 6: //PERAVIA INDUSTRIAL REPARTIDORES
                    Formato6(CuaSecuencia);
                    break;

                case 7: //Food Smart
                    Formato7(CuaSecuencia);
                    break;

                case 8: //Planeta Azul
                    Formato8(CuaSecuencia);
                    break;

                case 9: //Casa Rodriguez
                    Formato9(CuaSecuencia);
                    break;

                case 10: //Molinos del Sol
                    Formato10(CuaSecuencia);
                    break;

                case 11:// Formato 2 pulgadas - Planeta Azul
                    Formato11(CuaSecuencia);
                    break;

                case 12: //MISS KEY
                    Formato12(CuaSecuencia);
                    break;

                case 13: //PERAVIA INDUSTRIAL RANCHERO
                    Formato13(CuaSecuencia);
                    break;

                case 21: //Rec - de canastos
                    Formato21(CuaSecuencia);
                    break;

                case 22: //Eccus
                    Formato22(CuaSecuencia);
                    break;

                case 35:// Formato 2 pulgadas - Planeta Azul
                    Formato35(CuaSecuencia);
                    break;
            }
        }

        public void PrintFacturas(int CuaSecuencia, bool confirmado, PrinterManager printer)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            /* bool AbrirCuadre = true,*/
            var CerrarCuadre = true;

            if (cuadre.CuaEstatus == 1)
            {
                //AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                //AbrirCuadre = false;
                CerrarCuadre = true;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DE FACTURAS A CREDITO");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Entrega de Cuadre          :".PadRight(36) + Arguments.CurrentUser.RepCodigo.PadLeft(5) + "-" + CuaSecuencia.ToString(), 47);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Factura     Sec.  Cliente   Descuento  Monto");
            printer.DrawLine();

            double Totalfacturascredito = 0.0;
            int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
            foreach (Ventas ventas in myVentas.GetVentasaCreditoByfecha(CuaSecuencia, contado))
            {
                double Descuento = 0.0;
                string Facturas = "";
                Facturas = ventas.VenNCF;


                Totalfacturascredito += ventas.VenTotal;
                printer.DrawText(Facturas.PadRight(12) + ventas.VenSecuencia.ToString().PadRight(6) +
                ventas.CliCodigo.ToString().PadRight(12) + Descuento.ToString().PadRight(5) + ventas.VenTotal.ToString("N2").PadLeft(10));

            }

            if (Totalfacturascredito > 0)
            {
                printer.DrawText("Total Facturas a Credito: ".PadRight(33) + ("RD$" + Totalfacturascredito.ToString("N", new CultureInfo("en-US"))).PadLeft(10));
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("-No hay Facturas a credito-".CenterText(48));
                printer.DrawText("");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma vendedor:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato Entrega Factura 4: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            printer.Print();
        }
        private void Formato1(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");
            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                if (desc.Length >= 30)
                {
                    desc = desc.Substring(0, 30);
                }
                else
                {
                    desc = desc.PadRight(30);
                }

                var cantidad = prod.CuaCantidadInicial.ToString();

                if (prod.CuaCantidadDetalleInicial > 0)
                {
                    cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                }
                string unm = prod.UnmCodigo;
                if (prod.UnmCodigo.Length >= 3)
                    unm = prod.UnmCodigo.Substring(0, 3);

                if (prod.CuaCantidadInicial > 0 || prod.CuaCantidadDetalleInicial > 0)
                {
                    printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                }
            }

            if (CerrarCuadre)

            {

                printer.DrawLine();
                printer.DrawText("CARGAS ACEPTADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Referencia                      Fecha");
                printer.DrawText("----------------------------------------------");
                foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                {
                    var desc = cargasAceptadas.CarReferencia;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var fecha = cargasAceptadas.CarFecha.ToString();
                    var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                    printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                }
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /*
                                printer.DrawText("CONTEO FISICO");
                                printer.DrawText("");
                                printer.DrawText("");
                                printer.DrawText("PRODUCTOS CON FALTANTES");
                                printer.DrawText("");
                                printer.DrawText("------------------------------------------------");
                                printer.DrawText("Codigo-Descripcion                     ");
                                printer.DrawText("Cant.Total          Cant. Ent        Cant. Falt.");
                                printer.DrawText("------------------------------------------------");
                                printer.DrawText("");
                                int Largo = 35;

                                foreach (var myConFis in myCuadre.GetConteoFisicoByCuadre(CuaSecuencia))
                                {

                                    double CantidadLogica = myConFis.CuaCantidadFinal;
                                    double CantidadFisica = myConFis.CuaCantidadFisica;
                                    if (CantidadFisica < CantidadLogica)
                                    {
                                        if (myConFis.ProDescripcion.Length < 35)
                                        {
                                            Largo = myConFis.ProDescripcion.Length;
                                        }
                                        else
                                        {
                                            Largo = 35;
                                        }

                                        string codigo = myConFis.ProCodigo;
                                        string nombre = myConFis.ProDescripcion;

                                        double cantidadFaltante = CantidadLogica - CantidadFisica;


                                        string unidadMedida = myConFis.UnmCodigo;

                                        printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                                        printer.DrawText((CantidadLogica.ToString() + " " + unidadMedida).PadRight(15)
                                                + (CantidadFisica.ToString() + " " + unidadMedida ).PadRight(15) +
                                               (cantidadFaltante.ToString()+ " " + unidadMedida).PadLeft(9));

                                        printer.DrawText("");
                                    }
                                }
                                printer.DrawText("");
                                printer.DrawText("");
                                printer.DrawText("PRODUCTOS CON SOBRANTES");
                                printer.DrawText("");
                                printer.DrawText("------------------------------------------------");
                                printer.DrawText("Codigo-Descripcion      ");
                                printer.DrawText("Cant.Total          Cant. Ent        Cant. Sobr");
                                printer.DrawText("------------------------------------------------");
                                printer.DrawText(" ");
                                Largo = 35;
                                foreach (var myConFis in myCuadre.GetConteoFisicoByCuadre(CuaSecuencia))
                                {

                                    double CantidadLogica = myConFis.CuaCantidadFinal;
                                    double CantidadFisica = myConFis.CuaCantidadFisica;
                                    if (CantidadFisica > CantidadLogica)
                                    {
                                        if (myConFis.ProDescripcion.Length < 35)
                                        {
                                            Largo = myConFis.ProDescripcion.Length;
                                        }
                                        else
                                        {
                                            Largo = 35;
                                        }

                                        string codigo = myConFis.ProCodigo;
                                        string nombre = myConFis.ProDescripcion;

                                        double cantidadSobrante = CantidadFisica - CantidadLogica;


                                        string unidadMedida = myConFis.UnmCodigo;

                                        printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                                        printer.DrawText((CantidadLogica + " " + unidadMedida).PadRight(15) + 
                                            (CantidadFisica+ " " + unidadMedida).PadRight(15) + (cantidadSobrante + " " + unidadMedida).PadLeft(9));

                                        printer.DrawText("");
                                    }
                                }
                                */

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                printer.DrawText("----------------------------------------------");
                printer.DrawText("PRODUCTOS CARGADOS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawLine();
                foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 35)
                    {
                        desc = desc.Substring(0, 35);
                    }

                    var cantidad = !string.IsNullOrEmpty(prod.ProDatos3) && prod.ProDatos3.Equals("A") ? prod.CarCantidad.ToString("N2") : prod.CarCantidad.ToString();

                    if (prod.CarCantidadDetalle > 0)
                    {
                        cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                    }

                    printer.DrawText(desc.PadRight(35) + cantidad.PadRight(0));
                }
                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("----------------------------------------------");
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = !string.IsNullOrEmpty(prod.ProDatos3) && prod.ProDatos3.Equals("A") ? prod.CuaCantidadFinal.ToString() : prod.CuaCantidadFinal.ToString("N2");

                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 2 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal > 0 || prod.CuaCantidadDetalleFinal > 0)
                    {
                        printer.DrawText((desc + " ").PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                printer.DrawText("");
                printer.DrawLine();
                printer.DrawText("PEDIDOS REALIZADOS");
                printer.DrawText("");

                printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                printer.DrawLine();
                printer.DrawText("");
                int lastCliID = -1;
                string linea1 = "";
                double montoTotal = 0.0;
                List<string> linea2 = new List<string>();
                foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                {
                    if (lastCliID != ped.CliID)
                    {//valida cuando el cliente sea diferente
                        lastCliID = ped.CliID;
                    }

                    int Ciclo = 0;
                    PedidosTotal = 0.0;

                    foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                    {

                        string DescripcionLineas1 = "";
                        string DescripcionLineas2 = "";

                        try
                        {
                            if (ped.CliNombre.Length > 35)
                            {
                                DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                            }
                            else
                            {
                                DescripcionLineas1 = ped.CliNombre;
                            }
                        }
                        catch (Exception)
                        {
                            // TODO: handle exception
                        }

                        double precio = pedBycli.PedPrecio;
                        double adValorem = pedBycli.PedAdValorem;
                        double selectivo = pedBycli.PedSelectivo;
                        double descuento = pedBycli.PedDescuento;
                        double itbis = pedBycli.PedItbis;
                        double proUnidades = pedBycli.ProUnidades;
                        double pedCantidad = pedBycli.PedCantidad;
                        double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                        double cantidad = 0.0;


                        if (pedCantidadDetalle > 0)
                        {
                            cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                        }
                        else
                        {
                            cantidad = pedCantidad;
                        }

                        double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                        PedidosTotal += precioNeto * cantidad;

                        Ciclo++;
                        if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                        {
                            linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                        }

                        if (DescripcionLineas1.Trim().Length > 28)
                        {
                            DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                        }

                        linea2.Add(pedBycli.ProCodigo + "-"
                        + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                        + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                    }

                    printer.DrawText(linea1);
                    foreach (var i in linea2)
                    {
                        printer.DrawText(i);
                    }
                    printer.DrawText("");
                    linea2.Clear();

                    montoTotal += PedidosTotal;
                }

                printer.DrawText("");
                printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                printer.DrawText("");
                printer.DrawLine();


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("");
                printer.DrawText("VENTAS A CREDITO:");

                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("");
                double TotalVentas;
                double TotalVentasCredito;
                TotalVentasCredito = 0;
                foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(30) + ("RD$" + TotalVenta).PadLeft(5));
                }
                printer.DrawText("");
                string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(30) + "RD$" + TotalVentasCred);

                printer.DrawLine();
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("");
                double TotalVentasContado;
                TotalVentasContado = 0;
                double DecuentoTotal = 0.0;
                double PrecioTotal = 0.0;
                double ItebisTotal = 0.0;
                double TotalContado = 0.0;

                foreach (var prod in myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    // TotalVentasContado = TotalVentasContado + (Math.Round(prod.VenTotal,2,MidpointRounding.AwayFromZero) + Math.Round(prod.VenTotalItbis,2, MidpointRounding.AwayFromZero));
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    foreach (var det in myVentas.GetVentasDetalleaContadoByCuaSecuencia(CuaSecuencia, prod.CliID))
                    {
                        double Descuentos;
                        double Descuentos1;
                        double AdValoremU = det.VenAdValorem;
                        double SelectivoU = det.VenSelectivo;
                        double PrecioLista = (det.VenPrecio + AdValoremU + SelectivoU);
                        PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                        double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.VenCantidadDetalle));
                        CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                        double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                        double CantidadUnica = det.VenCantidad;
                        double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                        CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                        PrecioTotal += PrecioLista * CantidadReal;
                        PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                        Descuentos1 = (det.VenPrecio * det.VenDescuento) / 100;
                        Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                        Descuentos = (det.VenDescPorciento / 100) * det.VenPrecio;
                        Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                        if (Descuentos == 0.0)
                        {
                            Descuentos = det.VenDescuento;
                        }

                        double descTotalUnitario = Descuentos * CantidadReal;
                        descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                        DecuentoTotal += descTotalUnitario;
                        DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                        double tasaItbis = det.VenItbis;

                        double MontoItbis = ((PrecioLista - Descuentos) * (tasaItbis / 100));
                        MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                        ItebisTotal += (MontoItbis * CantidadReal);
                        ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                        if (tasaItbis != 0)
                        {
                            PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                            PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                        }

                        double subTotal = PrecioLista * CantidadReal;
                        subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                    }

                    TotalVentasContado = (PrecioTotal) - DecuentoTotal + ItebisTotal;//string VentaTotal = Total.ToString();//prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    TotalContado += TotalVentasContado;
                    printer.DrawText(desc.PadRight(30) + ("RD$" + TotalVentasContado.ToString()).PadLeft(5) + " " + myVentas.getFormasPago(prod.VenNCF));
                    PrecioTotal = 0.0;
                    DecuentoTotal = 0.0;
                    ItebisTotal = 0.0;
                }
                printer.DrawText("");
                string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(30) + "RD$" + TotalContado);

                printer.DrawLine();

                TotalVentas = TotalContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(30) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("PRODUCTOS VENDIDOS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                     VEND.");
                printer.DrawText("----------------------------------------------");

                foreach (var prod in myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (Producto.Length > 35)
                    {
                        Producto = Producto.Substring(0, 35);
                    }

                    var cantidad = prod.VenCantidad.ToString();

                    var CantidadTotal = myProd.ConvertirUnidadesACajas(myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID), prod.VenCantidad, prod.VenCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                    var CantidaUnidades = Math.Round((CantidadTotal - (int)CantidadTotal) * myProd.GetProUnidades(prod.ProID), 0);

                    /*   if (prod.VenCantidadDetalle > 0)
                       {*/
                    cantidad = (int)CantidadTotal + "/" + (int)CantidaUnidades;
                    //}

                    printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(6));
                }


                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        var CantidadTotal = myProd.ConvertirUnidadesACajas(myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID), prod.VenCantidad, prod.VenCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                        var CantidaUnidades = Math.Round((CantidadTotal - (int)CantidadTotal) * myProd.GetProUnidades(prod.ProID), 0);

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = (int)CantidadTotal + "/" + (int)CantidaUnidades;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS ANULADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("No. Fact.    Cliente                Valor");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("");
                double totalventasanuladas = 0;
                foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    if (venta.VenNCF == null)
                    {
                        venta.VenNCF = "--";
                    }
                    string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                    if (FacturaCliente.Length > 35)
                    {
                        FacturaCliente = FacturaCliente.Substring(0, 34);
                    }
                    string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                    totalventasanuladas += venta.VenTotal;
                }
                string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                printer.DrawText("------------------------------------------------");
                printer.DrawText("Codigo-Descripcion                       VEND.");
                printer.DrawText("------------------------------------------------");
                int Largo = 0;
                foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                {
                    if (ProVentasAnuladas.ProDescripcion.Length < 26)
                    {
                        Largo = ProVentasAnuladas.ProDescripcion.Length;
                    }
                    else
                    {
                        Largo = 26;
                    }
                    string codigo = ProVentasAnuladas.ProCodigo;
                    string nombre = ProVentasAnuladas.ProDescripcion;
                    string venCantidad = (ProVentasAnuladas.VenCantidadDetalle > 0 ? ProVentasAnuladas.VenCantidad + "/" + ProVentasAnuladas.VenCantidadDetalle : ProVentasAnuladas.VenCantidad.ToString());
                    string unidadMedida = ProVentasAnuladas.UnmCodigo;

                    printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                }

                printer.DrawText("");
                printer.DrawText("________________________________________________");

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;


                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;

                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUSHMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("CANJE DE CAJETILLAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion             Cajetillas Rec.");
                printer.DrawText("----------------------------------------------");

                int cantidadtotalCajetilla = 0;

                foreach (var canje in myVentas.getCanjeCajetillas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    string Descripcion = canje.ProNombre;

                    if (Descripcion.Trim().Length > 34)
                    {
                        Descripcion = Descripcion.Trim().Substring(0, 35);
                    }


                    printer.DrawText(canje.ProCodigo + "-" + Descripcion.PadRight(35) + canje.ComCantidad.ToString());

                    cantidadtotalCajetilla += canje.ComCantidad;
                }


                printer.DrawText("");
                printer.DrawText("TOTAL CAJETILLAS: " + cantidadtotalCajetilla.ToString().PadLeft(25));


                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("COBROS CHEQUES DEVUELTOS");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("----------------------------------------------");
                printer.DrawText("No.Recibo".PadRight(13) +
                "Documento".PadRight(18) + "Valor".PadLeft(13));
                printer.DrawText("----------------------------------------------");

                totalCobrosCKD = 0.00;
                double aplicado = 0.00;
                //string total;
                foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                    if (referencia.Length > 34)
                    {
                        referencia = referencia.Substring(0, 35);
                    }

                    aplicado = recibo.RecValor;
                    string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(referencia.PadRight(35) + aplicacion);

                    totalCobrosCKD += aplicado;

                }

                printer.DrawLine();
                string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);

                printer.DrawText("");
                printer.DrawText("");

                printer.DrawLine();
                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("COBROS REALIZADOS");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawLine();


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;
                double RecMontoTarjetaCreditoTotal = 0.0;

                foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    string codigo = "", cliente = "";
                    double totalCobrado = 0.0;
                    double efectivo = rec.RecMontoEfectivo;
                    double recMontoNC = rec.RecMontoNcr;
                    double recDescuento = rec.RecMontoDescuento;
                    double recMontoCheque = rec.RecMontoCheque;
                    double recMontoChequeFuturista = rec.RecMontoChequeF;
                    double recMontoTransferencia = rec.RecMontoTransferencia;
                    double recMontoSobrante = rec.RecMontoSobrante;
                    double recMontoRetencion = rec.RecRetencion;
                    double recMontoTarjetaCredito = rec.RecMontoTarjeta;

                    RecEfectivoTotal += efectivo;
                    RecMontoNCTotal += recMontoNC;
                    RecDescuentoTotal += recDescuento;
                    RecMontoChequeTotal += recMontoCheque;
                    RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                    RecMontoTransferenciaTotal += recMontoTransferencia;
                    RecMontoSobranteTotal += recMontoSobrante;
                    RecMontoRetencionTotal += recMontoRetencion;
                    RecMontoTarjetaCreditoTotal += recMontoTarjetaCredito;

                    string RecTipo = "";
                    codigo = rec.CliCodigo;
                    cliente = rec.CliNombre;
                    if (string.IsNullOrWhiteSpace(cliente))
                    {
                        cliente = "Cliente Suprimido";
                    }
                    totalCobrado = rec.RecTotal;
                    RecTipo = rec.RecTipo;

                    string cli = codigo.ToString() + "-" + cliente;

                    if (cli.Length > 24)
                    {
                        cli = cli.Substring(0, 25);
                    }

                    string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                    string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                    CobrosTotal += totalCobrado;


                }



                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;
                double RecMontoTarjetaCreditoTotalCrCon = 0.0;

                foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                {
                    //string codigo = "", cliente = "";
                    //double totalCobrado = 0.0;
                    double efectivoCrCon = rec.RecMontoEfectivo;
                    double recMontoNCCrCon = rec.RecMontoNcr;
                    double recDescuentoCrCon = rec.RecMontoDescuento;
                    double recMontoChequeCrCon = rec.RecMontoCheque;
                    double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                    double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                    double recMontoSobranteCrCon = rec.RecMontoSobrante;
                    double recMontoRetencionCrCon = rec.RecRetencion;
                    double recMontoTarjetaCreditoCrCon = rec.RecMontoTarjeta;

                    RecEfectivoTotalCrCon += efectivoCrCon;
                    RecMontoNCTotalCrCon += recMontoNCCrCon;
                    RecDescuentoTotalCrCon += recDescuentoCrCon;
                    RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                    RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                    RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                    RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                    RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    RecMontoTarjetaCreditoTotalCrCon += recMontoTarjetaCreditoCrCon;
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;
                RecMontoTarjetaCreditoTotalCrCon = RecMontoTarjetaCreditoTotalCrCon - TarjetaCredito;

                printer.DrawLine();
                string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("TARJETA CREDITO   :   ".PadRight(35) + ("RD$" + RecMontoTarjetaCreditoTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(9));
                printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(9)); ;

                totalCobros = 0.00;
                totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal + RecMontoTarjetaCreditoTotal);
                totalCobros = totalCobros - totalCobrosCKD;

                /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                printer.DrawText("");*/

                printer.DrawLine();

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));
                printer.DrawText("Cheques devueltos : ".PadRight(35) + ("RD$" + totalCobrosCKD.ToString("N", new CultureInfo("en-US"))));
                printer.DrawText("Pushmoney         : ".PadRight(35) + ("-RD$" + CompraCreditoTotal.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString());
                    printer.DrawText("------------------------------------------------");
                }


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 1");
            printer.DrawText("");
            printer.Print();


        }

        private void Formato3(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                /* if(cont == 0)
                 {
                     printer.TextAlign = Justification.CENTER;
                     printer.DrawText("- No hay productos en inventario inicial -");
                     printer.TextAlign = Justification.LEFT;
                 }*/
            }
            else if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count == 0)
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("- No hay productos en inventario inicial -");
                printer.TextAlign = Justification.LEFT;
            }

            if (CerrarCuadre)
            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia                      Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                string LPCuadre = "";
                bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
                if (!NoUseLP)
                {
                    if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre()))
                    {
                        LPCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                    }
                    else
                    {
                        LPCuadre = myUsosMul.GetFirstListaPrecio();
                    }
                }

                var ListToAlm = new DS_Almacenes().GetAlmacenesByAlmIDParameter(DS_RepresentantesParametros.GetInstance().GetParConteoFisicoAlmacenesParaContar());

                foreach (var alm in ListToAlm)
                {

                    printer.DrawLine();
                    printer.DrawText("INVENTARIO FINAL: " + alm.AlmDescripcion);
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawText("----------------------------------------------");

                    int cont = 0;
                    double ValoracionFinal = 0.0;

                    foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, alm.AlmID))
                    {
                        string Precio;
                        if (NoUseLP)
                        {
                            Precio = myCuadre.GetPrecioInProductos(prod.ProID);
                        }
                        else
                        {
                            Precio = myCuadre.GetPrecioinListaPrecio(prod.ProID, LPCuadre);
                        }
                        double CantidadTotal = prod.CuaCantidadFinal;
                        ValoracionFinal += (Convert.ToDouble(Precio) * CantidadTotal);

                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 34)
                        {
                            desc = desc.Substring(0, 34);
                        }

                        var cantidad = prod.CuaCantidadFinal.ToString();

                        if (prod.CuaCantidadDetalleFinal > 0)
                        {
                            cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                        }
                        string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                        if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                        {
                            cont++;
                            printer.DrawText((desc + " ").PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }

                    if (cont > 0)
                    {
                        printer.DrawText("VALORACION INVENTARIO:  ".PadRight(35) + "RD$" + ValoracionFinal.ToString("N", new CultureInfo("en-US")));
                    }


                    if (cont == 0)
                    {
                        printer.TextAlign = Justification.CENTER;
                        printer.DrawText("- No hay productos en inventario final -");
                        printer.TextAlign = Justification.LEFT;
                    }

                }

                ////////// CAMBIOS MERCANCIAS ///////////
                if (DS_RepresentantesParametros.GetInstance().GetParCambiosMercancia())
                {
                    printer.DrawText(" ");
                    printer.DrawText("CAMBIOS DE MERCANCIA");
                    printer.DrawText("-----------------------------------------------");
                    printer.DrawText("CAMBIOS POR CLIENTES");
                    printer.DrawText("Sec.     Codigo-Cliente");
                    printer.DrawText("-----------------------------------------------");
                    var Cambios = myCambios.GetAllCambiosMercanciaByCuadreByClientes(CuaSecuencia);

                    foreach (var cam in Cambios)
                    {
                        var cliCodigo = cam.CliCodigo;
                        var cliNombre = cam.CliNombre;
                        printer.DrawText(cam.CamSecuencia.ToString().PadRight(6) + " " + (cliCodigo + "-" + cliNombre).PadLeft(22));

                    }

                    printer.DrawText("");
                    printer.DrawText("-----------------------------------------------");
                    printer.DrawText("PRODUCTOS CAMBIADOS");
                    //printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawText("Codigo-Descripcion                         Cant");
                    printer.DrawText("-----------------------------------------------");
                    var CambiosByPro = myCambios.GetAllCambiosMercanciaByCuadreByProductos(CuaSecuencia);
                    foreach (var cam_pro in CambiosByPro)
                    {
                        var camCantidad = cam_pro.CamCantidad;
                        var camCantidaDetalle = cam_pro.CamCantidadDetalle;

                        var desc = cam_pro.ProCodigo + "-" + cam_pro.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        //printer.DrawText(desc.PadRight(35) + "  " + (camCantidad + "/" + camCantidaDetalle).PadLeft(10), 47);
                        printer.DrawText(desc.PadRight(35) + "  " + (camCantidad.ToString()).PadLeft(10), 47);


                    }
                    //printer.DrawText("");
                    printer.DrawLine();
                }

                /////////////////////////////////////////////////////

                if (myPedidos.GetPedidosByCuadre(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("PEDIDOS REALIZADOS");
                    printer.DrawText("");

                    printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                    printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                    printer.DrawLine();
                    printer.DrawText("");
                    int lastCliID = -1;
                    string linea1 = "";
                    double montoTotal = 0.0;
                    List<string> linea2 = new List<string>();
                    foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                    {
                        if (lastCliID != ped.CliID)
                        {//valida cuando el cliente sea diferente
                            lastCliID = ped.CliID;
                        }

                        int Ciclo = 0;
                        PedidosTotal = 0.0;

                        foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                        {

                            string DescripcionLineas1 = "";
                            string DescripcionLineas2 = "";

                            try
                            {
                                if (ped.CliNombre.Length > 35)
                                {
                                    DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                    DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                                }
                                else
                                {
                                    DescripcionLineas1 = ped.CliNombre;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: handle exception
                            }

                            double precio = pedBycli.PedPrecio;
                            double adValorem = pedBycli.PedAdValorem;
                            double selectivo = pedBycli.PedSelectivo;
                            double descuento = pedBycli.PedDescuento;
                            double itbis = pedBycli.PedItbis;
                            double proUnidades = pedBycli.ProUnidades;
                            double pedCantidad = pedBycli.PedCantidad;
                            double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                            double cantidad = 0.0;


                            if (pedCantidadDetalle > 0)
                            {
                                cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                            }
                            else
                            {
                                cantidad = pedCantidad;
                            }

                            double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                            PedidosTotal += precioNeto * cantidad;

                            Ciclo++;
                            if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                            {
                                linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                            }

                            if (DescripcionLineas1.Trim().Length > 28)
                            {
                                DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                            }

                            linea2.Add(pedBycli.ProCodigo + "-"
                            + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                            + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                        }

                        printer.DrawText(linea1);
                        foreach (var i in linea2)
                        {
                            printer.DrawText(i);
                        }
                        printer.DrawText("");
                        linea2.Clear();


                        montoTotal += PedidosTotal;
                    }

                    printer.DrawText("");
                    printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                    //printer.DrawText("");
                    printer.DrawLine();
                }


                //printer.DrawText("");
                //printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("ID   NCF      .Cliente                 Valor");
                printer.DrawText("----------------------------------------------");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    //printer.DrawText("");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    //printer.DrawText("");

                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado = TotalVentasContado + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }

                        var indicadorDescuentoOferta = "    ";

                        if (prod.VenDescuento > 0 || prod.VenIndicadorOferta)
                        {
                            indicadorDescuentoOferta = "(";
                            indicadorDescuentoOferta += prod.VenDescuento > 0 ? "D" : "";
                            indicadorDescuentoOferta += prod.VenIndicadorOferta ? "O" : "";
                            indicadorDescuentoOferta += indicadorDescuentoOferta.Length == 4 ? ")" : " )";

                        }

                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(indicadorDescuentoOferta + "" + desc.PadRight(31) + "RD$" + VentaTotal.PadLeft(8), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(35) + "RD$" + TotalVentasCont);

                    printer.DrawLine();
                }

                TotalVentas = TotalVentasContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(35) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("LEYENDA:");
                printer.DrawText("(D)-Venta con Descuento");
                printer.DrawText("(O)-Venta con Oferta");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }




                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    //printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                    printer.DrawLine();
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                // double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                //Clientes no visitados de la ruta
                var Clientesnovisitados = myVentas.GetClientesnoVisitadosPorCuadre(cuadre);

                if (Clientesnovisitados != null && Clientesnovisitados.Count > 0)
                {
                    //printer.DrawLine();

                    printer.DrawText("");
                    printer.DrawText("CLIENTES NO VISITADOS DE LA RUTA");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo     Nombre                    ");
                    printer.DrawText("----------------------------------------------");

                    foreach (var cliente in Clientesnovisitados)
                    {
                        string CodigoNombreCliente = cliente.CliCodigo.ToString() + "        " + cliente.CliNombre.ToString();
                        printer.DrawText(CodigoNombreCliente.PadRight(44));
                    }
                    printer.DrawText("");
                }


                //Clientes visitados con no venta y motivo de no venta
                var VisitasnoVenta = myVentas.GetClientesVisitasnoEfectivas(cuadre);

                if (VisitasnoVenta != null && VisitasnoVenta.Count > 0)
                {
                    printer.DrawText("CLIENTES VISITADOS SIN VENTA");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo - Nombre             Motivo       ");
                    printer.DrawText("----------------------------------------------");

                    var clientes = new List<model.Clientes>();
                    for (int i = 0; i < VisitasnoVenta.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(VisitasnoVenta[i].CliMotivo))
                        {
                            foreach (var cli in VisitasnoVenta)
                            {
                                if (!string.IsNullOrWhiteSpace(cli.CliMotivo) && cli.CliID == VisitasnoVenta[i].CliID && string.IsNullOrWhiteSpace(VisitasnoVenta[i].CliMotivo))
                                {
                                    VisitasnoVenta[i].CliMotivo = cli.CliMotivo;
                                    cli.CliID = 0;
                                }
                            }

                        }
                    }

                    foreach (var cli in VisitasnoVenta)
                    {
                        if (cli.CliID != 0)
                        {
                            string motivo = "";
                            string CodigoNombreCliente = cli.CliCodigo.ToString() + " - " + cli.CliNombre.ToString();
                            if (!string.IsNullOrWhiteSpace(cli.CliMotivo))
                            {
                                motivo = cli.CliMotivo;
                            }
                            printer.DrawText(CodigoNombreCliente.PadRight(25) + motivo.PadRight(20), 48);
                        }

                    }
                }


                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));

                //printer.DrawText("");
                //printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.GetResumenByCua(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 3");
            printer.DrawText("");
            printer.Print();


        }

        private void Formato2(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario inicial -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia                      Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("----------------------------------------------");
                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.CuaCantidadFinal.ToString();

                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        cont++;
                        printer.DrawText((desc + " ").PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario final -");
                    printer.TextAlign = Justification.LEFT;
                }

                if (myPedidos.GetPedidosByCuadre(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("PEDIDOS REALIZADOS");
                    printer.DrawText("");

                    printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                    printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                    printer.DrawLine();
                    printer.DrawText("");
                    int lastCliID = -1;
                    string linea1 = "";
                    double montoTotal = 0.0;
                    List<string> linea2 = new List<string>();
                    foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                    {
                        if (lastCliID != ped.CliID)
                        {//valida cuando el cliente sea diferente
                            lastCliID = ped.CliID;
                        }

                        int Ciclo = 0;
                        PedidosTotal = 0.0;

                        foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                        {

                            string DescripcionLineas1 = "";
                            string DescripcionLineas2 = "";

                            try
                            {
                                if (ped.CliNombre.Length > 35)
                                {
                                    DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                    DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                                }
                                else
                                {
                                    DescripcionLineas1 = ped.CliNombre;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: handle exception
                            }

                            double precio = pedBycli.PedPrecio;
                            double adValorem = pedBycli.PedAdValorem;
                            double selectivo = pedBycli.PedSelectivo;
                            double descuento = pedBycli.PedDescuento;
                            double itbis = pedBycli.PedItbis;
                            double proUnidades = pedBycli.ProUnidades;
                            double pedCantidad = pedBycli.PedCantidad;
                            double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                            double cantidad = 0.0;


                            if (pedCantidadDetalle > 0)
                            {
                                cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                            }
                            else
                            {
                                cantidad = pedCantidad;
                            }

                            double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                            PedidosTotal += precioNeto * cantidad;

                            Ciclo++;
                            if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                            {
                                linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                            }

                            if (DescripcionLineas1.Trim().Length > 28)
                            {
                                DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                            }

                            linea2.Add(pedBycli.ProCodigo + "-"
                            + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                            + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                        }

                        printer.DrawText(linea1);
                        foreach (var i in linea2)
                        {
                            printer.DrawText(i);
                        }
                        printer.DrawText("");
                        linea2.Clear();


                        montoTotal += PedidosTotal;
                    }

                    printer.DrawText("");
                    printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                    printer.DrawText("");
                    printer.DrawLine();
                }


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado = TotalVentasContado + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                    printer.DrawLine();
                }

                TotalVentas = TotalVentasContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }




                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 2");
            printer.DrawText("");
            printer.Print();


        }

        private void Formato4(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("ALMACEN DESPACHO");
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 24)
                    {
                        desc = desc.Substring(0, 24);
                    }


                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en el Almacen Despacho -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            printer.DrawText(" ");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("ALMACEN VENTAS");
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera()).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 24)
                    {
                        desc = desc.Substring(0, 24);
                    }


                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en el Almacen de Ventas  -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia        Almacen            Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuenciaByAlmacen(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var almacen = (cargasAceptadas.AlmID != -1 && cargasAceptadas.AlmID == DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho() ? cargasAceptadas.AlmID + " - Despacho" : cargasAceptadas.AlmID + " - Venta");

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(13) + "  " + almacen.PadRight(15) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(13));
                    }



                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS ALMACEN DESPACHO");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(25) + prod.CarLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }


                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS ALMACEN VENTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion        Lote        Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera()))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(25) + prod.CarLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }
                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("ALMACEN DESPACHO");
                printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                printer.DrawText("----------------------------------------------");
                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.CuaCantidadFinal.ToString();

                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        cont++;
                        printer.DrawText((desc + " ").PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }
                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en Almacen Despacho -");
                    printer.TextAlign = Justification.LEFT;
                }


                printer.DrawText("----------------------------------------------");
                printer.DrawText("ALMACEN VENTAS");
                printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                printer.DrawText("----------------------------------------------");
                int count = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.CuaCantidadFinal.ToString();

                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        count++;
                        printer.DrawText((desc + " ").PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                    }
                }

                if (count == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en Almacen Ventas -");
                    printer.TextAlign = Justification.LEFT;
                }

                if (myPedidos.GetPedidosByCuadre(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("PEDIDOS REALIZADOS");
                    printer.DrawText("");

                    printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                    printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                    printer.DrawLine();
                    printer.DrawText("");
                    int lastCliID = -1;
                    string linea1 = "";
                    double montoTotal = 0.0;
                    List<string> linea2 = new List<string>();
                    foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                    {
                        if (lastCliID != ped.CliID)
                        {//valida cuando el cliente sea diferente
                            lastCliID = ped.CliID;
                        }

                        int Ciclo = 0;
                        PedidosTotal = 0.0;

                        foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                        {

                            string DescripcionLineas1 = "";
                            string DescripcionLineas2 = "";

                            try
                            {
                                if (ped.CliNombre.Length > 35)
                                {
                                    DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                    DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                                }
                                else
                                {
                                    DescripcionLineas1 = ped.CliNombre;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: handle exception
                            }

                            double precio = pedBycli.PedPrecio;
                            double adValorem = pedBycli.PedAdValorem;
                            double selectivo = pedBycli.PedSelectivo;
                            double descuento = pedBycli.PedDescuento;
                            double itbis = pedBycli.PedItbis;
                            double proUnidades = pedBycli.ProUnidades;
                            double pedCantidad = pedBycli.PedCantidad;
                            double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                            double cantidad = 0.0;


                            if (pedCantidadDetalle > 0)
                            {
                                cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                            }
                            else
                            {
                                cantidad = pedCantidad;
                            }

                            double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                            PedidosTotal += precioNeto * cantidad;

                            Ciclo++;
                            if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                            {
                                linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                            }

                            if (DescripcionLineas1.Trim().Length > 28)
                            {
                                DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                            }

                            linea2.Add(pedBycli.ProCodigo + "-"
                            + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                            + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                        }

                        printer.DrawText(linea1);
                        foreach (var i in linea2)
                        {
                            printer.DrawText(i);
                        }
                        printer.DrawText("");
                        linea2.Clear();


                        montoTotal += PedidosTotal;
                    }

                    printer.DrawText("");
                    printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                    printer.DrawText("");
                    printer.DrawLine();
                }


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado, true).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado, true))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado, true).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado, true))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado = TotalVentasContado + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                    printer.DrawLine();
                }

                TotalVentas = TotalVentasContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    printer.DrawText("");
                }

                double TotalEntregas = 0;
                if (myVentas.getEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("ENTREGAS REALIZADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     Total");
                    printer.DrawText("----------------------------------------------");

                    TotalEntregas = 0;
                    foreach (var ent in myVentas.getEntregasRealizadas(CuaSecuencia))
                    {
                        var Cliente = ent.CliCodigo + "-" + ent.CliNombre;

                        if (Cliente.Length > 35)
                        {
                            Cliente = Cliente.Substring(0, 35);
                        }

                        var Total = ent.VenTotal;
                        TotalEntregas = TotalEntregas + ent.VenTotal;
                        printer.DrawText(Cliente.PadRight(35) + ("RD$" + Total).PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalEntregas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL ENTREGAS: ".PadRight(34) + "RD$" + TotalEntregas);

                    printer.DrawText("");
                    printer.DrawText("");
                }

                if (myVentas.getProductosEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS ENTREGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Nombre                       Cantidad");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosEntregasRealizadas(CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 2");
            printer.DrawText("");
            printer.Print();


        }


        private void Formato5(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            ////double VentasContadoTotal = 0.0;
            // double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Paq/Caj");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario inicial -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasByEstatus(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia            Fecha             Estado");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasByEstatus(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(26) + cargasAceptadas.EstadoDescripcion.PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Paq/Caj");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = !string.IsNullOrEmpty(prod.ProDatos3) && prod.ProDatos3.Equals("A") ? prod.CarCantidad.ToString("N2") : prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                string LPCuadre = "";
                bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
                if (!NoUseLP)
                {
                    if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre()))
                    {
                        LPCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                    }
                    else
                    {
                        LPCuadre = myUsosMul.GetFirstListaPrecio();
                    }
                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion          Cant         Monto");
                printer.DrawText("----------------------------------------------");
                int cont = 0;
                double ValoracionFinal = 0.0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    string Precio;
                    if (NoUseLP)
                    {
                        Precio = myCuadre.GetPrecioInProductos(prod.ProID);
                    }
                    else
                    {
                        Precio = myCuadre.GetPrecioinListaPrecio(prod.ProID, LPCuadre);
                    }
                    double CantidadTotal = prod.CuaCantidadFinal;
                    ValoracionFinal += (Convert.ToDouble(Precio) * CantidadTotal);
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.ProDatos3.Equals("A") ? Convert.ToInt32(prod.CuaCantidadFinal).ToString() : Convert.ToInt32(prod.CuaCantidadFinal).ToString();
                    double Monto = Convert.ToDouble(Precio) * Convert.ToDouble(cantidad);
                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        cont++;
                        printer.DrawText((desc + " ") + cantidad.PadLeft(8) + (Monto.ToString("N2")).PadLeft(13));
                    }
                }
                printer.DrawLine();
                if (cont > 0)
                {
                    printer.DrawText("VALORACION INVENTARIO: " + ("RD$" + ValoracionFinal.ToString("N2")).PadLeft(23));
                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario final -");
                    printer.TextAlign = Justification.LEFT;
                }

                if (myPedidos.GetPedidosByCuadre(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("PEDIDOS REALIZADOS");
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                    printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                    printer.DrawLine();
                    printer.DrawText("");
                    int lastCliID = -1;
                    string linea1 = "";
                    double montoTotal = 0.0;
                    List<string> linea2 = new List<string>();
                    foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                    {
                        if (lastCliID != ped.CliID)
                        {//valida cuando el cliente sea diferente
                            lastCliID = ped.CliID;
                        }

                        int Ciclo = 0;
                        PedidosTotal = 0.0;

                        foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                        {

                            string DescripcionLineas1 = "";
                            string DescripcionLineas2 = "";

                            try
                            {
                                if (ped.CliNombre.Length > 35)
                                {
                                    DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                    DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                                }
                                else
                                {
                                    DescripcionLineas1 = ped.CliNombre;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: handle exception
                            }

                            double precio = pedBycli.PedPrecio;
                            double adValorem = pedBycli.PedAdValorem;
                            double selectivo = pedBycli.PedSelectivo;
                            double descuento = pedBycli.PedDescuento;
                            double itbis = pedBycli.PedItbis;
                            double proUnidades = pedBycli.ProUnidades;
                            double pedCantidad = pedBycli.PedCantidad;
                            double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                            double cantidad = 0.0;


                            if (pedCantidadDetalle > 0)
                            {
                                cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                            }
                            else
                            {
                                cantidad = pedCantidad;
                            }

                            double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                            PedidosTotal += precioNeto * cantidad;

                            Ciclo++;
                            if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                            {
                                linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                            }

                            if (DescripcionLineas1.Trim().Length > 28)
                            {
                                DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                            }

                            linea2.Add(pedBycli.ProCodigo + "-"
                            + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                            + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                        }

                        printer.DrawText(linea1);
                        foreach (var i in linea2)
                        {
                            printer.DrawText(i);
                        }
                        printer.DrawText("");
                        linea2.Clear();


                        montoTotal += PedidosTotal;
                    }

                    printer.DrawText("");
                    printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                    printer.DrawText("");
                    printer.DrawLine();
                }


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");

                printer.DrawText("");
                printer.DrawText("");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                double TotalItbisVentasCredito = 0.0;
                double TotalCredito = 0.0;
                double TotalContado = 0.0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                double subtotal = 0;
                if (myVentas.GetVentasaCreditoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("NCF         Cliente                 Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");


                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByVenNCF(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliID.ToString().Substring(0, prod.CliID.ToString().Length - 1) + "-" + prod.CliNombre;
                        TotalVentasCredito += prod.VenTotal;
                        subtotal += prod.VenTotalSinItbis;
                        TotalItbisVentasCredito += prod.VenTotalItbis;
                        if (desc.Length > 32)
                        {
                            desc = desc.Substring(0, 32);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc + ("RD$" + TotalVenta).PadLeft(15), 47);
                    }
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    string SubTotalVentasCred = subtotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS A CREDITO   : " + ("RD$" + SubTotalVentasCred).PadLeft(16));
                    string TotalItbisVentasCred = TotalItbisVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("MONTO ITBIS VENTAS A CREDITO: " + ("RD$" + TotalItbisVentasCred).PadLeft(16));
                    TotalCredito = TotalVentasCredito;
                    string TotalVentasCred = TotalCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("MONTO TOTAL VENTAS A CREDITO: " + ("RD$" + TotalVentasCred).PadLeft(16));

                    printer.DrawLine();
                }

                double TotalVentasContado = 0;
                double TotalItbisVentasContado = 0.0;
                string TotalVentasConta = "0.0";
                if (myVentas.GetVentasaContadoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("NCF         Cliente                 Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    subtotal = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByVenNCFSinItbis(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliID.ToString().Substring(0, prod.CliID.ToString().Length - 1) + "-" + prod.CliNombre;
                        TotalVentasContado += prod.VenTotal;
                        TotalItbisVentasContado += prod.VenTotalItbis;
                        if (desc.Length > 32)
                        {
                            desc = desc.Substring(0, 32);
                        }
                        subtotal += prod.VenTotalSinItbis;
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc + ("RD$" + VentaTotal).PadLeft(15), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = subtotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS A CONTADO:    " + ("RD$" + TotalVentasCont).PadLeft(16));
                    string TotalItbisVentasCont = TotalItbisVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("MONTO ITBIS VENTAS A CONTADO: ") + ("RD$" + TotalItbisVentasCont).PadLeft(16));
                    TotalContado = TotalVentasContado;
                    TotalVentasConta = TotalContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("MONTO TOTAL VENTAS A CONTADO: ") + ("RD$" + TotalVentasConta).PadLeft(16));

                    printer.DrawLine();
                }

                TotalVentas = TotalContado + TotalCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL:".PadRight(34) + ("RD$" + TotalVentas2).PadRight(11));
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }


                        var cantidad = prod.VenCantidad.ToString();

                        var CantidadTotal = myProd.ConvertirUnidadesACajas(myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID), prod.VenCantidad, prod.VenCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                        var CantidaUnidades = Math.Round((CantidadTotal - (int)CantidadTotal) * myProd.GetProUnidades(prod.ProID), 0);

                        /*   if (prod.VenCantidadDetalle > 0)
                           {*/

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = (int)CantidadTotal + "/" + (int)CantidaUnidades;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }




                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                var myventas = myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (myventas.Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    double TotalItbisVentasanuladas = 0.0;
                    double TotalVentasanuladas = 0;
                    double TotalVentasConItbisanuladas = 0;
                    foreach (var venta in myventas)
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente + ("RD$" + TotaldeVenta).PadLeft(12));
                        TotalItbisVentasanuladas += venta.VenTotalItbis;
                        TotalVentasanuladas += venta.VenTotalSinItbis;
                        TotalVentasConItbisanuladas += venta.VenTotal;
                    }
                    printer.DrawLine();
                    string TotalVentasAn = TotalVentasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS ANULADAS: " + ("RD$" + TotalVentasAn).ToString().PadLeft(20));
                    string TotalItbisVentasAn = TotalItbisVentasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("ITBIS VENTAS ANULADAS:    " + ("RD$" + TotalItbisVentasAn).ToString().PadLeft(20));

                    totalventasanuladas = TotalVentasConItbisanuladas;
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:    " + ("RD$" + TotaldeVentasAn).ToString().PadLeft(20));
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }


                var myrec = myRec.GetRecibosByCuadreAnulados(CuaSecuencia);

                if (myrec.Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("RECIBOS ANULADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("recibo           cliente                codigo");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalrecibosanulados = 0;
                    foreach (var recibos in myrec)
                    {
                        string facturacliente = recibos.RecSecuencia +"     "+ recibos.CliNombre;
                        if (facturacliente.Length > 26)
                        {
                            facturacliente = facturacliente.Substring(0, 20);
                        }
                        else
                        {
                            for(int i = facturacliente.Length; i <= 26; i++)
                            {
                                facturacliente += " ";
                            }
                            
                        }

                        string totalderecibos = recibos.RecTotal.ToString("n", new CultureInfo("en-us"));
                        printer.DrawText(facturacliente + recibos.CliCodigo.PadLeft(12));
                        totalrecibosanulados += recibos.RecTotal;
                    }
                    printer.DrawLine();
                    string totalrecibosan = totalrecibosanulados.ToString("n", new CultureInfo("en-us"));
                    printer.DrawText("TOTAL RECIBOS ANULADOS: " + ("RD$" + totalrecibosan).ToString().PadLeft(20));

                    printer.DrawText("");
                    printer.DrawText("");
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;

                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre + ("RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString()).PadLeft(60));

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUSHMONEY:  ").PadRight(34) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                //RecEfectivoTotalCrCon = RecEfectivoTotalCrCon;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   " + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("CHEQUES           :   " + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("CHEQUES DIFERIDOS :   " + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("TRANSFERENCIAS    :   " + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("RETENCION         :   " + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("DESCUENTOS        :   " + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("NOTAS DE CREDITO  :   " + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("SOBRANTE          :   " + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(70)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : " + ("RD$" + TotalVentasConta.ToString(new CultureInfo("en-US"))).PadLeft(75));

                printer.DrawText("Cobros Realizados : " + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                printer.DrawText("Cheques Devueltos : " + ("RD$" + totalCobrosCKD.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                printer.DrawText("PushMoney :         " + ("RD$ -" + CompraCreditoTotal.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                //CompraCreditoTotal

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((Convert.ToDouble(TotalVentasConta) + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    /*
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    */
                    Resumen.mPromVentasPorVisitas = (TotalVentas / Resumen.mCantidadVisitasPositivas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 5");
            printer.DrawText("");
            printer.Print();


        }
        private void Formato12(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            ////double VentasContadoTotal = 0.0;
            // double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                      Cant.");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario inicial -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia                      Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                      Cant.");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = !string.IsNullOrEmpty(prod.ProDatos3) && prod.ProDatos3.Equals("A") ? prod.CarCantidad.ToString("N2") : prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                string LPCuadre = "";
                bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
                if (!NoUseLP)
                {
                    if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre()))
                    {
                        LPCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                    }
                    else
                    {
                        LPCuadre = myUsosMul.GetFirstListaPrecio();
                    }
                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion          Cant         Monto");
                printer.DrawText("----------------------------------------------");
                int cont = 0;
                double ValoracionFinal = 0.0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    string Precio;
                    if (NoUseLP)
                    {
                        Precio = myCuadre.GetPrecioInProductos(prod.ProID);
                    }
                    else
                    {
                        Precio = myCuadre.GetPrecioinListaPrecio(prod.ProID, LPCuadre);
                    }
                    double CantidadTotal = prod.CuaCantidadFinal;
                    ValoracionFinal += (Convert.ToDouble(Precio) * CantidadTotal);
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.ProDatos3.Equals("A") ? Convert.ToInt32(prod.CuaCantidadFinal).ToString() : Convert.ToInt32(prod.CuaCantidadFinal).ToString();
                    double Monto = Convert.ToDouble(Precio) * Convert.ToDouble(cantidad);
                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        cont++;
                        printer.DrawText((desc + " ") + cantidad.PadLeft(8) + (Monto.ToString("N2")).PadLeft(13));
                    }
                }
                printer.DrawLine();
                if (cont > 0)
                {
                    printer.DrawText("VALORACION INVENTARIO: " + ("RD$" + ValoracionFinal.ToString("N2")).PadLeft(23));
                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario final -");
                    printer.TextAlign = Justification.LEFT;
                }

                if (myPedidos.GetPedidosByCuadre(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("PEDIDOS REALIZADOS");
                    printer.DrawText("");
                    printer.DrawLine();
                    printer.DrawText("No. Orden  Cliente" + "                   Valor Ref.");
                    printer.DrawText("Codigo-Descripción" + "                   Cant/Und");
                    printer.DrawLine();
                    printer.DrawText("");
                    int lastCliID = -1;
                    string linea1 = "";
                    double montoTotal = 0.0;
                    List<string> linea2 = new List<string>();
                    foreach (var ped in myPedidos.GetPedidosByCuadre(CuaSecuencia))
                    {
                        if (lastCliID != ped.CliID)
                        {//valida cuando el cliente sea diferente
                            lastCliID = ped.CliID;
                        }

                        int Ciclo = 0;
                        PedidosTotal = 0.0;

                        foreach (var pedBycli in myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia))
                        {

                            string DescripcionLineas1 = "";
                            string DescripcionLineas2 = "";

                            try
                            {
                                if (ped.CliNombre.Length > 35)
                                {
                                    DescripcionLineas1 = ped.CliNombre.Substring(0, 35);
                                    DescripcionLineas2 = ped.CliNombre.Substring(35, ped.CliNombre.Length);
                                }
                                else
                                {
                                    DescripcionLineas1 = ped.CliNombre;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: handle exception
                            }

                            double precio = pedBycli.PedPrecio;
                            double adValorem = pedBycli.PedAdValorem;
                            double selectivo = pedBycli.PedSelectivo;
                            double descuento = pedBycli.PedDescuento;
                            double itbis = pedBycli.PedItbis;
                            double proUnidades = pedBycli.ProUnidades;
                            double pedCantidad = pedBycli.PedCantidad;
                            double pedCantidadDetalle = pedBycli.PedCantidadDetalle;
                            double cantidad = 0.0;


                            if (pedCantidadDetalle > 0)
                            {
                                cantidad = pedCantidad + (pedCantidadDetalle / proUnidades);
                            }
                            else
                            {
                                cantidad = pedCantidad;
                            }

                            double precioNeto = (precio + selectivo + adValorem - descuento) * (1 + (itbis / 100));
                            PedidosTotal += precioNeto * cantidad;

                            Ciclo++;
                            if (Ciclo == myPedidos.GetPedidosByClientes(ped.CliID, ped.PedSecuencia).Count)
                            {
                                linea1 = ped.PedSecuencia + " " + DescripcionLineas1.Trim().PadRight(36) + PedidosTotal;
                            }

                            if (DescripcionLineas1.Trim().Length > 28)
                            {
                                DescripcionLineas1 = DescripcionLineas1.Trim().Substring(0, 28);
                            }

                            linea2.Add(pedBycli.ProCodigo + "-"
                            + (pedBycli.ProDescripcion.Trim().Length > 20 ? pedBycli.ProDescripcion.Trim().Substring(0, 20).PadRight(36) : pedBycli.ProDescripcion.Trim().PadRight(36))
                            + pedCantidad + "/" + pedCantidadDetalle + (pedBycli.PedIndicadorOferta ? " -O" : "   "));

                        }

                        printer.DrawText(linea1);
                        foreach (var i in linea2)
                        {
                            printer.DrawText(i);
                        }
                        printer.DrawText("");
                        linea2.Clear();


                        montoTotal += PedidosTotal;
                    }

                    printer.DrawText("");
                    printer.DrawText("TOTAL PEDIDOS:  ".PadRight(36) + "RD$" + montoTotal);
                    printer.DrawText("");
                    printer.DrawLine();
                }


                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");

                printer.DrawText("");
                printer.DrawText("");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                double TotalItbisVentasCredito = 0.0;
                double TotalCredito = 0.0;
                double TotalContado = 0.0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                double subtotal = 0;
                if (myVentas.GetVentasaCreditoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("NCF         Cliente                 Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");


                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito += prod.VenTotal;
                        subtotal += prod.VenTotalSinItbis;
                        TotalItbisVentasCredito += prod.VenTotalItbis;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + ("RD$" + TotalVenta).PadRight(10), 47);
                    }
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    string SubTotalVentasCred = subtotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS A CREDITO   : " + ("RD$" + SubTotalVentasCred).PadLeft(13));
                    string TotalItbisVentasCred = TotalItbisVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("MONTO ITBIS VENTAS A CREDITO: " + ("RD$" + TotalItbisVentasCred).PadLeft(13));
                    TotalCredito = TotalVentasCredito;
                    string TotalVentasCred = TotalCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("MONTO TOTAL VENTAS A CREDITO: " + ("RD$" + TotalVentasCred).PadLeft(13));

                    printer.DrawLine();
                }

                double TotalVentasContado = 0;
                double TotalItbisVentasContado = 0.0;
                string TotalVentasConta = "0.0";
                if (myVentas.GetVentasaContadoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("NCF         Cliente                 Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    subtotal = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaSinItbis(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado += prod.VenTotal;
                        TotalItbisVentasContado += prod.VenTotalItbis;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        subtotal += prod.VenTotalSinItbis;
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        //printer.DrawText(desc + ("RD$" + VentaTotal).PadLeft(16), 47);
                        printer.DrawText(desc.PadRight(32) + ("RD$" + VentaTotal).PadLeft(14), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = subtotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS A CONTADO:    " + ("RD$" + TotalVentasCont).PadLeft(16));
                    string TotalItbisVentasCont = TotalItbisVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("MONTO ITBIS VENTAS A CONTADO: ") + ("RD$" + TotalItbisVentasCont).PadLeft(16));
                    TotalContado = TotalVentasContado;
                    TotalVentasConta = TotalContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("MONTO TOTAL VENTAS A CONTADO: ") + ("RD$" + TotalVentasConta).PadLeft(16));

                    printer.DrawLine();
                }

                TotalVentas = TotalContado + TotalCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL:".PadRight(34) + ("RD$" + TotalVentas2).PadRight(11));
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }


                        var cantidad = prod.VenCantidad.ToString();

                        var CantidadTotal = myProd.ConvertirUnidadesACajas(myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID), prod.VenCantidad, prod.VenCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                        var CantidaUnidades = Math.Round((CantidadTotal - (int)CantidadTotal) * myProd.GetProUnidades(prod.ProID), 0);

                        /*   if (prod.VenCantidadDetalle > 0)
                           {*/

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = (int)CantidadTotal + "/" + (int)CantidaUnidades;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }




                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                var myventas = myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (myventas.Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    double TotalItbisVentasanuladas = 0.0;
                    double TotalVentasanuladas = 0;
                    double TotalVentasConItbisanuladas = 0;
                    foreach (var venta in myventas)
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente + ("RD$" + TotaldeVenta).PadLeft(12));
                        TotalItbisVentasanuladas += venta.VenTotalItbis;
                        TotalVentasanuladas += venta.VenTotalSinItbis;
                        TotalVentasConItbisanuladas += venta.VenTotal;
                    }
                    printer.DrawLine();
                    string TotalVentasAn = TotalVentasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("SUBTOTAL VENTAS ANULADAS: " + ("RD$" + TotalVentasAn).ToString().PadLeft(20));
                    string TotalItbisVentasAn = TotalItbisVentasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("ITBIS VENTAS ANULADAS:    " + ("RD$" + TotalItbisVentasAn).ToString().PadLeft(20));

                    totalventasanuladas = TotalVentasConItbisanuladas;
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:    " + ("RD$" + TotaldeVentasAn).ToString().PadLeft(20));
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Cheque = 0.00, NotaCredito = 0.00, Retencion = 0.00, Transferencia = 0.00;
                /*
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre + ("RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString()).PadLeft(60));

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUSHMONEY:  ").PadRight(34) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }
                */
                //string total = "";
                /*
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }
                */


                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                //RecEfectivoTotalCrCon = RecEfectivoTotalCrCon;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   " + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("CHEQUES           :   " + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("CHEQUES DIFERIDOS :   " + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("TRANSFERENCIAS    :   " + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("RETENCION         :   " + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("DESCUENTOS        :   " + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("NOTAS DE CREDITO  :   " + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(70));
                    printer.DrawText("SOBRANTE          :   " + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(70)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : " + ("RD$" + TotalVentasConta.ToString(new CultureInfo("en-US"))).PadLeft(75));

                printer.DrawText("Cobros Realizados : " + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                //printer.DrawText("Cheques Devueltos : " + ("RD$" + totalCobrosCKD.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                //printer.DrawText("PushMoney :         " + ("RD$ -" + CompraCreditoTotal.ToString("N", new CultureInfo("en-US"))).PadLeft(75));

                //CompraCreditoTotal

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((Convert.ToDouble(TotalVentasConta) + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    /*
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    */
                    Resumen.mPromVentasPorVisitas = (TotalVentas / Resumen.mCantidadVisitasPositivas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 12");
            printer.DrawText("");
            printer.Print();


        }

        private void Formato21(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            //double CompraCreditoTotal = 0.0;
            //double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            //double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");
            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                if (desc.Length >= 30)
                {
                    desc = desc.Substring(0, 30);
                }
                else
                {
                    desc = desc.PadRight(30);
                }

                var cantidad = prod.CuaCantidadInicial.ToString();

                if (prod.CuaCantidadDetalleInicial > 0)
                {
                    cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                }
                string unm = prod.UnmCodigo;
                if (prod.UnmCodigo.Length >= 3)
                    unm = prod.UnmCodigo.Substring(0, 3);

                if (prod.CuaCantidadInicial > 0 || prod.CuaCantidadDetalleInicial > 0)
                {
                    printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                }
            }

            if (CerrarCuadre)
            {

                DS_Devoluciones myDev = new DS_Devoluciones();
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("DEVOLUCIONES REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Secuencia         Cliente             Cantidad");
                printer.DrawText("----------------------------------------------");
                var devoluciones = myDev.GetDevolucionesByClientes();
                if (devoluciones.Count > 0)
                {

                    double TotalDevoluciones = devoluciones.Sum(e => e.DevCantidad);
                    foreach (var dev in devoluciones)
                    {
                        var Cliente = dev.DevSecuencia + "-" + dev.Name;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = dev.DevCantidad;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(34) + (TotalTexto).PadLeft(11), 47);
                    }
                    printer.DrawText("");
                    string TotalConducesTexto = TotalDevoluciones.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL DEVOLUCIONES: ".PadRight(34) + ("RD$" + TotalConducesTexto).PadLeft(11));

                    printer.DrawText("");
                    printer.DrawText("");

                    var productosDevoluciones = myDev.GetDevolucionesByProductos();
                    if (productosDevoluciones.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS EN DEVOLUCIONES");
                        printer.DrawText("----------------------------------------------");
                        printer.DrawText("Codigo-Descripcion                    Cantidad");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosDevoluciones)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }

                            var cantidad = prod.DevCantidad.ToString();

                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 21");
            printer.DrawText("");
            printer.Print();
        }

        private void Formato6(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }
            var myParam = DS_RepresentantesParametros.GetInstance();
            bool mostrarCajasUnidades = myParam.GetParCuadresMostrarCajasUnidades();

            printer.PrintEmpresa(CuaSecuencia, putfecha: myParam.Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            // double CompraCreditoTotal = 0.0;
            //double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            //double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO ");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");

            var parametrosAlmacenes = DS_RepresentantesParametros.GetInstance().GetAlmacenesAgrupados("ALMID").ToList();
            foreach (var parametro in parametrosAlmacenes)
            {
                int almid = -1;
                int.TryParse(parametro.ParValor, out almid);
                string almcen = parametro.ParDescripcion.ToUpper().Trim();
                printer.DrawText("----------------------------------------------");
                printer.DrawText(almcen);
                //printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                printer.DrawText($"Codigo-Descripcion          Lote      {(mostrarCajasUnidades ? "Caj" : "Cant")}/Und");
                printer.DrawText("----------------------------------------------");

                var inventarioPorAlmacen = myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, almid).ToList();
                if (inventarioPorAlmacen != null && inventarioPorAlmacen.Count > 0)
                {

                    int cont = 0;
                    foreach (var prod in inventarioPorAlmacen)
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                        if (desc.Length >= 24)
                        {
                            desc = desc.Substring(0, 24);
                        }


                        var cantidad = prod.CuaCantidadInicial.ToString();
                        if (mostrarCajasUnidades)
                        {
                            int cajas = (int)(prod.CuaCantidadInicial / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                            int unidades = (int)(prod.CuaCantidadInicial - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                            cantidad = $"{cajas}/{unidades}";
                        }
                        else if (prod.CuaCantidadDetalleInicial != 0)
                        {
                            cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                        }
                        string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                        if (prod.CuaCantidadInicial != 0)
                        {
                            cont++;
                            printer.DrawText(desc.PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                        }

                    }
                    if (cont == 0)
                    {
                        printer.TextAlign = Justification.CENTER;
                        printer.DrawText($"- No hay productos en el {almcen} - ");
                        printer.TextAlign = Justification.LEFT;
                    }
                }
                else
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText($"- No hay productos en el {almcen} - ");
                    printer.TextAlign = Justification.LEFT;
                }
                printer.DrawText(" ");

            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia        Almacen            Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuenciaByAlmacen(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        //var almacen = (cargasAceptadas.AlmID != -1 ? cargasAceptadas.AlmID.ToString() : "").ToString();
                        var almacen = cargasAceptadas.AlmDescripcion;

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(13) + "  " + almacen.PadRight(15) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(13));
                    }

                    printer.DrawText("");
                    foreach (var parAlmacen in parametrosAlmacenes)
                    {
                        int almid = -1;
                        int.TryParse(parAlmacen.ParValor, out almid);
                        string almacen = parAlmacen.ParDescripcion.ToUpper().Trim();

                        printer.DrawText("----------------------------------------------");
                        printer.DrawText($"PRODUCTOS CARGADOS {almacen}");
                        printer.DrawText("----------------------------------------------");
                        // printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                        printer.DrawText($"Codigo-Descripcion          Lote      {(mostrarCajasUnidades ? "Caj" : "Cant")}/Und");
                        printer.DrawLine();
                        foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia, almid))
                        {
                            var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (desc.Length > 24)
                            {
                                desc = desc.Substring(0, 24);
                            }

                            var cantidad = prod.CarCantidad.ToString();
                            if (mostrarCajasUnidades)
                            {
                                int cajas = (int)(prod.CarCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                                int unidades = (int)(prod.CarCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));
                                cantidad = $"{cajas}/{unidades}";
                            }
                            else if (prod.CarCantidadDetalle > 0)
                            {
                                cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                            }

                            printer.DrawText(desc.PadRight(25) + prod.CarLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                        }
                    }

                }


                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                foreach (var parametro in parametrosAlmacenes)
                {
                    int almid = -1;
                    int.TryParse(parametro.ParValor, out almid);
                    string almcen = parametro.ParDescripcion.ToUpper().Trim();

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText(almcen);
                    //printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
                    printer.DrawText($"Codigo-Descripcion          Lote      {(mostrarCajasUnidades ? "Caj" : "Cant")}/Und");
                    printer.DrawText("----------------------------------------------");
                    int cont = 0;
                    foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacen(CuaSecuencia, almid))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var cantidad = prod.CuaCantidadFinal.ToString();

                        if (mostrarCajasUnidades)
                        {
                            int cajas = (int)(prod.CuaCantidadFinal / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                            int unidades = (int)(prod.CuaCantidadFinal - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                            cantidad = $"{cajas}/{unidades}";
                        }
                        else if (prod.CuaCantidadDetalleFinal > 0)
                        {
                            cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                        }
                        string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                        if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                        {
                            cont++;
                            printer.DrawText((desc + " ").PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
                        }
                    }
                    if (cont == 0)
                    {
                        printer.TextAlign = Justification.CENTER;
                        printer.DrawText($"- No hay productos en el {almcen} - ");
                        printer.TextAlign = Justification.LEFT;
                    }
                }


                double TotalEntregas = 0;
                DS_EntregasRepartidorTransacciones myEnt = new DS_EntregasRepartidorTransacciones();
                var entregas = myEnt.GetEntregasTransaccionesRealizadasFromCuadre(CuaSecuencia);
                printer.DrawText(" ");
                printer.DrawText("ENTREGAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                     Total");
                printer.DrawText("----------------------------------------------");
                if (entregas.Count > 0)
                {

                    TotalEntregas = entregas.Sum(e => e.EntTotal);
                    foreach (var ent in entregas)
                    {
                        var Cliente = ent.CliCodigo + "-" + ent.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = ent.EntTotal;
                        // printer.DrawText(Cliente.PadRight(34) + ("RD$" + Total).PadLeft(10), 47);
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(34) + ("RD$" + TotalTexto).PadLeft(11), 47);
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalEntregas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL ENTREGAS: ".PadRight(34) + ("RD$" + TotalEntregasTotalEntregas).PadLeft(11));

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosEntregados = myEnt.getProductosEntregasTransaccionesRealizadas(CuaSecuencia);
                    if (productosEntregados.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS ENTREGADOS");
                        printer.DrawText("----------------------------------------------");
                        printer.DrawText($"Codigo-Nombre                      {(mostrarCajasUnidades ? "    Caj/Und" : "Cantidad")}");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosEntregados)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }

                            var cantidad = prod.EntCantidad.ToString();
                            if (mostrarCajasUnidades)
                            {
                                int cajas = (int)(prod.EntCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                                int unidades = (int)(prod.EntCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                                cantidad = $"{cajas}/{unidades}";

                            }
                            else if (prod.EntCantidadDetalle > 0)
                            {
                                cantidad = cantidad + "/" + prod.EntCantidadDetalle;
                            }

                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }

                printer.DrawText("");
                printer.DrawText("");
                DS_Conduces myCond = new DS_Conduces();
                printer.DrawText("CONDUCES REALIZADOS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                     Total");
                printer.DrawText("----------------------------------------------");
                var conduces = myCond.getConducesRealizados(CuaSecuencia);
                if (conduces.Count > 0)
                {


                    double TotalConduces = conduces.Sum(e => e.ConMontoTotal);
                    foreach (var con in conduces)
                    {
                        var Cliente = con.CliCodigo + "-" + con.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = con.ConMontoTotal;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(34) + ("RD$" + TotalTexto).PadLeft(11), 47);
                    }
                    printer.DrawText("");
                    string TotalConducesTexto = TotalConduces.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL CONDUCES: ".PadRight(34) + ("RD$" + TotalConducesTexto).PadLeft(11));

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosEnConduces = myCond.getProductosConducesRealizadas(CuaSecuencia);
                    if (productosEnConduces.Count > 0)
                    {
                        printer.DrawText("PRODUCTOS EN CONDUCES");
                        printer.DrawText("----------------------------------------------");
                        //printer.DrawText("Codigo-Nombre                       Cantidad");
                        printer.DrawText($"Codigo-Nombre                      {(mostrarCajasUnidades ? "    Caj/Und" : "Cantidad")}");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosEnConduces)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }

                            var cantidad = prod.ConCantidad.ToString();
                            if (mostrarCajasUnidades)
                            {
                                int cajas = (int)(prod.ConCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                                int unidades = (int)(prod.ConCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                                cantidad = $"{cajas}/{unidades}";
                            }

                            //if (prod.EntCantidadDetalle > 0)
                            //{
                            //    cantidad = cantidad + "/" + prod.EntCantidadDetalle;
                            //}

                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }

                DS_Devoluciones myDev = new DS_Devoluciones();
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("DEVOLUCIONES REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                     Total");
                printer.DrawText("----------------------------------------------");
                var devoluciones = myDev.getDevolucionesRealizados(CuaSecuencia);
                if (devoluciones.Count > 0)
                {


                    double TotalDevoluciones = devoluciones.Sum(e => e.DevMontoTotal);
                    foreach (var dev in devoluciones)
                    {
                        var Cliente = dev.CliCodigo + "-" + dev.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = dev.DevMontoTotal;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(34) + ("RD$" + TotalTexto).PadLeft(11), 47);
                    }
                    printer.DrawText("");
                    string TotalConducesTexto = TotalDevoluciones.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL DEVOLUCIONES: ".PadRight(34) + ("RD$" + TotalConducesTexto).PadLeft(11));

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosDevoluciones = myDev.getProductosDevolucionesRealizadas(CuaSecuencia);
                    if (productosDevoluciones.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS EN DEVOLUCIONES");
                        printer.DrawText("----------------------------------------------");
                        //printer.DrawText("Codigo-Nombre                       Cantidad");
                        printer.DrawText($"Codigo-Nombre                      {(mostrarCajasUnidades ? "    Caj/Und" : "Cantidad")}");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosDevoluciones)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }

                            var cantidad = prod.DevCantidad.ToString();
                            if (mostrarCajasUnidades)
                            {
                                int cajas = (int)(prod.DevCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                                int unidades = (int)(prod.DevCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));
                                cantidad = $"{cajas}/{unidades}";
                            }
                            //if (prod.EntCantidadDetalle > 0)
                            //{
                            //    cantidad = cantidad + "/" + prod.EntCantidadDetalle;
                            //}

                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("MONTO A DEPOSITAR POR SECTOR:");
                printer.Font = PrinterFont.BODY;

                double montoGeneralEntregar = 0;
                var sectores = new DS_Sectores().GetSectores();
                var entregasPorCuadre = myEnt.GetMontoTotalEntregasPorSectorFromCuadre(CuaSecuencia);
                var conducesPorCuadres = myCond.GetMontoTotalConducesPorSectorFromCuadre(CuaSecuencia);
                foreach (var sector in sectores)
                {


                    printer.DrawText("");
                    printer.Bold = true;
                    printer.DrawText(sector.SecDescripcion.Trim());
                    printer.Bold = false;

                    double montoEntrega = entregasPorCuadre == null ? 0 : entregasPorCuadre.Where(s => s.SecCodigo == sector.SecCodigo).Sum(s => s.EntTotal);

                    printer.DrawText("");
                    printer.DrawText("   Total Entregas    : ".PadRight(34) + ("RD$" + montoEntrega.ToString("N", new CultureInfo("en-US"))).PadLeft(11));

                    double montoConduce = conducesPorCuadres == null ? 0 : conducesPorCuadres.Where(s => s.SecCodigo == sector.SecCodigo).Sum(s => s.ConMontoTotal);
                    printer.DrawText("   Total Conduces    : ".PadRight(34) + ("RD$" + montoConduce.ToString("N", new CultureInfo("en-US"))).PadLeft(11));

                    double totalPorSector = montoConduce + montoEntrega;
                    printer.Bold = true;
                    printer.DrawText("   Total             : ".PadRight(34) + ("RD$" + totalPorSector.ToString("N", new CultureInfo("en-US"))).PadLeft(11));
                    printer.Bold = false;
                    montoGeneralEntregar += totalPorSector;
                }



                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = montoGeneralEntregar.ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total a Entregar    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;
                /*
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }
                */
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                //printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText(Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 2");
            printer.DrawText("");
            printer.Print();


        }


        //RANCHERO ESPECIAL
        private void Formato13(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }
            var myParam = DS_RepresentantesParametros.GetInstance();
           // bool mostrarCajasUnidades = myParam.GetParCuadresMostrarCajasUnidades();

            printer.PrintEmpresa(CuaSecuencia, putfecha: myParam.Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }


            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");



            if (CerrarCuadre)

            {

                DS_EntregasRepartidorTransacciones myEnt = new DS_EntregasRepartidorTransacciones();
                printer.DrawText(" ");
                printer.DrawText("PEDIDOS REALIZADOS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                      Total");
                printer.DrawText("----------------------------------------------");
                var pedidos = myEnt.GetPedidosByCuadre(CuaSecuencia);
                if (pedidos.Count > 0)
                {

                    double TotalPedidos = pedidos.Sum(p => p.PedMontoTotal);
                    foreach (var ped in pedidos)
                    {
                        var Cliente = ped.CliCodigo + "-" + ped.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = ped.PedMontoTotal;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(33) + ("RD$" + TotalTexto).PadLeft(12), 47);
                         
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalPedidos.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL PEDIDOS: ".PadRight(33) + ("RD$" + TotalEntregasTotalEntregas).PadLeft(12), 47);

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosEnPedidos = myEnt.GetProductosPedidosRealizadosCajasUnidades(CuaSecuencia);
                    if (productosEnPedidos.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS PEDIDOS");
                        printer.DrawText("----------------------------------------------");
                        printer.DrawText($"Codigo-Nombre                         Caj/Und");// {(mostrarCajasUnidades ? "    Caj/Und" : "Cantidad")}");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosEnPedidos)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }

                            int cajasPorDetalle = (int)(prod.PedCantidadDetalle / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                            int cajas = (int)prod.PedCantidad + cajasPorDetalle;
                            int unidades = (int)(prod.PedCantidadDetalle - (cajasPorDetalle * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                            string cantidad = $"{cajas}/{unidades}";


                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }




                double TotalEntregas = 0;
                var entregas = myEnt.GetEntregasTransaccionesRealizadasFromCuadre(CuaSecuencia);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("ENTREGAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                      Total");
                printer.DrawText("----------------------------------------------");
                if (entregas.Count > 0)
                {

                    TotalEntregas = entregas.Sum(e => e.EntTotal);
                    foreach (var ent in entregas)
                    {
                        var Cliente = ent.CliCodigo + "-" + ent.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = ent.EntTotal;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(33) + ("RD$" + TotalTexto).PadLeft(12), 47);
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalEntregas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL ENTREGAS: ".PadRight(33) + ("RD$" + TotalEntregasTotalEntregas).PadLeft(12), 47);

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosEntregados = myEnt.getProductosEntregasTransaccionesRealizadasCajasYUnidades(CuaSecuencia);
                    if (productosEntregados.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS ENTREGADOS");
                        printer.DrawText("----------------------------------------------");
                        printer.DrawText($"Codigo-Nombre                         Caj/Und");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosEntregados)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }


                            int cajasPorDetalle = (int)(prod.EntCantidadDetalle / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                            int cajas = (int)prod.EntCantidad + cajasPorDetalle;
                            int unidades = (int)(prod.EntCantidadDetalle - (cajasPorDetalle * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                            string cantidad = $"{cajas}/{unidades}";
                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }



                DS_Devoluciones myDev = new DS_Devoluciones();
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("DEVOLUCIONES REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                      Total");
                printer.DrawText("----------------------------------------------");
                var devoluciones = myDev.getDevolucionesRealizados(CuaSecuencia);
                if (devoluciones.Count > 0)
                {


                    double TotalDevoluciones = devoluciones.Sum(e => e.DevMontoTotal);
                    foreach (var dev in devoluciones)
                    {
                        var Cliente = dev.CliCodigo + "-" + dev.CliNombre;

                        if (Cliente.Length > 34)
                        {
                            Cliente = Cliente.Substring(0, 33);
                        }

                        var Total = dev.DevMontoTotal;
                        string TotalTexto = Total.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(Cliente.PadRight(33) + ("RD$" + TotalTexto).PadLeft(12), 47);
                    }
                    printer.DrawText("");
                    string TotalConducesTexto = TotalDevoluciones.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL DEVOLUCIONES: ".PadRight(33) + ("RD$" + TotalConducesTexto).PadLeft(12), 47);

                    printer.DrawText("");
                    printer.DrawText("");


                    var productosDevoluciones = myDev.getProductosDevolucionesRealizadasCajasyUnidades(CuaSecuencia);
                    if (productosDevoluciones.Count > 0)
                    {
                        printer.DrawText(" ");
                        printer.DrawText("PRODUCTOS EN DEVOLUCIONES");
                        printer.DrawText("----------------------------------------------");
                        printer.DrawText($"Codigo-Nombre                         Caj/Und");
                        printer.DrawText("----------------------------------------------");

                        foreach (var prod in productosDevoluciones)
                        {
                            var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                            if (Producto.Length > 35)
                            {
                                Producto = Producto.Substring(0, 35);
                            }
                            
                            int cajasPorDetalle = (int)(prod.DevCantidadDetalle / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                            int cajas = (int)prod.DevCantidad + cajasPorDetalle;
                            int unidades = (int)(prod.DevCantidadDetalle - (cajasPorDetalle * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                            string cantidad = $"{cajas}/{unidades}";

                            printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                        }
                    }
                }






                printer.DrawText("");
                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("COBROS REALIZADOS");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawLine();

                var recibos = myEnt.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia);



                double totalCobros = 0.0;
                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (recibos.Count > 0)
                {
                    foreach (var rec in recibos)
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myEnt.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myEnt.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                

                if (recibos.Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");

                    printer.DrawText("EFECTIVO          : ".PadRight(26) + ("RD$" + RecEfectivoTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("CHEQUES           : ".PadRight(26) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("CHEQUES DIFERIDOS : ".PadRight(26) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("TRANSFERENCIAS    : ".PadRight(26) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("RETENCION         : ".PadRight(26) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("DESCUENTOS        : ".PadRight(26) + ("RD$" + RecDescuentoTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("NOTAS DE CREDITO  : ".PadRight(26) + ("RD$" + RecMontoNCTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));
                    printer.DrawText("SOBRANTE          : ".PadRight(26) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N", new CultureInfo("en-US"))));//.PadLeft(11));

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                   // totalCobros = totalCobros;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                //printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));
                
                printer.DrawText("COBROS REALIZADOS : ".PadRight(26) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))).PadLeft(11));

    
                //printer.Font = PrinterFont.BODY;
                printer.DrawText("");
                printer.DrawText("");

                //printer.Bold = true;
                //string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                //printer.Font = PrinterFont.TITLE;
                //printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                //printer.Font = PrinterFont.BODY;







                //printer.DrawText("");
                //printer.Font = PrinterFont.TITLE;
                //printer.DrawText("MONTO A DEPOSITAR POR SECTOR:");
                //printer.Font = PrinterFont.BODY;

                //double montoGeneralEntregar = 0;
                //var sectores = new DS_Sectores().GetSectores();
                //var entregasPorCuadre = myEnt.GetMontoTotalEntregasPorSectorFromCuadre(CuaSecuencia);
                ////var conducesPorCuadres = null;// myCond.GetMontoTotalConducesPorSectorFromCuadre(CuaSecuencia);
                //foreach (var sector in sectores)
                //{


                //    printer.DrawText("");
                //    printer.Bold = true;
                //    printer.DrawText(sector.SecDescripcion.Trim());
                //    printer.Bold = false;

                //    double montoEntrega = entregasPorCuadre == null ? 0 : entregasPorCuadre.Where(s => s.SecCodigo == sector.SecCodigo).Sum(s => s.EntTotal);

                //    printer.DrawText("");
                //    printer.DrawText("   Total Entregas    : ".PadRight(34) + ("RD$" + montoEntrega.ToString("N", new CultureInfo("en-US"))).PadLeft(11));

                //    //double montoConduce = 0;
                //    //printer.DrawText("   Total Conduces    : ".PadRight(34) + ("RD$" + montoConduce.ToString("N", new CultureInfo("en-US"))).PadLeft(11));

                //    double totalPorSector = montoEntrega;
                //    printer.Bold = true;
                //    printer.DrawText("   Total             : ".PadRight(34) + ("RD$" + totalPorSector.ToString("N", new CultureInfo("en-US"))).PadLeft(11));
                //    printer.Bold = false;
                //    montoGeneralEntregar += totalPorSector;
                //}



                //printer.DrawText("");
                //printer.DrawText("");

                //printer.Bold = true;
                //string TotalDeposito = montoGeneralEntregar.ToString("N", new CultureInfo("en-US"));
                //printer.Font = PrinterFont.TITLE;
                //printer.DrawText("Total a Entregar    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
               // printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText(Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 13");
            printer.DrawText("");
            printer.Print();


        }


        private void Formato7(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + " ) " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("VENTAS REALIZADAS");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("NCF         Cliente                 Valor");
            printer.DrawText("----------------------------------------------");

            double TotalVentas = 0;
            double TotalVentasCredito = 0;
            string contado = DS_RepresentantesParametros.GetInstance().GetParMultiConIdFormaPagoContado();
            if (myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, true).Count > 0)
            {
                printer.DrawText("VENTAS A CREDITO:");
                printer.DrawText("");

                TotalVentasCredito = 0;
                foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, true))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                printer.DrawLine();
            }

            double TotalVentasContado = 0;

            if (myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, true).Count > 0)
            {
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("");

                TotalVentasContado = 0;
                foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, true))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasContado = TotalVentasContado + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                printer.DrawLine();
            }

            TotalVentas = TotalVentasContado + TotalVentasCredito;
            string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);


            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 7");
            printer.DrawText("");
            printer.Print();


        }


        private void Formato8(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            var myRep = new DS_Representantes();
            var repAy1 = myRep.GetRepNombre(cuadre.RepAyudante1);
            var repAy2 = myRep.GetRepNombre(cuadre.RepAyudante2);

            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + " ) " + Arguments.CurrentUser.RepNombre);
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 1: (" + cuadre.RepAyudante1 + " ) " + repAy1);
            }
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 2: (" + cuadre.RepAyudante2 + " ) " + repAy2);
            }

            printer.DrawText("No. Liquidacion: " + Functions.CrearNoLiquidacion(Arguments.CurrentUser.RepCodigo, CuaSecuencia).ToString());
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }

            ////////// CAMBIOS MERCANCIAS ///////////
            if (DS_RepresentantesParametros.GetInstance().GetParCambiosMercancia())
            {
                printer.DrawText("");
                printer.DrawText(" ");
                printer.DrawText("CAMBIOS DE MERCANCIA");
                printer.DrawText("-----------------------------------------------");
                printer.DrawText("CAMBIOS POR CLIENTES");
                printer.DrawText("Sec.     Codigo-Cliente");
                printer.DrawText("-----------------------------------------------");
                var Cambios = myCambios.GetAllCambiosMercanciaByCuadreByClientes(CuaSecuencia);

                foreach (var cam in Cambios)
                {
                    var cliCodigo = cam.CliCodigo;
                    var cliNombre = cam.CliNombre;
                    printer.DrawText(cam.CamSecuencia.ToString().PadRight(6) + " " + (cliCodigo + "-" + cliNombre).PadLeft(22));

                }

                printer.DrawText("");
                printer.DrawText("-----------------------------------------------");
                printer.DrawText("PRODUCTOS CAMBIADOS");
                //printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("Codigo-Descripcion                         Cant");
                printer.DrawText("-----------------------------------------------");
                var CambiosByPro = myCambios.GetAllCambiosMercanciaByCuadreByProductos(CuaSecuencia);
                foreach (var cam_pro in CambiosByPro)
                {
                    var camCantidad = cam_pro.CamCantidad;
                    var camCantidaDetalle = cam_pro.CamCantidadDetalle;

                    var desc = cam_pro.ProCodigo + "-" + cam_pro.ProDescripcion;

                    if (desc.Length > 35)
                    {
                        desc = desc.Substring(0, 35);
                    }

                    //printer.DrawText(desc.PadRight(35) + "  " + (camCantidad + "/" + camCantidaDetalle).PadLeft(10), 47);
                    printer.DrawText(desc.PadRight(35) + "  " + (camCantidad.ToString()).PadLeft(10), 47);


                }
                //printer.DrawText("");
                printer.DrawLine();
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("VENTAS REALIZADAS");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("NCF         Cliente                 Valor");
            printer.DrawText("----------------------------------------------");

            double TotalVentas = 0;
            double TotalVentasCredito = 0;
            string contado = DS_RepresentantesParametros.GetInstance().GetParMultiConIdFormaPagoContado();
            if (myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CREDITO:");
                printer.DrawText("");

                TotalVentasCredito = 0;
                foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                printer.DrawLine();
            }

            double TotalVentasContado = 0;

            if (myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("");

                TotalVentasContado = 0;
                foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasContado = TotalVentasContado + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                printer.DrawLine();
            }

            TotalVentas = TotalVentasContado + TotalVentasCredito;
            string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);


            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 8");
            printer.DrawText("");
            printer.Print();


        }


        private void Formato35(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            var myRep = new DS_Representantes();
            var repAy1 = myRep.GetRepNombre(cuadre.RepAyudante1);
            var repAy2 = myRep.GetRepNombre(cuadre.RepAyudante2);

            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + " ) " + Arguments.CurrentUser.RepNombre);
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 1: (" + cuadre.RepAyudante1 + " ) " + repAy1);
            }
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 2: (" + cuadre.RepAyudante2 + " ) " + repAy2);
            }

            printer.DrawText("No. Liquidacion: " + Functions.CrearNoLiquidacion(Arguments.CurrentUser.RepCodigo, CuaSecuencia).ToString());
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }

            ////////// CAMBIOS MERCANCIAS ///////////
            if (DS_RepresentantesParametros.GetInstance().GetParCambiosMercancia())
            {

                var Cambios = myCambios.GetAllCambiosMercanciaByCuadreByClientes(CuaSecuencia);

                if (Cambios != null)
                {
                    if (Cambios.Count > 0)
                    {
                        printer.DrawText("");
                        printer.DrawText(" ");
                        printer.DrawText("CAMBIOS DE MERCANCIA");
                        printer.DrawText("--------------------------------");
                        printer.DrawText("CAMBIOS POR CLIENTES");
                        printer.DrawText("Sec.     Codigo-Cliente");
                        printer.DrawText("--------------------------------");

                        foreach (var cam in Cambios)
                        {
                            var cliCodigo = cam.CliCodigo;
                            var cliNombre = cam.CliNombre;
                            printer.DrawText(cam.CamSecuencia.ToString().PadRight(6) + " " + (cliCodigo + "-" + cliNombre).PadLeft(22));

                        }

                        printer.DrawText("");
                        printer.DrawText("--------------------------------");
                        printer.DrawText("PRODUCTOS CAMBIADOS");
                        //printer.DrawText("Codigo-Descripcion                    Cant/Und");
                        printer.DrawText("Codigo-Descripcion");
                        printer.DrawText("Cantidad");
                        printer.DrawText("--------------------------------");

                        var CambiosByPro = myCambios.GetAllCambiosMercanciaByCuadreByProductos(CuaSecuencia);

                        foreach (var cam_pro in CambiosByPro)
                        {
                            var camCantidad = cam_pro.CamCantidad;
                            var camCantidaDetalle = cam_pro.CamCantidadDetalle;

                            var desc = cam_pro.ProCodigo + "-" + cam_pro.ProDescripcion;

                            if (desc.Length > 35)
                            {
                                desc = desc.Substring(0, 35);
                            }

                            //printer.DrawText(desc.PadRight(35) + "  " + (camCantidad + "/" + camCantidaDetalle).PadLeft(10), 47);
                            printer.DrawText(desc.PadRight(35));
                            printer.DrawText(camCantidad.ToString(), 47);

                        }
                        //printer.DrawText("");
                        printer.DrawText("--------------------------------");
                    }
                }

            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("VENTAS REALIZADAS");
            printer.DrawText("--------------------------------");
            printer.DrawText("NCF         Cliente");
            printer.DrawText("Valor");
            printer.DrawText("--------------------------------");

            double TotalVentas = 0;
            double TotalVentasCredito = 0;
            string contado = DS_RepresentantesParametros.GetInstance().GetParMultiConIdFormaPagoContado();
            if (myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CREDITO:");
                printer.DrawText("");

                TotalVentasCredito = 0;
                foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                printer.DrawText("--------------------------------");
            }

            double TotalVentasContado = 0;

            if (myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("");

                TotalVentasContado = 0;
                foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPago(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasContado = TotalVentasContado + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32));
                    printer.DrawText("RD$" + VentaTotal, 47);
                }
                printer.DrawText("");
                string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                printer.DrawText("--------------------------------");
            }

            TotalVentas = TotalVentasContado + TotalVentasCredito;
            string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("TOTAL GENERAL: ".PadRight(18) + "RD$" + TotalVentas2);


            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 35");
            printer.DrawText("");
            printer.Print();


        }

        public void ImprimirFormato3(int CuaSecuencia)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool AbrirCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
            }

            if (AbrirCuadre)
            {
                printer.DrawText(" 	APERTURA CUADRE DE VENTAS ");
            }
            else
            {
                printer.DrawText(" 	CIERRE CUADRE DE VENTAS ");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;

            var myCuaImpr = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            printer.DrawText("");
            printer.DrawText("Fecha:      " + DateTime.Now.ToString());
            printer.DrawText("Chofer:     " + Arguments.CurrentUser.RepCodigo, 48);
            printer.DrawText("Ficha:      " + myCuaImpr.VehFicha);
            printer.DrawText("Cuadre No.: " + CuaSecuencia);

            printer.DrawText("");

            printer.DrawText("ARQUEO DE CONTADOR");
            printer.DrawLine();
            printer.DrawText("Cant. Ini.    Cant. Log     Cant. Fis     Dif");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText(myCuadre.GetCuaContadorInicial(CuaSecuencia).ToString() + myCuadre.GetVehContador(myCuaImpr.VehFicha).ToString().PadLeft(22) +
                myCuadre.GetCuaContadorFinal(CuaSecuencia).ToString().PadLeft(14) + (myCuadre.GetCuaContadorFinal(CuaSecuencia) - myCuadre.GetVehContador(myCuaImpr.VehFicha)).ToString().PadLeft(8));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("ARQUEO DE KILOMETRAJE");
            printer.DrawLine();
            printer.DrawText("Km. Ini.      Km. Log       Km. Fis       Dif");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText(myCuadre.GetKmInicial(CuaSecuencia).ToString() + myCuadre.GetKmVehiculo(myCuaImpr.VehFicha).ToString().PadLeft(20) +
                             myCuadre.GetKmFin(CuaSecuencia).ToString().PadLeft(14) +
                            (myCuadre.GetKmFin(CuaSecuencia) - myCuadre.GetKmVehiculo(myCuaImpr.VehFicha)).ToString().PadLeft(10));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("PEDIDOS A CONTADO");
            printer.DrawLine();
            printer.DrawText("Cliente                    Sec           Monto");
            printer.DrawLine();

            var PedidosContado = myCuadre.GetPedidosaContadoByCuaSecuencia(CuaSecuencia);
            double montoTotalCon = 0.0;

            foreach (var ped in PedidosContado)
            {
                montoTotalCon += ped.PedidosTotal;

                if (ped.CliNombre.Length > 27)
                {
                    ped.CliNombre = ped.CliNombre.Substring(0, 26);
                }

                printer.DrawText(ped.CliNombre.Trim() + ped.PedSecuencia.ToString().PadLeft(4) + ped.PedidosTotal.ToString("N2").PadRight(18));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("Total: " + montoTotalCon.ToString("N2").PadLeft(39));
            printer.DrawText("");
            printer.DrawText("PEDIDOS A CREDITO");
            printer.DrawLine();
            printer.DrawText("Cliente                    Sec           Monto");
            printer.DrawLine();


            var PedidosCredito = myCuadre.GetPedidosaCreditoByCuaSecuencia(CuaSecuencia);
            double montoTotalCred = 0.0;

            foreach (var ped in PedidosCredito)
            {
                montoTotalCred += ped.PedidosTotal;

                if (ped.CliNombre.Length > 27)
                {
                    ped.CliNombre = ped.CliNombre.Substring(0, 26);
                }

                printer.DrawText(ped.CliNombre.Trim() + ped.PedSecuencia.ToString().PadLeft(4) + ped.PedidosTotal.ToString("N2").PadLeft(18));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("Total: " + montoTotalCred.ToString("N2").PadLeft(39));

            printer.DrawText("");
            printer.DrawText("COBROS");
            printer.DrawLine();
            printer.DrawText("Cliente                    Sec           Monto");
            printer.DrawLine();

            var PedidosCobros = myCuadre.GetCuandreImprimirPedidosCobros(CuaSecuencia);
            double montoTotalCob = 0.0;

            foreach (var ped in PedidosCobros)
            {
                montoTotalCob += ped.RecTotal;

                if (ped.CliNombre.Length > 27)
                {
                    ped.CliNombre = ped.CliNombre.Substring(0, 26);
                }

                printer.DrawText(ped.CliNombre + ped.RecSecuencia + ped.RecTotal);
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("Total: " + montoTotalCob.ToString("N2").PadLeft(39));

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            //buscar la documentacion
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato Cuadres 3");

            printer.Print();

        }

        private void Formato22(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader(), Notbold: true);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            ////double VentasContadoTotal = 0.0;
            // double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            //double PedidosTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en inventario inicial -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia                      Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                string LPCuadre = "";
                bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
                if (!NoUseLP)
                {
                    if (!String.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre()))
                    {
                        LPCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                    }
                    else
                    {
                        LPCuadre = myUsosMul.GetFirstListaPrecio();
                    }
                }

                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Cant/Und");
                printer.DrawText("----------------------------------------------");

                double TotalVentasCredito = 0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");
                    printer.DrawText("----------------------------------------------");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito += prod.VenCantidad;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        printer.DrawText(desc.PadRight(32) + prod.VenCantidad.ToString().PadLeft(12), 47);
                    }
                    printer.DrawText("");
                    printer.DrawText("TOTAL LIBRAS VENDIDAS A CREDITO: " + (TotalVentasCredito.ToString()));

                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuenciaSinItbis(CuaSecuencia, contado).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaSinItbis(CuaSecuencia, contado))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado += prod.VenCantidad;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }

                        string VentaTotal = prod.VenCantidad.ToString();
                        printer.DrawText(desc.PadRight(32) + VentaTotal.PadLeft(12), 47);
                    }

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    printer.DrawText(("TOTAL LIBRAS VENDIDAS A CONTADO: ").PadRight(33) + (TotalVentasContado.ToString()).PadRight(11));

                    printer.DrawText("----------------------------------------------");
                }

                printer.DrawText("TOTAL GENERAL:".PadRight(33) + ((TotalVentasContado + TotalVentasCredito).ToString()).PadRight(11));
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }




                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 22");
            printer.DrawText("");
            printer.Print();


        }


        private void Formato9(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            var myRep = new DS_Representantes();
            var repAy1 = myRep.GetRepNombre(cuadre.RepAyudante1);
            var repAy2 = myRep.GetRepNombre(cuadre.RepAyudante2);

            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RepCodigo + " ) " + Arguments.CurrentUser.RepNombre);
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 1: (" + cuadre.RepAyudante1 + " ) " + repAy1);
            }
            if (repAy1 != "")
            {
                printer.DrawText("Ayudante 2: (" + cuadre.RepAyudante2 + " ) " + repAy2);
            }

            printer.DrawText("No. Liquidacion: " + Functions.CrearNoLiquidacion(Arguments.CurrentUser.RepCodigo, CuaSecuencia).ToString());
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }

            ////////// CAMBIOS MERCANCIAS ///////////
            if (DS_RepresentantesParametros.GetInstance().GetParCambiosMercancia())
            {
                printer.DrawText("");
                printer.DrawText(" ");
                printer.DrawText("CAMBIOS DE MERCANCIA");
                printer.DrawText("-----------------------------------------------");
                printer.DrawText("CAMBIOS POR CLIENTES");
                printer.DrawText("Sec.     Codigo-Cliente");
                printer.DrawText("-----------------------------------------------");
                var Cambios = myCambios.GetAllCambiosMercanciaByCuadreByClientes(CuaSecuencia);

                foreach (var cam in Cambios)
                {
                    var cliCodigo = cam.CliCodigo;
                    var cliNombre = cam.CliNombre;
                    printer.DrawText(cam.CamSecuencia.ToString().PadRight(6) + " " + (cliCodigo + "-" + cliNombre).PadLeft(22));

                }

                printer.DrawText("");
                printer.DrawText("-----------------------------------------------");
                printer.DrawText("PRODUCTOS CAMBIADOS");
                //printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("Codigo-Descripcion                         Cant");
                printer.DrawText("-----------------------------------------------");
                var CambiosByPro = myCambios.GetAllCambiosMercanciaByCuadreByProductos(CuaSecuencia);
                foreach (var cam_pro in CambiosByPro)
                {
                    var camCantidad = cam_pro.CamCantidad;
                    var camCantidaDetalle = cam_pro.CamCantidadDetalle;

                    var desc = cam_pro.ProCodigo + "-" + cam_pro.ProDescripcion;

                    if (desc.Length > 35)
                    {
                        desc = desc.Substring(0, 35);
                    }

                    //printer.DrawText(desc.PadRight(35) + "  " + (camCantidad + "/" + camCantidaDetalle).PadLeft(10), 47);
                    printer.DrawText(desc.PadRight(35) + "  " + (camCantidad.ToString()).PadLeft(10), 47);


                }
                //printer.DrawText("");
                printer.DrawLine();
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("VENTAS REALIZADAS");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("NCF         Cliente                 Valor");
            printer.DrawText("----------------------------------------------");

            double TotalVentas = 0;
            double TotalVentasCredito = 0;
            string contado = DS_RepresentantesParametros.GetInstance().GetParMultiConIdFormaPagoContado();
            if (myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPagoCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CREDITO:");
                printer.DrawText("");

                TotalVentasCredito = 0;
                foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPagoCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                printer.DrawLine();
            }

            double TotalVentasContado = 0;

            if (myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPagoConTotalCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
            {
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("");

                TotalVentasContado = 0;
                foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPagoConTotalCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasContado = TotalVentasContado + prod.VenTotal;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                }
                printer.DrawText("");
                string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                printer.DrawLine();
            }

            TotalVentas = TotalVentasContado + TotalVentasCredito;
            string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);


            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 8");
            printer.DrawText("");
            printer.Print();


        }

        private void Formato10(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            //double VentasContadoTotal = 0.0;
            //double VentasCreditoTotal = 0.0;
            double CompraCreditoTotal = 0.0;
            //double TotalGeneralVentas = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;
            int VenSecuenciaAlmRanchero = 0;
            int VenSecuenciaAlmAveriaBueno = 0;
            int VenSecuenciaAlmAveriaMal = 0;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("ALMACEN VENTAS");
            printer.DrawText("Codigo-Descripcion                    Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuenciaByAlmacenAgrupadoPorProducto(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacenAgrupadoPorProducto(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 24)
                    {
                        desc = desc.Substring(0, 24);
                    }


                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en el Almacen Despacho -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            printer.DrawText(" ");


            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia        Almacen            Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuenciaByAlmacen(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var almacen = (cargasAceptadas.AlmID != -1 && cargasAceptadas.AlmID == DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho() ? cargasAceptadas.AlmID + " - Despacho" : cargasAceptadas.AlmID + " - Venta");

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(13) + "  " + almacen.PadRight(15) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(13));
                    }



                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS ALMACEN VENTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargadosAgrupadosxProducto(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);

                    }



                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("ALMACEN VENTAS");
                printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("----------------------------------------------");
                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuenciaByAlmacenAgrupadoPorProducto(CuaSecuencia, DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDespacho()))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                    if (desc.Length > 24)
                    {
                        desc = desc.Substring(0, 24);
                    }

                    var cantidad = prod.CuaCantidadFinal.ToString();

                    if (prod.CuaCantidadDetalleFinal > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;

                    if (prod.CuaCantidadFinal != 0 || prod.CuaCantidadDetalleFinal != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en Almacen Despacho -");
                    printer.TextAlign = Justification.LEFT;
                }



                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado, true).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuencia(CuaSecuencia, contado, true))
                    {

                        if (!string.IsNullOrWhiteSpace(prod.VenReferencia))
                        {
                            if (int.Parse(prod.VenReferencia) == DS_RepresentantesParametros.GetInstance().GetParAlmacenVentaRanchera())
                            {
                                VenSecuenciaAlmRanchero = prod.VenSecuencia;
                            }
                            else if (int.Parse(prod.VenReferencia) == DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaDevolucion())
                            {
                                VenSecuenciaAlmAveriaBueno = prod.VenSecuencia;
                            }
                            else if (int.Parse(prod.VenReferencia) == DS_RepresentantesParametros.GetInstance().GetParAlmacenIdParaMalEstado())
                            {
                                VenSecuenciaAlmAveriaMal = prod.VenSecuencia;
                            }
                        }


                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado, true).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuencia(CuaSecuencia, contado, true))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado = TotalVentasContado + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                    printer.DrawLine();
                }

                TotalVentas = TotalVentasContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    printer.DrawText("");
                }

                double TotalEntregas = 0;
                if (myVentas.getEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("ENTREGAS REALIZADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     Total");
                    printer.DrawText("----------------------------------------------");

                    TotalEntregas = 0;
                    foreach (var ent in myVentas.getEntregasRealizadas(CuaSecuencia))
                    {
                        var Cliente = ent.CliCodigo + "-" + ent.CliNombre;

                        if (Cliente.Length > 35)
                        {
                            Cliente = Cliente.Substring(0, 35);
                        }

                        var Total = ent.VenTotal;
                        TotalEntregas = TotalEntregas + ent.VenTotal;
                        printer.DrawText(Cliente.PadRight(35) + ("RD$" + Total).PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalEntregas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL ENTREGAS: ".PadRight(34) + "RD$" + TotalEntregas);

                    printer.DrawText("");
                    printer.DrawText("");
                }

                if (myVentas.getProductosEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS ENTREGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Nombre                       Cantidad");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosEntregasRealizadas(CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 10");
            printer.DrawText("");
            printer.Print();

            if (DS_RepresentantesParametros.GetInstance().GetParCuadreImprimirFacturasxFaltantes())
            {
                if (VenSecuenciaAlmRanchero > 0)
                {
                    FormatoVenta9(VenSecuenciaAlmRanchero, false);
                }

                if (VenSecuenciaAlmAveriaBueno > 0)
                {
                    FormatoVenta9(VenSecuenciaAlmAveriaBueno, false);
                }

                if (VenSecuenciaAlmAveriaMal > 0)
                {
                    FormatoVenta9(VenSecuenciaAlmAveriaMal, false);
                }
            }



        }

        private void FormatoVenta9(int venSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: true);

            var venta = myVentas.GetBySecuencia(venSecuencia, confirmado);

            if (venta == null)
            {
                venta = myVentas.GetBySecuencia(venSecuencia, true);
                if (venta == null)
                {
                    throw new Exception("No se encontraron los datos de la venta");
                }
                else
                {
                    confirmado = true;
                }
            }


            printer.DrawText("");
            printer.DrawText("");


            if (venta.CliTipoComprobanteFAC != "99")
            {
                printer.DrawText("NCF: " + venta.VenNCF);

                if (venta.CliTipoComprobanteFAC == "01")
                {
                    printer.DrawText("Valida hasta: " + venta.VenNCFFechaVencimiento);
                }

                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");

            printer.TextAlign = Justification.CENTER;
            var NCFdivided = venta.VenNCF.ToCharArray();
            string NCFTipo = NCFdivided[1].ToString() + NCFdivided[2].ToString();
            var TipoNCF = SqliteManager.GetInstance().Query<model.UsosMultiples>("select Descripcion from UsosMultiples " +
                "where CodigoGrupo = 'NCFTIPO2018' and CodigoUso = '" + NCFTipo + "'", new string[] { });
            if (TipoNCF != null)
            {
                printer.DrawText(TipoNCF[0].Descripcion);
            }
            else
            {
                printer.DrawText("FACTURA VALIDA PARA CREDITO FISCAL");
            }
            printer.DrawText("");
            printer.DrawText(venta.VenCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");
            printer.DrawText("");
            printer.DrawText(venta.ConDescripcion);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Venta: " + Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, 48);
            printer.DrawText(myAlm.GetDescripcionAlmacen(int.Parse(venta.VenReferencia)));
            printer.DrawText("Fecha venta: " + venta.VenFecha);
            printer.DrawText("Cliente: " + venta.CliNombre, 48);
            printer.DrawText("RNC: " + venta.CliRnc, 48);
            printer.DrawText("Codigo: " + venta.CliCodigo);
            printer.DrawText("Calle: " + venta.CliCalle, 46);
            printer.DrawText("Urb: " + venta.CliUrbanizacion);
            printer.DrawText("Telefono: " + venta.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cant. Precio  Monto Itbis  Descuento  Importe");
            printer.DrawLine();

            double subTotalTotal = 0, itbisTotal = 0, descuentoTotal = 0, total = 0;
            int ventotal = 0;

            foreach (var det in myVentas.GetDetalleBySecuenciaSinLote(venSecuencia, confirmado))
            {
                var producto = det.ProCodigo + "-" + det.ProDescripcion.Trim();

                if (producto.Length >= 35)
                {
                    producto = producto.Substring(0, 35);
                }
                else
                {
                    producto = producto.PadRight(35);
                }
                printer.DrawText(producto);

                var cantidad = det.VenCantidad.ToString("N2");
                var precioLista = det.VenPrecio + det.VenAdValorem + det.VenSelectivo;
                var montoItbis = (precioLista - det.VenDescuento) * (det.VenItbis / 100);

                var precioConItbis = precioLista + montoItbis;
                var cantidadTotal = ((double.Parse(det.VenCantidadDetalle.ToString()) / det.ProUnidades) + det.VenCantidad);

                var montoItbisTotal = montoItbis * cantidadTotal;
                var subTotal = (precioLista - det.VenDescuento + montoItbis) * cantidadTotal;

                if (det.VenCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.VenCantidadDetalle.ToString("N2");
                }

                itbisTotal += montoItbisTotal;
                total += subTotal;
                descuentoTotal += (det.VenDescuento * cantidadTotal);
                subTotalTotal += (precioLista * cantidadTotal);
                ventotal += 1;

                printer.DrawText(cantidad.PadRight(8) + precioConItbis.ToString("N2").PadRight(9) +
                    montoItbisTotal.ToString("N2").PadRight(11) + det.VenDescuento.ToString("N2").PadRight(10) + subTotal.ToString("N2").PadLeft(5));
                printer.DrawText("");
            }

            printer.DrawLine();
            printer.DrawText("SKU: " + ventotal.ToString());
            printer.DrawText("");
            printer.DrawText("SubTotal:    " + subTotalTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Descuento:   " + descuentoTotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis: " + itbisTotal.ToString("N2").PadLeft(15));
            printer.Bold = true;
            printer.DrawText("Total:       " + total.ToString("N2").PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");

            if (((venta.ConID == DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado()) || (venta.ConID == DS_RepresentantesParametros.GetInstance().GetParSegundoConIdFormaPagoContado())) && venta.ConID != -1)
            {
                printer.TextAlign = Justification.CENTER;
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("");
                printer.DrawText("RECIBO DE PAGO");
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("FORMA DE PAGO");
                printer.TextAlign = Justification.LEFT;

                var controller = new DS_Recibos();

                var recibo = controller.GetReciboAplicacionByCxcDocumento(Arguments.CurrentUser.RepCodigo + "-" + venSecuencia, confirmado);

                if (recibo != null)
                {
                    var formasPago = controller.GetRecibosFormasPagoBySecuencia(recibo.RecSecuencia, confirmado);

                    if (formasPago != null)
                    {
                        foreach (var rec in formasPago)
                        {
                            switch (rec.ForID)
                            {
                                case 1:
                                    printer.DrawText("Efectivo: " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 2:
                                    printer.DrawText("Cheque " + (rec.RefIndicadorDiferido ? "diferido" : "normal") + "  Numero: " + rec.RefNumeroCheque.ToString());
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));

                                    if (rec.RefIndicadorDiferido)
                                    {
                                        printer.DrawText("Fecha: " + rec.RefFecha);
                                    }
                                    break;
                                case 4:
                                    printer.DrawText("Transferencia: " + rec.RefNumeroCheque);
                                    printer.DrawText("Fecha   : " + rec.RefFecha);
                                    printer.DrawText("Banco   : " + rec.BanNombre, 48);
                                    printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                                    break;
                                case 5:
                                    printer.DrawText("Retencion: " + rec.RefValor.ToString("N2").PadLeft(34));
                                    break;
                                case 6:
                                    printer.DrawText("Tarjeta crédito: " + rec.RefValor.ToString("N2").PadLeft(28));
                                    break;
                            }
                        }
                        printer.DrawLine();
                        printer.DrawText("Total pago: " + total.ToString("N2").PadLeft(35));
                        printer.DrawText("");
                    }
                }
            }

            var Representantes = new DS_Representantes();
            var RepVendedor = "";
            RepVendedor = Representantes.GetRepNombre(venta.RepVendedor);
            var day = "";
            switch (Convert.ToInt32(DateTime.Today.DayOfWeek))
            {

                case 1:
                    day = "Lunes";
                    break;
                case 2: //guaraguano
                    day = "Martes";
                    break;

                case 3: //Foodsmart
                    day = "Miercoles";
                    break;

                case 4: //LA TABACALERA - FRUTOS FERRER
                    day = "Jueves";
                    break;

                case 5:
                    day = "Viernes";
                    break;

                case 6:
                    day = "Sabado";
                    break;
                case 7:
                    day = "Domingo";
                    break;
            }


            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("-------------------------------------");
            printer.DrawText("Cedula");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Distribuidor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Vendedor: " + RepVendedor);
            printer.DrawText("Secuencia: " + venta.VisSecuencia);
            printer.DrawText("Dia de Visita: " + day);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato venta 9: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");

            myVentas.ActualizarCantidadImpresion(venta.rowguid);

            printer.Print();

        }

        private void Formato11(int CuaSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var cuadre = myCuadre.GetCuadresBySecuencia(CuaSecuencia);

            if (cuadre == null)
            {
                throw new Exception("Error cargando los datos del cuadre");
            }

            printer.PrintEmpresa(CuaSecuencia, putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }


            double CompraCreditoTotal = 0.0;
            double totalCobrosCKD = 0.00, totalCobros = 0.0;
            bool AbrirCuadre = true, CerrarCuadre = true;

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            if (cuadre.CuaEstatus == 1)
            {
                AbrirCuadre = true;
                CerrarCuadre = false;
            }
            else if (cuadre.CuaEstatus == 2)
            {
                AbrirCuadre = false;
                CerrarCuadre = true;
            }

            if (AbrirCuadre)
            {
                printer.DrawText("APERTURA CUADRE DE INVENTARIO ");
            }
            else
            {
                printer.DrawText("CIERRE CUADRE DE INVENTARIO");
            }

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: (" + Arguments.CurrentUser.RutID + " ) " + Arguments.CurrentUser.RepNombre);
            var vehiculo = myVehiculo.GetVehicleById(cuadre.VehID);
            //if (Convert.ToInt32(vehiculo) != 0 || vehiculo != null)
            if (vehiculo != null)
            {
                printer.DrawText("Ficha Vehiculo: " + vehiculo.VehFicha);
            }
            printer.DrawText("");
            var fechaValidaApertura = DateTime.TryParse(cuadre.CuaFechaInicio, out DateTime fecha1);
            printer.DrawText("Fecha Apertura: " + (fechaValidaApertura ? fecha1.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaInicio));
            if (CerrarCuadre)
            {
                var fechaValidaCierre = DateTime.TryParse(cuadre.CuaFechaFin, out DateTime fecha2);
                printer.DrawText("Fecha Cierre:   " + (fechaValidaCierre ? fecha2.ToString("dd/MM/yyyy hh:mm tt") : cuadre.CuaFechaFin));
            }
            printer.DrawText("");


            printer.DrawText("INVENTARIO INICIAL");
            printer.DrawText("----------------------------------------------");
            printer.DrawText("Codigo-Descripcion                Cant/Und");
            printer.DrawText("----------------------------------------------");

            if (myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia).Count > 0)
            {

                int cont = 0;
                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 24)
                    {
                        desc = desc.Substring(0, 24);
                    }


                    var cantidad = prod.CuaCantidadInicial.ToString();

                    if (prod.CuaCantidadDetalleInicial != 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleInicial;
                    }
                    string unm = prod.UnmCodigo.Length > 3 ? prod.UnmCodigo.Substring(0, 3) : prod.UnmCodigo;
                    if (prod.CuaCantidadInicial != 0)
                    {
                        cont++;
                        printer.DrawText(desc.PadRight(25) + cantidad.PadLeft(10), 47);
                    }

                }
                if (cont == 0)
                {
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("- No hay productos en el Almacen Despacho -");
                    printer.TextAlign = Justification.LEFT;
                }
            }

            printer.DrawText(" ");
            printer.DrawText("----------------------------------------------");

            if (CerrarCuadre)

            {
                if (myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    printer.DrawText("CARGAS ACEPTADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Referencia                      Fecha");
                    printer.DrawText("----------------------------------------------");
                    foreach (var cargasAceptadas in myCarga.GetCargasAceptadasByCuaSecuencia(CuaSecuencia))
                    {
                        var desc = " ";
                        if (string.IsNullOrWhiteSpace(cargasAceptadas.CarReferencia))
                        {
                            desc = " ";
                        }
                        else
                        {
                            desc = cargasAceptadas.CarReferencia;
                        }

                        if (desc.Length > 24)
                        {
                            desc = desc.Substring(0, 24);
                        }

                        var fecha = cargasAceptadas.CarFecha.ToString();
                        var fechaValidaCarga = DateTime.TryParse(fecha, out DateTime oficial);
                        printer.DrawText(desc.PadRight(25) + "  " + (fechaValidaCarga ? oficial.ToString("dd/MM/yyyy") : fecha).PadLeft(15));
                    }

                    printer.DrawText("");

                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("PRODUCTOS CARGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                    Cant/Und");
                    printer.DrawLine();
                    foreach (var prod in myCarga.GetProductosCargados(CuaSecuencia))
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (desc.Length > 35)
                        {
                            desc = desc.Substring(0, 35);
                        }

                        var cantidad = prod.CarCantidad.ToString();

                        if (prod.CarCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                        }

                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                printer.DrawLine();
                printer.DrawText("INVENTARIO FINAL");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("Codigo-Descripcion                    Cant/Und");
                printer.DrawText("----------------------------------------------");

                foreach (var prod in myCuadre.GetCuadresDetalleBySecuencia(CuaSecuencia))
                {
                    var desc = prod.ProCodigo + "-" + prod.ProDescripcion.Trim();

                    if (desc.Length >= 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    else
                    {
                        desc = desc.PadRight(30);
                    }

                    var cantidad = prod.CuaCantidadFinal.ToString();

                    if (prod.CuaCantidadDetalleInicial > 0)
                    {
                        cantidad = cantidad + "/" + prod.CuaCantidadDetalleFinal;
                    }
                    string unm = prod.UnmCodigo;
                    if (prod.UnmCodigo.Length >= 3)
                        unm = prod.UnmCodigo.Substring(0, 3);

                    if (prod.CuaCantidadFinal > 0 || prod.CuaCantidadDetalleFinal > 0)
                    {
                        printer.DrawText(desc.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }


                printer.DrawText("----------------------------------------------");

                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("VENTAS REALIZADAS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");

                double TotalVentas = 0;
                double TotalVentasCredito = 0;
                string contado = DS_RepresentantesParametros.GetInstance().GetParMultiConIdFormaPagoContado();
                if (myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPagoCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");

                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasaCreditoByCuaSecuenciaConVariasFormaPagoCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + TotalVenta.PadRight(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCred = TotalVentasCredito.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL VENTAS A CREDITO: ").PadRight(34) + "RD$" + TotalVentasCred);
                    printer.DrawLine();
                }

                double TotalVentasContado = 0;

                if (myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPagoConTotalCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false).Count > 0)
                {
                    printer.DrawText("VENTAS A CONTADO:");
                    printer.DrawText("");

                    TotalVentasContado = 0;
                    foreach (var prod in myVentas.GetVentasaContadoByCuaSecuenciaConVariasFormaPagoConTotalCalculado(CuaSecuencia, contado, Arguments.CurrentUser.RepCargo.ToString() == "VENDEDOR" ? true : false))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasContado = TotalVentasContado + prod.VenTotal;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(32) + "RD$" + VentaTotal.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalVentasCont = TotalVentasContado.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS A CONTADO: ".PadRight(34) + "RD$" + TotalVentasCont);

                    printer.DrawLine();
                }

                TotalVentas = TotalVentasContado + TotalVentasCredito;
                string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
                printer.DrawText("TOTAL GENERAL: ".PadRight(34) + "RD$" + TotalVentas2);
                printer.DrawText("");
                printer.DrawText("");

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS VENDIDOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosSinOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    printer.DrawText("");
                }

                double TotalEntregas = 0;
                if (myVentas.getEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("ENTREGAS REALIZADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     Total");
                    printer.DrawText("----------------------------------------------");

                    TotalEntregas = 0;
                    foreach (var ent in myVentas.getEntregasRealizadas(CuaSecuencia))
                    {
                        var Cliente = ent.CliCodigo + "-" + ent.CliNombre;

                        if (Cliente.Length > 35)
                        {
                            Cliente = Cliente.Substring(0, 35);
                        }

                        var Total = ent.VenTotal;
                        TotalEntregas = TotalEntregas + ent.VenTotal;
                        printer.DrawText(Cliente.PadRight(35) + ("RD$" + Total).PadLeft(10), 47);
                    }
                    printer.DrawText("");
                    string TotalEntregasTotalEntregas = TotalEntregas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL ENTREGAS: ".PadRight(34) + "RD$" + TotalEntregas);

                    printer.DrawText("");
                    printer.DrawText("");
                }

                if (myVentas.getProductosEntregasRealizadas(CuaSecuencia).Count > 0)
                {
                    printer.DrawText("PRODUCTOS ENTREGADOS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Nombre                       Cantidad");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosEntregasRealizadas(CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }
                }

                if (myVentas.getProductosVendidos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("PRODUCTOS OFERTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                     VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosOferta(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;

                        if (Producto.Length > 35)
                        {
                            Producto = Producto.Substring(0, 35);
                        }

                        var cantidad = prod.VenCantidad.ToString();

                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }

                        printer.DrawText(Producto.PadRight(35) + cantidad.PadLeft(10), 47);
                    }

                    printer.DrawText("");
                    printer.DrawText("");
                }


                if (myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.DrawText("VENTAS ANULADAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No. Fact.    Cliente                Valor");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("");
                    double totalventasanuladas = 0;
                    foreach (var venta in myVentas.getVentasAnuladas(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        if (venta.VenNCF == null)
                        {
                            venta.VenNCF = "--";
                        }
                        string FacturaCliente = venta.VenNCF.ToString() + "  " + venta.CliNombre.ToString();
                        if (FacturaCliente.Length > 35)
                        {
                            FacturaCliente = FacturaCliente.Substring(0, 34);
                        }
                        string TotaldeVenta = venta.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(FacturaCliente.PadRight(35) + "RD$" + TotaldeVenta);
                        totalventasanuladas += venta.VenTotal;
                    }
                    string TotaldeVentasAn = totalventasanuladas.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TOTAL VENTAS ANULADAS:  ".PadRight(35) + "RD$" + TotaldeVentasAn.ToString());
                    printer.DrawText("");
                    printer.DrawText("");


                    printer.DrawText("PRODUCTOS VENTAS ANULADAS");
                    printer.DrawText("------------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                       VEND.");
                    printer.DrawText("------------------------------------------------");
                    int Largo = 0;
                    foreach (var ProVentasAnuladas in myVentas.GetProductosVentasAnuladas(CuaSecuencia))
                    {
                        if (ProVentasAnuladas.ProDescripcion.Length < 26)
                        {
                            Largo = ProVentasAnuladas.ProDescripcion.Length;
                        }
                        else
                        {
                            Largo = 26;
                        }
                        string codigo = ProVentasAnuladas.ProCodigo;
                        string nombre = ProVentasAnuladas.ProDescripcion;
                        double venCantidad = ProVentasAnuladas.VenCantidad;
                        string unidadMedida = ProVentasAnuladas.UnmCodigo;

                        printer.DrawText((codigo + "-" + nombre.Substring(0, Largo)).PadRight(35) + venCantidad.ToString().PadLeft(9));

                    }
                }

                double Efectivo = 0.00, Cheque = 0.00, NotaCredito = 0.00, OrdenPago = 0.00, Retencion = 0.00, Transferencia = 0.00, TarjetaCredito = 0.00;
                if (myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {

                    printer.DrawText("");
                    printer.DrawText("________________________________________________");
                    printer.DrawLine();
                    printer.DrawText("PUSHMONEY");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.        Cliente                      Valor");
                    printer.DrawText("----------------------------------------------");
                    //printer.bold = true;

                    CompraCreditoTotal = 0;


                    foreach (var compra in myVentas.getPushmoney(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        int Sec = compra.ComSecuencia;

                        string Cliente = compra.CliNombre;

                        if (string.IsNullOrWhiteSpace(Cliente))
                        {
                            Cliente = "Cliente Suprimido";
                        };
                        //21 VenPrecio
                        double ComTotal = compra.ComTotal;
                        CompraCreditoTotal += ComTotal;

                        string Nombre = Sec.ToString() + " " + Cliente.ToString();
                        string TotalCompra = compra.ComTotal.ToString("N", new CultureInfo("en-US"));

                        if (Nombre.Length > 34)
                        {
                            Nombre = Nombre.Substring(0, 35);
                        }

                        printer.DrawText(Nombre.PadRight(35) + "RD$" + TotalCompra + " " + compra.TipoPagoDescripcion.ToString());

                        if (compra.ComTipoPago == "2")
                        {
                            Cheque += ComTotal;
                        }
                        else if (compra.ComTipoPago == "1")
                        {
                            Efectivo += ComTotal;
                        }
                        else if (compra.ComTipoPago == "3")
                        {
                            NotaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "18")
                        {
                            OrdenPago += ComTotal;
                        }
                        else if (compra.ComTipoPago == "5")
                        {
                            Retencion += ComTotal;
                        }
                        else if (compra.ComTipoPago == "6")
                        {
                            TarjetaCredito += ComTotal;
                        }
                        else if (compra.ComTipoPago == "4")
                        {
                            Transferencia += ComTotal;
                        }
                    }

                    printer.DrawText("");
                    printer.Bold = true;
                    string ComprasTotal = CompraCreditoTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(("TOTAL PUHSMONEY:  ").PadRight(35) + "RD$" + ComprasTotal);
                    printer.DrawText("");
                    printer.Bold = false;
                }

                double aplicado = 0.00;
                //string total = "";
                if (myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS CHEQUES DEVUELTOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("No.Recibo".PadRight(13) +
                    "Documento".PadRight(18) + "Valor".PadLeft(13));
                    printer.DrawText("----------------------------------------------");

                    totalCobrosCKD = 0.00;

                    foreach (var recibo in myVentas.getChequesDevueltos(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string referencia = recibo.RecNumero.ToString() + " " + recibo.cxcDocumento;
                        if (referencia.Length > 34)
                        {
                            referencia = referencia.Substring(0, 35);
                        }

                        aplicado = recibo.RecValor;
                        string aplicacion = aplicado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(referencia.PadRight(35) + aplicacion);

                        totalCobrosCKD += aplicado;

                    }

                    printer.DrawLine();
                    string TotalCKD = totalCobrosCKD.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Total Cheques devueltos:          ".PadRight(35) + TotalCKD);
                    printer.DrawText("");
                    printer.DrawText("");
                }



                if (myVentas.getDocumentosRec(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawText("");

                    printer.Font = PrinterFont.TITLE;
                    printer.DrawText("COBROS REALIZADOS");
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                    printer.DrawLine();
                }


                double CobrosTotal = 0.0;
                double RecEfectivoTotal = 0.0;
                double RecMontoNCTotal = 0.0;
                double RecDescuentoTotal = 0.0;
                double RecMontoChequeTotal = 0.0;
                double RecMontoChequeFuturistaTotal = 0.0;
                double RecMontoTransferenciaTotal = 0.0;
                double RecMontoSobranteTotal = 0.0;
                double RecMontoRetencionTotal = 0.0;

                //double CobrosTotalCrCon = 0.0;
                double RecEfectivoTotalCrCon = 0.0;
                double RecMontoNCTotalCrCon = 0.0;
                double RecDescuentoTotalCrCon = 0.0;
                double RecMontoChequeTotalCrCon = 0.0;
                double RecMontoChequeFuturistaTotalCrCon = 0.0;
                double RecMontoTransferenciaTotalCrCon = 0.0;
                double RecMontoSobranteTotalCrCon = 0.0;
                double RecMontoRetencionTotalCrCon = 0.0;

                if (myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    foreach (var rec in myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        string codigo = "", cliente = "";
                        double totalCobrado = 0.0;
                        double efectivo = rec.RecMontoEfectivo;
                        double recMontoNC = rec.RecMontoNcr;
                        double recDescuento = rec.RecMontoDescuento;
                        double recMontoCheque = rec.RecMontoCheque;
                        double recMontoChequeFuturista = rec.RecMontoChequeF;
                        double recMontoTransferencia = rec.RecMontoTransferencia;
                        double recMontoSobrante = rec.RecMontoSobrante;
                        double recMontoRetencion = rec.RecRetencion;

                        RecEfectivoTotal += efectivo;
                        RecMontoNCTotal += recMontoNC;
                        RecDescuentoTotal += recDescuento;
                        RecMontoChequeTotal += recMontoCheque;
                        RecMontoChequeFuturistaTotal += recMontoChequeFuturista;
                        RecMontoTransferenciaTotal += recMontoTransferencia;
                        RecMontoSobranteTotal += recMontoSobrante;
                        RecMontoRetencionTotal += recMontoRetencion;

                        string RecTipo = "";
                        codigo = rec.CliCodigo;
                        cliente = rec.CliNombre;
                        if (string.IsNullOrWhiteSpace(cliente))
                        {
                            cliente = "Cliente Suprimido";
                        }
                        totalCobrado = rec.RecTotal;
                        RecTipo = rec.RecTipo;

                        string cli = codigo.ToString() + "-" + cliente;

                        if (cli.Length > 24)
                        {
                            cli = cli.Substring(0, 25);
                        }

                        string FormaPago = myVentas.getFormasPago(rec.cxcReferencia);

                        string TotalCo = totalCobrado.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(cli.PadRight(25) + " " + "RD$" + TotalCo + "  " + FormaPago /*+ " " + rec.RecSecuencia.ToString()*/);

                        CobrosTotal += totalCobrado;


                    }


                    foreach (var rec in myVentas.getRecibosCreditoByCuaSecuencia2(Arguments.CurrentUser.RepCodigo, CuaSecuencia))
                    {
                        //string codigo = "", cliente = "";
                        //double totalCobrado = 0.0;
                        double efectivoCrCon = rec.RecMontoEfectivo;
                        double recMontoNCCrCon = rec.RecMontoNcr;
                        double recDescuentoCrCon = rec.RecMontoDescuento;
                        double recMontoChequeCrCon = rec.RecMontoCheque;
                        double recMontoChequeFuturistaCrCon = rec.RecMontoChequeF;
                        double recMontoTransferenciaCrCon = rec.RecMontoTransferencia;
                        double recMontoSobranteCrCon = rec.RecMontoSobrante;
                        double recMontoRetencionCrCon = rec.RecRetencion;

                        RecEfectivoTotalCrCon += efectivoCrCon;
                        RecMontoNCTotalCrCon += recMontoNCCrCon;
                        RecDescuentoTotalCrCon += recDescuentoCrCon;
                        RecMontoChequeTotalCrCon += recMontoChequeCrCon;
                        RecMontoChequeFuturistaTotalCrCon += recMontoChequeFuturistaCrCon;
                        RecMontoTransferenciaTotalCrCon += recMontoTransferenciaCrCon;
                        RecMontoSobranteTotalCrCon += recMontoSobranteCrCon;
                        RecMontoRetencionTotalCrCon += recMontoRetencionCrCon;
                    }
                }

                RecEfectivoTotalCrCon = RecEfectivoTotalCrCon - Efectivo;/*compras.getmontoTotalPuhsmoneyContado(CuaSecuencia);*/
                RecMontoChequeTotalCrCon = RecMontoChequeTotalCrCon - Cheque;/*compras.getmontoTotalPuhsmoneyCredito(CuaSecuencia);*/
                RecMontoTransferenciaTotalCrCon = RecMontoTransferenciaTotalCrCon - Transferencia;
                RecMontoNCTotalCrCon = RecMontoNCTotalCrCon - NotaCredito;
                RecMontoRetencionTotalCrCon = RecMontoRetencionTotalCrCon - Retencion;

                if (myVentas.getDocumentosRecSinFactura(Arguments.CurrentUser.RepCodigo, CuaSecuencia).Count > 0)
                {
                    printer.DrawLine();
                    string TotalCobros = CobrosTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("TotalCobros       :   ".PadRight(26) + "RD$" + TotalCobros);
                    printer.DrawText("");
                    printer.DrawText("");
                    printer.DrawText("EFECTIVO          :   ".PadRight(35) + ("RD$" + RecEfectivoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES           :   ".PadRight(35) + ("RD$" + RecMontoChequeTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("CHEQUES DIFERIDOS :   ".PadRight(35) + ("RD$" + RecMontoChequeFuturistaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("TRANSFERENCIAS    :   ".PadRight(35) + ("RD$" + RecMontoTransferenciaTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("RETENCION         :   ".PadRight(35) + ("RD$" + RecMontoRetencionTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("DESCUENTOS        :   ".PadRight(35) + ("RD$" + RecDescuentoTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("NOTAS DE CREDITO  :   ".PadRight(35) + ("RD$" + RecMontoNCTotalCrCon.ToString("N2")).PadLeft(10));
                    printer.DrawText("SOBRANTE          :   ".PadRight(35) + ("RD$" + RecMontoSobranteTotalCrCon.ToString("N2")).PadLeft(10)); ;

                    totalCobros = 0.00;
                    totalCobros = (RecEfectivoTotal + RecMontoChequeTotal + RecMontoChequeFuturistaTotal + RecMontoTransferenciaTotal);
                    totalCobros = totalCobros - totalCobrosCKD;

                    /*	printer.DrawText(Funciones.ReservarCaracteres("Total",18)+":"+convertDecimal(totalCobros));
                    printer.DrawText("");*/

                    printer.DrawLine();
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("DEPOSITO:");
                printer.Font = PrinterFont.BODY;

                printer.DrawText("");
                printer.DrawText("Ventas Contado    : ".PadRight(35) + ("RD$" + TotalVentasContado.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("Cobros Realizados : ".PadRight(35) + ("RD$" + totalCobros.ToString("N", new CultureInfo("en-US"))));

                printer.DrawText("");
                printer.DrawText("");

                printer.Bold = true;
                string TotalDeposito = ((TotalVentasContado + CobrosTotal) - CompraCreditoTotal).ToString("N", new CultureInfo("en-US"));
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total Deposito    : ".PadRight(35) + "RD$" + TotalDeposito.PadLeft(9));
                printer.Font = PrinterFont.BODY;

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("R E S U M E N");
                printer.DrawText("------------------------------------------------");
                printer.TextAlign = Justification.LEFT;

                var Resumen = myVentas.getResumen(Arguments.CurrentUser.RepCodigo, CuaSecuencia);

                if (Resumen != null)
                {
                    if (Resumen.mCantidadClientesAVisitar > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + Resumen.mCantidadClientesAVisitar);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS A VISITAR      : " + "0");
                    }

                    if (Resumen.mCantidadClientesVisitados > 0)
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + Resumen.mCantidadClientesVisitados);
                    }
                    else
                    {
                        printer.DrawText("No. DE NEGOCIOS VISITADOS      : " + "0");
                    }


                    if (Resumen.mCantidadVisitasPositivas > 0)
                    {
                        printer.DrawText("Visitas Positivas              : " + Resumen.mCantidadVisitasPositivas);
                    }
                    else
                    {
                        printer.DrawText("Visitas Positivas              : " + "0");
                    }

                    if (Resumen.mTotalTiempoRuta != "0")
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + Resumen.mTotalTiempoRuta);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO TOTAL DE LA RUTA        : " + "0");
                    }

                    if (Resumen.mTiempoPromVisitas != "0")
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + Resumen.mTiempoPromVisitas);
                    }
                    else
                    {
                        printer.DrawText("TIEMPO PROMEDIO DE LAS VISITAS : " + "0");
                    }

                    if (Resumen.mNumFacturasGeneradas > 0)
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + Resumen.mNumFacturasGeneradas);
                    }
                    else
                    {
                        printer.DrawText("No. DE FACTURAS GENERADAS      : " + "0");
                    }

                    string Efecti = Resumen.mEfectividad.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("Efectividad                    : " + Efecti + "%");
                    double VentasporCuadre = myVentas.getCantidadVentasByCuadre(CuaSecuencia);
                    Resumen.mPromVentasPorVisitas = (VentasporCuadre / Resumen.mNumFacturasGeneradas);
                    printer.DrawText("PROMEDIO DE VENTAS POR VISITAS : " + (Resumen.mPromVentasPorVisitas).ToString("N", new CultureInfo("en-US")));
                    printer.DrawText("------------------------------------------------");
                }

                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");

                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID + ") " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("__________________________");
                printer.DrawText("Liquidador");
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cuadres 11");
            printer.DrawText("");
            printer.Print();


        }
    }
}






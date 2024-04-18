using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class ReporteVentasconItbisFormat : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Ventas myVentas;
        public ReporteVentasconItbisFormat()
        {
            myVentas = new DS_Ventas();
        }
        public void Print(DateTime desde, DateTime hasta, PrinterManager printer)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCuadres())
            {
                default:
                case 5: //Miss Key
                    Formato5(desde,hasta);
                    break;
            }
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer)
        {
            throw new NotImplementedException();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            throw new NotImplementedException();
        }

        private void Formato5(DateTime desde, DateTime hasta)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }
            printer.PrintEmpresa();
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("Desde          : " + desde.ToString("dd-MM-yyyy"));
            printer.DrawText("Hasta          : " + hasta.ToString("dd-MM-yyyy"));
            printer.DrawText("Fecha impresion: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("Vendedor       : " + Arguments.CurrentUser.RepCodigo + " - " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("");
            printer.Bold = true;

            //Titulo del reporte
            printer.DrawText("RESUMEN VENTAS CON ITBIS");
                printer.DrawText("----------------------------------------------");
                printer.DrawText("NCF         Cliente                 Valor");
                printer.DrawText("----------------------------------------------");
            printer.Bold = false;

            //Variables de Total de Vetnass
            double TotalVentas = 0;

            //Variables totales de Ventas a Credito
            double TotalVentasCredito = 0;
            double TotalItbisVentasCredito = 0.0;
            double TotalCredito = 0.0;

            //Variables totales de Ventas a Contado
            double TotalContado = 0.0;
            int contado = DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
            double TotalVentasContado = 0;
            double TotalItbisVentasContado = 0.0;
            string TotalVentasConta = "0.0";

            //Verificación de Ventas a Contado
            if (myVentas.GetVentasaContadoconItbis(desde, hasta).Count > 0)
            {
                printer.DrawText("VENTAS A CONTADO:");
                printer.DrawText("");

                TotalVentasContado = 0;
                foreach (var prod in myVentas.GetVentasaContadoconItbis(desde, hasta))
                {
                    var desc = prod.VenNCF + " " + prod.CliNombre;
                    TotalVentasContado = TotalVentasContado + prod.VenTotal;
                    TotalItbisVentasContado += prod.VenTotalItbis;
                    if (desc.Length > 30)
                    {
                        desc = desc.Substring(0, 30);
                    }
                    string VentaTotal = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                    printer.DrawText(desc.PadRight(30) + ("RD$" + VentaTotal).PadLeft(16), 47);
                }
                printer.DrawText("");
                TotalContado = TotalVentasContado + TotalItbisVentasContado;
                TotalVentasConta = (TotalVentasContado + TotalItbisVentasContado).ToString("N", new CultureInfo("en-US"));
                printer.DrawText(("MONTO TOTAL VENTAS A CONTADO:").PadRight(31) + ("RD$" + TotalVentasConta).PadLeft(15));
                printer.DrawLine();
            }


            //Verificación de Ventas a Credito
            if (myVentas.GetVentasCreditoconItbis(desde, hasta).Count > 0)
                {
                    printer.DrawText("VENTAS A CREDITO:");
                    printer.DrawText("");

                  
                    TotalVentasCredito = 0;
                    foreach (var prod in myVentas.GetVentasCreditoconItbis(desde, hasta))
                    {
                        var desc = prod.VenNCF + " " + prod.CliNombre;
                        TotalVentasCredito = TotalVentasCredito + prod.VenTotal;
                        TotalItbisVentasCredito += prod.VenTotalItbis;
                        if (desc.Length > 30)
                        {
                            desc = desc.Substring(0, 30);
                        }
                        string TotalVenta = prod.VenTotal.ToString("N", new CultureInfo("en-US"));
                        printer.DrawText(desc.PadRight(30) + ("RD$" + TotalVenta).PadLeft(16), 47);
                    }
                    printer.DrawText("");
                    TotalCredito = TotalVentasCredito + TotalItbisVentasCredito;
                    string TotalVentasCred = (TotalVentasCredito + TotalItbisVentasCredito).ToString("N", new CultureInfo("en-US"));
                    printer.DrawText("MONTO TOTAL VENTAS A CREDITO: " + ("RD$" + (TotalVentasCred)).PadLeft(16));
                    printer.DrawLine();
                }

            //Total General
            TotalVentas = TotalContado + TotalCredito;
            string TotalVentas2 = TotalVentas.ToString("N", new CultureInfo("en-US"));
            printer.DrawText("TOTAL GENERAL:".PadRight(30) + ("RD$" + TotalVentas2).PadLeft(16));
            printer.DrawText("");
            printer.DrawText("");

            //Detalle de las ventas - Productos
            if (myVentas.getProductosVendidosVentasItbis(desde, hasta).Count > 0)
                {
                    printer.DrawText("DETALLE DE VENTAS");
                    printer.DrawText("----------------------------------------------");
                    printer.DrawText("Codigo-Descripcion                 CANT VEND.");
                    printer.DrawText("----------------------------------------------");

                    foreach (var prod in myVentas.getProductosVendidosVentasItbis(desde, hasta))
                    {
                        var Producto = prod.ProCodigo + "-" + prod.ProDescripcion;
                        if (Producto.Length > 30)
                        {
                            Producto = Producto.Substring(0, 30);
                        }
                        var cantidad = prod.VenCantidad.ToString();
                        if (prod.VenCantidadDetalle > 0)
                        {
                            cantidad = cantidad + "/" + prod.VenCantidadDetalle;
                        }
                        printer.DrawText(Producto.PadRight(30) + cantidad.PadLeft(16), 47);
                    }
                }

            //Información general de la versión
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Reporte Ventas con Itbis");
            printer.DrawText("");
            printer.Print();

        }
    }
    }



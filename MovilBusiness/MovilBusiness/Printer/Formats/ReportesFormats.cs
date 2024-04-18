using MovilBusiness.Configuration;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Views.Components.TemplateSelector;
using System;
using System.Collections.Generic;
using System.Text;
using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer;
using MovilBusiness.Utils;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class ReportesFormats 
    {
        string DecriccionReporte;
        public void Print(List<RowLinker> data, PrinterManager printer, DateTime desde, DateTime hasta, int Tipo = 0)
        {           
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }
            
            printer.PrintEmpresa(putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            bool desdeyhasta = true;

            if (data[0] is SubTitle w)
            {
                desdeyhasta = w.Description.ToUpper() == "RESUMEN DE INVENTARIO" ? false : true;
            }

            if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader() && desdeyhasta)
            {
                printer.DrawText("Desde: " + desde.ToString("dd_MM-yyyy"));
                printer.DrawText("Hasta: " + hasta.ToString("dd-MM-yyyy"));
                //printer.DrawText("Fecha impresion: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"));
            }
                printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo + " - " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("");

            int Counting = 0;
            bool IsFromRecibos = false; 

            foreach (var item in data){
                if (item.Bold || item.IsHeader)
                {
                        printer.Bold = true;
                }

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }
                
                if (item is ReportesNameQuantity r)
                {
                    var name = r.Name;

                    if (name.Length > 28)
                    {
                        name = name.Substring(0, 28);
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}

                    if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        if (Counting == 0)
                        {
                            printer.DrawText("Desde: " + desde.ToString("dd_MM-yyyy") + " - Hasta: " + hasta.ToString("dd-MM-yyyy"));
                            printer.DrawText("");
                            printer.DrawLine();
                            printer.DrawText(name.PadRight(29) + "  " + r.Amount.PadRight(7)+"  "+ r.Quantity.PadRight(10));
                           // printer.DrawText("                            " + r.Amount.PadRight(9));
                            printer.DrawLine();
                            Counting++;
                        }
                        else
                        {
                            printer.DrawText(name.PadRight(39) + "  " + r.Quantity.PadRight(10));
                            printer.DrawText("");
                            printer.DrawText("                     ________ " + r.AmountFormated.PadRight(7));
                            printer.DrawText("");

                        }
                    }
                    else {
                        printer.DrawText(name.PadRight(30) + "  " + r.Quantity.PadRight(10) + r.Amount.PadRight(5));
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}
                }
                else if (item is FacturasVencidas f)
                {
                    printer.DrawText(f.CliNombre);
                    printer.DrawText(f.Factura.PadRight(21) + f.Fecha.PadRight(15) + f.Balance.ToString("N2").PadRight(10));
                }
                else if (item is Totales t)
                {
                    if (DecriccionReporte.Contains("PUSHMONEY")) {
                        printer.TextAlign = Justification.LEFT;
                        printer.Bold = true;
                        printer.DrawText("Total Cajetillas: " + ("   ________ "+"$"+t.Total.ToString("N2")).PadLeft(9));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                    else
                    {
                        printer.TextAlign = Justification.RIGHT;
                        printer.Bold = true;
                        printer.DrawText("Total: " + t.Total.ToString("N2").PadLeft(37));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                }
                else if (item is GastosReportes g)
                {
                    var proveedor = g.GasNombreProveedor;
                    var tipo = g.TipoGasto;

                    if (proveedor.Length > 26)
                    {
                        proveedor = proveedor.Substring(0, 26);
                    }
                    if (tipo.Length > 21)
                    {
                        tipo = tipo.Substring(0, 21);
                    }
                    printer.DrawText("");
                    printer.DrawText("Fecha           : " + g.GasFecha.PadRight(16));
                    printer.DrawText("Proveedor       : " +proveedor.PadRight(26));
                    printer.DrawText("RNC             : " + g.GasRNC.PadRight(18));
                    printer.DrawText("NCF             : " + g.GasNCF.PadRight(26));
                    printer.DrawText("No. Factura     : " + g.GasNoDocumento.PadRight(26));
                    printer.DrawText("Fecha Factura   : " + Functions.FormatDate(g.GasFechaDocumento, "dd-MM-yyyy"));
                    printer.DrawText("Tipo Gasto      : " + tipo.PadRight(21));
                    printer.DrawText("Monto Suj. Itbis: " + g.GasBaseImponible.PadLeft(7));
                    printer.DrawText("Itbis           : " + g.GasItebis.PadLeft(7));
                    printer.DrawText("Propina         : " + g.GasPropina.PadLeft(7));
                    printer.DrawText("Monto Total     : " + g.GasMontoTotal.PadLeft(7));

                    //printer.DrawText(proveedor.PadRight(26) + " RNC: " + g.GasRNC.PadRight(18));
                    //printer.DrawText("Fecha: " + g.GasFecha.PadRight(16) + "NCF: " + g.GasNCF.PadRight(22));
                    //printer.DrawText("Tipo: " + tipo.PadRight(21) + "Factura: " + g.GasNoDocumento.PadRight(15));
                    //printer.DrawText("Fecha Factura: " + Functions.FormatDate(g.GasFechaDocumento, "dd-MM-yyyy").PadRight(16) + "Monto Suj. Itbis: " + g.GasFechaDocumento.PadRight(22));
                    //printer.DrawText("Itbis: " + g.GasItebis.PadRight(16) + "Propina: " + g.GasPropina.PadRight(22));
                    //printer.DrawText("Monto Total: " + g.GasMontoTotal);
                    printer.DrawLine();
                }
                else if (item is GastosTotales gt)
                {
                    printer.DrawText("Total Itbis  : " + gt.TotalItbis.PadLeft(7));
                    printer.DrawText("Total Propina: " + gt.TotalPropina.PadLeft(7));
                    printer.DrawText("Total General: " + gt.Total.PadLeft(7));
                    //printer.DrawText(gt.TotalItbis.PadRight(14) + gt.TotalPropina.PadRight(18) + gt.Total.PadRight(12));
                }
                else if (item is Inventarios i)
                {
                    if(IsFromRecibos)
                    {
                        printer.DrawLine();
                    }
                    var desc = i.ProDescripcion;

                    if (desc.Length > 32)
                    {
                        desc = desc.Substring(0, 31);
                    }

                    string unm = "";

                    if (i.invCantidad >0 || i.InvCantidadDetalle > 0) {

                        if (!string.IsNullOrWhiteSpace(i.UnmCodigo))
                        {
                            unm = i.UnmCodigo.Substring(0, 3);
                            printer.DrawText(desc.ReplaceSymbol().PadRight(32) + i.InvCantidadLabel.PadLeft(8) + unm.PadLeft(5) );
                        }
                        else
                        {
                            printer.DrawText(desc.ReplaceSymbol().PadRight(26) + i.InvCantidadLabel.PadLeft(19));
                        }
                    }
                }
                else if (item is SubTitle s)
                {
                    string cadena = s.Description.ToUpper().Replace(" ", "");
                    int length = cadena.Length;
                    DecriccionReporte = cadena;
                   // DecriccionReporte = cadena.Substring(9, length-1);
                    printer.TextAlign = Justification.CENTER;
                    printer.Bold = true;
                    printer.DrawText(s.Description.ToUpper());
                    printer.Bold = false;
                    printer.TextAlign = Justification.LEFT;
                }
                else if (item is RecibosMontoResumen re)
                {
                    if (string.IsNullOrEmpty(re.DepFecha) || (!string.IsNullOrEmpty(re.DepFecha) &&
                        re.DepFecha.Contains("Fecha")))
                    {
                        bool isvalid = double.TryParse(re.Monto, out double monto);

                        printer.DrawText(re.CliNombre);
                        printer.DrawText(re.RecSecuencia.PadRight(13) +
                           re.CliCodigo.PadRight(11) + re.RecDescuento.PadRight(9) + (isvalid ? monto.ToString("N2") : re.Monto ));

                        if (re.CliNombre.Contains("CLIEN"))
                        {
                            printer.DrawLine();
                        }
                    }
                    else
                    {
                        printer.DrawText(re.CliNombre);
                        printer.DrawText(re.RecSecuencia.PadRight(14) + re.RecDescuento.PadRight(14) 
                            + ("$"+re.RecMonto).PadRight(10) + re.DepFecha.PadRight(10));

                        if (re.CliNombre.Contains("Cliente"))
                        {
                            printer.DrawLine();
                        }
                    }
                    IsFromRecibos = true;
                }
                   
                else
                {
                    printer.DrawText("");
                }

                printer.Bold = false;

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                //if(item is Inventarios inv)
                //{ 
                //    invs.Add(inv);
                //}

            }

            //foreach(var recview in invs)
            //{
            //    printer.DrawText($"{recview.ProDescripcion}      ".PadRight(35) + recview.invCantidad.ToString("N2").PadLeft(10));
            //}

            printer.DrawText("");
            printer.DrawLine();

            if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
            {
                printer.DrawText("Fecha impresion: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                printer.DrawText("");
            }
                printer.DrawText("");
                printer.DrawText("");
            if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("("+Arguments.CurrentUser.RutID.ToString()+") " + Arguments.CurrentUser.RepNombre);
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + "(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato Reportes 1");

            printer.Print();
        }

        public void PrintResumenRecibos(List<RowLinker> data, PrinterManager printer, DateTime desde, DateTime hasta, int Tipo = 0)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoResumenDeRecibos())
            {
                default:
                    Print1(data, printer, desde, hasta,Tipo);
                    break;
                case 2:
                    Print2(data, printer, desde, hasta, Tipo);
                    break;
                case 3:
                    Print3(data, printer, desde, hasta, Tipo);
                    break;
            }
        }
        // Formato con CliCodigo
        public void Print1(List<RowLinker> data, PrinterManager printer, DateTime desde, DateTime hasta, int Tipo = 0)
        {           
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            double Efectivo = 0
            , Transferencia = 0
            , Recibos = 0
            , Cheque = 0
            , ChequeF = 0
            , TaCredito = 0
            , NotasCr = 0
            , Desc = 0
            , Monto = 0;

            printer.PrintEmpresa(putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            int Counting = 0;
            int CountingAnulados = 0;
            double CountingMontoTotal = 0.00;
            //bool IsFromRecibos = false; 

            foreach (var item in data){
                if (item.Bold || item.IsHeader)
                {
                        printer.Bold = true;
                }

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }
                
                if (item is ReportesNameQuantity r)
                {
                    var name = r.Name;

                    if (name.Length > 28)
                    {
                        name = name.Substring(0, 28);
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}

                    if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        if (Counting == 0)
                        {
                            printer.DrawText("Desde: " + desde.ToString("dd_MM-yyyy") + " - Hasta: " + hasta.ToString("dd-MM-yyyy"));
                            printer.DrawText("");
                            printer.DrawLine();
                            printer.DrawText(name.PadRight(29) + "  " + r.Amount.PadRight(7)+"  "+ r.Quantity.PadRight(10));
                           // printer.DrawText("                            " + r.Amount.PadRight(9));
                            printer.DrawLine();
                            Counting++;
                        }
                        else
                        {
                            printer.DrawText(name.PadRight(39) + "  " + r.Quantity.PadRight(10));
                            printer.DrawText("");
                            printer.DrawText("                     ________ " + r.AmountFormated.PadRight(7));
                            printer.DrawText("");

                        }
                    }
                    else {
                        printer.DrawText(name.PadRight(28) + "  " + r.Quantity.PadRight(9) + r.Amount.PadRight(10));
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}
                }
                else if (item is FacturasVencidas f)
                {
                    printer.DrawText(f.CliNombre);
                    printer.DrawText(f.Factura.PadRight(21) + f.Fecha.PadRight(15) + f.Balance.ToString("N2").PadRight(10));
                }
                else if (item is Totales t)
                {
                    if (DecriccionReporte.Contains("PUSHMONEY")) {
                        printer.TextAlign = Justification.LEFT;
                        printer.Bold = true;
                        printer.DrawText("Total Cajetillas: " + ("   ________ "+"$"+t.Total.ToString("N2")).PadLeft(7));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                    else
                    {
                        printer.TextAlign = Justification.RIGHT;
                        printer.Bold = true;
                        printer.DrawText("Total: " + t.Total.ToString("N2").PadLeft(37));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                }
                else if (item is GastosReportes g)
                {
                    var proveedor = g.GasNombreProveedor;
                    var tipo = g.TipoGasto;

                    if (proveedor.Length > 26)
                    {
                        proveedor = proveedor.Substring(0, 26);
                    }
                    if (tipo.Length > 21)
                    {
                        tipo = tipo.Substring(0, 21);
                    }

                    printer.DrawText(proveedor.PadRight(26) + "RNC: " + g.GasRNC.PadRight(18));
                    printer.DrawText("Fecha: " + g.GasFecha.PadRight(16) + "NCF: " + g.GasNCF.PadRight(22));
                    printer.DrawText("Tipo: " + tipo.PadRight(21) + "Factura: " + g.GasNoDocumento.PadRight(15));
                    printer.DrawText("Monto: " + g.GasMontoTotal);
                    printer.DrawLine();
                }
                else if (item is GastosTotales gt)
                {
                    printer.DrawText(gt.TotalItbis.PadRight(14) + gt.TotalPropina.PadRight(18) + gt.Total.PadRight(12));
                }
                else if (item is Inventarios i)
                {
                    var desc = i.ProDescripcion;

                    if (i.invCantidad >0 || i.InvCantidadDetalle > 0) {

                        switch (i.ForID)
                        {
                            case 4:
                                Transferencia += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            case 6:
                                TaCredito += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            //case 2:
                            //    //Cheque += i.invCantidad + i.InvCantidadDetalle;
                            //    break;
                            case 1:
                                Efectivo += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            default:
                                if(desc.Contains("Cantidad"))
                                {
                                    Recibos += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if(desc.Contains("Desc"))
                                {
                                    Desc += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if(desc.Contains("Monto"))
                                {
                                    Monto += i.invCantidad + i.InvCantidadDetalle;
                                }
                                break;
                        }
                    }
                }
                else if (item is SubTitle s)
                {
                    string cadena = s.Description.ToUpper().Replace(" ", "");
                    int length = cadena.Length;
                    DecriccionReporte = cadena;
                   // DecriccionReporte = cadena.Substring(9, length-1);
                    printer.DrawText(s.Description.ToUpper());


                    if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        printer.DrawText("Desde: " + desde.ToString("dd/MM/yyyy"));
                        printer.DrawText("Hasta: " + hasta.ToString("dd/MM/yyyy"));
                        printer.DrawText("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                    }
                    printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo + " - " + Arguments.CurrentUser.RepNombre);
                    printer.DrawText("");
                }
                else if (item is RecibosMontoResumen re)
                {

                    string value = "";

                    if (string.IsNullOrEmpty(re.DepFecha) || (!string.IsNullOrEmpty(re.DepFecha) &&
                        re.DepFecha.Contains("Fecha")))
                    {
                        bool isvalid = double.TryParse(re.Monto, out double monto);

                        bool isfisrttime = re.CliNombre.Contains("CLIEN");

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }else
                        {

                            Recibos recFormaPago = new DS_Recibos().GetRecFormPago(int.Parse(re.RecSecuencia));

                            Dictionary<string, int> keyValuePairs = new Dictionary<string, int>
                            {
                                { "FT", (int)recFormaPago.RecMontoEfectivo},
                                { "CQ", (int)recFormaPago.RecMontoCheque },
                                { "CQF", (int)recFormaPago.RecMontoChequeF },
                                { "TC", (int)recFormaPago.RecMontoTransferencia },
                            };
                            
                            int ValueMax = keyValuePairs.Max(m => m.Value);
                            
                            value = keyValuePairs.FirstOrDefault(c => c.Value > 0 && c.Value == ValueMax).Key;

                            ChequeF += recFormaPago.RecMontoChequeF;
                            Cheque += recFormaPago.RecMontoCheque;
                        }

                        
                        if (isfisrttime)
                        {
                            printer.DrawText(re.RecSecuencia.PadRight(10) + re.CliCodigo.PadRight(13) + re.RecDescuento + (isvalid ? monto.ToString("N2").PadLeft(17) : re.Monto).PadLeft(17));
                        }
                        else
                        {
                            printer.DrawText(re.RecSecuencia.PadRight(10) + re.CliCodigo + Double.Parse(re.RecDescuento).ToString("N2").PadLeft(12) + (isvalid ? monto.ToString("N2").PadLeft(15) : re.Monto).PadLeft(15) + " " + value);
                        }

                        //printer.DrawText(re.RecSecuencia.PadRight(13) +
                        //   re.CliCodigo.PadRight(11) + re.RecDescuento.PadRight(9) + (isvalid ? monto.ToString("N2") : re.Monto).PadLeft(11) + (!isfisrttime ? " " + value : ""));

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }else
                        {
                            CountingAnulados++;
                            CountingMontoTotal += double.Parse(re.Monto.Replace("$",""));
                        }
                    }
                    else
                    {

                        printer.DrawText(re.CliNombre);
                        printer.DrawText(re.RecSecuencia.PadRight(14) + re.RecDescuento.PadLeft(14) 
                            + ("$"+re.RecMonto).PadLeft(8) + re.DepFecha.PadRight(10));

                        if (re.CliNombre.Contains("Cliente"))
                        {
                            printer.DrawLine();
                        }else
                        {
                            NotasCr += re.RecMontoNcr;
                        }
                    }
                    //IsFromRecibos = true;
                }

                printer.Bold = false;

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                //if(item is Inventarios inv)
                //{
                //    invs.Add(inv);
                //}

            }


            //foreach(var recview in invs)
            //{
            //    printer.DrawText($"{recview.ProDescripcion}      ".PadRight(35) + recview.invCantidad.ToString("N2").PadLeft(10));
            //}
            printer.DrawLine();
            printer.DrawText("LEYENDA");
            printer.DrawText("* Recibos anulados");
            printer.DrawText("Cantidad: " + CountingAnulados.ToString().PadLeft(12) + ("$" + CountingMontoTotal.ToString("N2")).PadLeft(23));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Cantidad Recibos: " + Recibos);
            printer.DrawText("Total Efectivo  :" + ("$" + Efectivo.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque    :" + ("$" + Cheque.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque F  :" + ("$" + ChequeF.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Transf.   :" + ("$" + Transferencia.ToString("N2")).PadLeft(13));
            printer.DrawText("Total T. Credito:" + ("$" + TaCredito.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Notas Cr. :" + ("$" + NotasCr.ToString("N2")).PadLeft(13));
            printer.DrawText("Desc Total      :" + ("$" + Desc.ToString("N2")).PadLeft(13));
            printer.DrawText("Monto Total     :" + ("$" + Monto.ToString("N2")).PadLeft(13));

            printer.DrawText("");
                printer.DrawText("");
            if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("("+Arguments.CurrentUser.RutID.ToString()+") " + Arguments.CurrentUser.RepNombre);
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawLine();
            printer.DrawText("Entregado por ");

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + "(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato Resumen De Recibos 1");

            printer.Print();
        }
        // Formato con CliNombre                                       Acromax
        public void Print2(List<RowLinker> data, PrinterManager printer, DateTime desde, DateTime hasta, int Tipo = 0)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            double Efectivo = 0
            , Transferencia = 0
            , Recibos = 0
            , Cheque = 0
            , ChequeF = 0
            , TaCredito = 0
            , NotasCr = 0
            , Desc = 0
            , Monto = 0;

            printer.PrintEmpresa(putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            int Counting = 0;
            int CountingAnulados = 0;
            double CountingMontoTotal = 0.00;
            //bool IsFromRecibos = false; 

            foreach (var item in data)
            {
                if (item.Bold || item.IsHeader)
                {
                    printer.Bold = true;
                }

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                if (item is ReportesNameQuantity r)
                {
                    var name = r.Name;

                    if (name.Length > 28)
                    {
                        name = name.Substring(0, 28);
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}

                    if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        if (Counting == 0)
                        {
                            printer.DrawText("Desde: " + desde.ToString("dd_MM-yyyy") + " - Hasta: " + hasta.ToString("dd-MM-yyyy"));
                            printer.DrawText("");
                            printer.DrawLine();
                            printer.DrawText(name.PadRight(29) + "  " + r.Amount.PadRight(7) + "  " + r.Quantity.PadRight(10));
                            // printer.DrawText("                            " + r.Amount.PadRight(9));
                            printer.DrawLine();
                            Counting++;
                        }
                        else
                        {
                            printer.DrawText(name.PadRight(39) + "  " + r.Quantity.PadRight(10));
                            printer.DrawText("");
                            printer.DrawText("                     ________ " + r.AmountFormated.PadRight(7));
                            printer.DrawText("");

                        }
                    }
                    else
                    {
                        printer.DrawText(name.PadRight(28) + "  " + r.Quantity.PadRight(9) + r.Amount.PadRight(10));
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}
                }
                else if (item is FacturasVencidas f)
                {
                    printer.DrawText(f.CliNombre);
                    printer.DrawText(f.Factura.PadRight(21) + f.Fecha.PadRight(15) + f.Balance.ToString("N2").PadRight(10));
                }
                else if (item is Totales t)
                {
                    if (DecriccionReporte.Contains("PUSHMONEY"))
                    {
                        printer.TextAlign = Justification.LEFT;
                        printer.Bold = true;
                        printer.DrawText("Total Cajetillas: " + ("   ________ " + "$" + t.Total.ToString("N2")).PadLeft(7));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                    else
                    {
                        printer.TextAlign = Justification.RIGHT;
                        printer.Bold = true;
                        printer.DrawText("Total: " + t.Total.ToString("N2").PadLeft(37));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                }
                else if (item is GastosReportes g)
                {
                    var proveedor = g.GasNombreProveedor;
                    var tipo = g.TipoGasto;

                    if (proveedor.Length > 26)
                    {
                        proveedor = proveedor.Substring(0, 26);
                    }
                    if (tipo.Length > 21)
                    {
                        tipo = tipo.Substring(0, 21);
                    }

                    printer.DrawText(proveedor.PadRight(26) + "RNC: " + g.GasRNC.PadRight(18));
                    printer.DrawText("Fecha: " + g.GasFecha.PadRight(16) + "NCF: " + g.GasNCF.PadRight(22));
                    printer.DrawText("Tipo: " + tipo.PadRight(21) + "Factura: " + g.GasNoDocumento.PadRight(15));
                    printer.DrawText("Monto: " + g.GasMontoTotal);
                    printer.DrawLine();
                }
                else if (item is GastosTotales gt)
                {
                    printer.DrawText(gt.TotalItbis.PadRight(14) + gt.TotalPropina.PadRight(18) + gt.Total.PadRight(12));
                }
                else if (item is Inventarios i)
                {
                    var desc = i.ProDescripcion;

                    if (i.invCantidad > 0 || i.InvCantidadDetalle > 0)
                    {

                        switch (i.ForID)
                        {
                            case 4:
                                Transferencia += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            case 6:
                                TaCredito += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            case 2:
                                Cheque += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            case 1:
                                Efectivo += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            default:
                                if (desc.Contains("Cantidad"))
                                {
                                    Recibos += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if (desc.Contains("Desc"))
                                {
                                    Desc += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if (desc.Contains("Monto"))
                                {
                                    Monto += i.invCantidad + i.InvCantidadDetalle;
                                }
                                break;
                        }
                    }
                }
                else if (item is SubTitle s)
                {
                    string cadena = s.Description.ToUpper().Replace(" ", "");
                    int length = cadena.Length;
                    DecriccionReporte = cadena;
                    // DecriccionReporte = cadena.Substring(9, length-1);
                    printer.DrawText(s.Description.ToUpper());


                    if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        printer.DrawText("Desde: " + desde.ToString("dd/MM/yyyy"));
                        printer.DrawText("Hasta: " + hasta.ToString("dd/MM/yyyy"));
                        printer.DrawText("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                    }
                    printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo + " - " + Arguments.CurrentUser.RepNombre);
                    printer.DrawText("");
                }
                else if (item is RecibosMontoResumen re)
                {
                    if (string.IsNullOrEmpty(re.DepFecha) || (!string.IsNullOrEmpty(re.DepFecha) &&
                        re.DepFecha.Contains("Fecha")))
                    {
                        bool isvalid = double.TryParse(re.Monto, out double monto);

                        bool isfisrttime = re.CliNombre.Contains("CLIEN");

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }

                        printer.DrawText(re.CliNombre);
                        if (isfisrttime)
                        {
                            printer.DrawText(re.RecSecuencia.PadRight(20) + re.RecDescuento + (isvalid ? monto.ToString("N2").PadLeft(20) : re.Monto).PadLeft(20));
                        }
                        else
                        {
                            printer.DrawText(re.RecSecuencia + Double.Parse(re.RecDescuento).ToString("N2").PadLeft(22) + (isvalid ? monto.ToString("N2").PadLeft(20) : re.Monto).PadLeft(20));
                        }

                        ChequeF += monto;

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }
                        else
                        {
                            CountingAnulados++;
                            CountingMontoTotal += double.Parse(re.Monto.Replace("$", ""));
                        }

                    }
                    else
                    {
                        printer.DrawText(re.CliNombre);
                        printer.DrawText(re.RecSecuencia.PadRight(14) + re.RecDescuento.PadRight(14)
                            + ("$" + re.RecMonto).PadRight(10) + re.DepFecha.PadRight(10));

                        if (re.CliNombre.Contains("Cliente"))
                        {
                            printer.DrawLine();
                        }
                        else
                        {
                            NotasCr += re.RecMontoNcr;
                        }
                    }
                    //IsFromRecibos = true;
                }

                printer.Bold = false;

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                //if(item is Inventarios inv)
                //{
                //    invs.Add(inv);
                //}

            }

            //foreach(var recview in invs)
            //{
            //    printer.DrawText($"{recview.ProDescripcion}      ".PadRight(35) + recview.invCantidad.ToString("N2").PadLeft(10));
            //}
            printer.DrawLine();
            printer.DrawText("LEYENDA");
            printer.DrawText("* Recibos anulados");
            printer.DrawText("Cantidad: " + CountingAnulados.ToString().PadLeft(12) + ("$" + CountingMontoTotal.ToString("N2")).PadLeft(25));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Cantidad Recibos: " + Recibos);
            printer.DrawText("Total Efectivo  :" + ("$" + Efectivo.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque    :" + ("$" + Cheque.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque F  :" + ("$" + ChequeF.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Transf.   :" + ("$" + Transferencia.ToString("N2")).PadLeft(13));
            printer.DrawText("Total T. Credito:" + ("$" + TaCredito.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Notas Cr. :" + ("$" + NotasCr.ToString("N2")).PadLeft(13));
            printer.DrawText("Desc Total      :" + ("$" + Desc.ToString("N2")).PadLeft(13));
            printer.DrawText("Monto Total     :" + ("$" + Monto.ToString("N2")).PadLeft(13));

            printer.DrawText("");
            printer.DrawText("");
            if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawLine();
            printer.DrawText("Entregado por ");

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + "(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato Resumen De Recibos 2");

            printer.Print();
        }

        public void Print3(List<RowLinker> data, PrinterManager printer, DateTime desde, DateTime hasta, int Tipo = 0)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            double Efectivo = 0
            , Transferencia = 0
            , Recibos = 0
            , Cheque = 0
            , ChequeF = 0
            , TaCredito = 0
            , NotasCr = 0
            , Desc = 0
            , Monto = 0;

            printer.PrintEmpresa(putfecha: DS_RepresentantesParametros.GetInstance().Getdateprintheader());
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            int Counting = 0;
            int CountingAnulados = 0;
            double CountingMontoTotal = 0.00;
            //bool IsFromRecibos = false; 

            foreach (var item in data)
            {
                if (item.Bold || item.IsHeader)
                {
                    printer.Bold = true;
                }

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                if (item is ReportesNameQuantity r)
                {
                    var name = r.Name;

                    if (name.Length > 28)
                    {
                        name = name.Substring(0, 28);
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}

                    if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        if (Counting == 0)
                        {
                            printer.DrawText("Desde: " + desde.ToString("dd_MM-yyyy") + " - Hasta: " + hasta.ToString("dd-MM-yyyy"));
                            printer.DrawText("");
                            printer.DrawLine();
                            printer.DrawText(name.PadRight(29) + "  " + r.Amount.PadRight(7) + "  " + r.Quantity.PadRight(10));
                            // printer.DrawText("                            " + r.Amount.PadRight(9));
                            printer.DrawLine();
                            Counting++;
                        }
                        else
                        {
                            printer.DrawText(name.PadRight(39) + "  " + r.Quantity.PadRight(10));
                            printer.DrawText("");
                            printer.DrawText("                     ________ " + r.AmountFormated.PadRight(7));
                            printer.DrawText("");

                        }
                    }
                    else
                    {
                        printer.DrawText(name.PadRight(28) + "  " + r.Quantity.PadRight(9) + r.Amount.PadRight(10));
                    }

                    //if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    //{
                    //    //printer.DrawLine();
                    //}
                }
                else if (item is FacturasVencidas f)
                {
                    printer.DrawText(f.CliNombre);
                    printer.DrawText(f.Factura.PadRight(21) + f.Fecha.PadRight(15) + f.Balance.ToString("N2").PadRight(10));
                }
                else if (item is Totales t)
                {
                    if (DecriccionReporte.Contains("PUSHMONEY"))
                    {
                        printer.TextAlign = Justification.LEFT;
                        printer.Bold = true;
                        printer.DrawText("Total Cajetillas: " + ("   ________ " + "$" + t.Total.ToString("N2")).PadLeft(7));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                    else
                    {
                        printer.TextAlign = Justification.RIGHT;
                        printer.Bold = true;
                        printer.DrawText("Total: " + t.Total.ToString("N2").PadLeft(37));
                        printer.Bold = false;
                        printer.TextAlign = Justification.LEFT;
                    }
                }
                else if (item is GastosReportes g)
                {
                    var proveedor = g.GasNombreProveedor;
                    var tipo = g.TipoGasto;

                    if (proveedor.Length > 26)
                    {
                        proveedor = proveedor.Substring(0, 26);
                    }
                    if (tipo.Length > 21)
                    {
                        tipo = tipo.Substring(0, 21);
                    }

                    printer.DrawText(proveedor.PadRight(26) + "RNC: " + g.GasRNC.PadRight(18));
                    printer.DrawText("Fecha: " + g.GasFecha.PadRight(16) + "NCF: " + g.GasNCF.PadRight(22));
                    printer.DrawText("Tipo: " + tipo.PadRight(21) + "Factura: " + g.GasNoDocumento.PadRight(15));
                    printer.DrawText("Monto: " + g.GasMontoTotal);
                    printer.DrawLine();
                }
                else if (item is GastosTotales gt)
                {
                    printer.DrawText(gt.TotalItbis.PadRight(14) + gt.TotalPropina.PadRight(18) + gt.Total.PadRight(12));
                }
                else if (item is Inventarios i)
                {
                    var desc = i.ProDescripcion;

                    if (i.invCantidad > 0 || i.InvCantidadDetalle > 0)
                    {

                        switch (i.ForID)
                        {
                            case 4:
                                Transferencia += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            case 6:
                                TaCredito += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            //case 2:
                            //    //Cheque += i.invCantidad + i.InvCantidadDetalle;
                            //    break;
                            case 1:
                                Efectivo += i.invCantidad + i.InvCantidadDetalle;
                                break;
                            default:
                                if (desc.Contains("Cantidad"))
                                {
                                    Recibos += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if (desc.Contains("Desc"))
                                {
                                    Desc += i.invCantidad + i.InvCantidadDetalle;
                                }
                                else if (desc.Contains("Monto"))
                                {
                                    Monto += i.invCantidad + i.InvCantidadDetalle;
                                }
                                break;
                        }
                    }
                }
                else if (item is SubTitle s)
                {
                    string cadena = s.Description.ToUpper().Replace(" ", "");
                    int length = cadena.Length;
                    DecriccionReporte = cadena;
                    // DecriccionReporte = cadena.Substring(9, length-1);
                    printer.DrawText(s.Description.ToUpper());


                    if (!DS_RepresentantesParametros.GetInstance().Getdateprintheader())
                    {
                        printer.DrawText("Desde: " + desde.ToString("dd/MM/yyyy"));
                        printer.DrawText("Hasta: " + hasta.ToString("dd/MM/yyyy"));
                        printer.DrawText("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                    }
                    printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo + " - " + Arguments.CurrentUser.RepNombre);
                    printer.DrawText("");
                }
                else if (item is RecibosMontoResumen re)
                {

                    string value = "";

                    if (string.IsNullOrEmpty(re.DepFecha) || (!string.IsNullOrEmpty(re.DepFecha) &&
                        re.DepFecha.Contains("Fecha")))
                    {
                        bool isvalid = double.TryParse(re.Monto, out double monto);

                        bool isfisrttime = re.CliNombre.Contains("CLIEN");

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }
                        else
                        {

                            Recibos recFormaPago = new DS_Recibos().GetRecFormPago(int.Parse(re.RecSecuencia));

                            Dictionary<string, int> keyValuePairs = new Dictionary<string, int>
                            {
                                { "FT", (int)recFormaPago.RecMontoEfectivo},
                                { "CQ", (int)recFormaPago.RecMontoCheque },
                                { "CQF", (int)recFormaPago.RecMontoChequeF },
                                { "TC", (int)recFormaPago.RecMontoTransferencia },
                            };

                            int ValueMax = keyValuePairs.Max(m => m.Value);

                            value = keyValuePairs.FirstOrDefault(c => c.Value > 0 && c.Value == ValueMax).Key;

                            ChequeF += recFormaPago.RecMontoChequeF;
                            Cheque += recFormaPago.RecMontoCheque;
                        }

                        if (isfisrttime)
                        {
                            printer.DrawText(re.RecSecuencia.PadRight(10) + re.CliCodigo.PadRight(13) + re.RecDescuento + (isvalid ? monto.ToString("N2").PadLeft(17) : re.Monto).PadLeft(17));
                        }
                        else
                        {
                            printer.DrawText(re.RecSecuencia.PadRight(10) + re.CliCodigo + Double.Parse(re.RecDescuento).ToString("N2").PadLeft(12) + (isvalid ? monto.ToString("N2").PadLeft(15) : re.Monto).PadLeft(15) + " " + value);
                        }

                        //printer.DrawText(re.RecSecuencia.PadRight(14) +
                          // re.CliCodigo.PadRight(14) + re.RecDescuento.PadRight(4) + (isvalid ? monto.ToString("N2") : re.Monto).PadLeft(6) + (!isfisrttime ? " " + value : ""));

                        if (isfisrttime)
                        {
                            printer.DrawLine();
                        }
                        else
                        {
                            CountingAnulados++;
                            CountingMontoTotal += double.Parse(re.Monto.Replace("$", ""));
                        }
                    }
                    else
                    {

                        printer.DrawText(re.CliNombre);
                        printer.DrawText(re.RecSecuencia.PadRight(14) + re.RecDescuento.PadRight(14)
                            + ("$" + re.RecMonto).PadRight(8) + re.DepFecha.PadRight(10));

                        if (re.CliNombre.Contains("Cliente"))
                        {
                            printer.DrawLine();
                        }
                        else
                        {
                            NotasCr += re.RecMontoNcr;
                        }
                    }
                    //IsFromRecibos = true;
                }

                printer.Bold = false;

                if (item.IsHeader)
                {
                    printer.DrawLine();
                }

                //if(item is Inventarios inv)
                //{
                //    invs.Add(inv);
                //}

            }


            //foreach(var recview in invs)
            //{
            //    printer.DrawText($"{recview.ProDescripcion}      ".PadRight(35) + recview.invCantidad.ToString("N2").PadLeft(10));
            //}
            //printer.DrawLine();
            //printer.DrawText("LEYENDA");
            //printer.DrawText("* Recibos anulados");
            //printer.DrawText("Cantidad: " + CountingAnulados.ToString().PadLeft(12) + ("$" + CountingMontoTotal.ToString("N2")).PadLeft(23));
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Cantidad Recibos: " + Recibos);
            printer.DrawText("Total Efectivo  :" + ("$" + Efectivo.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque    :" + ("$" + Cheque.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Cheque F  :" + ("$" + ChequeF.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Transf.   :" + ("$" + Transferencia.ToString("N2")).PadLeft(13));
            printer.DrawText("Total T. Credito:" + ("$" + TaCredito.ToString("N2")).PadLeft(13));
            printer.DrawText("Total Notas Cr. :" + ("$" + NotasCr.ToString("N2")).PadLeft(13));
            printer.DrawText("Desc Total      :" + ("$" + Desc.ToString("N2")).PadLeft(13));
            printer.DrawText("Monto Total     :" + ("$" + Monto.ToString("N2")).PadLeft(13));

            printer.DrawText("");
            printer.DrawText("");
            if (DS_RepresentantesParametros.GetInstance().Getdateprintheader())
            {
                printer.TextAlign = Justification.CENTER;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
                printer.TextAlign = Justification.LEFT;
            }

            printer.DrawLine();
            printer.DrawText("Entregado por ");

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + "(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato Resumen De Recibos 3");

            printer.Print();
        }
    }
}

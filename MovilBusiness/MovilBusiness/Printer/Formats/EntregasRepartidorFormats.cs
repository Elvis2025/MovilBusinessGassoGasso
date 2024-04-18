using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class EntregasRepartidorFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_EntregasRepartidorTransacciones myEnt;
        //private DS_EntregasRepartidor _myEnr;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        bool _IsEntregaTransacciones;


    public EntregasRepartidorFormats(DS_EntregasRepartidorTransacciones myEnt = null, bool IsEntregaTransacciones = false)
        {
            this.myEnt = myEnt;
           /// _myEnr = myEnr;
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

            _IsEntregaTransacciones = IsEntregaTransacciones;
        }

        public void Print(int entSecuecia, bool confirmado, PrinterManager printer,  string rowguid = "", int preFormato = -1, int entSecuecia2 = -1)
        {
            this.printer = printer;

            if (entSecuecia == -1 && Arguments.Values.CurrentVisSecuencia != -1)
            {
                ImprimirEntregasDeLaVisita();
                return;
            }
            
            if(_IsEntregaTransacciones)
            {
                Formato1(entSecuecia, confirmado);
            }
            else
            {
                switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor())
                {
                    default:
                    case 1:
                        Formato1(entSecuecia, confirmado);
                        break;
                    case 2:
                        Formato2(entSecuecia, confirmado, entSecuecia2);
                        break;
                }
            }
        }

        public void ImprimirEntregasDeLaVisita(bool todas = false, PrinterManager printerr = null)
        {
            if (printerr != null)
            {
                this.printer = printerr;
            }

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            List<EntregasTransacciones> entregas = null;

            if (todas)
            {
                entregas = myEnt.GetEntregasTransaccionesRealizadas();
            }
            else
            {
                entregas = myEnt.GetEntregaTransaccionDeLaActualVisita();
            }

            if (entregas == null || entregas.Count == 0)
            {
                throw new Exception("No hay entregas realizadas en esta visita");
            }

            decimal importebrutoGeneral = 0, subTotalGeneral = 0, descuentoGeneral = 0, itbisGeneral = 0, totalGeneral = 0;

            var ds = new DS_Empresa();
            foreach (var ent in entregas)
            {
                printer.Empresa = ds.GetEmpresa(ent.SecCodigo);                
                printer.PrintEmpresa();
                printer.Empresa = ds.GetEmpresa();

                printer.Font = PrinterFont.TITLE;
                printer.TextAlign = Justification.CENTER;
                printer.Bold = true;
                printer.DrawText("");
                printer.DrawText("ENTREGA REPARTIDOR");
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText(Arguments.CurrentUser.RepCodigo + " - " + ent.EntSecuencia);
                printer.TextAlign = Justification.LEFT;
                printer.Bold = false;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");

                string tipoNcfFiscal = DS_RepresentantesParametros.GetInstance().GetParTipoComprobanteFiscal();

                bool IsCreditFiscal = ent.EntNCF.Substring(1, 2) == (string.IsNullOrWhiteSpace(tipoNcfFiscal) ? "01" : tipoNcfFiscal);

                if (!string.IsNullOrWhiteSpace(ent.EntNCF) && ent.TitID != 1)
                {
                    printer.Bold = true;
                    printer.Font = PrinterFont.MAXTITLE;
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("NCF: " + ent.EntNCF);
                    if(IsCreditFiscal)
                    {
                        string valorfiscal = DS_RepresentantesParametros.GetInstance().GetParEntregasMostrarMensajeCreditoFiscal();
                        printer.DrawText(string.IsNullOrEmpty(valorfiscal) ? "VALIDA PARA CREDITO FISCAL" : valorfiscal);
                    }
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                }

                string numeroFactura = myEnt.GetEntregaTransaccionNumeroFactura(ent.CliID, ent.VenSecuencia, ent.EntNCF, false);
                if (!string.IsNullOrWhiteSpace(numeroFactura) && ent.TitID != 1)
                {
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Factura   : " + numeroFactura);
                }

                var fechaValida = DateTime.TryParse(ent.EntFecha, out DateTime fecha);
                printer.DrawText("Fecha     : " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ent.EntFecha));
                printer.DrawText("Codigo    : " + ent.CliCodigo);
                printer.DrawText("Cliente   : " + ent.CliNombre, 48);
                printer.DrawText("Calle     : " + ent.CliCalle, 45);
                printer.DrawText("Urb       : " + ent.CliUrbanizacion);
                if (IsCreditFiscal)
                {
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("RNC/Cedula: " + ent.CliRNC);
                }
                printer.DrawLine();
                //printer.DrawText("Descripcion");
                //printer.DrawText("Cantidad       Codigo            Precio");

                bool mostrarCajasUnidades = DS_RepresentantesParametros.GetInstance().GetParEntregasTransaccionesMostrarCajasUnidades();                
                printer.DrawText("Codigo - Descripcion");                
                if (mostrarCajasUnidades)
                {
                    printer.DrawText("Caj/Und  Precio  Descuento MontoItbis  Importe");
                }
                else
                {
                    printer.DrawText("Cantidad Precio  Descuento MontoItbis  Importe");
                }

                printer.DrawLine();


                var productos = myEnt.GetEntregaTransaccionesDetalle(ent.EntSecuencia, false); //myEnt.GetEntregaTransaccionesDetalleConLote(entSecuecia);//
                                                                                               //foreach (var det in productos)
                                                                                               //{
                                                                                               //    printer.DrawText(det.ProDescripcion);
                                                                                               //    printer.DrawText(det.EntCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.EntPrecio.ToString("C2").PadRight(12));
                                                                                               //}


                #region Comentado forma anterior de escibir el detalle
                /*
                double DecuentoTotal = 0.0;
                double PrecioTotal = 0.0;
                double ItebisTotal = 0.0;
                double SubTotal = 0.0;
                double Total = 0.0;
                foreach (var det in productos) //myVen.GetDetalleBySecuenciaTabacalera(venSecuencia, confirmado))
                {
                    double Descuentos;
                    double Descuentos1;
                    double AdValorem = det.EntAdValorem;
                    double Selectivo = det.EntSelectivo;
                    double PrecioLista = (det.EntIndicadorOferta ? 0.0 : det.EntPrecio + AdValorem + Selectivo);
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                    double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                    CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                    double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                    double CantidadUnica = det.EntCantidad;
                    double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                    CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                    PrecioTotal += (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                    PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                    Descuentos1 = (det.EntPrecio * det.EntDescuento) / 100;
                    Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                    Descuentos = (det.EntDescPorciento / 100) * det.EntPrecio;
                    Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                    if (Descuentos == 0.0)
                    {
                        Descuentos = det.EntDescuento;
                    }

                    double descTotalUnitario = (det.EntIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                    descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                    DecuentoTotal += (det.EntIndicadorOferta ? 0.0 : descTotalUnitario);
                    DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                    double tasaItbis = det.EntItbis;

                    double MontoItbis = (det.EntIndicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                    MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                    ItebisTotal += (MontoItbis * CantidadReal);
                    ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                    printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                    string cantidad = det.EntCantidad.ToString();

                    if (det.EntCantidadDetalle > 0)
                    {
                        cantidad += "/" + det.EntCantidadDetalle;
                    }

                    if (tasaItbis != 0)
                    {
                        PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                        PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                    }

                    double subTotal = (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                    subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                    printer.DrawText(cantidad.PadRight(15) + //Cantidad
                           (PrecioLista).ToString("N2").PadRight(18) + //Precio
                        "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                    printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                        (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                    "$" + subTotal.ToString("N2").PadRight(12)); //total


                }
                */
                #endregion

                decimal DescuentoTotal = 0;
                decimal PrecioTotal = 0;
                decimal ItebisTotal = 0;
                decimal ImporteBruto = 0;
                decimal SubTotal = 0;
                decimal Total = 0;
                foreach (var det in productos)
                {
                    decimal Descuentos;
                    decimal Descuentos1;
                    decimal AdValorem =(decimal) det.EntAdValorem;
                    decimal Selectivo = (decimal)det.EntSelectivo;
                    decimal entPrecio = (decimal)det.EntPrecio;
                    decimal PrecioLista = (det.EntIndicadorOferta ? 0.00m : entPrecio + AdValorem + Selectivo);
                    //PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                    double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                    CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                    double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                    double CantidadUnica = det.EntCantidad;
                    double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                    CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                    PrecioTotal += (det.EntIndicadorOferta ? 0.00m : PrecioLista * (decimal)CantidadReal);
                   // PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                    Descuentos1 = (entPrecio * (decimal)det.EntDescuento) / 100;
                    //Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                    Descuentos = (decimal)(det.EntDescPorciento / 100) * entPrecio;
                    //Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                    if (Descuentos == 0.00m)
                    {
                        Descuentos = (decimal)det.EntDescuento;
                    }

                    decimal descTotalUnitario = (det.EntIndicadorOferta ? 0.00m : Descuentos * (decimal)CantidadReal);
                    descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                    DescuentoTotal += (det.EntIndicadorOferta ? 0.00m : descTotalUnitario);
                    //DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                    decimal tasaItbis = (decimal)det.EntItbis;

                    decimal MontoItbis = (det.EntIndicadorOferta ? 0.00m : (((PrecioLista - Descuentos) * (decimal)CantidadReal) * (tasaItbis / 100)));// * CantidadReal;
                    

                    ItebisTotal += MontoItbis;
                   // ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                    printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                    string cantidad = det.EntCantidad.ToString();
                    if (mostrarCajasUnidades)
                    {
                        int cajas = (int)(det.EntCantidad / (det.ProUnidades > 0 ? det.ProUnidades : 1.0));
                        int unidades = (int)(det.EntCantidad - (cajas * (det.ProUnidades > 0 ? det.ProUnidades : 1.0)));

                        cantidad = $"{cajas}/{unidades}";
                    }
                    else if (det.EntCantidadDetalle > 0)
                    {
                        cantidad += "/" + det.EntCantidadDetalle;
                    }


                    //if (tasaItbis != 0)
                    //{
                    //    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                    //    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                    //}

                    MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                    decimal subTotal = (det.EntIndicadorOferta ? 0 : (PrecioLista * (decimal)CantidadReal) + MontoItbis - descTotalUnitario);
                    subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                    printer.DrawText(cantidad.PadRight(9) +             //Cantidad
                       (PrecioLista).ToString("N2").PadRight(9) +       //Precio
                       descTotalUnitario.ToString("N2").PadRight(9) +   //Descuento
                           MontoItbis.ToString("N2").PadRight(10) +     //Monto itbis
                                subTotal.ToString("N2").PadRight(9));   //Total



                }
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);
                DescuentoTotal = Math.Round(DescuentoTotal, 2, MidpointRounding.AwayFromZero);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);

                Total = (PrecioTotal) - DescuentoTotal + ItebisTotal;
                ImporteBruto = (PrecioTotal);
                SubTotal = (PrecioTotal - DescuentoTotal);

                importebrutoGeneral += ImporteBruto;
                subTotalGeneral += SubTotal;
                descuentoGeneral += DescuentoTotal;
                itbisGeneral += ItebisTotal;
                totalGeneral += Total;

                var bultosYunidades = myEnt.GetEntregaTransaccionesDetalleBultosYUnidades(ent.EntSecuencia, false);
                int bultos = 0;
                double unidad = 0;
                foreach (var item in bultosYunidades)
                {
                    bultos = item.ProUnidades;
                    unidad = item.EntCantidad;
                }

                printer.DrawLine();
                printer.DrawText("Total items        : " + productos.Count.ToString());
                printer.DrawText("Bultos Completados : " + bultos.ToString());
                printer.DrawText("Unidades Sueltas   : " + ((int)unidad).ToString());
                printer.DrawText("");
                printer.DrawText("Importe Bruto :" + ("$" + ImporteBruto.ToString("N2")).PadLeft(31));
                printer.DrawText("Descuento     :" + ("$" + DescuentoTotal.ToString("N2")).PadLeft(31));
                printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(31));
                printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(31));
                printer.Bold = true;
                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(31));
                printer.Bold = false;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                //  printer.Print();

                myEnt.ActualizarCantidadImpresion(ent.EntSecuencia, false);
            }


            //printer = new PrinterManager();
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Importe Bruto General :" + ("$" + importebrutoGeneral.ToString("N2")).PadLeft(23));
            printer.DrawText("Descuento General     :" + ("$" + descuentoGeneral.ToString("N2")).PadLeft(23));
            printer.DrawText("SubTotal General      :" + ("$" + subTotalGeneral.ToString("N2")).PadLeft(23));
            printer.DrawText("Total Itbis General   :" + ("$" + itbisGeneral.ToString("N2")).PadLeft(23));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total General         :" + ("$" + totalGeneral.ToString("N2")).PadLeft(23));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato entregas 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();


        }

        private void Formato1(int entSecuecia, bool confirmada)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var ent = myEnt.GetEntregaTransaccion(entSecuecia, confirmada);

            if (ent == null)
            {
                throw new Exception("No se encontraron los datos de la entrega");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText(ent.EntTipo == 1 ? "ENTREGA REPARTIDOR" : "RECEPCION DE DEVOLUCION");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText(Arguments.CurrentUser.RepCodigo + " - " + ent.EntSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            string tipoNcfFiscal = DS_RepresentantesParametros.GetInstance().GetParTipoComprobanteFiscal();

            bool IsCreditFiscal = ent.EntNCF.Substring(1, 2) == 
                (string.IsNullOrWhiteSpace(tipoNcfFiscal) ? "01" : tipoNcfFiscal);

            if (!string.IsNullOrWhiteSpace(ent.EntNCF) && ent.TitID != 1)
            {
                printer.Bold = true;
                printer.Font = PrinterFont.MAXTITLE;
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + ent.EntNCF);
                if(IsCreditFiscal)
                {
                    string valorfiscal = DS_RepresentantesParametros.GetInstance().GetParEntregasMostrarMensajeCreditoFiscal();
                    printer.DrawText(string.IsNullOrEmpty(valorfiscal) ? "VALIDA PARA CREDITO FISCAL" : valorfiscal);
                }                
                printer.Bold = false;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");
            }
            string numeroFactura = myEnt.GetEntregaTransaccionNumeroFactura(ent.CliID, ent.VenSecuencia, ent.EntNCF, false);
            if (!string.IsNullOrWhiteSpace(numeroFactura) && ent.TitID != 1)
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("Factura   : " + numeroFactura);
            }

            var fechaValida = DateTime.TryParse(ent.EntFecha, out DateTime fecha);
            printer.DrawText("Fecha     : " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ent.EntFecha));
            printer.DrawText("Codigo    : " + ent.CliCodigo);
            printer.DrawText("Cliente   : " + ent.CliNombre, 48);
            printer.DrawText("Calle     : " + ent.CliCalle, 45);
            printer.DrawText("Urb       : " + ent.CliUrbanizacion);
            if (IsCreditFiscal)
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("RNC/Cedula: " + ent.CliRNC);
            }
            printer.DrawLine();

             


            if (DS_RepresentantesParametros.GetInstance().GetParClientesCambiarValorPorCanid())
            {
                Arguments.Values.CurrentClient = new DS_Clientes().GetClienteById(ent.CliID);
                new DS_TablasRelaciones().GetTablasRelaciones("Clientes", "UsosMultiples");
            }

            bool mostrarCajasUnidades = DS_RepresentantesParametros.GetInstance().GetParEntregasTransaccionesMostrarCajasUnidades();

            printer.DrawText("Codigo - Descripcion");
            if (mostrarCajasUnidades)
            {
                printer.DrawText("Caj/Und  Precio  Descuento MontoItbis  Importe");
            }
            else
            {
                printer.DrawText("Cantidad Precio  Descuento MontoItbis  Importe");
            }
            printer.DrawLine();



            decimal DescuentoTotal = 0;
            decimal PrecioTotal = 0;
            decimal ItebisTotal = 0;
            decimal ImporteBruto = 0;
            decimal SubTotal = 0;
            decimal Total = 0;



            var productos = myEnt.GetEntregaTransaccionesDetalle(entSecuecia, confirmada); //myEnt.GetEntregaTransaccionesDetalleConLote(entSecuecia);//

            #region Comentado 
            /*
            foreach (var det in productos)
            {
                double Descuentos;
                double Descuentos1;
                double AdValorem = det.EntAdValorem;
                double Selectivo = det.EntSelectivo;
                double PrecioLista = (det.EntIndicadorOferta ? 0.0 : det.EntPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.EntCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.EntPrecio * det.EntDescuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.EntDescPorciento / 100) * det.EntPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.EntDescuento;
                }

                double descTotalUnitario = (det.EntIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.EntIndicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.EntItbis;

                double MontoItbis = (det.EntIndicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                ItebisTotal += (MontoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.EntCantidad.ToString();

                if (det.EntCantidadDetalle > 0)
                {
                    cantidad += "/" + det.EntCantidadDetalle;
                }

                if (tasaItbis != 0)
                {
                    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                }

                double subTotal = (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                       (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total


            } 
            */
            #endregion

            foreach (var det in productos)
            {
                decimal Descuentos;
                decimal Descuentos1;
                decimal AdValorem = (decimal)det.EntAdValorem;
                decimal Selectivo = (decimal)det.EntSelectivo;
                decimal entPrecio = (decimal)det.EntPrecio;
                decimal PrecioLista = (det.EntIndicadorOferta ? 0.00m : entPrecio + AdValorem + Selectivo);
                //PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.EntCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.EntIndicadorOferta ? 0.00m : PrecioLista * (decimal)CantidadReal);
                // PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (entPrecio * (decimal)det.EntDescuento) / 100;
                //Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (decimal)(det.EntDescPorciento / 100) * entPrecio;
                //Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.00m)
                {
                    Descuentos = (decimal)det.EntDescuento;
                }

                decimal descTotalUnitario = (det.EntIndicadorOferta ? 0.00m : Descuentos * (decimal)CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DescuentoTotal += (det.EntIndicadorOferta ? 0.00m : descTotalUnitario);
                //DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                decimal tasaItbis = (decimal)det.EntItbis;

                decimal MontoItbis = (det.EntIndicadorOferta ? 0.00m : (((PrecioLista - Descuentos) * (decimal)CantidadReal) * (tasaItbis / 100)));// * CantidadReal;


                ItebisTotal += MontoItbis;
                // ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.EntCantidad.ToString();
                if (mostrarCajasUnidades)
                {
                    int cajas = (int)(det.EntCantidad / (det.ProUnidades > 0 ? det.ProUnidades : 1.0));
                    int unidades = (int)(det.EntCantidad - (cajas * (det.ProUnidades > 0 ? det.ProUnidades : 1.0)));

                    cantidad = $"{cajas}/{unidades}";
                }
                else if (det.EntCantidadDetalle > 0)
                {
                    cantidad += "/" + det.EntCantidadDetalle;
                }


                //if (tasaItbis != 0)
                //{
                //    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                //    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                //}

                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                decimal subTotal = (det.EntIndicadorOferta ? 0 : (PrecioLista * (decimal)CantidadReal) + MontoItbis - descTotalUnitario);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(9) +                 //Cantidad
                       (PrecioLista).ToString("N2").PadRight(9) +       //Precio
                       descTotalUnitario.ToString("N2").PadRight(9) +   //Descuento
                           MontoItbis.ToString("N2").PadRight(10) +     //Monto itbis
                                subTotal.ToString("N2").PadRight(9));   //Total

            }
            PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);
            DescuentoTotal = Math.Round(DescuentoTotal, 2, MidpointRounding.AwayFromZero);
            ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);

            Total = (PrecioTotal) - DescuentoTotal + ItebisTotal;
            ImporteBruto = (PrecioTotal);
            SubTotal = (PrecioTotal - DescuentoTotal);

            var bultosYunidades = myEnt.GetEntregaTransaccionesDetalleBultosYUnidades(entSecuecia, confirmada);
            int bultos = 0;
            double unidad = 0;
            foreach (var item in bultosYunidades)
            {
                bultos = item.ProUnidades;
                unidad = item.EntCantidad;
            }

            printer.DrawLine();
            printer.DrawText("Total items        : " + productos.Count.ToString());
            printer.DrawText("Bultos Completados : " + bultos.ToString());
            printer.DrawText("Unidades Sueltas   : " + ((int)unidad).ToString());
            printer.DrawText("");
            printer.DrawText("Importe Bruto :" + ("$" + ImporteBruto.ToString("N2")).PadLeft(31));
            printer.DrawText("Descuento     :" + ("$" + DescuentoTotal.ToString("N2")).PadLeft(31));
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(31));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(31));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(31));
            printer.Bold = false;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato entregas 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();

            myEnt.ActualizarCantidadImpresion(entSecuecia, confirmada);
        }

        private void Formato1_OLD(int entSecuecia, bool confirmada)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var ent = myEnt.GetEntregaTransaccion(entSecuecia, confirmada);

            if (ent == null)
            {
                throw new Exception("No se encontraron los datos de la entrega");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText(ent.EntTipo == 1 ? "ENTREGA REPARTIDOR" : "RECEPCION DE DEVOLUCION");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText(Arguments.CurrentUser.RepCodigo + " - " + ent.EntSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");

            if (!string.IsNullOrWhiteSpace(ent.EntNCF))
            {
                printer.Bold = true;
                printer.Font = PrinterFont.MAXTITLE;
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + ent.EntNCF);
                printer.Bold = false;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");
            }
            string numeroFactura = myEnt.GetEntregaTransaccionNumeroFactura(ent.CliID, ent.VenSecuencia, ent.EntNCF, false);
            if (!string.IsNullOrWhiteSpace(numeroFactura))
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("Factura   : " + numeroFactura);
            }

            var fechaValida = DateTime.TryParse(ent.EntFecha, out DateTime fecha);
            printer.DrawText("Fecha     : " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ent.EntFecha));
            printer.DrawText("Codigo    : " + ent.CliCodigo);
            printer.DrawText("Cliente   : " + ent.CliNombre, 48);
            printer.DrawText("Calle     : " + ent.CliCalle, 45);
            printer.DrawText("Urb       : " + ent.CliUrbanizacion);
            if (ent.EntNCF.Substring(1, 2) == "01")
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("RNC/Cedula: " + ent.CliRNC);
            }
            printer.DrawLine();

            bool mostrarCajasUnidades = DS_RepresentantesParametros.GetInstance().GetParEntregasTransaccionesMostrarCajasUnidades();

            printer.DrawText("Codigo - Descripcion");         
            if (mostrarCajasUnidades)
            {
                printer.DrawText("Caj/Und        Precio            Descuento");
            }
            else
            {
                printer.DrawText("Cantidad       Precio            Descuento");
            }
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();



            decimal DecuentoTotal = 0;
            decimal PrecioTotal = 0;
            decimal ItebisTotal = 0;
            decimal SubTotal = 0;
            decimal Total = 0;

          
           
            var productos = myEnt.GetEntregaTransaccionesDetalle(entSecuecia, confirmada); //myEnt.GetEntregaTransaccionesDetalleConLote(entSecuecia);//

            #region Comentado 
            /*
            foreach (var det in productos)
            {
                double Descuentos;
                double Descuentos1;
                double AdValorem = det.EntAdValorem;
                double Selectivo = det.EntSelectivo;
                double PrecioLista = (det.EntIndicadorOferta ? 0.0 : det.EntPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.EntCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.EntPrecio * det.EntDescuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.EntDescPorciento / 100) * det.EntPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.EntDescuento;
                }

                double descTotalUnitario = (det.EntIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.EntIndicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.EntItbis;

                double MontoItbis = (det.EntIndicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                ItebisTotal += (MontoItbis * CantidadReal);
                ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.EntCantidad.ToString();

                if (det.EntCantidadDetalle > 0)
                {
                    cantidad += "/" + det.EntCantidadDetalle;
                }

                if (tasaItbis != 0)
                {
                    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                }

                double subTotal = (det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                       (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total


            } 
            */
            #endregion

            foreach (var det in productos)
            {
                double Descuentos;
                double Descuentos1;
                double AdValorem = det.EntAdValorem;
                double Selectivo = det.EntSelectivo;
                double PrecioLista = (det.EntIndicadorOferta ? 0.0 : det.EntPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.EntCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.EntCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (decimal)(det.EntIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (det.EntPrecio * det.EntDescuento) / 100;
                Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.EntDescPorciento / 100) * det.EntPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.EntDescuento;
                }

                double descTotalUnitario = (det.EntIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (decimal)(det.EntIndicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.EntItbis;

                decimal MontoItbis = (decimal)(det.EntIndicadorOferta ? 0.0 : (((PrecioLista - Descuentos) * CantidadReal) * (tasaItbis / 100)));// * CantidadReal;

                ItebisTotal += MontoItbis;
                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                //ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                


                string cantidad = det.EntCantidad.ToString(); 
                if (mostrarCajasUnidades)
                {
                    int cajas = (int)(det.EntCantidad / (det.ProUnidades > 0 ? det.ProUnidades : 1.0));
                    int unidades = (int)(det.EntCantidad - (cajas * (det.ProUnidades > 0 ? det.ProUnidades : 1.0)));

                    cantidad = $"{cajas}/{unidades}";                  
                }                
                else if (det.EntCantidadDetalle > 0)
                {
                    cantidad += "/" + det.EntCantidadDetalle;
                }

                //if (tasaItbis != 0)
                //{sum
                //    PrecioLista = PrecioLista + (PrecioLista * (tasaItbis / 100));
                //    PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);
                //}

                decimal subTotal = (det.EntIndicadorOferta ? 0 : (decimal)(PrecioLista * CantidadReal) + MontoItbis);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(17) + //Cantidad
                       (PrecioLista).ToString("N2").PadRight(19) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(11)); //Descuento



                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    MontoItbis.ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total
            }
            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("Total items: " + productos.Count.ToString());
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato entregas 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();

            myEnt.ActualizarCantidadImpresion(entSecuecia, confirmada);
        }

        private void Formato2(int traSecuecia, bool confirmada, int enrSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            var ent = myEnt.GetEntregasRepartidorTransacciones(traSecuecia, confirmada, enrSecuencia);

            if (ent == null)
            {
                throw new Exception("No se encontraron los datos de la entrega");
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ENTREGA REPARTIDOR");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("(PARA CONSULTA)");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");

            string tipoNcfFiscal = DS_RepresentantesParametros.GetInstance().GetParTipoComprobanteFiscal();

            bool IsCreditFiscal = !string.IsNullOrEmpty(ent.VenNCF) &&  ent.VenNCF.Substring(1, 2)
                == (string.IsNullOrWhiteSpace(tipoNcfFiscal) ? "01" : tipoNcfFiscal);

            if (!string.IsNullOrWhiteSpace(ent.VenNCF) && ent.TitID != 1)
            {
                printer.Bold = true;
                printer.Font = PrinterFont.MAXTITLE;
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("NCF: " + ent.VenNCF);

                if (IsCreditFiscal)
                {
                    string valorfiscal = DS_RepresentantesParametros.GetInstance().GetParEntregasMostrarMensajeCreditoFiscal();
                    printer.DrawText(string.IsNullOrEmpty(valorfiscal) ? "VALIDA PARA CREDITO FISCAL" : valorfiscal);
                }

                printer.Bold = false;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");
            }
            string numeroFactura = myEnt.GetEntregaEntregasRepartidorTransaccionesNumeroFactura(ent.CliID, ent.TraSecuencia, ent.VenNCF, false, ent.EnrSecuencia);
            if (!string.IsNullOrWhiteSpace(numeroFactura) && ent.TitID != 1)
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("Factura   : " + numeroFactura);
            }

            var fechaValida = DateTime.TryParse(ent.VenFecha, out DateTime fecha);
            printer.DrawText("Fecha     : " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ent.VenFecha));
            printer.DrawText("Codigo    : " + ent.CliCodigo);
            printer.DrawText("Cliente   : " + ent.CliNombre, 48);
            printer.DrawText("Calle     : " + ent.CliCalle, 45);
            printer.DrawText("Urb       : " + ent.CliUrbanizacion);
            if (IsCreditFiscal)
            {
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("RNC/Cedula: " + ent.CliRNC);
            }
            printer.DrawLine();


            bool mostrarCajasUnidades = DS_RepresentantesParametros.GetInstance().GetParEntregasTransaccionesMostrarCajasUnidades();

            printer.DrawText("Codigo - Descripcion");
            if (mostrarCajasUnidades)
            {
                printer.DrawText("Caj/Und        Precio            Descuento");
            }
            else
            {
                printer.DrawText("Cantidad       Precio            Descuento");
            }
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();



            decimal DescuentoTotal = 0;
            decimal PrecioTotal = 0;
            decimal ItebisTotal = 0;
            decimal SubTotal = 0;
            decimal Total = 0;

            var productos = myEnt.GetEntregasRepartidorTransaccionesDetalle(traSecuecia, confirmada, enrSecuencia); //myEnt.GetEntregaTransaccionesDetalleConLote(entSecuecia);//

            foreach (var det in productos)
            {
                decimal Descuentos;
                decimal Descuentos1;
                decimal AdValorem = (decimal)det.TraAdValorem;
                decimal Selectivo = (decimal)det.TraSelectivo;
                decimal entPrecio = (decimal)det.TraPrecio;
                decimal PrecioLista = (det.TraIndicadorOferta ? 0.00m : entPrecio + AdValorem + Selectivo);
                //PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.TraCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.TraCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.TraIndicadorOferta ? 0.00m : PrecioLista * (decimal)CantidadReal);
                // PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos1 = (entPrecio * (decimal)det.TraDescuento) / 100;
                //Descuentos1 = Math.Round(Descuentos1, 2, MidpointRounding.AwayFromZero);

                Descuentos = (decimal)(det.TraDesPorciento / 100) * entPrecio;
                //Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.00m)
                {
                    Descuentos = (decimal)det.TraDescuento;
                }

                decimal descTotalUnitario = (det.TraIndicadorOferta ? 0.00m : Descuentos * (decimal)CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                DescuentoTotal += (det.TraIndicadorOferta ? 0.00m : descTotalUnitario);

                decimal tasaItbis = (decimal)det.TraItbis;

                decimal MontoItbis = (det.TraIndicadorOferta ? 0.00m : (((PrecioLista - Descuentos) * (decimal)CantidadReal) * (tasaItbis / 100)));// * CantidadReal;


                ItebisTotal += MontoItbis;

                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.TraCantidad.ToString();
                if (mostrarCajasUnidades)
                {
                    int cajas = (int)(det.TraCantidad / (det.ProUnidades > 0 ? det.ProUnidades : 1.0));
                    int unidades = (int)(det.TraCantidad - (cajas * (det.ProUnidades > 0 ? det.ProUnidades : 1.0)));

                    cantidad = $"{cajas}/{unidades}";
                }
                else if (det.TraCantidadDetalle > 0)
                {
                    cantidad += "/" + det.TraCantidadDetalle;
                }

                MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);
                decimal subTotal = (det.TraIndicadorOferta ? 0 : (PrecioLista * (decimal)CantidadReal) + MontoItbis);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(17) + //Cantidad
                       (PrecioLista).ToString("N2").PadRight(19) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(11)); //Descuento



                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    MontoItbis.ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total


            }
            PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);
            DescuentoTotal = Math.Round(DescuentoTotal, 2, MidpointRounding.AwayFromZero);
            ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);

            Total = (PrecioTotal) - DescuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);

            printer.DrawLine();
            printer.DrawText("Total items: " + productos.Count.ToString());
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DescuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(27, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionEntregasRepartidor()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato entregas 2: Movilbusiness " + Functions.AppVersion);
            printer.Print();

            myEnt.ActualizarRepartidorCantidadImpresion(traSecuecia, confirmada);
        }
    }
}

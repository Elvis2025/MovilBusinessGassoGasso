
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Utils;
using System;
using System.Linq;
using System.Security.Cryptography;
using Xamarin.Forms;

namespace MovilBusiness.printers.formats
{
    public class PedidosFormats : IPrinterFormatter
    {
        private DS_Pedidos myPed;
        private PrinterManager printer;
        private DS_Visitas myvisit;
        private DS_Recibos myRec;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        public PedidosFormats(DS_Pedidos myPed)
        {
            this.myPed = myPed;
            myvisit = new DS_Visitas();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
            myRec = new DS_Recibos();
        }

        public void Print(int pedSecuencia, bool Confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            // Copias = copias;
            this.printer = printer;

            var formatos = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionPedidos();

            var formatToUse = formatos[0];

            if (forceFormat != -1)
            {
                formatToUse = forceFormat;
            }

            switch (formatToUse)
            {
                default:
                case 1: //sanofi
                    Formato1(pedSecuencia, Confirmado);
                    break;
                case 2: //leBetances
                    Formato2(pedSecuencia, Confirmado);
                    break;
                case 3: //Feltrex
                    Formato3(pedSecuencia, Confirmado);
                    break;
                case 4: //2 pulgadsa - CPCL
                    Formato4(pedSecuencia, Confirmado);
                    break;
                case 5: //Tabacalera
                    Formato5(pedSecuencia, Confirmado);
                    break;
                case 6: // Dinafa
                    Formato6(pedSecuencia, Confirmado);
                    break;
                case 7: //LAM
                    Formato7(pedSecuencia, Confirmado);
                    break;
                case 8: //Planeta Azul
                    Formato8(pedSecuencia, Confirmado);
                    break;
                case 9: //GASSO
                    Formato9(pedSecuencia, Confirmado);
                    break;
                case 10: // La Plaza
                    Formato10(pedSecuencia, Confirmado);
                    break;
                case 15: // Agroproductores
                    Formato15(pedSecuencia, Confirmado);
                    break;
                case 18: //Grupo Armenteros Modificado a Solicitud
                    Formato18(pedSecuencia, Confirmado);
                    break;
                case 19: //Grupo Armenteros
                    Formato19(pedSecuencia, Confirmado);
                    break;
                case 20: //Feltrex
                    Formato20(pedSecuencia, Confirmado);
                    break;
                case 21: //SUED
                    Formato21(pedSecuencia, Confirmado);
                    break;
                case 22: //Feltrex - Zebra
                    Formato22(pedSecuencia, Confirmado);
                    break;
                case 24: //Cano Ind
                    Formato24(pedSecuencia, Confirmado);
                    break;
                case 25: //Tabacalera - Version Similar a Formato 4 de Ventas 
                    Formato25(pedSecuencia, Confirmado);
                    break;
                case 35://Formato 2 pulgadas
                    Formato35(pedSecuencia, Confirmado);
                    break;
                case 36://Formato 
                    Formato36(pedSecuencia, Confirmado);
                    break;
                case 37: // La Plaza
                    Formato37(pedSecuencia, Confirmado);
                    break;
                case 38: // La Plaza
                    Formato38(pedSecuencia, Confirmado);
                    break;
                case 41: // Formato SAP con Descuento General - No Cambiar
                    Formato41(pedSecuencia, Confirmado);
                    break;
                case 42: //Formato 2 pulgadas AJFA
                    Formato42(pedSecuencia, Confirmado);
                    break;
                case 43: //ANDOSA
                    Formato43(pedSecuencia, Confirmado);
                    break;

            }
        }

        #region Sanofi
        private void Formato1(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            if (DS_RepresentantesParametros.GetInstance().GetDescuentoxPrecioNegociado())
            {
                foreach (var det in myPed.GetDetalleBySecuenciaConPrecioNegociado(pedSecuencia, pedidoConfirmado))
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParPedidosEditarPrecioNegconItebis())
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + Math.Round(det.PedPrecio * ((det.PedItbis / 100) + 1), 2).ToString("C2").PadLeft(12));
                    }
                    else
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadLeft(12));
                    }

                }
            }
            else
            {
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {
                    printer.DrawText(det.ProDescripcion, 48);
                    printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadRight(12));
                }
            }


            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
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
            printer.DrawText("Formato pedidos 1: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        #endregion

        #region LeBetances
        private void Formato2(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);
            var visipres = myvisit.GetClientePresentacion(ped.VisSecuencia);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            switch (ped.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Cliente: " + ped.CliNombre, 48);
                    printer.DrawText("Codigo: " + ped.CliCodigo);
                    printer.DrawText("Calle: " + ped.CliCalle, 45);
                    break;
            }

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuento = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(9));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                descuento += det.PedDescuento * cantidad;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + descuento.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 2: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
        #endregion

        #region Tabacalera
        private void Formato5(int pedSecuencia, bool confirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }


            Pedidos ped = myPed.GetBySecuenciaTabacalera(pedSecuencia, confirmado);
            bool putfecha = true;
            printer.PrintEmpresa(pedSecuencia, putfecha);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O");
            printer.Bold = false;

            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);
            printer.DrawText("Fecha      : " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : ped.PedFecha));
            printer.DrawText("Orden #    : " + ped.PedSecuencia, 48);
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + ped.CliCodigo);
            printer.DrawText("Cliente    : " + ped.CliNombre);
            printer.DrawText("Propietario: " + ped.CliPropietario);
            printer.DrawText("Calle      : " + ped.CliCalle, 48);
            printer.DrawText("Urb        : " + ped.CliUrbanizacion, 48);
            printer.DrawText("RNC/Cedula : " + ped.CliRnc, 48);
            printer.DrawText("Telefono   : " + ped.CliTelefono, 48);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();
            double total = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
            {
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidaddet = det.PedCantidad.ToString();

                printer.DrawText(det.ProDescripcion);

                cantidaddet = cantidaddet + "/" + det.PedCantidadDetalle;

                double precio = det.PedPrecio + det.PedAdValorem + det.PedSelectivo;

                printer.DrawText(cantidaddet.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + precio.ToString("N2").PadRight(12));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);

                total += (precio - det.PedDescuento) * cantidad;
            }

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Total   : " + total.ToString("N2").PadRight(15));
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Forma de pago  :" + ped.ConDescripcion.PadRight(15));
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de ");
            printer.DrawText("      Referencia");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha impresion: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del Vendedor");
            printer.DrawText(Arguments.CurrentUser.RepNombre);
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Version " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato pedidos 5");
            printer.Print();
        }


        private void Formato25(int pedSecuencia, bool confirmado)
        {

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            bool putfecha = true;
            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuenciaTabacalera(pedSecuencia, confirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O");
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(ped.PedCantidadImpresion == 0 ? "ORIGINAL" : "COPIA");

            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Orden #    : " + Arguments.CurrentUser.RepCodigo + "-" + pedSecuencia + " (" + ped.ConDescripcion + ")");
            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);
            printer.DrawText("Fecha orden: " + (fechaValida ? fecha.ToString("dd/MM/yyyy hh:mm tt") : ped.PedFecha));
            printer.DrawText("Ruta       : " + Arguments.CurrentUser.RutID);
            printer.DrawText("Codigo     : " + ped.CliCodigo);
            printer.DrawText("Cliente    : " + ped.CliNombre);
            printer.DrawText("");
            printer.DrawText("Propietario: " + ped.CliPropietario, 46);
            printer.DrawText("Calle      : " + ped.CliCalle, 46);
            printer.DrawText("Urb        : " + ped.CliUrbanizacion);
            printer.DrawText("RNC/Cedula : " + ped.CliRnc);
            printer.DrawText("Telefono   : " + ped.CliTelefono);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cantidad       Precio            Descuento");
            printer.DrawText("Itbis          Monto Itbis       Importe");
            printer.DrawLine();

            double DecuentoTotal = 0.0;
            double PrecioTotal = 0.0;
            double ItebisTotal = 0.0;
            double SubTotal = 0.0;
            double Total = 0.0;

            foreach (var det in myPed.GetDetalleBySecuenciaTabacalera(pedSecuencia, confirmado))
            {
                double Descuentos;
                double AdValorem = det.PedAdValorem;
                double Selectivo = det.PedSelectivo;
                double PrecioLista = (det.PedIndicadorOferta ? 0.0 : det.PedPrecio + AdValorem + Selectivo);
                PrecioLista = Math.Round(PrecioLista, 2, MidpointRounding.AwayFromZero);

                double CantidadDetalle = Convert.ToDouble(Convert.ToDecimal(det.PedCantidadDetalle));
                CantidadDetalle = Math.Round(CantidadDetalle, 2, MidpointRounding.AwayFromZero);
                double ProUnidades = Convert.ToDouble(Convert.ToDecimal(det.ProUnidades));
                double CantidadUnica = det.PedCantidad;
                double CantidadReal = (CantidadDetalle / ProUnidades) + CantidadUnica;
                CantidadReal = Math.Round(CantidadReal, 2, MidpointRounding.AwayFromZero);

                PrecioTotal += (det.PedIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                PrecioTotal = Math.Round(PrecioTotal, 2, MidpointRounding.AwayFromZero);

                Descuentos = (det.PedDesPorciento / 100) * det.PedPrecio;
                Descuentos = Math.Round(Descuentos, 2, MidpointRounding.AwayFromZero);

                if (Descuentos == 0.0)
                {
                    Descuentos = det.PedDescuento;
                }

                double descTotalUnitario = (det.PedIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario = Math.Round(descTotalUnitario, 2, MidpointRounding.AwayFromZero);

                double descTotalUnitario1 = (det.PedIndicadorOferta ? 0.0 : Descuentos * CantidadReal);
                descTotalUnitario1 = Math.Round(descTotalUnitario1, 2, MidpointRounding.AwayFromZero);

                DecuentoTotal += (det.PedIndicadorOferta ? 0.0 : descTotalUnitario);
                DecuentoTotal = Math.Round(DecuentoTotal, 2, MidpointRounding.AwayFromZero);

                double tasaItbis = det.PedItbis;

                double MontoItbis = (det.PedIndicadorOferta ? 0.0 : ((PrecioLista - Descuentos) * (tasaItbis / 100)));// * CantidadReal;
                //MontoItbis = Math.Round(MontoItbis, 2, MidpointRounding.AwayFromZero);

                //(precio - Descuento + Itbis) * cantidad = subtotal

                ItebisTotal += (MontoItbis * CantidadReal);
                //ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);


                printer.DrawText(det.ProCodigo + " - " + det.ProDescripcion);

                string cantidad = det.PedCantidad.ToString();

                if (det.PedCantidadDetalle > 0)
                {
                    cantidad += "/" + det.PedCantidadDetalle;
                }

                double subTotal = (det.PedIndicadorOferta ? 0.0 : PrecioLista * CantidadReal);
                subTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);

                printer.DrawText(cantidad.PadRight(15) + //Cantidad
                        (PrecioLista).ToString("N2").PadRight(18) + //Precio
                    "$" + descTotalUnitario.ToString("N2").PadRight(12)); //Descuento

                printer.DrawText((tasaItbis.ToString() + "%").PadRight(15) + //itbis
                    (MontoItbis * CantidadReal).ToString("N2").PadRight(18) + //Monto itbis
                "$" + subTotal.ToString("N2").PadRight(12)); //total
            }

            Total = (PrecioTotal) - DecuentoTotal + ItebisTotal;
            SubTotal = (PrecioTotal);
            ItebisTotal = Math.Round(ItebisTotal, 2, MidpointRounding.AwayFromZero);

            printer.DrawLine();
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal      :" + ("$" + SubTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Descuento     :" + ("$" + DecuentoTotal.ToString("N2")).PadLeft(15));
            printer.DrawText("Total Itbis   :" + ("$" + ItebisTotal.ToString("N2")).PadLeft(15));
            printer.Bold = true;
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("Total         :" + ("$" + Total.ToString("N2")).PadLeft(15));
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(1, 25) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(1, 25));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha impresion: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Por el cliente: -------------------------------");
            printer.DrawText("Aceptado y recibido conforme");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Por el Vendedor: ------------------------------");
            printer.DrawText("(" + Arguments.CurrentUser.RutID.ToString() + ") " + Arguments.CurrentUser.RepNombre);
            printer.DrawText(Arguments.CurrentUser.RepTelefono1.FormatTextToPhone().CenterText(48));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORIGINAL: CLIENTE, COPIA: CONTABILIDAD");
            printer.DrawText("");
            printer.DrawText("GRACIAS!!!");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Formato pedidos 25");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("");
            printer.Print();
        }
        #endregion

        #region Dinafa
        private void Formato6(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuento = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(9));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                descuento += det.PedDescuento * cantidad;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + descuento.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Precios sujetos a cambio, sin previo aviso");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 6: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
        #endregion

        private void Formato7(int pedSecuencia, bool pedidoConfirmado)
        {
            int cont = 0;
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold: true);
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }
            printer.TextAlign = Justification.LEFT;
            printer.Font = PrinterFont.BODY;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("Fecha: " + Convert.ToDateTime(ped.PedFecha).ToString("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.DrawText("O R D E N  D E  P E D I D O");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);
            if (ped.CliUrbanizacion != null)
                printer.DrawText("Urb: " + ped.CliUrbanizacion, 45);
            else
                printer.DrawText("Urb: ");
            printer.DrawText("Orden #: " + ped.PedSecuencia);
            printer.DrawText("");
            printer.DrawText("Fecha Impresion: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuento = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                cont++;
                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(9));
                printer.DrawText("");

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;
                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                descuento += det.PedDescuento * cantidad;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(33));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(33));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(33));
            printer.DrawText("");
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de", 45);
            printer.DrawText("Referencia");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawLine();
            printer.DrawText("Firma del cliente:");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("SKU: " + cont);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 7 : movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato8(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            if (DS_RepresentantesParametros.GetInstance().GetDescuentoxPrecioNegociado())
            {
                foreach (var det in myPed.GetDetalleBySecuenciaConPrecioNegociado(pedSecuencia, pedidoConfirmado))
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParPedidosEditarPrecioNegconItebis())
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + Math.Round(det.PedPrecio * ((det.PedItbis / 100) + 1), 2).ToString("C2").PadLeft(12));
                    }
                    else
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadLeft(12));
                    }

                }
            }
            else
            {
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {
                    printer.DrawText(det.ProDescripcion, 48);
                    printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadRight(12));
                }
            }


            printer.DrawLine();
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
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
            printer.DrawText("Formato pedidos 8: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        #region Importadora La Plaza
        private void Formato10(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("CODIGO   CANT   REFERENCIA            PRECIO");
            printer.DrawLine();

            printer.Font = PrinterFont.TITLE;
            if (DS_RepresentantesParametros.GetInstance().GetDescuentoxPrecioNegociado())
            {
                foreach (var det in myPed.GetDetalleBySecuenciaConPrecioNegociado(pedSecuencia, pedidoConfirmado))
                {
                    printer.DrawText(det.ProCodigo.PadRight(15) + det.PedCantidad.ToString().PadRight(18) + det.Referencia.PadLeft(12));
                }
            }
            else
            {
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {
                    printer.DrawText(det.ProCodigo.PadRight(10) + det.PedCantidad.ToString().PadRight(3) + det.Referencia.PadLeft(5) + det.PedPrecio.ToString("C2").PadLeft(8));
                }
            }


            printer.DrawLine();
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
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
            printer.DrawText("Formato pedidos 10: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        #endregion

        #region Formato Grupo Armenteros v2 - ESCPOS
        private void Formato18(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            //printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O   " + "No." + ped.PedSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.CliNombre.Length > 28)
            {
                ped.CliNombre = ped.CliNombre.Substring(0, 27);
            }
            printer.DrawText("Cliente          : " + ped.CliNombre, 48);
            printer.DrawText("RNC              : " + ped.CliRnc, 48);
            printer.DrawText("Codigo           : " + ped.CliCodigo);
            printer.DrawText("Direccion entrega: " + ped.CliCalle, 45);
            printer.DrawText("Fecha            : " + (fechaValida ? fecha.ToString("dd - MM - yyyy hh: mm tt") : ped.PedFecha));
            printer.DrawText("Condicion de pago: " + ped.ConDescripcion);
            printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Codigo  -  Codigo Barra");
            printer.DrawText("Descripcion");
            printer.DrawText("Cant.                 Precio             Desc.");
            printer.DrawText("Itbis            Monto Itbis          Subtotal");
            printer.Bold = false;
            printer.DrawLine();

            double SubTotalGeneral = 0.0;
            double DescuentoTotal = 0.0;
            double TotalItbisTotal = 0.0;
            double TotalGeneral = 0.0;
            double TotalCajas = 0.0;

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 47);
                }

                

                double PrecioBruto = 0.0;
                double DescUnitario = 0.0;
                double SubTotal = 0.0;
                double itbisUnitario = 0.0;
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }
                if (det.PedCantidadDetalle > 0)
                {
                    PrecioBruto = det.PedPrecio / det.ProUnidades * det.PedCantidadDetalle;
                    DescUnitario = (det.PedDescuento / det.ProUnidades) * det.PedCantidadDetalle;
                    SubTotal = ((det.PedPrecio / det.ProUnidades) * det.PedCantidadDetalle) - DescUnitario;

                }
                else
                {
                    PrecioBruto = det.PedPrecio * det.PedCantidad;
                    SubTotal = PrecioBruto - (det.PedDescuento * det.PedCantidad);
                    DescUnitario = det.PedDescuento * det.PedCantidad;
                }
                itbisUnitario = SubTotal * (det.PedItbis / 100);
                SubTotal += itbisUnitario;

                //Totales Abajo
                SubTotalGeneral += SubTotal;
                DescuentoTotal += DescUnitario;
                TotalItbisTotal += itbisUnitario;
                TotalCajas += det.PedCantidad;

                printer.DrawText(det.ProCodigo.Trim().PadRight(8) + "- " + det.ProReferencia, 48);
                printer.DrawText(det.ProDescripcion, 48);
                printer.DrawText(cantidad.PadRight(17) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(18));
                printer.DrawText((det.PedItbis.ToString() + "%").PadRight(10) + itbis.ToString("C2").PadLeft(18) + SubTotal.ToString("C2").PadLeft(18), 49);
                printer.DrawText("");

            }

            TotalGeneral = SubTotalGeneral + TotalItbisTotal;
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("SKU: " + ped.PedTotal + " " + "Cantidad Cajas:".PadLeft(28) + TotalCajas.ToString().PadLeft(2));
            printer.DrawText("");
            printer.DrawText("SubTotal:".PadRight(12) + (SubTotalGeneral - TotalItbisTotal).ToString("C2").PadLeft(12) + " " + "Descuento:".PadRight(10) + DescuentoTotal.ToString("C2").PadLeft(11));
            printer.DrawText("Total itbis:".PadRight(12) + TotalItbisTotal.ToString("C2").PadLeft(12) + " " + "Total:".PadRight(10) + SubTotalGeneral.ToString("C2").PadLeft(11));
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
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
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Formato pedidos 18: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        #endregion

        #region Formato Grupo Armenteros - ESCPOS
        private void Formato19(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            //printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O   " + "No." + ped.PedSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.CliNombre.Length > 28)
            {
                ped.CliNombre = ped.CliNombre.Substring(0, 27);
            }
            printer.DrawText("Cliente          : " + ped.CliNombre, 48);
            printer.DrawText("RNC              : " + ped.CliRnc, 48);
            printer.DrawText("Codigo           : " + ped.CliCodigo);
            printer.DrawText("Direccion entrega: " + ped.CliCalle, 45);
            printer.DrawText("Fecha            : " + (fechaValida ? fecha.ToString("dd - MM - yyyy hh: mm tt") : ped.PedFecha));
            printer.DrawText("Condicion de pago: " + ped.ConDescripcion);
            printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cant.   Codigo        Precio             Desc.");
            printer.DrawText("Itbis            Monto Itbis          Subtotal");
            //                Caj/Unid  Factura       Lote         Fecha
            printer.Bold = false;
            printer.DrawLine();

            double SubTotalGeneral = 0.0;
            double DescuentoTotal = 0.0;
            double TotalItbisTotal = 0.0;
            double TotalGeneral = 0.0;

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 47);
                }

                printer.DrawText(det.ProDescripcion, 48);

                double PrecioBruto = 0.0;
                double DescUnitario = 0.0;
                double SubTotal = 0.0;
                double itbisUnitario = 0.0;
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }
                if (det.PedCantidadDetalle > 0)
                {
                    PrecioBruto = det.PedPrecio / det.ProUnidades * det.PedCantidadDetalle;
                    DescUnitario = (det.PedDescuento / det.ProUnidades) * det.PedCantidadDetalle;
                    SubTotal = ((det.PedPrecio / det.ProUnidades) * det.PedCantidadDetalle) - DescUnitario;

                }
                else
                {
                    PrecioBruto = det.PedPrecio * det.PedCantidad;
                    SubTotal = PrecioBruto - (det.PedDescuento * det.PedCantidad);
                    DescUnitario = det.PedDescuento * det.PedCantidad;
                }
                itbisUnitario = SubTotal * (det.PedItbis / 100);
                SubTotal += itbisUnitario;

                //Totales Abajo
                SubTotalGeneral += SubTotal;
                DescuentoTotal += DescUnitario;
                TotalItbisTotal += itbisUnitario;

                printer.DrawText(cantidad.PadRight(8) + det.ProCodigo.Trim().PadRight(9) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(18));
                printer.DrawText((det.PedItbis.ToString() + "%").PadRight(10) + itbis.ToString("C2").PadLeft(18) + SubTotal.ToString("C2").PadLeft(18), 49);
                printer.DrawText("");

            }

            TotalGeneral = SubTotalGeneral + TotalItbisTotal;
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:".PadRight(12) + (SubTotalGeneral - TotalItbisTotal).ToString("C2").PadLeft(12) + " " + "Descuento:".PadRight(10) + DescuentoTotal.ToString("C2").PadLeft(11));
            printer.DrawText("Total itbis:".PadRight(12) + TotalItbisTotal.ToString("C2").PadLeft(12) + " " + "Total:".PadRight(10) + SubTotalGeneral.ToString("C2").PadLeft(11));
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
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
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Formato pedidos 19: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        #endregion

        #region Formato Feltrex
        private void Formato20(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            //printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("O R D E N  D E  P E D I D O   " + "No." + ped.PedSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("");
            if (ped.CliNombre.Length > 28)
            {
                ped.CliNombre = ped.CliNombre.Substring(0, 27);
            }
            printer.DrawText("Cliente          : " + ped.CliNombre, 48);
            //printer.DrawText("");
            printer.DrawText("RNC              : " + ped.CliRnc, 48);
            //printer.DrawText("");
            printer.DrawText("Codigo           : " + ped.CliCodigo);
            //printer.DrawText("");
            if (ped.CliCalle.Length > 25)
            {
                ped.CliCalle = ped.CliCalle.Substring(0, 25);
            }
            printer.DrawText("Direccion entrega: " + ped.CliCalle, 45);
            //printer.DrawText("");
            printer.DrawText("Fecha            : " + (fechaValida ? fecha.ToString("dd - MM - yyyy hh: mm tt") : ped.PedFecha));
            // printer.DrawText("");
            printer.DrawText("Condicion de pago: " + ped.ConDescripcion);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Descripcion");
            // printer.DrawText("");
            printer.DrawText("Cant.   Codigo        Precio             Desc.");
            //printer.DrawText("");
            printer.DrawText("Itbis            Monto Itbis          Subtotal");
            // printer.DrawText("");
            //                Caj/Unid  Factura       Lote         Fecha
            printer.Bold = false;
            printer.DrawLine();

            double SubTotalGeneral = 0.0;
            double DescuentoTotal = 0.0;
            double TotalItbisTotal = 0.0;
            double TotalGeneral = 0.0;


            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 47);
                }
                printer.DrawText(det.ProDescripcion, 48);
                //printer.DrawText("");
                double PrecioBruto = 0.0;
                double DescUnitario = 0.0;
                double SubTotal = 0.0;
                double itbisUnitario = 0.0;
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }
                if (det.PedCantidadDetalle > 0)
                {
                    PrecioBruto = det.PedPrecio / det.ProUnidades * det.PedCantidadDetalle;
                    DescUnitario = (det.PedDescuento / det.ProUnidades) * det.PedCantidadDetalle;
                    SubTotal = ((det.PedPrecio / det.ProUnidades) * det.PedCantidadDetalle) - DescUnitario;

                }
                else
                {
                    PrecioBruto = det.PedPrecio * det.PedCantidad;
                    SubTotal = PrecioBruto - (det.PedDescuento * det.PedCantidad);
                    DescUnitario = det.PedDescuento * det.PedCantidad;
                }
                itbisUnitario = SubTotal * (det.PedItbis / 100);
                SubTotal += itbisUnitario;

                //Totales Abajo
                SubTotalGeneral += SubTotal;
                DescuentoTotal += DescUnitario;
                TotalItbisTotal += itbisUnitario;

                printer.DrawText(cantidad.PadRight(8) + det.ProCodigo.Trim().PadRight(9) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(18), 49);
                // printer.DrawText("");
                printer.DrawText((det.PedItbis.ToString() + "%").PadRight(10) + itbis.ToString("C2").PadLeft(18) + SubTotal.ToString("C2").PadLeft(18), 49);
                printer.DrawText("");

            }

            TotalGeneral = SubTotalGeneral + TotalItbisTotal;
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:".PadRight(12) + (SubTotalGeneral - TotalItbisTotal).ToString("C2").PadLeft(12) + " " + "Descuento:".PadRight(10) + DescuentoTotal.ToString("C2").PadLeft(11));
            //printer.DrawText("");
            printer.DrawText("Total itbis:".PadRight(12) + TotalItbisTotal.ToString("C2").PadLeft(12) + " " + "Total:".PadRight(10) + SubTotalGeneral.ToString("C2").PadLeft(11));
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Nota: los precios mostrados de referencia", 45);
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;

            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Formato pedidos 20: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        #endregion

        #region SUED
        private void Formato21(int pedSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuenciaSued(pedSecuencia, confirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.PedCantidadImpresion == 0)
            {
                printer.DrawText("O R I G I N A L");
            }
            else
            {
                printer.DrawText("C O P I A  No. " + ped.PedCantidadImpresion);
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + ped.PedSecuencia, "H");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));

            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, confirmado))
            {
                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadRight(12));
            }


            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de ");
            printer.DrawText("      Referencia");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma Y Sello del Cliente:");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + myPed.GetDetalleBySecuencia(pedSecuencia, confirmado).Count.ToString());

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 21: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

            Hash pedidos = new Hash(confirmado ? "PedidosConfirmados" : "Pedidos");
            pedidos.Add("PedCantidadImpresion", ped.PedCantidadImpresion + 1);
            pedidos.ExecuteUpdate("PedSecuencia = " + ped.PedSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }
        #endregion

        #region Formato Feltrex - Zebra
        private void Formato22(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            //printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = false;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("O R D E N  D E  P E D I D O   " + "No." + ped.PedSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            if (ped.CliNombre.Length > 28)
            {
                ped.CliNombre = ped.CliNombre.Substring(0, 27);
            }
            ped.CliNombre = ped.CliNombre.TrimEnd(' ');
            printer.DrawText("Cliente          : " + ped.CliNombre, 48);
            printer.DrawText("RNC              : " + ped.CliRnc, 48);
            printer.DrawText("Codigo           : " + ped.CliCodigo);
            if (ped.CliCalle.Length > 30)
            {
                ped.CliCalle = ped.CliCalle.Substring(0, 30);
            }
            ped.CliCalle = ped.CliCalle.TrimEnd(' ');
            printer.DrawText("Direccion entrega: " + ped.CliCalle, 45);
            printer.DrawText("Fecha            : " + (fechaValida ? fecha.ToString("dd - MM - yyyy hh: mm tt") : ped.PedFecha));
            printer.DrawText("Condicion de pago: " + ped.ConDescripcion);
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cant.   Codigo        Precio             Desc.");
            printer.DrawText("Itbis            Monto Itbis          Subtotal");
            printer.Bold = false;
            printer.DrawLine();
            //printer.DrawText("");

            double SubTotalGeneral = 0.0;
            double DescuentoTotal = 0.0;
            double TotalItbisTotal = 0.0;
            double TotalGeneral = 0.0;


            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 47);
                }
                printer.DrawText(det.ProDescripcion, 48);
                //printer.DrawText("");
                double PrecioBruto = 0.0;
                double DescUnitario = 0.0;
                double SubTotal = 0.0;
                double itbisUnitario = 0.0;
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }
                if (det.PedCantidadDetalle > 0)
                {
                    PrecioBruto = det.PedPrecio / det.ProUnidades * det.PedCantidadDetalle;
                    DescUnitario = (det.PedDescuento / det.ProUnidades) * det.PedCantidadDetalle;
                    SubTotal = ((det.PedPrecio / det.ProUnidades) * det.PedCantidadDetalle) - DescUnitario;

                }
                else
                {
                    PrecioBruto = det.PedPrecio * det.PedCantidad;
                    SubTotal = PrecioBruto - (det.PedDescuento * det.PedCantidad);
                    DescUnitario = det.PedDescuento * det.PedCantidad;
                }
                itbisUnitario = SubTotal * (det.PedItbis / 100);
                SubTotal += itbisUnitario;

                //Totales Abajo
                SubTotalGeneral += SubTotal;
                DescuentoTotal += DescUnitario;
                TotalItbisTotal += itbisUnitario;

                printer.DrawText(cantidad.PadRight(8) + det.ProCodigo.Trim().PadRight(9) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(18), 49);
                // printer.DrawText("");
                printer.DrawText((det.PedItbis.ToString() + "%").PadRight(10) + itbis.ToString("C2").PadLeft(18) + SubTotal.ToString("C2").PadLeft(18), 49);
                printer.DrawText("");

            }

            TotalGeneral = SubTotalGeneral + TotalItbisTotal;
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:".PadRight(12) + (SubTotalGeneral - TotalItbisTotal).ToString("C2").PadLeft(12) + " " + "Descuento:".PadRight(10) + DescuentoTotal.ToString("C2").PadLeft(11));
            //printer.DrawText("");
            printer.DrawText("Total itbis:".PadRight(12) + TotalItbisTotal.ToString("C2").PadLeft(12) + " " + "Total:".PadRight(10) + SubTotalGeneral.ToString("C2").PadLeft(11));
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Nota: los precios mostrados de referencia", 45);
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;

            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            //printer.DrawText("");
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato pedidos 22: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        #endregion

        #region Cano Ind
        private void Formato24(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + ped.PedSecuencia);
            printer.DrawText("");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad   Codigo        Precio     Desc.");
            //                Caj/Unid  Factura       Lote         Fecha
            printer.Bold = false;
            printer.DrawLine();

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                printer.DrawText(det.ProDescripcion, 48);

                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();

                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }

                printer.DrawText(cantidad.PadRight(11) + det.ProCodigo.Trim().PadRight(14) + det.PedPrecio.ToString("C2").PadRight(11) + descuento.PadRight(11), 49);
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato pedidos 24: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        #endregion

        #region Formato 2 pulgadas
        private void Formato35(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawText("--------------------------------");
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawText("--------------------------------");

            double total = 0, subtotal = 0, totalItbis = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(10));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawText("--------------------------------");
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("--------------------------------");
            printer.Bold = false;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("SKU:  2");
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 35: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        #endregion
        private void Formato36(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawText("Moneda: " + myPed.GetMonNombrePedido(pedSecuencia, pedidoConfirmado));
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo          Precio       Itbis");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuento = 0;
            string MonSigla = myPed.GetMonSiglaPedido(pedSecuencia, pedidoConfirmado);

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion);
                if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo())
                {
                    printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N4").PadRight(13) + itbis.ToString("N4").PadRight(9));
                }
                else
                {
                    printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(16) + det.PedPrecio.ToString("N2").PadRight(13) + itbis.ToString("N2").PadRight(9));
                }


                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                descuento += det.PedDescuento * cantidad;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + (MonSigla + "$" + subtotal.ToString("N2").PadLeft(15)));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + (MonSigla + "$" + descuento.ToString("N2").PadLeft(15)));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + (MonSigla + "$" + totalItbis.ToString("N2").PadLeft(15)));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total      :" + (MonSigla + "$" + total.ToString("N2").PadLeft(15)));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 36: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        private void Formato37(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            //printer.DrawText("CODIGO          DESC");
            printer.DrawText("CODIGO        DESC    CANT     PRECIO     TOTAL");
            //printer.DrawText("CANT            PRECIO             PRECIO TOTAL");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0;
            if (DS_RepresentantesParametros.GetInstance().GetDescuentoxPrecioNegociado())
            {
                foreach (var det in myPed.GetDetalleBySecuenciaConPrecioNegociado(pedSecuencia, pedidoConfirmado))
                {
                    var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                    var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);

                    subtotal += det.PedPrecio * cantidad;
                    total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                    totalItbis += (det.PedPrecio * 0.18) * cantidad;

                    //printer.DrawText(det.ProCodigo.PadLeft(12) + det.Referencia);
                    printer.DrawText(det.ProCodigo.PadLeft(12) + det.Referencia.PadLeft(8) + det.PedCantidad.ToString().PadLeft(11) + det.PedPrecio.ToString().PadLeft(10) +
                                    (det.PedPrecio * cantidad).ToString("N2"));
                }
            }
            else
            {
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {

                    if (det.ProDescripcion.Length > 30)
                    {
                        det.ProDescripcion = det.ProDescripcion.Substring(0, 26);
                    }

                    var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                    var cantidad = ((det.PedCantidadDetalle > 0 && det.ProUnidades > 0 ? (double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) : 0) + det.PedCantidad);

                    subtotal += det.PedPrecio * cantidad;
                    total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                    totalItbis += (det.PedPrecio * 0.18) * cantidad;

                    printer.DrawText(det.ProCodigo + "  " + det.ProDescripcion);
                    printer.DrawText(det.PedCantidad.ToString() + det.PedPrecio.ToString().PadLeft(20) + (det.PedPrecio * cantidad).ToString("N2").PadLeft(25));
                }
            }

            printer.DrawLine();

            printer.Bold = true;
            printer.DrawText("TOTAL BRUTO  :" + subtotal.ToString("N2").PadLeft(33));
            printer.DrawText("MAS 18% ITBIS:" + totalItbis.ToString("N2").PadLeft(33));
            printer.DrawText("TOTAL NETO   :" + (subtotal + totalItbis).ToString("N2").PadLeft(33));
            printer.DrawText("");

            int recsecuencia = myPed.GetPedidosByPedSecuenciaForVenSec(pedSecuencia);

            if (recsecuencia > 0)
            {
                printer.DrawText("");
                printer.DrawText("Formas de pago");
                printer.DrawLine();
                //foreach formas pago
                double TotalCobrado = 0;
                foreach (RecibosFormaPago rec in myRec.GetRecibosFormasPagoBySecuencia(recsecuencia, false))
                {
                    TotalCobrado += rec.RefValor;

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
                        case 18:
                            printer.DrawText("Orden de Pago: " + rec.RefNumeroCheque);
                            printer.DrawText("Fecha   : " + rec.RefFecha);
                            printer.DrawText("COOP.   : " + rec.BanNombre, 48);
                            printer.DrawText("Monto   : " + rec.RefValor.ToString("N2").PadLeft(35));
                            break;
                        default:
                            printer.DrawText("");
                            break;
                    }
                    printer.DrawLine();
                }

            }

            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del Vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato pedidos 37: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        private void Formato9(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa(Notbold:true);
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);
            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy h:mm:ss tt") : ped.PedFecha));
            printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawText("Monto Pedido: " + myPed.GetMontoPedido(pedSecuencia));
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            printer.DrawText("Cant                  Precio              Total");
            printer.DrawLine();


            double total = 0, subtotal = 0, totalItbis = 0;

            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {

                if (det.ProDescripcion.Length > 30)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 26);
                }

                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                var cantidad = ((det.PedCantidadDetalle > 0 && det.ProUnidades > 0 ? (double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) : 0) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;
                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                totalItbis += (det.PedPrecio * 0.18) * cantidad;

                printer.DrawText(det.ProID.ToString().Trim() + "-" + det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString() +
                det.PedPrecio.ToString("C2").PadLeft(27) + (det.PedPrecio * cantidad).ToString("C2").PadLeft(19));
            }

            printer.DrawLine();
            printer.DrawText("Nota: Los precios mostrados son precios de ");
            printer.DrawText("      Referencia");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado).Count.ToString());
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 9: Movilbusiness " + Functions.AppVersion);
            printer.Print();

        }

        private void Formato38(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("You don't have the printer configured.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("Sales Order details not found.");
            }

            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SALES ORDER");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Customer: " + ped.CliNombre, 48);
            printer.DrawText("Code: " + ped.CliCodigo);
            printer.DrawText("Street: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Date: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Order #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawText("Terms: " + ped.ConDescripcion);
            printer.DrawLine();
            printer.DrawText("Description");
            //printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawText("Quantity       Code              Price");
            printer.DrawLine();

            if (DS_RepresentantesParametros.GetInstance().GetDescuentoxPrecioNegociado())
            {
                foreach (var det in myPed.GetDetalleBySecuenciaConPrecioNegociado(pedSecuencia, pedidoConfirmado))
                {
                    if (DS_RepresentantesParametros.GetInstance().GetParPedidosEditarPrecioNegconItebis())
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + Math.Round(det.PedPrecio * ((det.PedItbis / 100) + 1), 2).ToString("C2").PadLeft(12));
                    }
                    else
                    {
                        printer.DrawText(det.ProDescripcion, 48);
                        printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadLeft(12));
                    }

                }
            }
            else
            {
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {
                    printer.DrawText(det.ProDescripcion, 48);
                    printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadRight(12));
                }
            }


            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Customer's signature");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Seller: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Cell phone number: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Phone  number: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Sales Order Format 38: MovilBusiness v" + Functions.AppVersion);
            printer.Print();

        }

        private void Formato3(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            //printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd-MM-yyyy hh:mm tt") : ped.PedFecha));

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("O R D E N  D E  P E D I D O   " + "No." + ped.PedSecuencia);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            if (ped.CliNombre.Length > 28)
            {
                ped.CliNombre = ped.CliNombre.Substring(0, 27);
            }
            printer.DrawText("Cliente          : " + ped.CliNombre, 48);
            printer.DrawText("RNC              : " + ped.CliRnc, 48);
            printer.DrawText("Codigo           : " + ped.CliCodigo);
            printer.DrawText("Direccion entrega: " + ped.CliCalle, 45);
            printer.DrawText("Fecha            : " + (fechaValida ? fecha.ToString("dd - MM - yyyy hh: mm tt") : ped.PedFecha));
            printer.DrawText("Condicion de pago: " + ped.ConDescripcion);
            printer.DrawText("");
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion");
            printer.DrawText("Cant.   Codigo        Precio             Desc.");
            printer.DrawText("Itbis            Monto Itbis          Subtotal");
            //                Caj/Unid  Factura       Lote         Fecha
            printer.Bold = false;
            printer.DrawLine();

            double SubTotalGeneral = 0.0;
            double DescuentoTotal = 0.0;
            double TotalItbisTotal = 0.0;
            double TotalGeneral = 0.0;
            bool existeoferta = false;

            foreach (var det in myPed.GetDetalleBySecuenciaFeltrex(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProDescripcion.Length > 47)
                {
                    det.ProDescripcion = det.ProDescripcion.Substring(0, 47);
                }

                if (!det.PedIndicadorOferta)
                    printer.DrawText(det.ProDescripcion, 48);
                else if (!existeoferta && det.PedIndicadorOferta)
                    existeoferta = true;

                double PrecioBruto = 0.0;
                double DescUnitario = 0.0;
                double SubTotal = 0.0;
                double itbisUnitario = 0.0;
                var descuento = det.PedDescuento.ToString("C2") + " (" + det.PedDesPorciento + "%)";
                var cantidad = det.PedCantidad.ToString();
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);
                if (det.PedCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + det.PedCantidadDetalle;
                }
                if (det.PedCantidadDetalle > 0)
                {
                    PrecioBruto = det.PedPrecio / det.ProUnidades * det.PedCantidadDetalle;
                    DescUnitario = (det.PedDescuento / det.ProUnidades) * det.PedCantidadDetalle;
                    SubTotal = ((det.PedPrecio / det.ProUnidades) * det.PedCantidadDetalle) - DescUnitario;

                }
                else
                {
                    PrecioBruto = det.PedPrecio * det.PedCantidad;
                    SubTotal = PrecioBruto - (det.PedDescuento * det.PedCantidad);
                    DescUnitario = det.PedDescuento * det.PedCantidad;
                }
                itbisUnitario = SubTotal * (det.PedItbis / 100);
                SubTotal += itbisUnitario;

                //Totales Abajo
                SubTotalGeneral += SubTotal;
                DescuentoTotal += DescUnitario;
                TotalItbisTotal += itbisUnitario;

                if (det.PedIndicadorOferta)
                {
                    printer.DrawText(cantidad.PadRight(8) + "         ".PadRight(9) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(16) + " OF");
                } else
                {
                    printer.DrawText(cantidad.PadRight(8) + det.ProCodigo.Trim().PadRight(9) + det.PedPrecio.ToString("C2").PadLeft(11) + descuento.PadLeft(18));
                    printer.DrawText((det.PedItbis.ToString() + "%").PadRight(10) + itbis.ToString("C2").PadLeft(18) + SubTotal.ToString("C2").PadLeft(18), 49);
                }
                //printer.DrawText("");

            }

            TotalGeneral = SubTotalGeneral + TotalItbisTotal;
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("SKU: " + ped.PedTotal);
            printer.DrawText("");
            printer.DrawText("SubTotal:".PadRight(12) + (SubTotalGeneral - TotalItbisTotal).ToString("C2").PadLeft(12) + " " + "Descuento:".PadRight(10) + DescuentoTotal.ToString("C2").PadLeft(11));
            printer.DrawText("Total itbis:".PadRight(12) + TotalItbisTotal.ToString("C2").PadLeft(12) + " " + "Total:".PadRight(10) + SubTotalGeneral.ToString("C2").PadLeft(11));
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
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
            if (existeoferta)
            {
                printer.DrawText("LEYENDA");
                printer.DrawText("OF=OFERTA");
            }
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Formato pedidos 3: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        #region Formato con Descuento General
        private void Formato41(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuenciaConTotales(pedSecuencia, pedidoConfirmado);
            var visipres = myvisit.GetClientePresentacion(ped.VisSecuencia);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            switch (ped.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Cliente: " + ped.CliNombre, 48);
                    printer.DrawText("Codigo: " + ped.CliCodigo);
                    printer.DrawText("Calle: " + ped.CliCalle, 45);
                    break;
            }

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad    Codigo        Precio         Total");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuentoGeneral = 0, fleteTotal = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbisConDescuentoGeneral = ((det.PedPrecio - det.PedDescuento) - ((det.PedPrecio - det.PedDescuento) * (ped.PedPorCientoDsctoGlobal / 100))) * (det.PedItbis / 100);
                var precioConDescuento = (det.PedPrecio - det.PedDescuento);
                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                var totalLinea = precioConDescuento * cantidad;

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.ProCodigo.Trim().PadRight(14) + precioConDescuento.ToString("N2").PadRight(13) + totalLinea.ToString("N2").PadRight(7));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }
                subtotal += totalLinea;
                //totalItbis += (itbisConDescuentoGeneral * cantidad);

                if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                {
                    fleteTotal += det.PedFlete;
                }
            }

            //descuentoGeneral = ped.PedMontoDsctoGlobal;
            //total = (subtotal- descuentoGeneral) + totalItbis;
            total = ped.PedMontoTotal;
            totalItbis = ped.PedMontoITBIS;
            descuentoGeneral = ped.PedMontoDsctoGlobal;

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + descuentoGeneral.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
            {
                printer.DrawText("Total Flete:" + fleteTotal.ToString("N2").PadLeft(15));
                printer.TextAlign = Justification.RIGHT;
            }
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 41: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
        #endregion

        #region Formato AJFA - 2 pulgadas
        private void Formato42(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            //printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ORDEN DE PEDIDO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("Cliente: " + ped.CliNombre, 48);
            printer.DrawText("Codigo: " + ped.CliCodigo);
            printer.DrawText("Calle: " + ped.CliCalle, 45);

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawText("--------------------------------");
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad Codigo          Precio");
            printer.DrawText("--------------------------------");

            double total = 0, subtotal = 0, totalItbis = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion);
                printer.DrawText(det.PedCantidad.ToString().PadRight(8) + det.ProCodigo.Trim().PadRight(10) + det.PedPrecio.ToString("N2").PadLeft(12));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;

                totalItbis += (itbis * cantidad);
            }

            printer.DrawText("--------------------------------");
            printer.TextAlign = Justification.RIGHT;
            printer.Bold = true;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("--------------------------------");
            printer.Bold = false;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 31);
            printer.DrawText("");
            printer.DrawText("Firma cliente: ");
            printer.DrawText("--------------------------------");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("SKU:  2");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 42: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
        #endregion


        #region Formato 2 pulgadas - CPCL

            private void Formato4(int pedSecuencia, bool pedidoConfirmado)
            {
                if (printer == null || !printer.IsConnectionAvailable)
                {
                    throw new Exception("No tienes la impresora configurada.");
                }

                printer.PrintEmpresa(length1: 31, length2: 31);
                Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);

                if (ped == null)
                {
                    throw new Exception("No se encontraron los datos del pedido");
                }

                printer.DrawText("");
                printer.Font = PrinterFont.TITLE;
                printer.Bold = true;
                printer.DrawText("");
                printer.DrawText("ORDEN DE PEDIDO");
                printer.Bold = false;
                printer.Font = PrinterFont.BODY;
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("Cliente: " + ped.CliNombre, 31);
                printer.DrawText("Codigo: " + ped.CliCodigo);
                printer.DrawText("Calle: " + ped.CliCalle, 31
                    );

                var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

                printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
                printer.DrawText("Urb: " + ped.CliUrbanizacion, 31);
                printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
                printer.DrawText("-------------------------------");
                printer.DrawText("Codigo - Descripcion");
                printer.DrawText("Cantidad      Precio      Itbis");
                printer.DrawText("-------------------------------");

                int cant = 0;
                double total = 0, subtotal = 0, totalItbis = 0;
                foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
                {
                    var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                    printer.DrawText(det.ProCodigo.Trim() + " - " + det.ProDescripcion, 31);
                    printer.DrawText(det.PedCantidad.ToString().PadRight(8) + det.PedPrecio.ToString("N2").PadLeft(12) + itbis.ToString("N2").PadLeft(11));

                    if (det.ProUnidades == 0)
                    {
                        det.ProUnidades = 1;
                    }

                    var cantidad = ((det.PedCantidadDetalle / det.ProUnidades) + det.PedCantidad);

                    subtotal += det.PedPrecio * cantidad;

                    total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;

                    totalItbis += (itbis * cantidad);
                    cant++;
                }

                printer.DrawText("-------------------------------");
                printer.TextAlign = Justification.LEFT;
                printer.Bold = true;
                printer.DrawText("Subtotal   :    " + subtotal.ToString("N2").PadLeft(15));
                printer.DrawText("Total Itbis:    " + totalItbis.ToString("N2").PadLeft(15));
                printer.DrawText("Total      :    " + total.ToString("N2").PadLeft(15));
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("-------------------------------");
                printer.Bold = false;
                printer.DrawText("Nota: los precios mostrados son precios de referencia", 31);
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("------------------------------");
                printer.DrawText("Firma del cliente");
                printer.TextAlign = Justification.LEFT;
                printer.DrawText("");
                printer.DrawText("SKU:  " + cant);
                printer.DrawText("");

                printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.DrawText("Formato pedidos 4: movilbusiness " + Functions.AppVersion);
                printer.DrawText("");
                printer.Print();
            }

        #endregion

        #region Agroproductores

        private void Formato15(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuencia(pedSecuencia, pedidoConfirmado);
            var visipres = myvisit.GetClientePresentacion(ped.VisSecuencia);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            switch (ped.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.DrawText("");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.DrawText("");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Cliente: " + ped.CliNombre, 48);
                    printer.DrawText("Codigo: " + ped.CliCodigo);
                    printer.DrawText("Calle: " + ped.CliCalle, 45);
                    break;
            }

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            //  printer.DrawText("Urb: " + ped.CliUrbanizacion);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Descripcion");
            printer.DrawText("Cantidad       Codigo            Precio");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuento = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                var itbis = (det.PedPrecio - det.PedDescuento) * (det.PedItbis / 100);

                printer.DrawText(det.ProDescripcion, 48);
                printer.DrawText(det.PedCantidad.ToString().PadRight(15) + det.ProCodigo.Trim().PadRight(18) + det.PedPrecio.ToString("C2").PadRight(12));

                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);

                subtotal += det.PedPrecio * cantidad;

                total += (itbis + (det.PedPrecio - det.PedDescuento)) * cantidad;
                descuento += det.PedDescuento * cantidad;
                totalItbis += (itbis * cantidad);
            }

            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + descuento.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(1, 15) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(1, 15), 45);
                printer.DrawText("");
            }
            else
            {
                printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 15: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }

        #endregion

        private void Formato43(int pedSecuencia, bool pedidoConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();
            Pedidos ped = myPed.GetBySecuenciaConTotales(pedSecuencia, pedidoConfirmado);
            var visipres = myvisit.GetClientePresentacion(ped.VisSecuencia);

            if (ped == null)
            {
                throw new Exception("No se encontraron los datos del pedido");
            }

            switch (ped.CliIndicadorPresentacion)
            {
                case 1:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Nombre: " + visipres.VisNombre, 48);
                    printer.DrawText("Propietario: " + visipres.VisPropietario);
                    printer.DrawText("Contact: " + visipres.VisContacto, 45);
                    printer.DrawText("Email: " + visipres.VisEmail, 45);
                    printer.DrawText("Calle: " + visipres.VisCalle, 45);
                    printer.DrawText("Ciudad: " + visipres.VisCiudad, 45);
                    printer.DrawText("Telefono: " + visipres.VisTelefono, 45);
                    printer.DrawText("RNC: " + visipres.VisRNC, 45);
                    break;

                default:

                    printer.DrawText("");
                    printer.Font = PrinterFont.TITLE;
                    printer.Bold = true;
                    printer.DrawText("");
                    printer.TextAlign = Justification.CENTER;
                    printer.DrawText("ORDEN DE PEDIDO");
                    printer.Bold = false;
                    printer.Font = PrinterFont.BODY;
                    printer.DrawText("");
                    printer.TextAlign = Justification.LEFT;
                    printer.DrawText("Cliente: " + ped.CliNombre, 48);
                    printer.DrawText("Codigo: " + ped.CliCodigo);
                    printer.DrawText("Calle: " + ped.CliCalle, 45);
                    break;
            }

            var fechaValida = DateTime.TryParse(ped.PedFecha, out DateTime fecha);

            printer.DrawText("Fecha: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : ped.PedFecha));
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + ped.PedSecuencia);
            printer.DrawLine();
            printer.DrawText("Codigo-Descripcion");
            printer.DrawText("Cantidad    Precio        %Desc.   Total Linea");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuentoGeneral = 0, descuentoUnitario = 0, fleteTotal = 0;
            foreach (var det in myPed.GetDetalleBySecuencia(pedSecuencia, pedidoConfirmado))
            {
                if (det.ProUnidades == 0)
                {
                    det.ProUnidades = 1;
                }

                var itbisConDescuentoGeneral = ((det.PedPrecio - det.PedDescuento) - ((det.PedPrecio - det.PedDescuento) * (ped.PedPorCientoDsctoGlobal / 100))) * (det.PedItbis / 100);
                var precioConDescuento = (det.PedPrecio - det.PedDescuento);
                var cantidad = ((double.Parse(det.PedCantidadDetalle.ToString()) / det.ProUnidades) + det.PedCantidad);
                var totalLinea = Math.Round(precioConDescuento * cantidad,2);

                printer.DrawText(det.ProCodigo.Trim() + '-' + det.ProDescripcion);
                if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo() && ped.MonCodigo== "USD")
                {
                    printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.PedPrecio.ToString("N4").PadRight(14) + (det.PedDesPorciento.ToString() + "%").ToString().PadRight(11) + totalLinea.ToString("N2").PadLeft(6));
                }
                else
                {
                    printer.DrawText(det.PedCantidad.ToString().PadRight(12) + det.PedPrecio.ToString("N2").PadRight(14) + (det.PedDesPorciento.ToString() + "%").ToString().PadRight(11) + totalLinea.ToString("N2").PadLeft(6));
                }

                subtotal += Math.Round(det.PedPrecio * cantidad, 2);
                descuentoUnitario += Math.Round(det.PedDescuento * cantidad, 2);
                if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
                {
                    fleteTotal += det.PedFlete;
                }
            }
            
            totalItbis = ped.PedMontoITBIS;
            descuentoGeneral = ped.PedMontoDsctoGlobal + Math.Round(descuentoUnitario, 2);
            total = subtotal - descuentoGeneral + totalItbis;
            printer.DrawLine();
            printer.Bold = true;

            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Subtotal   :" + subtotal.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Descuento  :" + descuentoGeneral.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            printer.DrawText("Total Itbis:" + totalItbis.ToString("N2").PadLeft(15));
            printer.TextAlign = Justification.RIGHT;
            if (DS_RepresentantesParametros.GetInstance().GetCalculaFlete())
            {
                printer.DrawText("Total Flete:" + fleteTotal.ToString("N2").PadLeft(15));
                printer.TextAlign = Justification.RIGHT;
            }
            printer.DrawText("Total      :" + total.ToString("N2").PadLeft(15));
            printer.DrawLine();
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Nota: los precios mostrados son precios de referencia", 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("SKU:  2");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato pedidos 43: movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
    }
}



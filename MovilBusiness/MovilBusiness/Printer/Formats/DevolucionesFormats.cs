using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Printer;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.printers.formats
{
    public class DevolucionesFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Devoluciones myDev;
        private DS_Clientes myCli;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        private DS_RepresentantesParametros myparametro;
        private DS_UsosMultiples usosMultiples;

        public DevolucionesFormats(DS_Devoluciones myDev)
        {
            this.myDev = myDev;
            myCli = new DS_Clientes();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
            myparametro = DS_RepresentantesParametros.GetInstance();
            usosMultiples = new DS_UsosMultiples();

        }

        public void Print(int devSecuencia, PrinterManager printer, string rowguid = "") { Print(devSecuencia, false, printer, rowguid); }
        public void Print(int devSecuencia, bool devolucionConfirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;
            
            //Copias = copias;
            switch (myparametro.GetFormatoImpresionDevoluciones())
            {
                case 1:
                default:
                    Formato1(devSecuencia, devolucionConfirmado);
                    break;
                case 2: //UyC
                    Formato2(devSecuencia, devolucionConfirmado);
                    break;
                case 3: //leBetances
                    Formato3(devSecuencia, devolucionConfirmado);
                    break;
                case 4: //Disfarmacos
                    Formato4(devSecuencia, devolucionConfirmado);
                    break;
                case 5: //PERAVIA INDUSTRIAL
                    Formato5(devSecuencia, devolucionConfirmado);
                    break;
                case 6: //Feltrex
                    Formato6(devSecuencia, devolucionConfirmado);
                    break;
                case 7://Sued
                    Formato7(devSecuencia, devolucionConfirmado);
                    break;
                case 8://dinafa
                    Formato8(devSecuencia, devolucionConfirmado);
                    break;
                case 9://CALOSA
                    Formato9(devSecuencia, devolucionConfirmado);
                    break;
                case 10: //No definido
                    Formato10(devSecuencia, devolucionConfirmado);
                    break;
                case 11: //LAM
                    Formato11(devSecuencia, devolucionConfirmado);
                    break;
                case 12: //C. Federico Gomez
                    Formato12(devSecuencia, devolucionConfirmado);
                    break;
                case 13: 
                    Formato13(devSecuencia, devolucionConfirmado);
                    break;
                case 14:
                    Formato14(devSecuencia, devolucionConfirmado);
                    break;
                case 15: // Agroproductores
                    Formato15(devSecuencia, devolucionConfirmado);
                    break;
                case 16: // Pharmatech Dental
                    Formato16(devSecuencia, devolucionConfirmado);
                    break;
                case 24: 
                    Formato24(devSecuencia, devolucionConfirmado);
                    break;
                case 35://Formato 2 Pulgadas
                    Formato35(devSecuencia,devolucionConfirmado);
                    break;
                case 32://Rec - canastos
                    Formato32(devSecuencia,devolucionConfirmado);
                    break;
                case 33://GASSO
                    Formato33(devSecuencia,devolucionConfirmado);
                    break;
                case 34:
                    Formato34(devSecuencia, devolucionConfirmado);
                    break;
                case 41:  // Formato SAP con Descuento General - No Cambiar
                    Formato41(devSecuencia, devolucionConfirmado);
                    break;
                case 43:  // ANDOSA
                    Formato43(devSecuencia, devolucionConfirmado);
                    break;
            }
        }


        private void Formato9(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);

      
            //var motivos = myDev.GetMotivosDevolucionFromDetalle(devSecuencia, devolucionConfirmado);
            var list = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado);

            var motivo = list.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.MotDescripcion));
            if (motivo != null && !string.IsNullOrWhiteSpace(motivo.MotDescripcion))
            {
                printer.DrawText("Motivo: " + motivo.MotDescripcion, 48); 
            }

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");            
            printer.DrawText("Caj/Unid      Factura                Fecha");
            //printer.DrawText("Motivo");
            printer.DrawLine();

           
            foreach (DevolucionesDetalle dev in list)
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = true;                
                printer.DrawText(lblCantidad.PadRight(16) + dev.DevDocumento.PadRight(16) +   dev.DevFecha.PadRight(16), 49);
                printer.Bold = false;
                 
            }


            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 9: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato1(int devSecuencia, bool devolucionConfirmado)
        {
            if(printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if(devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if(cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();           
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;           
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: "+ cliente.CliNombre, 45);
            printer.DrawText("Codigo: "+cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            if (devolucion.DevCintillo != null)
            {
                printer.DrawText("Cintillo:" +devolucion.DevCintillo);
            }
            printer.DrawText("");

            
          /* if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
            {
                printer.DrawText("Motivo: " + devolucion.Motivo);
            }*/

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Motivo");
            printer.DrawLine();

            foreach(DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if(descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if(dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = true;
                printer.DrawText(lblCantidad.PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.PadRight(13) + dev.DevFecha.PadRight(11), 49);
                printer.Bold = false;
                if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                { 
                  printer.DrawText(dev.MotDescripcion.PadLeft(12) + (dev.DevIndicadorOferta ? "(oferta)".PadLeft(12) : ""));
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");         
            
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Certifico que los productos devueltos");
            printer.DrawText("mantuvieron las condiciones de almacenamiento");
            printer.DrawText("apropiadas para los productos (20-25 oC).");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato2(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.DrawText("");


            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);
            if (DS_RepresentantesParametros.GetInstance().GetParDevolucionesNumeroDocumento())
            {
                printer.DrawText("No. Documento: " + devolucion.DevReferencia);
            }
            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: "+ factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
          //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawLine();
            var subTotal = 0.0;
            var totalGeneral = 0.0;
            var totalItbis = 0.0;
            var totalDesc = 0.0;

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                var itbis = (dev.DevPrecio - dev.DevDescuento) * (dev.DevItebis / 100);
                var total = (itbis + (dev.DevPrecio - dev.DevDescuento)) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);

                totalItbis += itbis;
                totalGeneral += total;
                subTotal += (dev.DevPrecio - dev.DevDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalDesc += (dev.DevDescuento * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad));

                printer.DrawText(lblCantidad.PadRight(9) + dev.DevPrecio.ToString("N2").PadRight(9) + dev.DevDescuento.ToString("N2").PadRight(12) + itbis.ToString("N2").PadRight(9) + total.ToString("N2").PadRight(10), 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subTotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + totalDesc.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + totalGeneral.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));

            
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 48);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");          
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 2: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        private void Formato3(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa(Notbold:true);
            printer.DrawText("");
            //printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
          
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            //printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawLine();

            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);         
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");
/*
            var motivos = myDev.GetMotivosDevolucionFromDetalle(devSecuencia, devolucionConfirmado);

            printer.DrawText("Motivo: " + motivos, 48);
*/
            printer.DrawLine();
            printer.DrawText("Descripcion producto");
            //printer.DrawText("Codigo     Caj/Unid Factura   Lote     Fecha");
            printer.DrawText("Codigo   Caj/Unid Factura   Lote       Fecha");
            printer.DrawText("Motivo");
            printer.DrawLine();           

            var list = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado, true);

            if (list != null && list.Count > 0)
            {
                foreach (DevolucionesDetalle dev in list)
                {
                    var descripcion = dev.ProDescripcion;
                    if (descripcion.Length > 48)
                    {
                        descripcion = descripcion.Substring(0, 48);
                    }
                    printer.DrawText(descripcion);

                    string lblCantidad = dev.DevCantidad.ToString();

                    if (dev.DevCantidadDetalle > 0)
                    {
                        lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                    }
                    printer.Bold = true;
                    //printer.DrawText(dev.ProCodigo.PadRight(11) + lblCantidad.PadRight(9) + dev.DevDocumento.PadRight(10) + dev.DevLote.PadRight(9) + dev.DevFecha.PadRight(7), 48);
                    //printer.DrawText(dev.ProCodigo.PadRight(10) + lblCantidad.PadRight(8) + dev.DevDocumento.PadRight(10) + dev.DevLote.PadRight(11) + dev.DevFecha.PadRight(7), 48);
                    printer.DrawText(dev.ProCodigo.PadRight(10) + lblCantidad.PadRight(6) + dev.DevDocumento.PadRight(10) + dev.DevLote.PadRight(14) + dev.DevFecha.PadRight(7), 48);
                    
                    printer.Bold = false;
                    if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                    {
                        printer.DrawText(dev.MotDescripcion);
                    }
                    //printer.Font = PrinterFont.FOOTER;
                    //     printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
           // printer.DrawText("");
           // printer.DrawText("");
           // printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("_________________________________");
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            //printer.DrawText("");
           // printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
           // printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + (list != null ? list.Count : 0));
           // printer.DrawText("");
          //  printer.DrawText("");
          //  printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato devoluciones 3: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato4(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha Devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Motivo");
            printer.DrawLine();

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = true;
                printer.DrawText(dev.DevCantidad.ToString().PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.PadRight(13) + dev.DevFecha.PadRight(11), 49);
                printer.Bold = false;
                if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                {
                    printer.DrawText(dev.MotDescripcion);
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 4: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato5(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawLine();
            var subTotal = 0.0;
            var totalGeneral = 0.0;
            var totalItbis = 0.0;
            var totalDesc = 0.0;

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                var itbis = (dev.DevPrecio - dev.DevDescuento) * (dev.DevItebis / 100);
                var total = (itbis + (dev.DevPrecio - dev.DevDescuento)) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);

                totalItbis += itbis;
                totalGeneral += total;
                subTotal += (dev.DevPrecio - dev.DevDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalDesc += (dev.DevDescuento * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad));

                printer.DrawText(lblCantidad.PadRight(9) + dev.DevPrecio.ToString("N2").PadRight(9) + dev.DevDescuento.ToString("N2").PadRight(12) + itbis.ToString("N2").PadRight(9) + total.ToString("N2").PadRight(10), 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subTotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + totalDesc.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + totalGeneral.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 48);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 5: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }


        private void Formato32(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawLine();
            var subTotal = 0.0;
            var totalGeneral = 0.0;
            var totalItbis = 0.0;
            var totalDesc = 0.0;

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                var itbis = (dev.DevPrecio - dev.DevDescuento) * (dev.DevItebis / 100);
                var total = (itbis + (dev.DevPrecio - dev.DevDescuento)) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);

                totalItbis += itbis;
                totalGeneral += total;
                subTotal += (dev.DevPrecio - dev.DevDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalDesc += (dev.DevDescuento * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad));

                printer.DrawText(lblCantidad.PadRight(9) + dev.DevPrecio.ToString("N2").PadRight(9) + dev.DevDescuento.ToString("N2").PadRight(12) + itbis.ToString("N2").PadRight(9) + total.ToString("N2").PadRight(10), 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subTotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + totalDesc.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + totalGeneral.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del distribuidor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 48);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 32: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }
        private void Formato6(int devSecuencia, bool devolucionConfirmado)
        {
            int counter = 0;
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa(Notbold: true);
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha  : " + devolucion.DevFecha);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawLine();
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            if (!string.IsNullOrEmpty(devolucion.DevCintillo) && !devolucion.DevCintillo.Equals(""))
            {
                printer.DrawText("Cintillo:" + devolucion.DevCintillo);
            }
            //printer.DrawText("");


            /* if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
              {
                  printer.DrawText("Motivo: " + devolucion.Motivo);
              }*/

            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            //printer.DrawText("Descripcion Producto");
            //printer.DrawText("Codigo     Caj/Unid   Factura Lote        Fecha");
            printer.DrawText("Comentario");
            printer.DrawText("-----------------------------------------------");

            List<string> motivos = new List<string>();
            foreach(DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                if(motivos.Count == 0)
                {
                    motivos.Add(dev.MotDescripcion);
                }
                else
                {
                    if(motivos[motivos.Count-1] != dev.MotDescripcion)
                    {
                        motivos.Add(dev.MotDescripcion);
                    }
                }
            }

            foreach (var mot in motivos)
            {
                printer.Font = PrinterFont.TITLE;
                printer.DrawText(mot);
                printer.Font = PrinterFont.BODY;

                foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado,true))
                {
                    if (mot == dev.MotDescripcion)
                    {

                        var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                        if (descr.Length > 48)
                        {
                            descr = descr.Substring(0, 48);
                        }

                        printer.DrawText(descr, 48);

                        string lblCantidad = dev.DevCantidad.ToString();

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }
                        counter++;
                        printer.DrawText(lblCantidad.PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.ToString().Trim().PadRight(13) + dev.DevFecha.ToString().Trim().PadLeft(7), 49);
                        if (!string.IsNullOrWhiteSpace(dev.DevComentario))
                        {
                            if (dev.DevComentario.Length > 47)
                            {
                                dev.DevComentario = dev.DevComentario.Substring(0, 47);
                            }
                            printer.DrawText(dev.DevComentario);
                        }
                        printer.Font = PrinterFont.BODY;
                        printer.DrawText("");
                    }
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            printer.DrawText("Devolucion provisional sujeto a revision y");
            printer.DrawText("modificacion segun la politica");
            printer.DrawText("");
            printer.DrawText("Comentario: " + devolucion.Motivo);
            printer.DrawText("");
            printer.DrawText("Items: " + counter);
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 6: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato7(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaSued(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DEVOLUCION");
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText(devolucion.DevCantidadImpresion < 1 ? "O R I G I N A L" : "C O P I A  No." + devolucion.DevCantidadImpresion);           
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Orden #:: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + devSecuencia, "H");
            printer.DrawLine();
            printer.DrawText(" ");
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

       
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion Producto");
            printer.DrawText("Motivo  Caj/Unid  Factura     Lote      Fecha");
         
            printer.DrawLine();

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = true;
                printer.DrawText(dev.MotReferencia.PadRight(9)+" "+lblCantidad.PadRight(9) + dev.DevDocumento.PadRight(9) + dev.DevLote.PadRight(9) + dev.DevFecha.PadRight(9)+ (dev.DevIndicadorOferta?"*":""));
                printer.Bold = false;
                printer.DrawText("");

            }

            printer.DrawLine();
         
            printer.DrawText("Nota: Los productos marcados con (*) son");
            printer.DrawText("      ofertas");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("Firma del Cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
           
            printer.DrawText("Items: " + myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado).Count);
            printer.DrawText("Motivos:");
            foreach (MotivosDevolucion devMot in myDev.GetDevolucionDetalleMotivo(devSecuencia, devolucionConfirmado))
            {
                printer.DrawText(devMot.MotID + " = " + devMot.MotDescripcion);
            }
       
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
        
          
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 7: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            Hash devImp = new Hash(devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones");
            devImp.Add("DevCantidadImpresion", devolucion.DevCantidadImpresion + 1);
            devImp.ExecuteUpdate("DevSecuencia = " + devolucion.DevSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }
        private void Formato10(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.Bold = false;
            //printer.DrawText("");
            //printer.DrawText(devolucion.DevCantidadImpresion < 1 ? "O R I G I N A L" : "C O P I A  No." + devolucion.DevCantidadImpresion);
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha Devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;

            printer.DrawLine();
            printer.DrawText("Referencia     Descripcion                  Cnt");
            printer.DrawLine();

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var Ref = dev.ProCodigo;
                var descripcion = dev.ProDescripcion + " - " + dev.DevLote.Trim() + "-" + dev.DevFecha;
                var cnt = dev.DevCantidad + "+" + dev.DevCantidadDetalle;

                var cantLineas = Ref.Length / 15;

                if (((double)Ref.Length % 15.0) > 0.0)
                {
                    cantLineas += 1;
                }

                if (((double)descripcion.Length / 25.0) > cantLineas)
                {
                    cantLineas = (descripcion.Length / 25);

                    if (((double)descripcion.Length % 25.0) > 0.0)
                    {
                        cantLineas += 1;
                    }
                }

                if ((double)(cnt.Length / 8.0) > cantLineas)
                {
                    cantLineas = cnt.Length / 8;
                    if (((double)cnt.Length % 8.0) > 0.0)
                    {
                        cantLineas += 1;
                    }
                }

                var rows = new List<ThreeColumns>();

                for (int i = 0; i < cantLineas; i++)
                {
                    rows.Add(new ThreeColumns() { Column1 = "", Column2 = "", Column3 = "" });
                }

                var index = 0;
                var listIndex = 0;

                if (Ref.Length > 15)
                {
                    while (index < Ref.Length)
                    {
                        var restante = Ref.Substring(index).Length;

                        rows[listIndex].Column1 = Ref.Substring(index, restante > 15 ? 15 : restante);

                        index += (restante > 15 ? 15 : restante);

                        listIndex += 1;
                    }
                }
                else
                {
                    rows[0].Column1 = Ref;
                }

                listIndex = 0;
                index = 0;
                if (descripcion.Length > 25)
                {
                    while (index < descripcion.Length)
                    {
                        var restante = descripcion.Substring(index).Length;

                        rows[listIndex].Column2 = descripcion.Substring(index, restante > 25 ? 25 : restante);

                        index += (restante > 25 ? 25 : restante);

                        listIndex += 1;
                    }
                }
                else
                {
                    rows[0].Column2 = descripcion;
                }

                listIndex = 0;
                index = 0;
                if (cnt.Length > 8)
                {
                    while (index < cnt.Length)
                    {
                        var restante = cnt.Substring(index).Length;

                        rows[listIndex].Column3 = cnt.Substring(index, restante > 8 ? 8 : restante);

                        index += (restante > 8 ? 8 : restante);

                        listIndex += 1;
                    }
                }
                else
                {
                    rows[0].Column3 = cnt;
                }

                foreach (var row in rows)
                {
                    printer.DrawText(row.Column1.Trim().PadRight(15) + row.Column2.Trim().PadRight(25) + row.Column3.Trim().PadLeft(7));
                }
            }

            printer.DrawLine();

            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("Firma del Cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");

            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 8: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            var devImp = new Hash(devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones");
            devImp.Add("DevCantidadImpresion", devolucion.DevCantidadImpresion + 1);
            devImp.ExecuteUpdate("DevSecuencia = " + devolucion.DevSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }
        private void Formato8(int devSecuencia, bool devolucionConfirmado)
        {
            int counter = 0;
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa(Notbold: true);
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha  : " + devolucion.DevFecha);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawLine();
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            //printer.DrawText("");


            /* if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
              {
                  printer.DrawText("Motivo: " + devolucion.Motivo);
              }*/

            printer.DrawText("-----------------------------------------------");
            printer.DrawText("Referencia - Descripcion");
            printer.DrawText("Cant      Lote      Fecha fact.     Fecha venc.");
            printer.DrawText("-----------------------------------------------");

            List<string> motivos = new List<string>();
            bool repeticion = false;
            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                if (motivos.Count == 0)
                {
                    motivos.Add(dev.MotDescripcion);
                }
                else
                {
                   foreach(var n in motivos)
                    {
                        if(n == dev.MotDescripcion)
                        {
                            repeticion = true;
                        }
                    }

                    if (!repeticion)
                    {
                        motivos.Add(dev.MotDescripcion);
                    }

                    repeticion = false;
                }
            }

            foreach (var mot in motivos)
            {
                printer.Bold = true;
                printer.DrawText(mot);
                printer.Bold = false;
                //printer.DrawText("");

                foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                {
                    if (dev.DevIndicadorOferta == false)
                    {
                        if (mot == dev.MotDescripcion)
                        {
                            var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                            if (descr.Length > 48)
                            {
                                descr = descr.Substring(0, 47);
                            }

                            double CantidadOferta = myDev.GetDevolucionDetalleCantidadOferta(devSecuencia, dev.DevPosicion);

                            printer.DrawText(descr, 48);

                            string lblCantidad = dev.DevCantidad.ToString();

                            if (dev.DevCantidadDetalle > 0)
                            {
                                lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                            }

                            if(CantidadOferta > 0)
                            {
                                lblCantidad += " + " + CantidadOferta;
                            }
                            counter++;

                            //dev.DevLote = " " + dev.DevLote;

                            printer.DrawText(lblCantidad.PadRight(10) + dev.DevLote.PadRight(10) +  dev.DevDocumento.PadRight(15) + dev.DevFecha.PadLeft(12), 49);
                            printer.DrawText("");
                        }
                    }
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            printer.DrawText("Items: " + counter);
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 8: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato11(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaSued(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa(Notbold:true);
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("D E V O L U C I O N");
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText(devolucion.DevCantidadImpresion < 1 ? "O R I G I N A L" : "C O P I A  No." + devolucion.DevCantidadImpresion);
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText(devolucion.DevFecha.ToString());
            var fechaValidaApertura = DateTime.TryParse(devolucion.DevFecha, out DateTime fecha1);
            //String fecha = "";
            //if (!String.IsNullOrEmpty(fecha1.ToString()))
            //{
            //    fecha = fecha1.ToString().Replace('/', '-');
            //    fecha = fecha1.ToString("dd-MM-yyyy hh:mm tt");
            //}
            printer.DrawText(fechaValidaApertura ? fecha1.ToString().PadRight(27) : devolucion.DevFecha.PadRight(27) + ("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia).PadLeft(20));
            //printer.DrawText("Orden #:: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + devSecuencia, "H");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawLine();
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);        
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");


            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion Producto");
            printer.DrawText("Motivo     Caj/Detalle     Factura    Lote      ");

            printer.DrawLine();

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if (descr.Length > 47)
                {
                    descr = descr.Substring(0, 47);
                }

                printer.DrawText(descr, 47);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += " / " + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = false;
                printer.DrawText(dev.MotReferencia.PadRight(11) + " " + lblCantidad.PadRight(16) + dev.DevDocumento.PadRight(11) + dev.DevLote.PadRight(9) + (dev.DevIndicadorOferta ? "*" : ""));
                printer.Bold = false;
                printer.DrawText("");

            }
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.Bold = false;
            printer.DrawText("Firma del Cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");

            printer.DrawText("Items: " + myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado).Count);
            printer.DrawText("Motivos:");
            foreach (MotivosDevolucion devMot in myDev.GetDevolucionDetalleMotivo(devSecuencia, devolucionConfirmado))
            {
                printer.DrawText(devMot.MotID + " = " + devMot.MotDescripcion);
            }

            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");


            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Version Movilbusiness " + Functions.AppVersion);
            printer.DrawText("Formato devoluciones 11");
            printer.DrawText("");
            printer.Print();
            Hash devImp = new Hash(devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones");
            devImp.Add("DevCantidadImpresion", devolucion.DevCantidadImpresion + 1);
            devImp.ExecuteUpdate("DevSecuencia = " + devolucion.DevSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }


        private void Formato12(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaMang(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(devolucion.DevAccion == 1 ? "DEVOLUCION" : "NOTA DE CAMBIO");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            string recivonot = myparametro.GetParPrefSec() + " - " + devSecuencia.ToString();
            printer.DrawText((devolucion.DevCantidadImpresion == 0 ? "ORIGINAL" : "Copia No. " + devolucion.DevCantidadImpresion) + " - " + recivonot);
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            if (devolucion.DevCintillo != null)
            {
                printer.DrawText("Cintillo:" + devolucion.DevCintillo);
            }
            printer.DrawText("Accion: " + (devolucion.DevAccion == 1 ? "Credito" : "Cambio"));
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Motivo - Lote          Fecha      Caj/Unid");
            printer.DrawLine();

            string motivosDescripcion = "";

            List<DevolucionesDetalle> motivosdev = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado);

            foreach (DevolucionesDetalle dev in motivosdev)
            {
                string lblCantidad = dev.DevCantidad.ToString();
                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                string descr;

                descr = dev.ProCodigo + " - " + (dev.ProDescripcion.Length > 25 ? dev.ProDescripcion.Substring(0, 25) : dev.ProDescripcion) + (dev.DevIndicadorOferta == true ? "  Oferta" : "");

                if (descr.Length > 50)
                {
                    descr = descr.Substring(0, 50);
                }

                printer.DrawText(descr, 48);
                printer.DrawText(dev.MotID + " - " + dev.DevLote.PadLeft(5) + dev.DevFecha.PadLeft(18) + lblCantidad.PadLeft(9));

                if (!motivosDescripcion.Contains(dev.MotDescripcion))
                {
                    motivosDescripcion += dev.MotDescripcion + "|";
                }
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Codigo - Motivo Descripcion");
            printer.DrawLine();

            foreach (var motdev in motivosdev.GroupBy(m => new { m.MotID, m.MotDescripcion }))
            {
                printer.DrawText(motdev.Key.MotID.ToString() + " - " + motdev.Key.MotDescripcion);
            }

            string Condicion = "";
            if (!string.IsNullOrEmpty(devolucion.DevOtrosDatos))
            {
                dynamic devotrosdatos = JsonConvert.DeserializeObject<dynamic>(devolucion.DevOtrosDatos);
                Condicion = devotrosdatos[0].Valor == 1 ? "Fraccionados" : "Originales";
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("Condicion: " + Condicion);
            printer.DrawText("No. Documento: " + devolucion.DevReferencia);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            printer.DrawText("Certifico que los productos devueltos");
            printer.DrawText("mantuvieron las condiciones de almacenamiento");
            printer.DrawText("apropiadas para los productos (20-25 oC).");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 12: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            Hash devolucionUpdate = new Hash(devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones");
            devolucionUpdate.Add("DevCantidadImpresion", devolucion.DevCantidadImpresion + 1);
            devolucionUpdate.ExecuteUpdate("DevSecuencia = " + devSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }
        private void Formato35(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("--------------------------------");

            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            var motivos = myDev.GetMotivosDevolucionFromDetalle(devSecuencia, devolucionConfirmado);

            printer.DrawText("Motivo: " + motivos, 48);

            printer.DrawText("--------------------------------");
            printer.DrawText("Descripcion producto");
            printer.DrawText("Codigo     Caj/Unid Factura   Lote     Fecha");
            printer.DrawText("--------------------------------");

            var list = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado, true);

            if (list != null && list.Count > 0)
            {
                foreach (DevolucionesDetalle dev in list)
                {
                    printer.DrawText(dev.ProDescripcion, 48);

                    string lblCantidad = dev.DevCantidad.ToString();

                    if (dev.DevCantidadDetalle > 0)
                    {
                        lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                    }

                    printer.DrawText(dev.ProCodigo.PadRight(11) + lblCantidad.PadRight(9) + dev.DevDocumento.PadRight(10) + dev.DevLote.PadRight(9) + dev.DevFecha.PadRight(7), 48);
                    printer.Font = PrinterFont.FOOTER;
                    printer.DrawText("");
                    printer.Font = PrinterFont.BODY;
                }
            }

            printer.DrawText("--------------------------------");
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("____________________________");
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("____________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + (list != null ? list.Count : 0));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato devoluciones 35: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
        private void Formato13(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Solicitud de Reclamación");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawBarcode("128", Arguments.CurrentUser.RepCodigo + "-" + devSecuencia, "H");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo.Trim() + " - " + devSecuencia);
            //printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
           // printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Pueblo:" + cliente.cliSector, 45);
            printer.DrawText("Nombre: " + cliente.CliNombre, 45);
            printer.DrawText("");


            if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
            {
                printer.DrawText("Motivo: " + devolucion.Motivo);
            }

            printer.DrawLine();
            //printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote              Fecha");
            printer.DrawText("Tipo de Reclamación    Codigo    Cantida");
            printer.DrawText("Referencia             Factura");
            printer.DrawLine();
            int i = 0;  

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                i++;

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                //printer.DrawText(lblCantidad.PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.PadRight(13) + dev.DevFecha.PadRight(11), 49);
                printer.DrawText(
                    usosMultiples.GetUsoByCodigoGrupo("DEVACCION",
                    codigoUso: dev.DevAccion.ToString()).FirstOrDefault()?
                    .Descripcion.PadLeft(10) + dev.ProCodigo.PadLeft(14)  + lblCantidad.PadLeft(11), 49);

                printer.DrawText(dev.ProDescripcion +"  " + dev.DevDocumento.PadRight(14), 49);

                printer.DrawText("");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Items: " + i);
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 13: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato24(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaMang(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(devolucion.DevAccion == 1 ? "DEVOLUCION": "NOTA DE CAMBIO");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            string recivonot = myparametro.GetParPrefSec() + " - " + devSecuencia.ToString();
            printer.DrawText((devolucion.DevCantidadImpresion == 0 ? "ORIGINAL" : "Copia No. " + devolucion.DevCantidadImpresion) +" - " + recivonot);
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            if (devolucion.DevCintillo != null)
            {
                printer.DrawText("Cintillo:" + devolucion.DevCintillo);
            }
            printer.DrawText("Accion: " + (devolucion.DevAccion == 1? "Credito" : "Cambio"));
            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion - Caj/Unid");
            printer.DrawText("Motivo - Lote            Fecha");
            printer.DrawLine();

            string motivosDescripcion = "";

            List<DevolucionesDetalle> motivosdev = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado);

            foreach (DevolucionesDetalle dev in motivosdev)
            {
                string lblCantidad = dev.DevCantidad.ToString();
                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                string descr;

                if (dev.DevIndicadorOferta == true)
                {
                   descr = dev.ProCodigo + " - " + (dev.ProDescripcion.Length > 25 ? dev.ProDescripcion.Substring(0, 25) : dev.ProDescripcion) + " - " + lblCantidad + "  Oferta";
                }
                else
                {
                   descr = dev.ProCodigo + " - " + (dev.ProDescripcion.Length > 25 ? dev.ProDescripcion.Substring(0, 25) : dev.ProDescripcion) + " - " + lblCantidad;
                }
                    
                if (descr.Length > 50)
                {
                    descr = descr.Substring(0, 50);
                }

                printer.DrawText(descr, 48);
                printer.DrawText(dev.MotID + " - " + dev.DevLote.PadLeft(5) + dev.DevFecha.PadLeft(16));                

                if(!motivosDescripcion.Contains(dev.MotDescripcion))
                {                    
                    motivosDescripcion += dev.MotDescripcion + "|";
                }                
            }
            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("Codigo - Motivo Descripcion");
            printer.DrawLine();

            foreach (var motdev in motivosdev.GroupBy(m => new { m.MotID, m.MotDescripcion }))
            {
                printer.DrawText(motdev.Key.MotID.ToString() + " - " + motdev.Key.MotDescripcion);
            }

            string Condicion = "";
            if (!string.IsNullOrEmpty(devolucion.DevOtrosDatos))
            {
                dynamic devotrosdatos = JsonConvert.DeserializeObject<dynamic>(devolucion.DevOtrosDatos);
                Condicion =  devotrosdatos[0].Valor == 1 ? "Fraccionados" : "Originales";
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("Condicion: " + Condicion);
            printer.DrawText("No. Documento: " + devolucion.DevReferencia);
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            printer.DrawText("Certifico que los productos devueltos");
            printer.DrawText("mantuvieron las condiciones de almacenamiento");
            printer.DrawText("apropiadas para los productos (20-25 oC).");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato devoluciones 24: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
            Hash devolucionUpdate = new Hash(devolucionConfirmado ? "DevolucionesConfirmadas" : "Devoluciones");
            devolucionUpdate.Add("DevCantidadImpresion", devolucion.DevCantidadImpresion + 1);
            devolucionUpdate.ExecuteUpdate("DevSecuencia = " + devSecuencia + " And RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ");
        }

        private void Formato33(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }
            printer.Bold = false;
            printer.TextAlign = Justification.CENTER;
            printer.PrintEmpresa(TitleNotBold: true);
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("D E V O L U C I O N" + (devolucionConfirmado ? "  C O N F I R M A D A" : ""));
            printer.DrawText("");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            var fechaValida = DateTime.TryParse(devolucion.DevFecha, out DateTime fecha);
            printer.DrawText("Fecha: " + (fechaValida? fecha.ToString("dd/MM/yyyy h:mm:ss tt") : devolucion.DevFecha));
            printer.DrawText("");
            printer.DrawText("Moneda: " + devolucion.MonCodigo);
            printer.DrawText("");
            printer.DrawText("Cliente:  " + cliente.CliCodigo + "-" + cliente.CliNombre,46);
            printer.DrawText("          " + cliente.CliCalle, 46);
            printer.DrawText("");
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Sector: " + (Arguments.Values.CurrentSector != null? Arguments.Values.CurrentSector.SecCodigo +" - "+ Arguments.Values.CurrentSector.SecDescripcion : ""));
            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Producto");
            printer.DrawText("Codigo      Cantidad    Factura      Lote");
            printer.DrawText("Motivo                          Devolucion SAP");
            printer.DrawLine();

            int i = 0;
            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuenciaToUseProId(devSecuencia, devolucionConfirmado))
            {
                i++;
                var descr = dev.ProDescripcion;

                if (descr.Length > 47)
                {
                    descr = descr.Substring(0, 47);
                }

                printer.DrawText(descr, 47);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                printer.DrawText(dev.ProCodigo.PadRight(16) + lblCantidad.PadRight(8) + dev.DevDocumento.PadRight(12) + dev.DevLote.PadRight(10));

                if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                {
                    printer.DrawText((dev.MotDescripcion + (dev.DevIndicadorOferta ? "(oferta)": "")).PadRight(23) + ((!string.IsNullOrWhiteSpace(dev.DevNumeroERP)) ? dev.DevNumeroERP.PadLeft(23) : ""));
                }
                else
                {
                    printer.DrawText(((!string.IsNullOrWhiteSpace(dev.DevNumeroERP)) ? dev.DevNumeroERP.PadLeft(46) : ""));
                }
                printer.DrawText("");
            }
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("-----------------------------------------------");
            printer.DrawText("               Firma del Cliente      ");
            printer.DrawText(" ");
            printer.DrawText("Items: " + i);
            printer.DrawText(" ");
            printer.DrawText(" ");
            printer.DrawText("Vendedor:" + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular :" + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono :" + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText(" ");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato Devoluciones 33: MovilBusiness " + Functions.AppVersion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.Print();
        }


        private void Formato34(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("You don't have the printer configured.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Return details not found!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error loading Customer data!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("RETURN");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Return Date: " + devolucion.DevFecha);
            printer.DrawText("Return: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawText("Customer: " + cliente.CliNombre, 45);
            printer.DrawText("Code: " + cliente.CliCodigo);
            printer.DrawText("Street:" + cliente.CliCalle, 45);
            if (devolucion.DevCintillo != null)
            {
                printer.DrawText("Headband:" + devolucion.DevCintillo);
            }
            printer.DrawText("");


            /* if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
              {
                  printer.DrawText("Motivo: " + devolucion.Motivo);
              }*/

            printer.DrawLine();
            //printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            //printer.DrawText("Motivo");
            printer.DrawText("Code  -  Description");
            printer.DrawText("Box/Unit  Invoice       Lot          Date");
            printer.DrawText("Motive");
            printer.DrawLine();

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }
                printer.Bold = true;
                printer.DrawText(lblCantidad.PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.PadRight(13) + dev.DevFecha.PadRight(11), 49);
                printer.Bold = false;
                if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                {
                    printer.DrawText(dev.MotDescripcion.PadLeft(12) + (dev.DevIndicadorOferta ? "(offer)".PadLeft(12) : ""));
                }
            }

            printer.DrawLine();
            printer.DrawText("Printing Date: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTE: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Customer's signature");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            //printer.DrawText("I certify that the return products");
            //printer.DrawText("maintained storage conditions");
            //printer.DrawText("appropriate for the products (20-25 oC).");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Seller: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Cel Phone number: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Phone number: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Return format 34: MovilBusiness v" + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato14(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("D E V O L U C I O N");
            printer.DrawText("");


            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_Ventas().GetByNCF(devolucion.DevReferencia);
            if (DS_RepresentantesParametros.GetInstance().GetParDevolucionesNumeroDocumento())
            {
                printer.DrawText("No. Documento: " + devolucion.DevCintillo);
            }
            printer.DrawText("No. Orden Compra: " + factura.VenOrdenCompra);
            printer.DrawText("Factura: " + (factura?.RepCodigo + "-" + factura?.VenSecuencia));
            printer.DrawText("Fecha factura: " + (factura?.VenFecha));
            printer.DrawText("Ncf Afectado: " + devolucion.DevReferencia);
            if (!String.IsNullOrEmpty(cliente.CliNombreComercial))
            {
                printer.DrawText("Cliente: " + cliente.CliNombreComercial, 48);
                printer.DrawText("Sucursal: " + cliente.CliNombre, 48);
            }
            else
            {
                printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            }
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawLine();
            var subTotal = 0.0;
            var totalGeneral = 0.0;
            var totalItbis = 0.0;
            var totalDesc = 0.0;

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                var itbis = (dev.DevPrecio - dev.DevDescuento) * (dev.DevItebis / 100);
                var total = (itbis + (dev.DevPrecio - dev.DevDescuento)) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);

                totalItbis += itbis;
                totalGeneral += total;
                subTotal += (dev.DevPrecio - dev.DevDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalDesc += (dev.DevDescuento * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad));

                printer.DrawText(lblCantidad.PadRight(9) + dev.DevPrecio.ToString("N2").PadRight(9) + dev.DevDescuento.ToString("N2").PadRight(12) + itbis.ToString("N2").PadRight(9) + total.ToString("N2").PadRight(10), 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subTotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + totalDesc.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + totalGeneral.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));


            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 48);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 14: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato15(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SOLICITUD DE DEVOLUCION");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            //printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawText("Cant     Precio      Motivo");
            printer.DrawLine();
            var subTotal = 0.0;
            var totalGeneral = 0.0;
            var totalItbis = 0.0;
            var totalDesc = 0.0;

            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;
                var motivo = dev.MotDescripcion;

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    motivo = "Mercancia en mal estado";
                }

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }

                var itbis = (dev.DevPrecio - dev.DevDescuento) * (dev.DevItebis / 100);
                var total = (itbis + (dev.DevPrecio - dev.DevDescuento)) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);

                totalItbis += itbis * dev.DevCantidad;
                totalGeneral += total;
                subTotal += (dev.DevPrecio - dev.DevDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalDesc += (dev.DevDescuento * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad));

                printer.DrawText(lblCantidad.PadRight(9) + dev.DevPrecio.ToString("N2").PadRight(9) + motivo.PadRight(18) , 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;
            }

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subTotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + totalDesc.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + totalGeneral.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()), 45);
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 15: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato16(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);
            var accion = usosMultiples.GetDevolucionAccion().Where(x => x.CodigoUso == devolucion.DevAccion.ToString()).FirstOrDefault();

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa(Notbold: true);
            printer.DrawText("");
            //printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText(accion.Descripcion.ToUpper());

            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            //printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Orden #: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            printer.DrawLine();

            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");
            /*
                        var motivos = myDev.GetMotivosDevolucionFromDetalle(devSecuencia, devolucionConfirmado);

                        printer.DrawText("Motivo: " + motivos, 48);
            */
            printer.DrawLine();
            //printer.DrawText("Descripcion producto");
            //printer.DrawText("Codigo   Caj/Unid Factura   Lote       Fecha");
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            printer.DrawText("Motivo");
            printer.DrawLine();

            var list = myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado, true);

            if (list != null && list.Count > 0)
            {
                foreach (DevolucionesDetalle dev in list)
                {

                    var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                    if (descr.Length > 48)
                    {
                        descr = descr.Substring(0, 48);
                    }

                    printer.DrawText(descr, 48);

                    string lblCantidad = dev.DevCantidad.ToString();

                    if (dev.DevCantidadDetalle > 0)
                    {
                        lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                    }
                    printer.Bold = true;
                    printer.DrawText(lblCantidad.PadRight(10) + dev.DevDocumento.PadRight(14) + dev.DevLote.PadRight(13) + dev.DevFecha.PadRight(11), 49);
                    printer.Bold = false;
                    if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                    {
                        printer.DrawText(dev.MotDescripcion);
                    }
                    printer.Font = PrinterFont.BODY;

                    //var descripcion = dev.ProDescripcion;
                    //if (descripcion.Length > 48)
                    //{
                    //    descripcion = descripcion.Substring(0, 48);
                    //}
                    //printer.DrawText(descripcion);

                    //string lblCantidad = dev.DevCantidad.ToString();

                    //if (dev.DevCantidadDetalle > 0)
                    //{
                    //    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                    //}
                    //printer.Bold = true;
                    //printer.DrawText(dev.ProCodigo.PadRight(10) + lblCantidad.PadRight(6) + dev.DevDocumento.PadRight(10) + dev.DevLote.PadRight(14) + dev.DevFecha.PadRight(7), 48);

                    //printer.Bold = false;
                    //if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                    //{
                    //    printer.DrawText(dev.MotDescripcion);
                    //}
                    //printer.Font = PrinterFont.BODY;
                }
            }

            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            // printer.DrawText("");
            // printer.DrawText("");
            // printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                printer.DrawText("");
            }
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("_________________________________");
            printer.DrawText("Firma del cliente:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            //printer.DrawText("");
            // printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            // printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor:");
            printer.DrawLine();
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("Items: " + (list != null ? list.Count : 0));
            // printer.DrawText("");
            //  printer.DrawText("");
            //  printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato devoluciones 16: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato41(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaConTotales(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SOLICITUD DE DEVOLUCION");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            //printer.DrawText("Caj/Unid  Factura       Lote         Fecha");
            //printer.DrawText("Cant     Precio   Descuento   Itbis    Total");
            printer.DrawText("Cant     Precio      Motivo");
            printer.DrawLine();


            double total = 0, subtotal = 0, totalItbis = 0, descuentoGeneral = 0;
            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {
                var descr = dev.ProCodigo + "-" + dev.ProDescripcion;
                var motivo = dev.MotDescripcion;

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    motivo = "Mercancia en mal estado";
                }

                if (descr.Length > 48)
                {
                    descr = descr.Substring(0, 48);
                }

                printer.DrawText(descr, 48);

                string lblCantidad = dev.DevCantidad.ToString();

                if (dev.DevCantidadDetalle > 0)
                {
                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                }


                var precioConDescuento = (dev.DevPrecio - dev.DevDescuento);
                var cantidad = ((double.Parse(dev.DevCantidadDetalle.ToString()) / dev.ProUnidades) + dev.DevCantidad);
                var totalLinea = precioConDescuento * cantidad;

                printer.DrawText(lblCantidad.PadRight(9) + precioConDescuento.ToString("N2").PadRight(9) + motivo.PadRight(18), 49);
                printer.Font = PrinterFont.FOOTER;
                printer.DrawText("");
                printer.Font = PrinterFont.BODY;

                if (dev.ProUnidades == 0)
                {
                    dev.ProUnidades = 1;
                }
                subtotal += totalLinea;

                var itbisConDescuentoGeneral = (precioConDescuento - (precioConDescuento * (devolucion.DevPorCientoDsctoGlobal / 100))) * (dev.DevItebis / 100);
                total += (itbisConDescuentoGeneral + precioConDescuento) * ((dev.DevCantidadDetalle / dev.ProUnidades) + dev.DevCantidad);
                totalItbis += itbisConDescuentoGeneral * dev.DevCantidad;

            }

            
            total = devolucion.DevMontoTotal > 0 ? devolucion.DevMontoTotal : total;
            totalItbis = devolucion.DevMontoITBIS > 0 ? devolucion.DevMontoITBIS : totalItbis;
            descuentoGeneral = devolucion.DevMontoDsctoGlobal;

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subtotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + descuentoGeneral.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + total.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()), 45);
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 41: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

        private void Formato43(int devSecuencia, bool devolucionConfirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            Devoluciones devolucion = myDev.GetDevolucionBySecuenciaConTotales(devSecuencia, devolucionConfirmado);

            if (devolucion == null)
            {
                throw new Exception("Error cargando datos de la devolucion!");
            }

            Clientes cliente = myCli.GetClienteById(devolucion.CliID);

            if (cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente!");
            }

            printer.PrintEmpresa();
            printer.DrawText("");
            printer.DrawText("");
            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SOLICITUD DE DEVOLUCION");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha devolucion: " + devolucion.DevFecha);
            printer.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
            //printer.DrawLine();

            var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

            printer.DrawText("Factura: " + (factura?.HifDocumento));
            printer.DrawText("Fecha factura: " + (factura?.HifFecha));
            printer.DrawText("Ncf Afectado: " + factura?.HiFNCF);
            printer.DrawText("Cliente: " + cliente.CliNombre, 45);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Calle:" + cliente.CliCalle, 45);
            printer.DrawText("");

            printer.DrawLine();
            printer.DrawText("Codigo - Descripcion");
            printer.DrawText("Cantidad    Precio        %Desc.   Total Linea");
            printer.DrawText("Motivo");
            printer.DrawLine();

            double total = 0, subtotal = 0, totalItbis = 0, descuentoGeneral = 0, descuentoUnitario = 0, fleteTotal = 0;
            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
            {

                if (dev.ProUnidades == 0)
                {
                    dev.ProUnidades = 1;
                }

                var itbisConDescuentoGeneral = ((dev.DevPrecio - dev.DevDescuento) - ((dev.DevPrecio - dev.DevDescuento) * (devolucion.DevPorCientoDsctoGlobal / 100))) * (dev.DevItebis / 100);
                var precioConDescuento = (dev.DevPrecio - dev.DevDescuento);
                var cantidad = ((double.Parse(dev.DevCantidadDetalle.ToString()) / dev.ProUnidades) + dev.DevCantidad);
                var totalLinea = Math.Round(precioConDescuento * cantidad, 2);

                printer.DrawText(dev.ProCodigo.Trim() + '-' + dev.ProDescripcion);
                if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo() && devolucion.MonCodigo == "USD")
                {
                    printer.DrawText(dev.DevCantidad.ToString().PadRight(12) + dev.DevPrecio.ToString("N4").PadRight(14) + (dev.DevDescPorciento.ToString() + "%").ToString().PadRight(11) + totalLinea.ToString("N2").PadLeft(6));
                }
                else
                {
                    printer.DrawText(dev.DevCantidad.ToString().PadRight(12) + dev.DevPrecio.ToString("N2").PadRight(14) + (dev.DevDescPorciento.ToString() + "%").ToString().PadRight(11) + totalLinea.ToString("N2").PadLeft(6));
                }

                if (!string.IsNullOrWhiteSpace(dev.MotDescripcion))
                {
                    printer.DrawText(dev.MotDescripcion);
                }
                printer.Font = PrinterFont.BODY;

                subtotal += Math.Round(dev.DevPrecio * cantidad, 2);
                descuentoUnitario += Math.Round(dev.DevDescuento * cantidad, 2);
            }

            totalItbis = devolucion.DevMontoITBIS;
            descuentoGeneral = devolucion.DevMontoDsctoGlobal + Math.Round(descuentoUnitario, 2);
            total = subtotal - descuentoGeneral + totalItbis;

           

            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("TOTALES");
            printer.DrawText("");
            printer.Bold = false;
            printer.DrawText("SUBTOTAL : " + subtotal.ToString("N2").PadLeft(12));
            printer.DrawText("DESCUENTO: " + descuentoGeneral.ToString("N2").PadLeft(12));
            printer.DrawText("ITBIS    : " + totalItbis.ToString("N2").PadLeft(12));
            printer.DrawText("TOTAL    : " + total.ToString("N2").PadLeft(12));
            printer.DrawLine();
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()), 45);
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del cliente");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawLine();
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            var comentario = new DS_Mensajes().GetByTraSecuencia(devolucion.DevSecuencia, 2, devolucion.CliID);

            printer.DrawText("Comentario: " + (comentario != null ? comentario.MenDescripcion : ""), 45);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Formato devoluciones 43: Movilbusiness " + Functions.AppVersion);
            printer.Print();
        }

    }
}

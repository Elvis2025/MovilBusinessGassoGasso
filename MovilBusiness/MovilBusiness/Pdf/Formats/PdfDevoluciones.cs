using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfDevoluciones : IPdfGenerator
    {
        private DS_Devoluciones myDev;
        private DS_Clientes myCli;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        private string SectorID = "";
        private DS_UsosMultiples usosMultiples;
        public PdfDevoluciones(DS_Devoluciones myDev = null, string SecCodigo = "")
        {
            if (myDev == null || myCli==null)
            {
                myDev = new DS_Devoluciones();
                myCli = new DS_Clientes();
                myTitRepNot = new DS_TiposTransaccionReportesNotas();
                SectorID = SecCodigo;
                usosMultiples = new DS_UsosMultiples();
            }

            this.myDev = myDev;
        }

        public Task<string> GeneratePdf(int traSecuencia, bool confirmado = false)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones();

            switch (formato)
            {
                default:
                    return Formato1(traSecuencia, confirmado);
                case 15: //Agropoductores
                    return Formato15(traSecuencia, confirmado);
                case 16: //Pharmatech Dental
                    return Formato16(traSecuencia, confirmado);
                case 34: 
                    return Formato34(traSecuencia, confirmado);
                case 41: //Formato SAP con Descuento General - No Cambiar
                    return Formato41(traSecuencia, confirmado);
                case 43: //ANDOSA
                    return Formato43(traSecuencia, confirmado);
            }
        }

        private Task<string> Formato1(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {


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

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID))
                {

                    manager.PrintEmpresa();
                    manager.DrawText("");
                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("DEVOLUCION");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha devolucion: " + devolucion.DevFecha);
                    manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle:" + cliente.CliCalle);
                    manager.DrawText("");

                    if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
                    {
                        manager.DrawText("Motivo: " + devolucion.Motivo);
                    }

                    manager.DrawLine();

                    manager.DrawTableRow2(new List<string>() { "Cod-Descripcion", "Caj/Unid", "Factura", "Lote", "Fecha" }, numtocalular: 1, descrlength: 250);
                    manager.DrawLine();

                    foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {
                        var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                        if (descr.Length > 48)
                        {
                            descr = descr.Substring(0, 48);
                        }



                        string lblCantidad = dev.DevCantidad.ToString();

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow2(new List<string>() { descr.Trim(), lblCantidad, dev.DevDocumento, dev.DevLote, dev.DevFecha }, numtocalular: 1, descrlength: 250);
                    }

                    manager.DrawLine();
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");

                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Certifico que los productos devueltos");
                    manager.DrawText("mantuvieron las condiciones de almacenamiento");
                    manager.DrawText("apropiadas para los productos (20-25 oC).");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato devoluciones 1: Movilbusiness " + Functions.AppVersion);
                    manager.DrawText("");


                    return manager.FilePath;
                }
            });

        }

        //Agropoductores
        private Task<string> Formato15(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {
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

                using (var manager = PdfManagerPrue.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(devolucion.DevFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa2();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("SOLICITUD DE DEVOLUCION");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.LEFT;


                    manager.DrawText("Fecha: " + devolucion.DevFecha);
                    manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Teléfono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("DESCRIPCION".PadRight(136),withBorders: true, noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("CODIGO", noline: true);
                    manager.DrawText("FACTURA".PadLeft(40), noline: true);
                    manager.DrawText("CANT.".PadLeft(68), noline: true);
                    manager.DrawText("MOTIVO".PadLeft(180), noline: true);
                    manager.DrawText("CANT/RECIBIDA".PadLeft(235));
                    manager.DrawLine(true);

                    manager.Bold = false;


                    foreach (var dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {

                        string lblCantidad = dev.DevCantidad.ToString();
                        var motivo = dev.MotDescripcion;
                        if (string.IsNullOrWhiteSpace(motivo))
                        {
                            motivo = "Mercancia en mal estado";
                        }

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }

                        manager.DrawText(dev.ProCodigo, noline: true);
                        manager.DrawText(dev.DevDocumento.PadLeft(40), noline: true);
                        manager.DrawText(lblCantidad.PadLeft(68), noline: true);
                        manager.DrawText(motivo.PadLeft(197 - (motivo).Length), noline: true);
                        manager.DrawText("", noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (dev.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(dev.ProDescripcion.Substring(0, 38), alignCustom: 418);
                            manager.DrawText(dev.ProDescripcion.Substring(38, dev.ProDescripcion.Length - 38), alignCustom: 418);
                        }
                        else
                        {
                            manager.DrawText(dev.ProDescripcion, alignCustom: 418);
                        }
                        manager.TextAlign = Justification.LEFT;
                        manager.DrawLine(true);
                    }


                    manager.NewLine(true);
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();
                    if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
                    {
                        var nota = ("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                        if (nota.Length > 120)
                        {
                            manager.DrawText(nota.Substring(0, 120));
                            manager.DrawText(nota.Substring(120, nota.Length - 120));
                        }
                        else
                        {
                            manager.DrawText(nota);
                        }
                        manager.DrawText("");
                    }
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
                    manager.NewLine();
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA DEL CLIENTE");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA/FECHA TRANSPORTACION");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Observación: _____________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("CC: Cliente, Transportación y Almacen");
                    manager.DrawText("Formato devoluciones 15: Movilbusiness " + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato34(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {


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

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID))
                {

                    manager.PrintEmpresa();
                    manager.DrawText("");
                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("R E T U R N");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Return Date: " + devolucion.DevFecha);
                    manager.DrawText("Return: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
                    manager.DrawText("Customer: " + cliente.CliNombre);
                    manager.DrawText("Code: " + cliente.CliCodigo);
                    manager.DrawText("Street:" + cliente.CliCalle);
                    manager.DrawText("");

                    if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
                    {
                        manager.DrawText("Motive: " + devolucion.Motivo);
                    }

                    manager.DrawLine();

                    manager.Bold = true;
                    manager.DrawTableRow2(new List<string>() { "Code-Description", "Box/Unit", "Invoice", "Lot", "Date" }, numtocalular: 1, descrlength: 250);
                    manager.Bold = false;
                    manager.DrawLine();

                    foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {
                        var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                        if (descr.Length > 48)
                        {
                            descr = descr.Substring(0, 48);
                        }



                        string lblCantidad = dev.DevCantidad.ToString();

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow2(new List<string>() { descr.Trim(), lblCantidad, dev.DevDocumento, dev.DevLote, dev.DevFecha }, numtocalular: 1, descrlength: 250);
                    }

                    manager.DrawLine();
                    manager.DrawText("Printing Date: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("Customer's signature");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");

                    manager.Font = PrinterFont.BODY;
                    //manager.DrawText("Certifico que los productos devueltos");
                    //manager.DrawText("mantuvieron las condiciones de almacenamiento");
                    //manager.DrawText("apropiadas para los productos (20-25 oC).");
                    //manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Seller: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Cel Phone: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Phone number: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Return format 34: MovilBusiness v" + Functions.AppVersion);
                    manager.DrawText("");

                    return manager.FilePath;
                }
            });

        }

        private Task<string> Formato16(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {


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

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID))
                {

                    manager.PrintEmpresa();
                    manager.DrawText("");
                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText(accion.Descripcion.ToUpper());
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha devolucion: " + devolucion.DevFecha);
                    manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle:" + cliente.CliCalle);
                    manager.DrawText("");

                    if (!string.IsNullOrWhiteSpace(devolucion.Motivo))
                    {
                        manager.DrawText("Motivo: " + devolucion.Motivo);
                    }

                    manager.DrawLine();

                    manager.DrawTableRow2(new List<string>() { "Cod-Descripcion", "Caj/Unid", "Factura", "Lote", "Fecha" }, numtocalular: 1, descrlength: 250);
                    manager.DrawLine();

                    foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {
                        var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

                        if (descr.Length > 48)
                        {
                            descr = descr.Substring(0, 48);
                        }



                        string lblCantidad = dev.DevCantidad.ToString();

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }

                        manager.DrawTableRow2(new List<string>() { descr.Trim(), lblCantidad, dev.DevDocumento, dev.DevLote, dev.DevFecha }, numtocalular: 1, descrlength: 250);
                    }

                    manager.DrawLine();
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("Firma del cliente");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");

                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Certifico que los productos devueltos");
                    manager.DrawText("mantuvieron las condiciones de almacenamiento");
                    manager.DrawText("apropiadas para los productos (20-25 oC).");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato devoluciones 16: Movilbusiness " + Functions.AppVersion);
                    manager.DrawText("");


                    return manager.FilePath;
                }
            });

        }

        private Task<string> Formato41(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {
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

                using (var manager = PdfManagerPrue.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(devolucion.DevFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa2();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("SOLICITUD DE DEVOLUCION");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.LEFT;


                    manager.DrawText("Fecha: " + devolucion.DevFecha);
                    manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Teléfono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.TextAlign = Justification.RIGHT;
                    manager.DrawText("DESCRIPCION".PadRight(136), withBorders: true, noline: true);
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("CODIGO", noline: true);
                    manager.DrawText("FACTURA".PadLeft(40), noline: true);
                    manager.DrawText("CANT.".PadLeft(68), noline: true);
                    manager.DrawText("MOTIVO".PadLeft(180), noline: true);
                    manager.DrawText("CANT/RECIBIDA".PadLeft(235));
                    manager.DrawLine(true);

                    manager.Bold = false;


                    foreach (var dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {

                        string lblCantidad = dev.DevCantidad.ToString();
                        var motivo = dev.MotDescripcion;
                        if (string.IsNullOrWhiteSpace(motivo))
                        {
                            motivo = "Mercancia en mal estado";
                        }

                        if (dev.DevCantidadDetalle > 0)
                        {
                            lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
                        }

                        manager.DrawText(dev.ProCodigo, noline: true);
                        manager.DrawText(dev.DevDocumento.PadLeft(40), noline: true);
                        manager.DrawText(lblCantidad.PadLeft(68), noline: true);
                        manager.DrawText(motivo.PadLeft(197 - (motivo).Length), noline: true);
                        manager.DrawText("", noline: true);
                        manager.TextAlign = Justification.CENTERLEFT;
                        if (dev.ProDescripcion.Length >= 38)
                        {
                            manager.DrawText(dev.ProDescripcion.Substring(0, 38), alignCustom: 418);
                            manager.DrawText(dev.ProDescripcion.Substring(38, dev.ProDescripcion.Length - 38), alignCustom: 418);
                        }
                        else
                        {
                            manager.DrawText(dev.ProDescripcion, alignCustom: 418);
                        }
                        manager.TextAlign = Justification.LEFT;
                        manager.DrawLine(true);
                    }


                    manager.NewLine(true);
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();
                    if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
                    {
                        var nota = ("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                        if (nota.Length > 120)
                        {
                            manager.DrawText(nota.Substring(0, 120));
                            manager.DrawText(nota.Substring(120, nota.Length - 120));
                        }
                        else
                        {
                            manager.DrawText(nota);
                        }
                        manager.DrawText("");
                    }
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
                    manager.NewLine();
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA DEL CLIENTE");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA/FECHA TRANSPORTACION");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Observación: _____________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("CC: Cliente, Transportación y Almacen");
                    manager.DrawText("Formato devoluciones 15: Movilbusiness " + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato43(int devSecuencia, bool devolucionConfirmado)
        {
            return Task.Run(() =>
            {
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

                using (var manager = PdfManagerPrue.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID, closerText: true))
                {
                    var date = "";

                    if (DateTime.TryParse(devolucion.DevFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd/MM/yyyy");
                    }

                    manager.Font = PrinterFont.BODY;
                    manager.PrintEmpresa2();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();

                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("SOLICITUD DE DEVOLUCION");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.NewLine();
                    manager.NewLine();
                    manager.TextAlign = Justification.LEFT;


                    manager.DrawText("Fecha: " + devolucion.DevFecha);
                    manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);

                    var factura = new DS_HistoricoFacturas().GetById(devolucion.DevReferencia, devolucion.RepCodigo);

                    manager.DrawText("Factura: " + (factura?.HifDocumento));
                    manager.DrawText("Fecha factura: " + (factura?.HifFecha));
                    manager.DrawText("Ncf Afectado: " + factura?.HiFNCF);
                    manager.DrawText("Cliente: " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Calle: " + cliente.CliCalle);
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Teléfono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "Código - Descripción" }, true);
                    manager.DrawTableRow(new List<string>() { "Motivo", "Cantidad", "Precio", "%Desc.",  "Total Linea"  }, true);
                    manager.Bold = false;
                    double subtotal = 0.0;
                    double desc = 0.0;
                    double totalitbis = 0.0;
                    double total = 0.0;
                    double descuentoUnitario = 0.0;
                    foreach (var det in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
                    {
                        var itbisConDescuentoGeneral = ((det.DevPrecio - det.DevDescuento) - ((det.DevPrecio - det.DevDescuento) * (devolucion.DevPorCientoDsctoGlobal / 100))) * (det.DevItebis / 100);
                        var precioConDescuento = (det.DevPrecio - det.DevDescuento);
                        var cantidad = ((double.Parse(det.DevCantidadDetalle.ToString()) / det.ProUnidades) + det.DevCantidad);
                        var totalLinea = Math.Round(precioConDescuento * cantidad, 2);

                        manager.DrawTableRow(new List<string>() { det.ProCodigo.Trim() + '-'+ det.ProDescripcion.Trim() }, numtocalular: 2);
                        if (DS_RepresentantesParametros.GetInstance().GetParPrecioSinRedondeo() && devolucion.MonCodigo == "USD")
                        {
                            manager.DrawTableRow(new List<string>() { det.MotDescripcion.ToString(), det.DevCantidad.ToString("N2"), det.DevPrecio.ToString("N4"), (det.DevDescPorciento.ToString() + "%").ToString(), totalLinea.ToString("N2").PadLeft(13) });
                        }
                        else
                        {
                            manager.DrawTableRow(new List<string>() { det.MotDescripcion.ToString(), det.DevCantidad.ToString("N2"), det.DevPrecio.ToString("N2"), (det.DevDescPorciento.ToString() + "%").ToString(), totalLinea.ToString("N2").PadLeft(13) });
                        }

                        subtotal += Math.Round(det.DevPrecio * cantidad, 2);
                        descuentoUnitario += Math.Round(det.DevDescuento * cantidad, 2);

                    }
                    totalitbis = devolucion.DevMontoITBIS;
                    desc = devolucion.DevMontoDsctoGlobal + Math.Round(descuentoUnitario, 2);
                    total = subtotal - desc + totalitbis;

                    manager.NewLine();
                    manager.DrawLine();
                    manager.DrawText("Subtotal              : ".PadRight(10) + subtotal.ToString("N2").PadLeft(15));
                    manager.DrawText("Desc.                   : ".PadRight(10) + desc.ToString("N2").PadLeft(15));
                    manager.DrawText("Total itbis           : ".PadRight(10) + totalitbis.ToString("N2").PadLeft(15));
                    manager.DrawText("Total Devolucion : ".PadRight(10) + total.ToString("N2").PadLeft(15));


                    manager.NewLine(true);
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
                    manager.NewLine();
                    if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
                    {
                        var nota = ("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
                        if (nota.Length > 120)
                        {
                            manager.DrawText(nota.Substring(0, 120));
                            manager.DrawText(nota.Substring(120, nota.Length - 120));
                        }
                        else
                        {
                            manager.DrawText(nota);
                        }
                        manager.DrawText("");
                    }
                    manager.TextAlign = Justification.CENTER;
                    manager.NewLine();
                    manager.NewLine();
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA DEL CLIENTE");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("_______________________________________________", isline: true);
                    manager.DrawText("FIRMA/FECHA TRANSPORTACION");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.NewLine();
                    manager.NewLine();
                    manager.DrawText("Observación: _____________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.DrawText("___________________________________________________________________________________________________________________________________");
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("CC: Cliente, Transportación y Almacen");
                    manager.DrawText("Formato devoluciones 43: Movilbusiness " + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        //Agropoductores - Formato Viejo
        //private Task<string> Formato10(int devSecuencia, bool devolucionConfirmado)
        //{
        //    return Task.Run(() =>
        //    {


        //        Devoluciones devolucion = myDev.GetDevolucionBySecuencia(devSecuencia, devolucionConfirmado);

        //        if (devolucion == null)
        //        {
        //            throw new Exception("Error cargando datos de la devolucion!");
        //        }

        //        Clientes cliente = myCli.GetClienteById(devolucion.CliID);

        //        if (cliente == null)
        //        {
        //            throw new Exception("Error cargando los datos del cliente!");
        //        }

        //        using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + devSecuencia).Replace("/", ""), SectorID))
        //        {

        //            manager.PrintEmpresa();
        //            manager.DrawText("");
        //            manager.Bold = true;
        //            manager.Font = PrinterFont.TITLE;
        //            manager.TextAlign = Justification.CENTER;
        //            manager.DrawText("SOLICITUD DE DEVOLUCION");
        //            manager.Bold = false;
        //            manager.Font = PrinterFont.BODY;
        //            manager.DrawText("");
        //            manager.TextAlign = Justification.LEFT;
        //            manager.DrawText("Fecha: " + devolucion.DevFecha);
        //            manager.DrawText("Devolucion: " + Arguments.CurrentUser.RepCodigo + " - " + devSecuencia);
        //            manager.DrawText("Cliente: " + cliente.CliNombre);
        //            manager.DrawText("Codigo: " + cliente.CliCodigo);
        //            manager.DrawText("Calle: " + cliente.CliCalle);
        //            manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
        //            manager.DrawText("Teléfono: " + Arguments.CurrentUser.RepTelefono1);
        //            manager.DrawText("");

        //            manager.DrawLine();
        //            manager.DrawTableRow2(new List<string>() { "Cod-Descripcion", "Cantidad", "Factura", "Lote", "Fecha" }, numtocalular: 1, descrlength: 250);
        //            manager.DrawLine();

        //            foreach (DevolucionesDetalle dev in myDev.GetDevolucionesDetalleBySecuencia(devSecuencia, devolucionConfirmado))
        //            {
        //                var descr = dev.ProCodigo + " - " + dev.ProDescripcion;

        //                if (descr.Length > 48)
        //                {
        //                    descr = descr.Substring(0, 48);
        //                }

        //                string lblCantidad = dev.DevCantidad.ToString();

        //                if (dev.DevCantidadDetalle > 0)
        //                {
        //                    lblCantidad += "/" + dev.DevCantidadDetalle.ToString();
        //                }
        //                manager.DrawTableRow2(new List<string>() { descr.Trim(), lblCantidad, dev.DevDocumento, dev.DevLote, dev.DevFecha }, numtocalular: 1, descrlength: 250);
        //            }

        //            manager.DrawLine();
        //            manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy"));
        //            manager.DrawText("");
        //            if (myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()) != "")
        //            {
        //                var nota = ("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(2, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDevoluciones()));
        //                if (nota.Length > 120)
        //                {
        //                    manager.DrawText(nota.Substring(0, 120));
        //                    manager.DrawText(nota.Substring(120, nota.Length - 120));
        //                }
        //                else
        //                {
        //                    manager.DrawText(nota);
        //                }
        //                manager.DrawText("");
        //            }
        //            manager.DrawText("");
        //            manager.DrawText("");
        //            var myTranImg = new DS_TransaccionesImagenes();
        //            var firma = myTranImg.GetFirmaByTransaccion(2, devSecuencia.ToString());
        //            if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
        //            {
        //                manager.DrawImageForFirma(firma.TraImagen, 100);
        //            }
        //            manager.DrawLine();
        //            manager.TextAlign = Justification.CENTER;
        //            manager.DrawText("Firma del cliente");
        //            manager.DrawText("");
        //            manager.DrawText("");
        //            manager.DrawText("");
        //            manager.DrawLine();
        //            manager.TextAlign = Justification.CENTER;
        //            manager.DrawText("Firma/Fecha Transportación");
        //            manager.TextAlign = Justification.LEFT;
        //            manager.DrawText("");
        //            manager.DrawText("");
        //            manager.DrawText("Observación: ___________________________________________________________________________________________________");
        //            manager.DrawText("________________________________________________________________________________________________________________");
        //            manager.DrawText("________________________________________________________________________________________________________________");
        //            manager.DrawText("");
        //            manager.Font = PrinterFont.FOOTER;
        //            manager.DrawText("CC: Cliente, Transportación y Almacen");
        //            manager.DrawText("Formato devoluciones 15: Movilbusiness " + Functions.AppVersion);
        //            manager.DrawText("");


        //            return manager.FilePath;
        //        }
        //    });

        //}
    }
}

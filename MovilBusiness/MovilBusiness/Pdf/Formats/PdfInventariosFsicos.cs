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
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    class PdfInventariosFsicos : IPdfGenerator
    {
        private string SectorID = "";
        private DS_Productos myProd;
        DS_Clientes clientes;
        DS_InventariosFisicos invfisic;


        public PdfInventariosFsicos(string SecCodigo = "")
        {
            SectorID = SecCodigo;
            clientes = new DS_Clientes();
            invfisic = new DS_InventariosFisicos();
            myProd = new DS_Productos();
        }

        public Task<string> GeneratePdf(int invfisSecuencia, bool confirmado = false)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionInventarioFisicosPDF() == 0 ? DS_RepresentantesParametros.GetInstance().GetFormatoImpresionInventarioFisicos() : DS_RepresentantesParametros.GetInstance().GetFormatoImpresionInventarioFisicosPDF();

            switch (formato)
            {
                default:
                    return Formato1(invfisSecuencia);
                case 2:
                    //return Formato2(invfisSecuencia);
                    return Formato2(invfisSecuencia);
                case 3:
                    return Formato3(invfisSecuencia);
                case 4:
                    return Formato4(invfisSecuencia);
                case 5:
                    return Formato5(invfisSecuencia);
                case 6: //pharmatech inventario fisico Consignacion
                    return Formato6(invfisSecuencia);
            }
        }

        private Task<string> Formato1(int invfisSecuencia)
        {
            return Task.Run(() =>
            {

                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                 Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null? invfisico.CliID : -1);

                if (cliente == null)
                {
                    throw new Exception("No se encontraron los datos del cliente");
                }

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE INVENTARIO FISICO No. " + invfisSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + cliente.CliNombre);
                    manager.DrawText("Fecha   : " + invfisico.infFecha);
                    manager.NewLine();

                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string>() { "Cantidad", "Precio Venta", "Itbis" });
                    manager.Bold = false;

                    foreach (var det in invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
                    {
                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                        manager.DrawTableRow(new List<string>() { det.infCantidad?.ToString("N2").PadRight(32) + "/"+ det.infCantidadDetalle?.ToString("N2"), det.InvPrecioVenta.ToString("N2"), det.Itbis.ToString("N2") });
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf Inventarios Fisicos 1: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }
            });
        }

        private Task<string> Formato2(int invfisSecuencia)
        {
            return Task.Run(() =>
            {

                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                 Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null? invfisico.CliID : -1);

                if (cliente == null)
                {
                    throw new Exception("No se encontraron los datos del cliente");
                }

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE INVENTARIO FISICO No. " + invfisSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + cliente.CliNombre);
                    manager.DrawText("Fecha   : " + invfisico.infFecha);
                    manager.NewLine();

                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string>() { "Cantidad Gondola", "Cantidad Almacen", "Cantidad Total" });
                    manager.Bold = false;

                    foreach (var det in invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
                    {
                        int total = 0;

                        bool iscantidadNull = det.infCantidad == null;
                        bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                        total = (int)((iscantidadNull ? 0 : det.infCantidad)
                        + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                        manager.DrawTableRow(new List<string>() { iscantidadNull? "*" : det.infCantidad.ToString(),
                            iscantidadDetalleNull? "*" : det.infCantidadDetalle.ToString(), total.ToString() });
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.NewLine();

                    Visitas posicion = SqliteManager.GetInstance().Query<Visitas>
                    ($"SELECT VisLatitud, VisLongitud from Visitas where VisSecuencia = {invfisico.VisSecuencia}")
                    .FirstOrDefault();

                    manager.DrawText("Link de Geo posición: " + $"https://www.google.com/maps/search/?api=1&query={posicion.VisLatitud},{posicion.VisLongitud}");
                    manager.NewLine();
                    manager.DrawText("Formato pdf Inventarios Fisicos 2: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }
            });
        }
        private Task<string> Formato3(int invfisSecuencia)
        {
            return Task.Run(() =>
            {

                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

                if (cliente == null)
                {
                    throw new Exception("No se encontraron los datos del cliente");
                }

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE INVENTARIO FISICO No. " + invfisSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + cliente.CliNombre);
                    manager.DrawText("Fecha   : " + invfisico.infFecha);
                    manager.NewLine();

                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string>() { "Cantidad Gondola", "Unidades Gondola", "Cantidad Almacen", "Unidades Almacen" });
                    manager.Bold = false;

                    foreach (var det in invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
                    {
                        int total = 0;

                        bool iscantidadNull = det.infCantidad == null;
                        bool iscantidadDetalleNull = det.infCantidadDetalle == null;

                        total = (int)((iscantidadNull ? 0 : det.infCantidad)
                        + (iscantidadDetalleNull ? 0 : (det.infCantidadDetalle * det.ProUnidades)));

                        if (det.InvArea == 1)
                        {
                            manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                            manager.DrawTableRow(new List<string>() { iscantidadNull? "*" : det.infCantidad.ToString(),
                            iscantidadDetalleNull? "*" : det.infCantidadDetalle.ToString(), total.ToString(), "*", "*" });
                        }
                        else
                        {
                            manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                            manager.DrawTableRow(new List<string>() {"*", "*", iscantidadNull? "*" : det.infCantidad.ToString(),
                            iscantidadDetalleNull? "*" : det.infCantidadDetalle.ToString() });
                        }

                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                        manager.DrawTableRow(new List<string>() { iscantidadNull? "*" : det.infCantidad.ToString(),
                            iscantidadDetalleNull? "*" : det.infCantidadDetalle.ToString() });
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.NewLine();
                    manager.DrawText("Formato pdf Inventarios Fisicos 3: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }
            });
        }

        private Task<string> Formato4(int invfisSecuencia)
        {
            return Task.Run(() =>
            {

                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

                if (cliente == null)
                {
                    throw new Exception("No se encontraron los datos del cliente");
                }

                using (var manager = PdfManager.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("ORDEN DE INVENTARIO FISICO No. " + invfisSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + cliente.CliNombre);
                    manager.DrawText("Fecha   : " + invfisico.infFecha);
                    manager.NewLine();

                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string>() { "Cantidad", "Unidades", "Precio Venta", "Itbis" });
                    manager.Bold = false;

                    foreach (var det in invfisic.GetInventarioFisicoDetalles(invfisSecuencia))
                    {
                        manager.DrawText(det.ProCodigo + " - " + det.ProDescripcion);
                        manager.DrawTableRow(new List<string>() { det.infCantidad?.ToString("N2"), det.infCantidadDetalle?.ToString("N2"), det.InvPrecioVenta.ToString("N2"), det.Itbis.ToString("N2") });
                    }
                    manager.DrawLine();
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Teléfono : " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf Inventarios Fisicos 4: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }
            });
        }

        private Task<string> Formato5(int invfisSecuencia)
        {
            return Task.Run(() =>
            {
                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                if (invfisico == null)
                {
                    throw new Exception("No se encontraron los datos del inventario fisico");
                }

                Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1, RNC: true);

                using (var manager = PdfManagerPrue.NewDocument((cliente.CliNombre.Trim() + ":" + Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {

                    var date = "";

                    if (DateTime.TryParse(invfisico.infFecha, out DateTime fecha))
                    {
                        date = fecha.ToString("dd-MM-yyyy");
                    }
                    manager.PrintEmpresa(noinfo: true);
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.RIGHT;
                    manager.Bold = true;
                    manager.DrawTextNew("INVENTARIO FISICO");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("No. Documento                          Fecha                           Página      ");
                    manager.Bold = true;
                    manager.DrawText(invfisSecuencia.ToString().PadRight(35) + date.PadRight(35) + "1/1".PadRight(12));
                    manager.Bold = false;
                    //manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    //manager.DrawText("RNC".PadRight(50));
                    //manager.Bold = true;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("Cliente                                 RNC                                        ");
                    manager.Bold = true;
                    manager.DrawText(cliente.CliNombre.PadRight(40) + (!string.IsNullOrEmpty(cliente.CliRNC) ? cliente.CliRNC.PadRight(45) : "".PadRight(45)));
                    manager.Bold = false;
                    //manager.DrawText(cliente.CliRNC.PadRight(50));
                    //manager.DrawText(!string.IsNullOrEmpty(cliente.CliRNC) ? cliente.CliRNC.PadRight(50): "");
                    //manager.Bold = false;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.DrawText("Referencia                             Contacto                                          ");
                    manager.Bold = true;
                    manager.DrawText(Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia.ToString().PadRight(40) + Arguments.CurrentUser.RepNombre.PadRight(45));
                    manager.Bold = false;
                    manager.DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", ispointtoline: true);
                    manager.TextAlign = Justification.LEFT;
                   
                    var inventariofisico = invfisic.GetInventarioFisicoDetalles(invfisSecuencia);

                    if (inventariofisico.Any(i => i.infCantidadDetalle != null))
                    {
                        manager.Bold = true;
                        manager.DrawTableRow(new List<string>() { "Inventario gondola" }, true);
                        manager.DrawTableRow(new List<string>() { "Codigo", "Descripcion", "Cantidad (UND)"}, true, numtocalular: 2, cellscalcular: 300);

                        manager.Bold = false;

                        foreach (var det in inventariofisico.Where(i => i.infCantidadDetalle != null))
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(), det.infCantidadDetalle?.ToString() }, numtocalular: 2, cellscalcular: 300);
                        }

                    }

                    if (inventariofisico.Any(i => i.infCantidadLogica != null))
                    {
                        manager.Bold = true;
                        manager.DrawTableRow(new List<string>() { "Inventario tramo" }, true);
                        manager.DrawTableRow(new List<string>() { "Codigo", "Descripción", "Cantidad (UND)" }, true, numtocalular: 2, cellscalcular: 300);
                        manager.Bold = false;

                        foreach (var det in inventariofisico.Where(i => i.infCantidadLogica != null))
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(), det.infCantidadLogica?.ToString(),}, numtocalular: 2, cellscalcular: 300);
                        }

                    }

                    if (inventariofisico.Any(i => i.infCantidad != null))
                    {
                        manager.Bold = true;
                        manager.DrawTableRow(new List<string>() { "Inventario almacen" }, true);
                        manager.DrawTableRow(new List<string>() { "Codigo", "Descripción", "Cantidad (CAJA)" }, true, numtocalular: 2, cellscalcular: 300);
                        manager.Bold = false;

                        foreach (var det in inventariofisico.Where(i => i.infCantidad != null))
                        {
                            manager.DrawTableRow(new List<string>() { det.ProCodigo, det.ProDescripcion.Trim(), det.infCantidad?.ToString() }, numtocalular: 2, cellscalcular: 300);
                        }

                    }

                    manager.Bold = true;
                    manager.DrawTableRow(new List<string>() { "RESUMEN" }, true);
                    manager.DrawTableRow(new List<string>() { "Cod. - Descripción", "Cant. Almacen", "Total cajas", "Cant. Gondola", "Cant. Tramo", "Total unds." }, true);
                    manager.Bold = false;
                    double totalcajas = 0.0;
                    double totalunds = 0.0;

                    if (inventariofisico.Any(i => i.infCantidad == 0 && i.infCantidadDetalle == 0 && i.infCantidadLogica == 0))
                    {
                        manager.Bold = true;
                        manager.DrawText("Productos en 0");
                        manager.Bold = false;

                        foreach (var det in inventariofisico.Where(i => i.infCantidad == 0 && i.infCantidadDetalle == 0 && i.infCantidadLogica == 0))
                        {

                            manager.DrawTableRow(new List<string>() { det.ProCodigo + " - " + det.ProDescripcion.Trim(), "0",
                            "0", "0", "0", "0" });
                        }

                    }

                    if (inventariofisico.Any(i => Convert.ToInt16(i.infLote) > 0))
                    {

                        manager.Bold = true;
                        manager.DrawTableRow(new List<string>() { "Productos en punto de reposición" });
                        manager.Bold = false;


                        foreach (var det in inventariofisico.Where(i => Convert.ToInt16(i.infLote) > 0))
                        {
                            bool iscantidadAlmacenNULL = det.infCantidad == null;
                            bool iscantidadGondolaNULL = det.infCantidadDetalle == null;
                            bool iscantidadTramoNULL = det.infCantidadLogica == null;

                            totalcajas = (iscantidadAlmacenNULL ? 0.0 : Convert.ToDouble(det.infCantidad));
                            totalunds = (iscantidadGondolaNULL ? 0.0 : Convert.ToDouble(det.infCantidadDetalle)) + (iscantidadTramoNULL ? 0.0 : Convert.ToDouble(det.infCantidadLogica));

                            manager.DrawTableRow(new List<string>() { det.ProCodigo + " - " + det.ProDescripcion.Trim(), (iscantidadAlmacenNULL? "*" : det.infCantidad?.ToString()),
                            totalcajas .ToString(), (iscantidadGondolaNULL? "*": det.infCantidadDetalle?.ToString()), (iscantidadTramoNULL? "*": det.infCantidadLogica?.ToString()), totalunds.ToString() });

                        }
                    }

                    if (inventariofisico.Any(i => Convert.ToInt16(i.infLote) == 0 && (i.infCantidad > 0 || i.infCantidadDetalle > 0 || i.infCantidadLogica > 0)))
                    {

                        manager.Bold = true;
                        manager.DrawTableRow(new List<string>() { "Productos en estado normal" });
                        manager.Bold = false;

                        foreach (var det in inventariofisico.Where(i => Convert.ToInt16(i.infLote) == 0 && (i.infCantidad > 0 || i.infCantidadDetalle > 0 || i.infCantidadLogica > 0)))
                        {
                            bool iscantidadAlmacenNULL = det.infCantidad == null;
                            bool iscantidadGondolaNULL = det.infCantidadDetalle == null;
                            bool iscantidadTramoNULL = det.infCantidadLogica == null;

                            totalcajas = (iscantidadAlmacenNULL ? 0.0 : Convert.ToDouble(det.infCantidad));
                            totalunds = (iscantidadGondolaNULL ? 0.0 : Convert.ToDouble(det.infCantidadDetalle)) + (iscantidadTramoNULL ? 0.0 : Convert.ToDouble(det.infCantidadLogica));

                            manager.DrawTableRow(new List<string>() { det.ProCodigo + " - " + det.ProDescripcion.Trim(), (iscantidadAlmacenNULL? "*" : det.infCantidad?.ToString()),
                            totalcajas .ToString(), (iscantidadGondolaNULL? "*": det.infCantidadDetalle?.ToString()), (iscantidadTramoNULL? "*": det.infCantidadLogica?.ToString()), totalunds.ToString() });
                        }
                    }



                    manager.DrawLine(true);
                    manager.Bold = true;

                    manager.NewLine();


                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(1, invfisSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImageForFirma(firma.TraImagen, 100);
                    }
                    manager.DrawText("Firma:", noline: true);
                    manager.DrawText("                ________________________________", isline: true);
                    manager.Bold = false;
                    manager.NewLine();

                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor : " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular  : " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Fecha emisión : " + DateTime.Now.ToString("dd/MM/yyyy"));
                    manager.DrawText("Hora emisión : " + DateTime.Now.ToString("hh:mm"));
                    manager.DrawText("Formato pdf Inventario Fisico 5: MovilBusiness v" + Functions.AppVersion);

                    return manager.FilePath;
                }

            });

        }

        private Task<string> Formato6(int invfisSecuencia)
        {

            return Task.Run(() =>
            {
                var invfisico = invfisic.GetInventarioBySecuencia(invfisSecuencia);

                if (invfisico == null)
                {
                    throw new Exception("No se encontraron los datos del inventario fisico");
                }

                Clientes cliente = clientes.GetClienteByIdForVerf(invfisico != null ? invfisico.CliID : -1);

                using (var manager = PdfManager.NewDocument((Arguments.CurrentUser.RepCodigo + "-" + invfisSecuencia).Replace("/", ""), SectorID))
                {

                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("INVENTARIO FISICO No. " + invfisSecuencia);
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.Font = PrinterFont.BODY;

                    manager.DrawText("Cliente : " + cliente.CliNombre);
                    manager.DrawText("Codigo: " + cliente.CliCodigo);
                    manager.DrawText("Fecha inventario: " + invfisico.infFecha);
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("DETALLES");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();
                    manager.Bold = true;
                    manager.DrawText("Codigo-Descripcion");
                    manager.DrawTableRow(new List<string> { "Logica", "Fisica", "Diferencia", "Lote" });
                    manager.Bold = false;
                    manager.DrawLine();

                    List<InventarioFisicoDetalle> productos = invfisic.GetInventarioFisicoDetalles(invfisSecuencia).GroupBy(x => x.ProID).Select(x => x.FirstOrDefault()).ToList();

                    foreach (var prod in productos)
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;
                        manager.DrawText(desc);

                        var productosDetalle = invfisic.GetInventarioFisicoDetallesByProID(invfisSecuencia, prod.ProID);

                        foreach (var detalle in productosDetalle)
                        {
                            var cantidadLogica = detalle.infCantidadLogica.ToString() + "/" + 0;

                            var cantidad = myProd.ConvertirUnidadesACajas(
                                myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(detalle.ProID),
                                (double)detalle.infCantidad, (double)detalle.infCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                            var unidadesFisica = Math.Round((cantidad - (int)cantidad) * myProd.GetProUnidades(detalle.ProID), 0);

                            var TotalCaja = myProd.ConvertirUnidadesACajas(
                                myProd.ConvertirCajasAunidades((double)detalle.infCantidadLogica, 0, myProd.GetProUnidades(detalle.ProID),
                                (double)detalle.infCantidad, (double)detalle.infCantidadDetalle), myProd.GetProUnidades(detalle.ProID));

                            var DiferenciaCaja = TotalCaja;
                            var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(detalle.ProID), 0);

                            manager.DrawTableRow(new List<string>() { cantidadLogica, (int)cantidad + "/" + (int)unidadesFisica, (int)DiferenciaCaja + "/" + (int)DiferenciaUnidades, detalle.infLote });
                        }
                    }

                    if (productos == null || productos.Count == 0)
                    {
                        manager.DrawText("---------------- No hay Detalle ----------------");
                    }
                    manager.DrawLine();
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("PRODUCTOS CON SOBRANTES");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();
                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string> { "Logica", "Fisica", "Diferencia", "Lote" });
                    manager.DrawLine();
                    manager.Bold = false;
                    var Largo = 35;

                    List<InventarioFisicoDetalle> ProductosSobrantes = invfisic.GetProductosSobrantes(invfisSecuencia).GroupBy(x => x.ProID).Select(x => x.FirstOrDefault()).ToList();

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
                        manager.DrawText(codigo + "-" + nombre.Substring(0, Largo));

                        var productosDetalle = invfisic.GetProductosSobrantesByProId(invfisSecuencia, myInvFis.ProID);
                        
                        foreach (var myInvDetalle in productosDetalle)
                        {
                            string CantidadLogica = myInvDetalle.infCantidadLogica.ToString() + "/" + 0;
                            string CantidadFisica = myInvDetalle.infCantidad + "/" + myInvDetalle.infCantidadDetalle;

                            double cantidadSobrante = (double)myInvDetalle.infCantidadLogica - (double)myInvDetalle.infCantidad;
                            double cantidadSobranteDetalle = (double)myInvDetalle.infCantidadDetalle - 0;

                            var TotalCaja = myProd.ConvertirUnidadesACajas(
                                myProd.ConvertirCajasAunidades((double)myInvDetalle.infCantidadLogica, 0, myProd.GetProUnidades(myInvDetalle.ProID),
                                (double)myInvDetalle.infCantidad, (double)myInvDetalle.infCantidadDetalle), myProd.GetProUnidades(myInvDetalle.ProID));

                            var DiferenciaCaja = TotalCaja;
                            var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(myInvDetalle.ProID), 0);

                            manager.DrawTableRow(new List<string>() { CantidadLogica.ToString(), CantidadFisica.ToString(), (int)DiferenciaCaja + "/" + (int)DiferenciaUnidades, myInvDetalle.infLote });

                        }
                    }

                    if (ProductosSobrantes == null || ProductosSobrantes.Count == 0 )
                    {
                        manager.DrawText("---------------- No hay Detalle ----------------");
                    }

                    manager.DrawLine();
                    manager.DrawText("SKU: " + productos.Count.ToString());
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
                    manager.DrawText("Formato PDF Inventario Fisico 6");
                    manager.DrawText("");

                    return manager.FilePath;
                }
            });
        }
    }
}

using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfConteosFisicos : IPdfGenerator
    {
        private DS_ConteosFisicos myCon;
        private DS_Productos myProd;
        private DS_UsosMultiples myUsosMul;
        private DS_Almacenes myAlm;
        private string SectorID = "";
        public PdfConteosFisicos(string SecCodigo = "")
        {
            myCon = new DS_ConteosFisicos();
            myProd = new DS_Productos();
            myUsosMul = new DS_UsosMultiples();
            myAlm = new DS_Almacenes();
            SectorID = SecCodigo;
        }

        public Task<string> GeneratePdf(int conSecuencia, bool confirmado = false)
        {
            int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionConteoFisico();

            switch (formato)
            {
                default:
                    return Formato1(conSecuencia);
            }
        }

        private Task<string> Formato1(int conSecuencia)
        {
            
            return Task.Run(() =>
            {
                var conteo = myCon.GetConteoBySecuencia(conSecuencia);

                if (conteo == null)
                {
                    throw new Exception("No se encontraron los datos del conteo");
                }

                using (var manager = PdfManager.NewDocument((Arguments.CurrentUser.RepCodigo + "-" + conSecuencia).Replace("/", ""), SectorID))
                {
                    manager.PrintEmpresa();

                    manager.NewLine();
                    manager.NewLine();

                    manager.Font = PrinterFont.TITLE;
                    manager.Bold = true;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("CONTEO FISICO");
                    manager.DrawText(conteo.ConEstatusConteo == 1 ? "Cuadrado" : "Descuadrado");
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
                    manager.DrawText("Conteo #: " + conSecuencia);

                    DateTime.TryParse(conteo.ConFecha.Replace("T", " "), out DateTime fecha);

                    manager.DrawText("Fecha conteo: " + fecha.ToString("dd-MM-yyyy hh:mm tt"));
                    if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
                    {
                        manager.DrawText("Auditor: " + conteo.RepAuditor);
                    }
                    manager.DrawText("Cuadre: " + conteo.CuaSecuencia);
                    manager.DrawText("Estado: " + conteo.EstatusDescripcion);
                    if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen())
                    {
                        manager.DrawText("Almacen: " + conteo.AlmID + "-" + myAlm.GetDescripcionAlmacen(conteo.AlmID));
                    }
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("PRODUCTOS EN INVENTARIO");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();
                    manager.Bold = true;
                    manager.DrawText("Descripcion");
                    manager.DrawTableRow(new List<string> {"Logica", "Fisica", "Diferencia" });
                    manager.Bold = false;
                    manager.DrawLine();

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
                            int ParCliIDForRep = DS_RepresentantesParametros.GetInstance().GetParClienteForRepresentantes();
                            if (ParCliIDForRep > 0)
                            {
                                DS_Clientes myCli = new DS_Clientes();
                                var cliente = myCli.GetClienteById(ParCliIDForRep);
                                LPCuadre = cliente.LiPCodigo;
                                if (Arguments.CurrentUser.TipoRelacionClientes == 2)
                                {
                                    var cliDetalle = myCli.GetDetalleFromCliente(ParCliIDForRep, Arguments.CurrentUser.RepCodigo.Trim());

                                    if (cliDetalle != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(cliDetalle.LipCodigo))
                                        {
                                            LPCuadre = cliDetalle.LipCodigo;
                                        }
                                    }
                                }

                            }
                            if (string.IsNullOrWhiteSpace(LPCuadre))
                            {
                                LPCuadre = myUsosMul.GetFirstListaPrecio();
                            }
                        }
                    }

                    var productos = myCon.GetDetalleConteoBySecuencia(conSecuencia, LPCuadre);
                    double MontoDiferencia = 0.0;

                    foreach (var prod in productos)
                    {
                        var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                        var cantidadLogica = prod.ConCantidadLogica.ToString() + "/" + prod.ConCantidadDetalleLogica.ToString();

                        var cantidad = myProd.ConvertirUnidadesACajas(
                            myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID),
                            prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                        var unidadesFisica = Math.Round((cantidad - (int)cantidad) * myProd.GetProUnidades(prod.ProID), 0);

                        var TotalCaja = myProd.ConvertirUnidadesACajas(
                            myProd.ConvertirCajasAunidades(prod.ConCantidadLogica, prod.ConCantidadDetalleLogica, myProd.GetProUnidades(prod.ProID),
                            prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                        var DiferenciaCaja = TotalCaja;
                        var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(prod.ProID), 0);

                        double itbis = double.Parse(prod.Itbis.ToString()) / 100;
                        double PrecioProducto = Math.Round(prod.Precio + (prod.Precio * (itbis)), 2);
                        if (TotalCaja < 0)
                        {
                            MontoDiferencia = MontoDiferencia + Math.Abs(PrecioProducto * DiferenciaCaja);
                        }
                        manager.DrawText(desc);
                        manager.DrawTableRow(new List<string>() { cantidadLogica, (int)cantidad + "/" + (int)unidadesFisica, (int)DiferenciaCaja + "/" + (int)DiferenciaUnidades });

                        manager.DrawText("");

                    }

                    if (productos == null)
                    {
                        manager.DrawText("---------------- No hay Detalle ----------------");
                    }
                    manager.DrawLine();
                    manager.DrawText("Cantidad de items: " + productos.Count.ToString());
                    manager.DrawText("Total monto diferencia: RD$ " + MontoDiferencia.ToString("N2"));

                    manager.DrawText("");
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("PRODUCTOS CON SOBRANTES");
                    manager.TextAlign = Justification.LEFT;
                    manager.Bold = false;
                    manager.DrawLine();
                    manager.Bold = true;
                    manager.DrawText("Codigo - Descripcion");
                    manager.DrawTableRow(new List<string> { "Logica", "Fisica", "Diferencia" });
                    manager.DrawLine();
                    manager.Bold = false;
                    manager.DrawText(" ");
                    var Largo = 35;

                    var ProductosSobrantes = myCon.GetProductosSobrantes(conSecuencia, DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen(), conteo.AlmID);
                    foreach (var myConFis in ProductosSobrantes)
                    {
                        string CantidadLogica = myConFis.ConCantidadLogica.ToString() + "/" + myConFis.ConCantidadDetalleLogica.ToString();
                        string CantidadFisica = myConFis.ConCantidad + "/" + myConFis.ConCantidadDetalle;

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

                        double cantidadSobrante = myConFis.ConCantidadLogica - myConFis.ConCantidad;
                        double cantidadSobranteDetalle = myConFis.ConCantidadDetalle - myConFis.ConCantidadDetalleLogica;

                        var TotalCaja = myProd.ConvertirUnidadesACajas(
                            myProd.ConvertirCajasAunidades(myConFis.ConCantidadLogica, myConFis.ConCantidadDetalleLogica, myProd.GetProUnidades(myConFis.ProID),
                            myConFis.ConCantidad, myConFis.ConCantidadDetalle), myProd.GetProUnidades(myConFis.ProID));

                        var DiferenciaCaja = TotalCaja;
                        var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(myConFis.ProID), 0);

                        manager.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                        manager.DrawTableRow(new List<string>() { CantidadLogica.ToString(), CantidadFisica.ToString(), (int)DiferenciaCaja + "/" + (int)DiferenciaUnidades });

                        manager.DrawText("");

                    }
                    if (ProductosSobrantes == null)
                    {
                        manager.DrawText("---------------- No hay Detalle ----------------");
                    }

                    manager.DrawLine();
                    manager.DrawText("Cantidad de items: " + productos.Count.ToString());
                    manager.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.TextAlign = Justification.CENTER;
                    var myTranImg = new DS_TransaccionesImagenes();
                    var firma = myTranImg.GetFirmaByTransaccion(8, conSecuencia.ToString());
                    if (firma != null && firma.TraImagen != null && firma.TraImagen.Length > 1)
                    {
                        manager.DrawImage(firma.TraImagen, 100);
                    }
                    manager.DrawText("_____________________________________________");
                    manager.DrawText("Firma vendedor");

                    if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
                    {
                        manager.DrawText("");
                        manager.DrawText("");
                        manager.DrawText("");
                        manager.DrawText("");
                        manager.DrawText("_____________________________________________");
                        manager.DrawText("Firma auditor");
                    }
                    manager.TextAlign = Justification.LEFT;
                    manager.DrawText("");
                    manager.DrawText("");
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
                    manager.DrawText("Formato conteos 1");
                    manager.DrawText("");

                    return manager.FilePath;
                }
            });
        }
    }
}

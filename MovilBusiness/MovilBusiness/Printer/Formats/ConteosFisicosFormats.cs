using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;

namespace MovilBusiness.Printer.Formats
{
    public class ConteosFisicosFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_ConteosFisicos myCon;
        private DS_Productos myProd;
        private DS_UsosMultiples myUsosMul;
        private DS_Almacenes myAlm;

        public ConteosFisicosFormats()
        {
            myCon = new DS_ConteosFisicos();
            myProd = new DS_Productos();
            myUsosMul = new DS_UsosMultiples();
            myAlm = new DS_Almacenes();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionConteoFisico())
            {
                case 1:
                default:
                    Formato1(traSecuencia, confirmado);
                    break;
                case 2:
                    Formato2(traSecuencia, confirmado);
                    break;
                case 3:
                    Formato3(traSecuencia, confirmado);
                    break;
            }
        }

        private void Formato1(int conSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            var conteo = myCon.GetConteoBySecuencia(conSecuencia);

            if (conteo == null)
            {
                throw new Exception("Error cargando los datos del conteo");
            }
            bool putfecha = true;

            printer.PrintEmpresa(conSecuencia, putfecha);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CONTEO FISICO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText(conteo.ConEstatusConteo == 1 ? "Cuadrado" : "Descuadrado");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Conteo #: " + conSecuencia);

            DateTime.TryParse(conteo.ConFecha.Replace("T", " "), out DateTime fecha);

            printer.DrawText("Fecha conteo: " + fecha.ToString("dd-MM-yyyy hh:mm tt"));
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
            {
                printer.DrawText("Auditor: " + conteo.RepAuditor);
            }
            printer.DrawText("Cuadre: " + conteo.CuaSecuencia);
            printer.DrawText("Estado: " + conteo.EstatusDescripcion);
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen())
            {
                printer.DrawText("Almacen: " + conteo.AlmID+"-"+myAlm.GetDescripcionAlmacen(conteo.AlmID));
            }
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("PRODUCTOS EN INVENTARIO");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion                                        ");
            printer.DrawText("Logica           Fisica          Diferencia");
            printer.Bold = false;
            printer.DrawLine();

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
            double MontoDiferenciaCaja = 0.0;
            double DiferenciaCaja = 0;
            foreach (var prod in productos)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                var cantidadLogica = prod.ConCantidadLogica.ToString() + "/" + prod.ConCantidadDetalleLogica.ToString();
                double cantidad = 0;
                double unidadesFisica = 0;
                double TotalCaja = 0;
                double DiferenciaUnidades = 0;

                if (prod.ConCantidad > 0 || prod.ConCantidadDetalle > 0)
                {         
                    cantidad = myProd.ConvertirUnidadesACajas(
                    myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID),
                    prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                    unidadesFisica = Math.Round((cantidad - (int)cantidad) * myProd.GetProUnidades(prod.ProID), 0);

                    TotalCaja  = myProd.ConvertirUnidadesACajas(
                    myProd.ConvertirCajasAunidades(prod.ConCantidadLogica, prod.ConCantidadDetalleLogica, myProd.GetProUnidades(prod.ProID), 
                    prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));
                    
                    DiferenciaCaja = TotalCaja;
                    DiferenciaUnidades = Math.Round((DiferenciaCaja - DiferenciaCaja) * myProd.GetProUnidades(prod.ProID), 0);
                }
                else
                {
                    DiferenciaCaja = prod.ConCantidadLogica;
                    DiferenciaUnidades = prod.ConCantidadDetalleLogica;
                }

                //double itbis = double.Parse(prod.Itbis.ToString()) / 100;

                MontoDiferencia += DiferenciaUnidades;
                MontoDiferenciaCaja += DiferenciaCaja;
                printer.DrawText(desc);

                printer.DrawText(cantidadLogica.PadRight(17) + (cantidad+"/"+unidadesFisica).PadRight(17) 
                    + DiferenciaCaja + "/" + DiferenciaUnidades);

                printer.DrawText("");
             
            }
            if (productos == null)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }
            printer.DrawText("------------------------------------------------");
            printer.DrawText("Total monto diferencia: $RD" + MontoDiferenciaCaja + "/" + MontoDiferencia);
            printer.DrawText("");

            var ProductosSobrantes = myCon.GetProductosSobrantes(conSecuencia, DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen(), conteo.AlmID);

            if (ProductosSobrantes != null && ProductosSobrantes.Count > 0)
            {
                printer.DrawText("");
                printer.DrawText("PRODUCTOS CON SOBRANTES");
                printer.DrawText("");
                printer.DrawText("------------------------------------------------");
                printer.DrawText("Codigo-Descripcion      ");
                printer.DrawText("Logica           Fisica          Diferencia");
                printer.DrawText("------------------------------------------------");
                printer.DrawText(" ");
                var Largo = 35;
                foreach (var myConFis in ProductosSobrantes)
                {
                    string CantidadLogica = myConFis.ConCantidadLogica.ToString() + "/" + myConFis.ConCantidadDetalleLogica.ToString();
                    string CantidadFisica = myConFis.ConCantidad + "/" + myConFis.ConCantidadDetalle;

                    /* if (myConFis.ConCantidad < myConFis.ConCantidadLogica || myConFis.ConCantidadDetalle < myConFis.ConCantidadDetalleLogica)
                     {*/
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

                    DiferenciaCaja = TotalCaja;
                    var DiferenciaUnidades = Math.Round((DiferenciaCaja - (int)DiferenciaCaja) * myProd.GetProUnidades(myConFis.ProID), 2);

                    printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                    printer.DrawText(CantidadLogica.ToString().PadRight(15)
                            + CantidadFisica.ToString().PadRight(15) +
                           (DiferenciaCaja + "/" + DiferenciaUnidades).PadLeft(9));

                    printer.DrawText("");
                }
            }

            if (ProductosSobrantes == null)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }

            printer.DrawLine();
            printer.DrawText("Cantidad de items: " + productos.Count.ToString());
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");
            
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor()) { 
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("Firma auditor");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato conteos 1");
            printer.DrawText("");
            printer.Print();
        }
        
        private void Formato2(int conSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            var conteo = myCon.GetConteoBySecuencia(conSecuencia);

            if (conteo == null)
            {
                throw new Exception("Error cargando los datos del conteo");
            }
            bool putfecha = true;

            printer.PrintEmpresa(conSecuencia, putfecha);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CONTEO FISICO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText(conteo.ConEstatusConteo == 1 ? "Cuadrado" : "Descuadrado");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Conteo #: " + conSecuencia);

            DateTime.TryParse(conteo.ConFecha.Replace("T", " "), out DateTime fecha);

            printer.DrawText("Fecha conteo: " + fecha.ToString("dd-MM-yyyy hh:mm tt"));
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
            {
                printer.DrawText("Auditor: " + conteo.RepAuditor);
            }
            printer.DrawText("Cuadre: " + conteo.CuaSecuencia);
            printer.DrawText("Estado: " + conteo.EstatusDescripcion);
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen())
            {
                printer.DrawText("Almacen: " + conteo.AlmID+"-"+myAlm.GetDescripcionAlmacen(conteo.AlmID));
            }
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("DETALLES");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion                                        ");
            printer.DrawText("Logica           Fisica          Diferencia");
            printer.Bold = false;
            printer.DrawLine();

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

            var productos = DS_RepresentantesParametros.GetInstance().GetParConteosFisicosLotesAgrupados() ? myCon.GetDetalleConteoBySecuenciaConLotesAgrupados(conSecuencia, LPCuadre) : myCon.GetDetalleConteoBySecuencia(conSecuencia, LPCuadre);
            double MontoDiferencia = 0.0;

            foreach (var prod in productos)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                var cantidadLogica = prod.ConCantidadLogica.ToString();

                var cantidad = myProd.ConvertirUnidadesACajas(
                    myProd.ConvertirCajasAunidades(0, 0, myProd.GetProUnidades(prod.ProID),
                    prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                var unidadesFisica = Math.Round((cantidad - (int)cantidad) * myProd.GetProUnidades(prod.ProID), 0);

                var TotalCaja = myProd.ConvertirUnidadesACajas(
                    myProd.ConvertirCajasAunidades(prod.ConCantidadLogica, prod.ConCantidadDetalleLogica, myProd.GetProUnidades(prod.ProID), 
                    prod.ConCantidad, prod.ConCantidadDetalle), myProd.GetProUnidades(prod.ProID));

                var DiferenciaCaja = Math.Round(TotalCaja,2);
                var DiferenciaUnidades = Math.Round((DiferenciaCaja -DiferenciaCaja) * myProd.GetProUnidades(prod.ProID),0);

                double itbis = double.Parse(prod.Itbis.ToString()) / 100;
                double PrecioProducto = Math.Round(prod.Precio + (prod.Precio * (itbis)),2);
                if (TotalCaja < 0) 
                {
                    MontoDiferencia  += Math.Abs(PrecioProducto * DiferenciaCaja);
                }

                if(DiferenciaCaja <= 0)
                {
                    printer.DrawText(desc);
                    printer.DrawText(cantidadLogica.PadRight(22) + cantidad.ToString().PadRight(17) + DiferenciaCaja);
                    printer.DrawText("");
                }
             
            }
            if (productos == null || productos.Count <= 0)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }
            printer.DrawText("------------------------------------------------");
            printer.DrawText("Total monto diferencia: $RD" +MontoDiferencia);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("PRODUCTOS CON SOBRANTES");
            printer.DrawText("");
            printer.DrawText("------------------------------------------------");
            printer.DrawText("Codigo-Descripcion      ");
            printer.DrawText("Logica           Fisica          Diferencia");
            printer.DrawText("------------------------------------------------");
            printer.DrawText(" ");
            var Largo = 35;

            var ProductosSobrantes = DS_RepresentantesParametros.GetInstance().GetParConteosFisicosLotesAgrupados() ? myCon.GetProductosSobrantesConLotesAgrupados(conSecuencia, DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen(), conteo.AlmID) : myCon.GetProductosSobrantes(conSecuencia, DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen(), conteo.AlmID);
            foreach (var myConFis in ProductosSobrantes)
            {

                string CantidadLogica = myConFis.ConCantidadLogica.ToString();
                string CantidadFisica = myConFis.ConCantidad.ToString();

               /* if (myConFis.ConCantidad < myConFis.ConCantidadLogica || myConFis.ConCantidadDetalle < myConFis.ConCantidadDetalleLogica)
                {*/
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

                    double cantidadSobrante = Math.Round(myConFis.ConCantidad - myConFis.ConCantidadLogica,2);
                    double cantidadSobranteDetalle = Math.Round(myConFis.ConCantidadDetalle - myConFis.ConCantidadDetalleLogica,2);
  
                    string cantidades = cantidadSobranteDetalle > 0 ? cantidadSobrante.ToString() + "/" + 
                    cantidadSobranteDetalle.ToString(): cantidadSobrante.ToString();

                    printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                    printer.DrawText(CantidadLogica.ToString().PadRight(15) + CantidadFisica.ToString().PadRight(15) + cantidades.PadLeft(9));
                    
                    printer.DrawText("");
            }
            if (ProductosSobrantes == null)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }

            printer.DrawLine();
            printer.DrawText("Cantidad de items: " + productos.Count.ToString());
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");
            
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor()) { 
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("Firma auditor");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato conteos 2");
            printer.DrawText("");
            printer.Print();
        }


        private void Formato3(int conSecuencia, bool confirmado)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }


            var conteo = myCon.GetConteoBySecuencia(conSecuencia);

            if (conteo == null)
            {
                throw new Exception("Error cargando los datos del conteo");
            }
            bool putfecha = true;

            printer.PrintEmpresa(conSecuencia, putfecha);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CONTEO FISICO");
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText(conteo.ConEstatusConteo == 1 ? "Cuadrado" : "Descuadrado");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Conteo #: " + conSecuencia);

            DateTime.TryParse(conteo.ConFecha.Replace("T", " "), out DateTime fecha);

            printer.DrawText("Fecha conteo: " + fecha.ToString("dd-MM-yyyy hh:mm tt"));
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
            {
                printer.DrawText("Auditor: " + conteo.RepAuditor);
            }
            printer.DrawText("Cuadre: " + conteo.CuaSecuencia);
            printer.DrawText("Estado: " + conteo.EstatusDescripcion);
            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen())
            {
                printer.DrawText("Almacen: " + conteo.AlmID + "-" + myAlm.GetDescripcionAlmacen(conteo.AlmID));
            }
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("PRODUCTOS EN INVENTARIO");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawLine();
            printer.Bold = true;
            printer.DrawText("Descripcion                                        ");
            printer.DrawText("Logica           Fisica          Diferencia");
            printer.Bold = false;
            printer.DrawLine();

            string LPCuadre = "";
            bool NoUseLP = DS_RepresentantesParametros.GetInstance().GetParNoListaPrecios();
            if (!NoUseLP)
            {

                string ListaPreciosCuadre = DS_RepresentantesParametros.GetInstance().GetParListaPreciosCuadre();
                if (!string.IsNullOrEmpty(ListaPreciosCuadre))
                {
                    LPCuadre = ListaPreciosCuadre;
                }
                else
                {
                    LPCuadre = myUsosMul.GetFirstListaPrecio();
                }
            }

            var productos = myCon.GetDetalleConteoBySecuencia(conSecuencia, LPCuadre);
            double MontoDiferencia = 0.0;
            double DiferenciaCaja = 0;
            foreach (var prod in productos)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                var cantidadLogica = prod.ConCantidadLogica.ToString() + "/" + prod.ConCantidadDetalleLogica.ToString();
                double DiferenciaUnidades = 0;

                if (prod.ConCantidad > 0 || prod.ConCantidadDetalle > 0)
                {
                    DiferenciaCaja = Math.Round((prod.ConCantidad - prod.ConCantidadLogica), 2);
                    DiferenciaUnidades = Math.Round((prod.ConCantidadDetalle - prod.ConCantidadDetalleLogica), 2);
                }
                else
                {
                    DiferenciaCaja = prod.ConCantidadLogica;
                    DiferenciaUnidades = prod.ConCantidadDetalleLogica;
                }

                double itbis = double.Parse(prod.Itbis.ToString()) / 100;
                double PrecioProducto = Math.Round(prod.Precio + (prod.Precio * (itbis)), 2);

                MontoDiferencia += Math.Abs(PrecioProducto * DiferenciaCaja);
                
                printer.DrawText(desc);

                printer.DrawText(cantidadLogica.PadRight(17) + (prod.ConCantidad + "/" + prod.ConCantidadDetalle).PadRight(17)
                    + DiferenciaCaja + "/" + DiferenciaUnidades);

                printer.DrawText("");

            }
            if (productos == null)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }
            printer.DrawText("------------------------------------------------");
            printer.DrawText("Total monto diferencia: $RD" + MontoDiferencia);
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("PRODUCTOS CON SOBRANTES");
            printer.DrawText("");
            printer.DrawText("------------------------------------------------");
            printer.DrawText("Codigo-Descripcion      ");
            printer.DrawText("Logica           Fisica          Diferencia");
            printer.DrawText("------------------------------------------------");
            printer.DrawText(" ");
            var Largo = 35;
            DiferenciaCaja = 0;
            var ProductosSobrantes = myCon.GetProductosSobrantes(conSecuencia, DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAlmacen(), conteo.AlmID);
            foreach (var myConFis in ProductosSobrantes)
            {

                string CantidadLogica = myConFis.ConCantidadLogica.ToString() + "/" + myConFis.ConCantidadDetalleLogica.ToString();
                string CantidadFisica = myConFis.ConCantidad + "/" + myConFis.ConCantidadDetalle;

                /* if (myConFis.ConCantidad < myConFis.ConCantidadLogica || myConFis.ConCantidadDetalle < myConFis.ConCantidadDetalleLogica)
                 {*/
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

                double cantidadSobrante = myConFis.ConCantidadLogica < myConFis.ConCantidad? Math.Abs(myConFis.ConCantidadLogica - myConFis.ConCantidad) : 0;
                double cantidadSobranteDetalle = myConFis.ConCantidadDetalleLogica < myConFis.ConCantidadDetalle? Math.Abs(myConFis.ConCantidadDetalle - myConFis.ConCantidadDetalleLogica) : 0;

                printer.DrawText(codigo + "-" + nombre.Substring(0, Largo));
                printer.DrawText(CantidadLogica.ToString().PadRight(15)
                        + CantidadFisica.ToString().PadRight(15) +
                       (cantidadSobrante + "/" + cantidadSobranteDetalle).PadLeft(9));

                printer.DrawText("");
            }
            if (ProductosSobrantes == null)
            {
                printer.DrawText("---------------- No hay Detalle ----------------");
            }

            printer.DrawLine();
            printer.DrawText("Cantidad de items: " + productos.Count.ToString());
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");

            if (DS_RepresentantesParametros.GetInstance().GetParConteoFisicoPorAuditor())
            {
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("");
                printer.DrawText("__________________________");
                printer.DrawText("Firma auditor");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato conteos 3");
            printer.DrawText("");
            printer.Print();
        }


    }
}

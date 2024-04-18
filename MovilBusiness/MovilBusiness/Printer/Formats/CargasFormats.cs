using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class CargasFormats : IPrinterFormatter
    {
        private PrinterManager printer;
        private DS_Cargas myCar;
        
    public CargasFormats(DS_Cargas myCar)
        {
            this.myCar = myCar;

        }

        public void Print(int CarSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionCargasInventario())
            {
                default:
                    Formato1(CarSecuencia);
                    break;
                case 2: //Nutriciosa
                    Formato2(CarSecuencia);
                    break;
                case 3: //Eccus
                    Formato3(CarSecuencia);
                    break;
            }
        }

        private void Formato1(int carSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var carga = myCar.GetCargaBySecuencia(carSecuencia);

            if(carga == null)
            {
                throw new Exception("Error cargando los datos de la carga de inventario");
            }
            bool putfecha = true;
            printer.PrintEmpresa(putfecha: putfecha);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
             
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CARGA DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Carga #: " + carSecuencia);
            printer.DrawText("Fecha de la carga: " + carga.CarFecha);
            printer.DrawText("Almacen: " + carga.AlmDescripcion);
            printer.DrawText("Cuadre: " + carga.CuaID);
            printer.DrawText("Cantidad total: " + carga.CarCantidadTotal);
            printer.DrawText("");
            
            printer.DrawText("Estado: " + carga.EstadoDescripcion);
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("PRODUCTOS");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;

            int totalCajas = 0;
            int totalUnidades = 0;
            var CargasProductos = myCar.GetProductosCarga(carSecuencia).ToList();
            bool mostrarCajasUnidades  = CargasProductos.Where(p => p.UsarCajasUnidades == true).Count() > 0;
            if (mostrarCajasUnidades)
            {
                printer.DrawText("Codigo-Descripcion                      Caj/Und");
            }
            else
            {
                printer.DrawText("Codigo-Descripcion                     Cantidad");
            }

            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            foreach (var prod in CargasProductos)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                if(desc.Length > 40)
                {
                    desc = desc.Substring(0, 40);
                }

                var cantidad = prod.CarCantidad.ToString();
                if (mostrarCajasUnidades)
                {
                    int cajas = (int)(prod.CarCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                    int unidades = (int)(prod.CarCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                    cantidad = $"{cajas}/{unidades}";
                    totalCajas += cajas;
                    totalUnidades += unidades;
                }
                else if(prod.CarCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + prod.CarCantidadDetalle;
                }

                printer.DrawText(desc.PadRight(42) + cantidad.PadRight(11));
            }

            printer.DrawLine();
            if (mostrarCajasUnidades)
            {
                printer.Bold = true;
                string totalCajasUnidades  =   $"{totalCajas}/{totalUnidades}";
                printer.DrawText("TOTAL CAJAS/UNDIDADES: ".PadRight(39) + totalCajasUnidades.PadRight(14));
                printer.Bold = false;
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cargas 1");
            printer.DrawText("");
            printer.Print();
        }

        private void Formato2(int carSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var carga = myCar.GetCargaBySecuencia(carSecuencia);

            if (carga == null)
            {
                throw new Exception("Error cargando los datos de la carga de inventario");
            }
            bool putfecha = true;
            printer.PrintEmpresa(putfecha: putfecha);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CARGA DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Carga #: " + carSecuencia);
            printer.DrawText("Fecha de la carga: " + carga.CarFecha);
            printer.DrawText("Almacen: " + carga.AlmID);
            printer.DrawText("Cuadre: " + carga.CuaID);
            printer.DrawText("Cantidad total: " + carga.CarCantidadTotal);
            printer.DrawText("");

            printer.DrawText("Estado: " + carga.EstadoDescripcion);
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("PRODUCTOS");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            foreach (var prod in myCar.GetProductosCarga(carSecuencia))
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                if (desc.Length > 24)
                {
                    desc = desc.Substring(0, 24);
                }

                //var cantidad = prod.CarCantidad.ToString();

                printer.DrawText(desc.PadRight(25) + prod.CarLote.PadLeft(10) + 
                    (prod.CarCantidad.ToString() + "/" + prod.CarCantidadDetalle).PadLeft(10), 47);
            }

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cargas 2");
            printer.DrawText("");
            printer.Print();
        }

        private void Formato3(int carSecuencia)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var carga = myCar.GetCargaBySecuencia(carSecuencia);

            if (carga == null)
            {
                throw new Exception("Error cargando los datos de la carga de inventario");
            }
            bool putfecha = true;
            printer.PrintEmpresa(putfecha: putfecha, Notbold:true);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CARGA DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Carga #: " + carSecuencia);
            printer.DrawText("Fecha de la carga: " + carga.CarFecha);
            printer.DrawText("Almacen: " + carga.AlmID);
            printer.DrawText("Cuadre: " + carga.CuaID);
            printer.DrawText("Cantidad total: " + carga.CarCantidadTotal);
            printer.DrawText("");

            printer.DrawText("Estado: " + carga.EstadoDescripcion);
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("PRODUCTOS");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            foreach (var prod in myCar.GetProductosCarga(carSecuencia))
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

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("__________________________");
            printer.DrawText("Firma vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato cargas 2");
            printer.DrawText("");
            printer.Print();
        }
    }
}

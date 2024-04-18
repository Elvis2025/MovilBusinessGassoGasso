using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class InventariosFormats
    {
        private DS_Inventarios myInv;
        private PrinterManager printer;

        public InventariosFormats(DS_Inventarios myInv)
        {
            this.myInv = myInv;
        }

        public void Print(int formato, PrinterManager printer, int almId = -1)
        {
            this.printer = printer;

            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionInventario())
            {
                default:
                case 1: //DEFAULT
                    Formato1(almId);
                    break;                
                case 2: //Nutriciosa
                    Formato2(almId);
                    break;;                
                case 3: //Eccus
                    Formato3(almId);
                    break;

            }
        }

        private void Formato1(int almId = -1)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var inventario = myInv.GetInventario(true, almId);

            if(inventario == null || inventario.Count == 0)
            {
                throw new Exception("El inventario esta vacio");
            }

            printer.PrintEmpresa();
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("RESUMEN DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            var almacen = inventario.Where(i => !string.IsNullOrWhiteSpace(i.AlmDescripcion)).FirstOrDefault();
            string almDescripcion = "";
            if (almacen != null)
            {
                almDescripcion = almacen.AlmDescripcion.Trim();
            }
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Nombre: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Almacen: " + almDescripcion);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("PRODUCTOS");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;
            bool mostrarCajasUnidades = inventario.Where(p => p.UsarCajasUnidades == true).Count() > 0;
            if (mostrarCajasUnidades)
                printer.DrawText("Codigo-Descripcion                      Caj/Und");
            else
                printer.DrawText("Codigo-Descripcion                     Cantidad");
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            int totalCajas = 0;
            int totalUnidades = 0;
            foreach (var prod in inventario)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                if (desc.Length > 43)
                {
                    desc = desc.Substring(0, 43);
                }

                var cantidad = prod.invCantidad.ToString();
                if (prod.UsarCajasUnidades)
                {
                    int cajas = (int)(prod.invCantidad / (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0));
                    int unidades = (int)(prod.invCantidad - (cajas * (prod.ProUnidades > 0 ? prod.ProUnidades : 1.0)));

                    cantidad = $"{cajas}/{unidades}";
                    totalCajas += cajas;
                    totalUnidades += unidades;
                }
                else if (prod.InvCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + prod.InvCantidadDetalle;
                }

                printer.DrawText(desc.PadRight(43) + cantidad.PadRight(10));
            }

            printer.DrawLine();
            if (mostrarCajasUnidades)
            {
                printer.Bold = true;
                string totalCajasUnidades = $"{totalCajas}/{totalUnidades}";
                printer.DrawText("TOTAL CAJAS/UNDIDADES: ".PadRight(39) + totalCajasUnidades.PadRight(14));
                printer.Bold = false;
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato inventario 1");
            printer.DrawText("");
            printer.Print();
        }


        private void Formato2(int almId = -1)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var inventario = myInv.GetInventario(true, almId);

            if (inventario == null || inventario.Count == 0)
            {
                throw new Exception("El inventario esta vacio");
            }

            printer.PrintEmpresa();
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("RESUMEN DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            var almacen = inventario.Where(i => !string.IsNullOrWhiteSpace(i.AlmDescripcion)).FirstOrDefault();
            string almDescripcion = "";
            if (almacen != null)
            {
                almDescripcion = almacen.AlmDescripcion.Trim();
            }
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Nombre: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Almacen: " + almDescripcion, 48);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("PRODUCTOS");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            foreach (var prod in inventario)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                if (desc.Length > 24)
                {
                    desc = desc.Substring(0, 24);
                }

                var cantidad = prod.invCantidad.ToString();

                if (prod.InvCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + prod.InvCantidadDetalle;
                }

                printer.DrawText((desc + " ").PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
            }

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato inventario 2");
            printer.DrawText("");
            printer.Print();
        }


        private void Formato3(int almId = -1)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }

            var inventario = myInv.GetInventario(true, almId);

            if (inventario == null || inventario.Count == 0)
            {
                throw new Exception("El inventario esta vacio");
            }

            printer.PrintEmpresa(Notbold:true);
            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("RESUMEN DE INVENTARIO");
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            var almacen = inventario.Where(i => !string.IsNullOrWhiteSpace(i.AlmDescripcion)).FirstOrDefault();
            string almDescripcion = "";
            if (almacen != null)
            {
                almDescripcion = almacen.AlmDescripcion.Trim();
            }
            printer.DrawText("Usuario: " + Arguments.CurrentUser.RepCodigo);
            printer.DrawText("Nombre: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Almacen: " + almDescripcion, 48);
            printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd/MM/yyyy hh:mm tt"));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("PRODUCTOS");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("------------------------------------------------");
            printer.Bold = true;
            printer.DrawText("Codigo-Descripcion          Lote      Cant/Und");
            printer.Bold = false;
            printer.DrawText("------------------------------------------------");

            foreach (var prod in inventario)
            {
                var desc = prod.ProCodigo + "-" + prod.ProDescripcion;

                if (desc.Length > 24)
                {
                    desc = desc.Substring(0, 24);
                }

                var cantidad = prod.invCantidad.ToString();

                if (prod.InvCantidadDetalle > 0)
                {
                    cantidad = cantidad + "/" + prod.InvCantidadDetalle;
                }

                printer.DrawText((desc + " ").PadRight(25) + prod.InvLote.PadLeft(10) + cantidad.PadLeft(10), 47);
            }

            printer.DrawLine();
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Version: " + Functions.AppVersion + " MovilBusiness");
            printer.DrawText("Formato inventario 2");
            printer.DrawText("");
            printer.Print();
        }
    }
}

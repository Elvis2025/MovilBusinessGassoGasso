using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public class EntregasFormats : IPrinterFormatter
    {
        private DS_Entregas myEnt;

        public EntregasFormats()
        {
            myEnt = new DS_Entregas();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            printer.PrintEmpresa();

            var entrega = myEnt.GetBySecuencia(traSecuencia);

            if (entrega == null)
            {
                throw new Exception("No se encontraron los datos");
            }

            var cliente = new DS_Clientes().GetClienteById(entrega.CliID);

            if(cliente == null)
            {
                throw new Exception("Error cargando los datos del cliente");
            }

            printer.DrawText("");
            printer.DrawText("");

            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;

            if(entrega.EntTipo == 19)
            {
                printer.DrawText("E N T R E G A  D E  P R O M O C I O N");
            }
            else
            {
                printer.DrawText("E N T R E G A  D E  M E R C A N C I A");
            }
            
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.Font = PrinterFont.BODY;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Sec: " + Arguments.CurrentUser.RepCodigo + "-" + traSecuencia, 48);
            printer.DrawText("Fecha: " + entrega.EntFecha);
            //printer.DrawText("Cuadre: " + Arguments.Values.CurrentCuaSecuencia);
                       
            printer.DrawText("Cliente: " + cliente.CliNombre, 48);
            printer.DrawText("Codigo: " + cliente.CliCodigo);
            printer.DrawText("Propietario: " + cliente.CliContacto);
            printer.DrawText("RNC/Cedula: " + cliente.CliRNC, 46);
            printer.DrawText("Calle: " + cliente.CliCalle, 46);
            printer.DrawText("Urb: " + cliente.CliUrbanizacion);
            printer.DrawText("Telefóno: " + cliente.CliTelefono);

            if(entrega.EntTipo == 19 && DS_RepresentantesParametros.GetInstance().GetParEntregasPromocionesUsarCanastos())
            {
                printer.DrawText("Canastos: " + entrega.EntCantidadCanastos);
            }

            printer.DrawLine();
            /*printer.DrawText("Codigo - Descripcion ");
            printer.DrawText("Cantidad         Precio              Descuento");
            printer.DrawText("Itbis            Monto Itbis         Importe");*/
            printer.DrawText("Codigo-Descripcion                     Cantidad");
            printer.DrawLine();

            var productos = myEnt.GetDetalleBySecuencia(traSecuencia);

            foreach(var det in productos)
            {
                var desc = (det.ProCodigo + "-" + det.ProDescripcion);
                if (desc.Length > 39)
                {
                    desc = desc.Substring(0, 39);
                }

                printer.DrawText(desc.PadRight(39) + det.EntCantidad.ToString());
            }

            printer.DrawText("");
            printer.DrawLine();
            printer.DrawText("Fecha de Impresion:  " + Functions.CurrentDate("dd/MM/yyyy"));
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato entregas 1 : movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();

        }
    }
}

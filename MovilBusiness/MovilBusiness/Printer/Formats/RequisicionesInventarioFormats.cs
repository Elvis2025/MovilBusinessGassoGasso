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
    public class RequisicionesInventarioFormats : IPrinterFormatter
    {
		private DS_RequisicionesInventario myReq;
		public RequisicionesInventarioFormats()
        {
			myReq = new DS_RequisicionesInventario();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1)
        {
            Formato1(traSecuencia, printer);
        }

		private void Formato1(int reqSecuencia, PrinterManager printer)
		{
			if (printer == null || !printer.IsConnectionAvailable)
			{
				throw new Exception("Error conectando con la impresora.");
			}

			var transfer = myReq.GetBySecuencia(reqSecuencia);

			if (transfer == null)
			{
				throw new Exception("Error cargando los datos de la requisición");
			}

			printer.PrintEmpresa(reqSecuencia, true);
			printer.DrawText("");

			if (!printer.IsEscPos)
			{
				printer.DrawText("");
			}

			printer.Font = PrinterFont.TITLE;
			printer.Bold = true;
			printer.TextAlign = Justification.CENTER;
			printer.DrawText("REQUISICION DE INVENTARIO");
			printer.Bold = false;
			printer.Font = PrinterFont.BODY;
			printer.TextAlign = Justification.LEFT;
			if (!printer.IsEscPos)
			{
				printer.DrawText("");
			}

			printer.DrawText("");
			var fechaValida = DateTime.TryParse(transfer.ReqFecha, out DateTime fecha);

			printer.DrawText("Requisición #     : " + reqSecuencia);
			printer.DrawText("Fecha Requisición : " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : transfer.ReqFecha));
			
			if (transfer.CuaSecuencia > 0)
			{
				printer.DrawText("Cuadre            : " + transfer.CuaSecuencia);
			}
			printer.DrawText("Estado            : " + transfer.EstDescripcion);
			printer.DrawText("");
			printer.DrawLine();
			printer.Bold = true;
			printer.DrawText("Codigo-Descripcion                     Cantidad");
			printer.Bold = false;
			printer.DrawLine();

			var productos = myReq.GetDetalleBySecuencia(reqSecuencia);

			foreach (var p in productos)
			{
				var desc = p.ProCodigo + "-" + p.ProDescripcion;

				if (desc.Length > 40)
				{
					desc = desc.Substring(0, 40);
				}

				printer.DrawText(desc.PadRight(42) + ((int)p.ReqCantidad).ToString().PadRight(11));
			}

			printer.DrawLine();
			printer.DrawText("Fecha impresion: " + Functions.CurrentDate("dd/MM/yyyy"));
			printer.DrawText("Items: " + productos.Count);
			printer.DrawText("");
			printer.DrawText("");
			printer.DrawText("");
			printer.DrawText("");
			printer.TextAlign = Justification.CENTER;
			printer.DrawText("-------------------------------------");
			printer.DrawText("Firma del vendedor");
			printer.TextAlign = Justification.LEFT;
			printer.DrawText("");
			printer.DrawText("");
			printer.DrawText("");
			printer.Font = PrinterFont.FOOTER;
			printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
			printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
			printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
			printer.DrawText("Formato req inventario 1: Movilbusiness " + Functions.AppVersion);
			printer.Print();
		}
    }
}

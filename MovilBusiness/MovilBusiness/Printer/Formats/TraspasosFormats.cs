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
    public class TraspasosFormats : IPrinterFormatter
    {
		private PrinterManager printer;
		private DS_TransferenciasAlmacenes myTra;

		public TraspasosFormats()
        {
			myTra = new DS_TransferenciasAlmacenes();
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
			this.printer = printer;

			Formato1(traSecuencia);
		}

        private void Formato1(int traSecuencia)
        {
			if (printer == null || !printer.IsConnectionAvailable)
			{
				throw new Exception("Error conectando con la impresora.");
			}

			var transfer = myTra.GetBySecuencia(traSecuencia);

			if (transfer == null)
			{
				throw new Exception("Error cargando los datos del traspaso");
			}

			printer.PrintEmpresa(traSecuencia, true);
			printer.DrawText("");

			if (!printer.IsEscPos)
			{
				printer.DrawText("");
			}

			printer.Font = PrinterFont.TITLE;
			printer.Bold = true;
			printer.TextAlign = Justification.CENTER;
			printer.DrawText("TRASPASO");
			printer.Bold = false;
			printer.Font = PrinterFont.BODY;
			printer.TextAlign = Justification.LEFT;
			if (!printer.IsEscPos)
			{
				printer.DrawText("");
			}

			printer.DrawText("");
			printer.DrawText("");
			var fechaValida = DateTime.TryParse(transfer.TraFecha, out DateTime fecha);

			printer.DrawText("Traspaso Secuencia: " + traSecuencia);
			printer.DrawText("Fecha del traspaso: " + (fechaValida ? fecha.ToString("dd/MM/yyyy") : transfer.TraFecha));
			printer.DrawText("Cuadre: " + transfer.CuaSecuencia);

            string representante;
            string repDestino;
            if (transfer.TraTipo == 1)
            {
                representante = transfer.AlmCodigoDestino + "-" + transfer.RepNombreDestino;
                repDestino = Arguments.CurrentUser.RepCodigo + "-" + Arguments.CurrentUser.RepNombre;
            }
            else
            {
                representante = Arguments.CurrentUser.RepCodigo + "-" + Arguments.CurrentUser.RepNombre;
                repDestino = transfer.AlmCodigoDestino + "-" + transfer.RepNombreDestino;
            }
            printer.DrawText("Representante: " + representante);
			printer.DrawText("Representante Destino: " + repDestino, 46);
			printer.DrawText("");
			printer.DrawLine();
			printer.Bold = true;
			printer.DrawText("Codigo-Descripcion                     Cantidad");
			printer.Bold = false;
			printer.DrawLine();			

			var productos = myTra.GetDetalleBySecuencia(traSecuencia);

			foreach(var p in productos)
            {
				var desc = p.ProCodigo + "-" + p.ProDescripcion;

				if (desc.Length > 40)
				{
					desc = desc.Substring(0, 40);
				}

				printer.DrawText(desc.PadRight(42) + ((int)p.TadCantidad).ToString().PadRight(11));
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
			printer.DrawText("Formato traspasos 1: Movilbusiness " + Functions.AppVersion);
			printer.Print();
		}
    }
}

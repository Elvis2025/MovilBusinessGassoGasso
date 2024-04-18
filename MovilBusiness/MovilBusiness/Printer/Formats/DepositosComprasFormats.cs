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
    public class DepositosComprasFormats : IPrinterFormatter
    {
        private DS_DepositosCompras myDep;
        private DS_Compras myCom;
        private PrinterManager printer;
        private DS_TiposTransaccionReportesNotas myTitRepNot;
        
    public DepositosComprasFormats(DS_DepositosCompras myDep)
        {
            this.myDep = myDep;
            myCom = new DS_Compras();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();

        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            this.printer = printer;

            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("Error conectando con la impresora.");
            }
            
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras())
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

        private void Formato1(int depSecuencia, bool confirmado)
        {
            var deposito = myDep.GetBySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("ENTREGA DEPOSITO DE COMPRAS");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Bold = false;
            if (deposito.DepEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");
            
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Deposito        : " + Arguments.CurrentUser.RepCodigo + "-" + depSecuencia);
            printer.DrawText("Fecha deposito  : " + deposito.DepFecha);
            printer.DrawText("Cant. compras   : " + deposito.DepCantidadCompra);
            printer.DrawText("Desde           : " + deposito.DepCompraDesde);
            printer.DrawText("Hasta           : " + deposito.DepCompraHasta);
            printer.DrawText("Total compras   : " + deposito.DepMonto.ToString("N2"));

            var montoReponer = deposito.DepMontoCajaChica - deposito.DepMonto;
            printer.DrawText("Total reponer   : " + montoReponer.ToString("N2"));
            printer.DrawText("Fecha impresion : " + Functions.CurrentDate("dd-MM-yyyy"));

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SECUENCIA DE LAS COMPRAS REALIZADAS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = true;
            //printer.DrawText("Numero              Monto");
            printer.DrawText("Sec.    Cliente                  Monto");
            printer.Bold = false;
            printer.DrawLine();

            var compras = myCom.GetComprasByDeposito(depSecuencia);

            foreach(var compra in compras)
            {
                var cliente = compra.CliNombre;

                if(cliente.Length > 25)
                {
                    cliente = cliente.Substring(0, 25);
                }

                printer.DrawText(compra.ComSecuencia.ToString().PadRight(8) + cliente.PadRight(25) + compra.ComTotal.ToString("N2"));
                if(compra.ComEstatus == 0)
                {
                    printer.DrawText("**ANULADO**");
                }
            }

            printer.DrawLine();

            printer.DrawText("Total compras: " + deposito.DepMonto.ToString("N2"));
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato deposito compras 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato2(int depSecuencia, bool confirmado)
        {
            var deposito = myDep.GetBySecuencia(depSecuencia, confirmado);

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito!");
            }

            printer.PrintEmpresa();

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Bold = true;
            printer.Font = PrinterFont.TITLE;
            printer.DrawText("ENTREGA DEPOSITO DE COMPRAS");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Bold = false;
            if (deposito.DepEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.DrawText("");

            printer.Font = PrinterFont.BODY;
            printer.DrawText("Deposito        : " + Arguments.CurrentUser.RepCodigo + "-" + depSecuencia);
            printer.DrawText("Fecha deposito  : " + deposito.DepFecha);
            printer.DrawText("Cant. compras   : " + deposito.DepCantidadCompra);
            printer.DrawText("Desde           : " + deposito.DepCompraDesde);
            printer.DrawText("Hasta           : " + deposito.DepCompraHasta);
            printer.DrawText("Total compras   : " + deposito.DepMonto.ToString("N2"));

            var montoReponer = deposito.DepMontoCajaChica - deposito.DepMonto;
            printer.DrawText("Total reponer   : " + montoReponer.ToString("N2"));
            printer.DrawText("Fecha impresion : " + Functions.CurrentDate("dd-MM-yyyy"));

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DETALLE DE COMPRAS REALIZADAS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawLine();
            printer.Bold = true;
            //printer.DrawText("Numero              Monto");
            printer.DrawText("Cod.         Descripcion                  Cant.");
            printer.Bold = false;
            printer.DrawLine();

            var compras = myCom.GetComprasByDepositoyProductos(depSecuencia);
            var cantidadCajas = 0;
            foreach (var compra in compras)
            {
                var prodescripcion = compra.ProDescripcion;

                if (prodescripcion.Length > 25)
                {
                    prodescripcion = prodescripcion.Substring(0, 25);
                }

                printer.DrawText(compra.ProCodigo.ToString().PadRight(13) + prodescripcion.PadRight(25) +"       "+ compra.ComCantidad.ToString());
                cantidadCajas += compra.ComCantidad;
            }

            printer.DrawLine();

            printer.DrawText("Total Cajas: " + cantidadCajas);
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("_________________________________");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato deposito compras 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

        private void Formato3(int depSecuencia, bool confirmado)
        {
            var deposito = myDep.GetBySecuencia(depSecuencia, confirmado);
            

            if (deposito == null)
            {
                throw new Exception("Error cargando datos del deposito!");
            }

            printer.PrintEmpresa(Notbold: true);

            printer.DrawText("");

            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }

            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("ENTREGA DEPOSITO DE COMPRAS");
            printer.Bold = true;
            printer.DrawText("Secuencia: " + Arguments.CurrentUser.RepCodigo + "-" + deposito.DepSecuencia);
            if (!printer.IsEscPos)
            {
                printer.DrawText("");
            }
            printer.Bold = false;
            if (deposito.DepEstatus == 0)
            {
                printer.DrawText("ANULADO");
            }
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("----------------------------------------------");
            printer.Font = PrinterFont.BODY;
            printer.DrawText("Cant. Compras      : " + deposito.DepCantidadCompra.ToString().PadLeft(24));
            printer.DrawText("Compras Desde-Hasta: " + (deposito.DepCompraDesde + "-" + deposito.DepCompraHasta).PadLeft(24));
            printer.DrawText("Fecha Desde-Hasta  : " + (myCom.GetFechaComprasDesdeHasta(depSecuencia)).PadLeft(24));
            printer.DrawText("Total Caja Chica   : " + deposito.DepMontoCajaChica.ToString("N2").PadLeft(24));
            printer.DrawText("Total Compras      : " + deposito.DepMonto.ToString("N2").PadLeft(24));
            var montoReponer = deposito.DepMontoCajaChica - deposito.DepMonto;
            printer.DrawText("Total a Reponer    : " + montoReponer.ToString("N2").PadLeft(24));
           

           // printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("--------------------------------------------");
            printer.DrawText("SECUENCIA DE LAS COMPRAS REALIZADAS");
            printer.DrawText("--------------------------------------------");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = true;
            //printer.DrawText("Numero              Monto");
            printer.DrawText("Sec.    Cliente                          Monto");
            printer.DrawText("----------------------------------------------");


            var compras = myCom.GetComprasByDeposito(depSecuencia);

            foreach (var compra in compras)
            {
                var cliente = compra.CliNombre;

                if (cliente.Length > 25)
                {
                    cliente = cliente.Substring(0, 25);
                }

                printer.DrawText(compra.ComSecuencia.ToString().PadRight(8) + cliente.PadRight(25) + compra.ComTotal.ToString("N2").PadLeft(13));
                if (compra.ComEstatus == 0)
                {
                    printer.DrawText("**ANULADO**");
                }
            }

            printer.DrawText("--------------------------------------------");
            printer.DrawText("Total compras: " + deposito.DepMonto.ToString("N2").PadLeft(31));
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha Impresion: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm ff"));
            printer.DrawText("");
            printer.DrawText("");
            if (myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()) != "")
            {
                printer.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(26, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionDepositosCompras()));
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("-------------------------------------");
            printer.DrawText("Firma del vendedor");
            printer.TextAlign = Justification.LEFT;
            //printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("");
            printer.DrawText("Formato deposito compras 3: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }

    }
}

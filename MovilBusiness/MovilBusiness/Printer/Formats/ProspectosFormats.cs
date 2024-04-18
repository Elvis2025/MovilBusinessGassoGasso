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

namespace MovilBusiness.Printer.Formats
{
    public class ProspectosFormats : IPrinterFormatter
    {
        private DS_Clientes myCli;

        public ProspectosFormats(DS_Clientes myCli)
        {
            this.myCli = myCli;
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionProspectos())
            {
                case 1:
                default:
                    Formato1(traSecuencia, printer);
                    break;
                case 2:
                    Formato2(traSecuencia, printer);
                    break;
            }
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionProspectos())
            {
                case 1:
                default:
                    Formato1(traSecuencia, printer, rowguid);
                    break;
                case 2:
                    Formato2(traSecuencia, printer, rowguid);
                    break;
            }
        }

        private void Formato1(int traSecuencia, PrinterManager printer, string rowguid = "")
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            Clientes cliente = myCli.GetClienteProspectoById(traSecuencia);

            if (cliente == null)
            {
                return;
            }

            List<ClientesReferencias> referencias = myCli.GetReferencias(traSecuencia);

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CREACION DE PROSPECTOS");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha: " + Functions.CurrentDate("dd/MM/yyyy"));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("DATOS GENERALES");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");

            var uso = new DS_UsosMultiples();

            var tiposClientes = uso.GetTiposClientes();

            var tipo = "";
            if (tiposClientes != null && tiposClientes.Count > 0)
            {
                var item = tiposClientes.Where(x => x.CodigoUso.Trim() == cliente.CliTipoCliente.ToString() && x.CodigoGrupo.Trim().ToUpper() == "SOLTIPOCLIENTE").FirstOrDefault();
                tipo = item != null ? item.Descripcion : "";
            }

            printer.DrawText("Tipo de Cliente: " + tipo);

            var tiposLocales = uso.GetTiposLocales();
            var tipoLoc = "";
            if (tiposLocales != null && tiposLocales.Count > 0)
            {
                var item = tiposLocales.Where(x => x.CodigoUso == cliente.CliTipoLocal.ToString() && x.CodigoGrupo.Trim().ToUpper() == "SOLTIPOLOCAL").FirstOrDefault();
                tipoLoc = item != null ? item.Descripcion : "";
            }

            printer.DrawText("Tipo de Local: " + tipoLoc);

            var condicion = new DS_CondicionesPago().GetByConId(cliente.ConID);

            var condicionPago = "";

            if (condicion != null)
            {
                condicionPago = condicion.ConDescripcion;
            }

            printer.DrawText("Condicion Pago: " + condicionPago);
            printer.DrawText("Nombre: " + cliente.CliNombre, 45);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Direccion #: " + cliente.CliCasa, 45);
            printer.DrawText("Sector: " + cliente.cliSector, 45);
            printer.DrawText("RNC: " + cliente.CliRNC);
            printer.DrawText("Telefono: " + cliente.CliTelefono);
            printer.DrawText("Email: " + cliente.CliCorreoElectronico);

            var prov = new DS_Provincias().GetProvinciaById(cliente.ProID);

            var provincia = "";
            if (prov != null)
            {
                provincia = prov.ProNombre;
            }

            printer.DrawText("Provincia: " + provincia);

            var mun = new DS_Municipios().GetMunicipioById(cliente.MunID);

            var municipio = "";
            if (mun != null)
            {
                municipio = mun.MunDescripcion;
            }

            printer.DrawText("Municipio: " + municipio);
            printer.DrawText("Contacto: " + cliente.CliContacto, 45);
            printer.DrawText("Celular Contacto: " + cliente.CliContactoCelular);

            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("REFERENCIAS");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            /*Referencias*/
            var tiposReferencias = uso.GetTiposReferenciasProspectos();

            foreach (var referencia in referencias)
            {
                printer.DrawText("");

                var tipoRef = tiposReferencias.Where(x => x.CodigoUso.Trim().ToUpper() == referencia.CliRefTipo.Trim().ToUpper()).FirstOrDefault();

                var tiporef = tipoRef != null ? tipoRef.Descripcion : "";

                printer.DrawText("Tipo    : " + tiporef);
                printer.DrawText("Nombre  : " + referencia.CliRefNombre, 45);
                printer.DrawText("Telefono: " + referencia.CliRefTelefono);
                printer.DrawText("Cuenta  : " + referencia.CliRefCuenta);
            }

            printer.DrawText("");
            /*Crediticios*/
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("DATOS CREDITICIOS");
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");
            printer.Bold = false;

            printer.DrawText("Emisor del Cheque : " + cliente.CliNombreEmisionCheques, 45);
            printer.DrawText("Estimado de Compra: " + cliente.CliEstimadoCompras);
            printer.DrawText("Inventario        : " + cliente.CliInventario);
            printer.DrawText("Credito Solicitado: " + cliente.CliLimiteCreditoSolicitado);
            //printer.DrawText("Credito Aprobado: " + solCredito.getSolLimiteCreditoAprobado());

            printer.DrawText("");
            /*Propietario*/
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("PROPIETARIO");
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("Nombre   : " + cliente.CliPropietario, 45);
            printer.DrawText("Direccion: " + cliente.CliPropietarioDireccion, 45);
            printer.DrawText("Cedula   : " + cliente.CliCedulaPropietario);
            printer.DrawText("Telefono : " + cliente.CliPropietarioTelefono);
            printer.DrawText("Celular  : " + cliente.CliPropietarioCelular);
            printer.DrawText("");

            /*Familiar*/
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("FAMILIAR");
            printer.DrawText("");
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;

            printer.DrawText("Nombre   :" + cliente.CliFamiliarNombre, 45);
            printer.DrawText("Direccion:" + cliente.CliFamiliarDireccion);
            printer.DrawText("Cedula   :" + cliente.CliFamiliarCedula);
            printer.DrawText("Telefono :" + cliente.CliFamiliarTelefono);
            printer.DrawText("Celular  :" + cliente.CliFamiliarCelular);


            printer.DrawText("");
            printer.DrawText("");

            int numofFormat = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionProspectos();

            string nota = new DS_TiposTransaccionReportesNotas()
                .GetNotaXTipoTransaccionReporte
                (53, numofFormat == -1? 1 : numofFormat
                );

            if (nota != "")
            {
                printer.DrawText("NOTA: " + nota, 45);
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato prospectos 1: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }


        private void Formato2(int traSecuencia, PrinterManager printer, string rowguid = "")
        {
            //if (printer == null || !printer.IsConnectionAvailable)
            //{
            //    throw new Exception("No tienes la impresora configurada.");
            //}

            Clientes cliente = myCli.GetClienteProspectoById(traSecuencia);

            if (cliente == null)
            {
                return;
            }

            List<ClientesReferencias> referencias = myCli.GetReferencias(traSecuencia);

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("CREACION DE PROSPECTOS");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText("Fecha: " + Functions.CurrentDate("dd/MM/yyyy"));
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("DATOS GENERALES");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");

            var uso = new DS_UsosMultiples();

            var tipoNegocio = new DS_TiposNegocio().GetTipoById(cliente.TiNID);
            var tipo = "";
            if (tipoNegocio != null)
            {
                tipo = tipoNegocio.TinDescripcion;
            }

            var condicion = new DS_CondicionesPago().GetByConId(cliente.ConID);
            var condicionPago = "";
            if (condicion != null)
            {
                condicionPago = condicion.ConDescripcion;
            }

            var prov = new DS_Provincias().GetProvinciaById(cliente.ProID);
            var provincia = "";
            if (prov != null)
            {
                provincia = prov.ProNombre;
            }

            var mun = new DS_Municipios().GetMunicipioById(cliente.MunID);
            var municipio = "";
            if (mun != null)
            {
                municipio = mun.MunDescripcion;
            }

            printer.DrawText("Tipo de Negocio: " + tipo);
            printer.DrawText("Condicion Pago: " + condicionPago);
            printer.DrawText("Nombre: " + cliente.CliNombre, 45);
            printer.DrawText("Calle: " + cliente.CliCalle, 45);
            printer.DrawText("Direccion #: " + cliente.CliCasa, 45);
            printer.DrawText("Sector: " + cliente.cliSector, 45);
            printer.DrawText("RNC: " + cliente.CliRNC);
            printer.DrawText("Telefono: " + cliente.CliTelefono);
            printer.DrawText("Email: " + cliente.CliCorreoElectronico);
            printer.DrawText("Provincia: " + provincia);
            printer.DrawText("Municipio: " + municipio);
            printer.DrawText("Contacto: " + cliente.CliContacto, 45);
            printer.DrawText("Celular Contacto: " + cliente.CliContactoCelular);


            /*Propietario*/
            printer.DrawText("");
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("PROPIETARIO");
            printer.DrawText("");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;

            printer.DrawText("Nombre   : " + cliente.CliPropietario, 45);
            printer.DrawText("Direccion: " + cliente.CliPropietarioDireccion, 45);
            printer.DrawText("Cedula   : " + cliente.CliCedulaPropietario);
            printer.DrawText("Telefono : " + cliente.CliPropietarioTelefono);
            printer.DrawText("Celular  : " + cliente.CliPropietarioCelular);
            printer.DrawText("");
            printer.DrawText("");

            int numofFormat = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionProspectos();

            string nota = new DS_TiposTransaccionReportesNotas()
                .GetNotaXTipoTransaccionReporte
                (53, numofFormat == -1 ? 1 : numofFormat
                );

            if (nota != "")
            {
                printer.DrawText("NOTA: " + nota, 45);
                printer.DrawText("");
            }

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato prospectos 2: Movilbusiness " + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }
    }
}

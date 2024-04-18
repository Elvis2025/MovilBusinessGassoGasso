using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MovilBusiness.Printer.Formats
{
    public class SACFormats : IPrinterFormatter
    {
        private DS_SolicitudActualizacionClientes MySol;

        public SACFormats(DS_SolicitudActualizacionClientes MySol)
        {
            this.MySol = MySol;
        }

        public void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int preFormato = -1, int traSecuencia2 = -1)
        {
            switch (DS_RepresentantesParametros.GetInstance().GetFormatoImpresionSAC())
            {
                case 1:
                default:
                    Formato1(traSecuencia, printer, rowguid);
                    break;
            }
        }

        private void Formato1(int traSecuencia, PrinterManager printer, string rowguid = "")
        {
            if (printer == null || !printer.IsConnectionAvailable)
            {
                throw new Exception("No tienes la impresora configurada.");
            }

            SolicitudActualizacionClientes sac = MySol.GetSACById(traSecuencia);
            Clientes cli = MySol.GetClienteById(sac.CliID);

            if (sac == null)
            {
                return;
            }

            printer.PrintEmpresa();

            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.TITLE;
            printer.Bold = true;
            printer.TextAlign = Justification.CENTER;
            printer.DrawText("SOLICITUD DE ACTUALIZACION DE CLIENTE");
            printer.Font = PrinterFont.BODY;
            printer.TextAlign = Justification.LEFT;
            printer.Bold = false;
            printer.DrawText("");
            printer.DrawText("");
            printer.DrawText($"Solicitud No.: {sac.RepCodigo}-{sac.SACSecuencia}");
            printer.DrawText("Fecha: " + Functions.CurrentDate("dd/MM/yyyy"));
            printer.DrawText($"Cliente: {cli.CliCodigo} - {cli.CliNombre}");
            printer.DrawText("");
            printer.TextAlign = Justification.CENTER;
            printer.Bold = true;
            printer.DrawText("DATOS SOLICITADOS MODIFICADOS");
            printer.Bold = false;
            printer.TextAlign = Justification.LEFT;
            printer.DrawText("");

            var prov = new DS_Provincias().GetProvinciaById(sac.ProID);

            var provincia = "";
            if (prov != null)
            {
                provincia = prov.ProNombre;
            }

            var mun = new DS_Municipios().GetMunicipioById(sac.MunID);

            var municipio = "";
            if (mun != null)
            {
                municipio = mun.MunDescripcion;
            }

            var uso = new DS_UsosMultiples();

            var tiposClientes = uso.GetTiposClientes();

            var tipo = "";
            if (tiposClientes != null && tiposClientes.Count > 0)
            {
                var item = tiposClientes.Where(x => x.CodigoUso.Trim() == sac.CliTipoCliente.ToString() && x.CodigoGrupo.Trim().ToUpper() == "SOLTIPOCLIENTE").FirstOrDefault();
                tipo = item != null ? item.Descripcion : "";
            }

            var tiposLocales = uso.GetTiposLocales();
            var tipoLoc = "";
            if (tiposLocales != null && tiposLocales.Count > 0)
            {
                var item = tiposLocales.Where(x => x.CodigoUso == sac.CliTipoLocal.ToString() && x.CodigoGrupo.Trim().ToUpper() == "SOLTIPOLOCAL").FirstOrDefault();
                tipoLoc = item != null ? item.Descripcion : "";
            }

            var tiposComprobante = uso.GetTiposComprobante2018();
            var tipoCom = "";
            if (tiposComprobante != null && tiposComprobante.Count > 0)
            {
                var item = tiposComprobante.Where(x => x.CodigoUso == sac.CliTipoComprobanteFAC.ToString() && x.CodigoGrupo.Trim().ToUpper() == "NCFTIPO2018").FirstOrDefault();
                tipoCom = item != null ? item.Descripcion : "";
            }

            var tipoNegocio = "";
            if (sac.TiNID > 0)
            {
                var tipoNeg = new ObservableCollection<TiposNegocio>()
                { new DS_TiposNegocio().GetTipoById(sac.TiNID) };

                tipoNegocio = tipoNeg?.FirstOrDefault().TinDescripcion;
            }

            string cliNombre, cliRNC, cliPropietario, clicedulaPropietario, cliPaginaWeb, cliCorreoElectronico, cliContacto, 
                cliSector, cliUrbanizacion, cliCalle, cliCasa,cliContactoFechaNacimiento, cliTelefono, cliFax, cliEncargadoPago, cliTipoComprobanteFAC = "";

            cliNombre = !string.IsNullOrEmpty(cli.CliNombre) ? cli.CliNombre.ToUpper() : "";
            cliRNC = !string.IsNullOrEmpty(cli.CliRNC) ? cli.CliRNC.ToUpper() : "";
            cliPropietario = !string.IsNullOrEmpty(cli.CliPropietario) ? cli.CliPropietario.ToUpper() : "";
            clicedulaPropietario = !string.IsNullOrEmpty(cli.CliCedulaPropietario) ? cli.CliCedulaPropietario.ToUpper() : "";
            cliPaginaWeb = !string.IsNullOrEmpty(cli.CliPaginaWeb) ? cli.CliPaginaWeb.ToUpper() : "";
            cliCorreoElectronico = !string.IsNullOrEmpty(cli.CliCorreoElectronico) ? cli.CliCorreoElectronico.ToUpper() : "";
            cliContacto = !string.IsNullOrEmpty(cli.CliContacto) ? cli.CliContacto.ToUpper() : "";
            cliSector = !string.IsNullOrEmpty(cli.cliSector) ? cli.cliSector.ToUpper() : "";
            cliUrbanizacion = !string.IsNullOrEmpty(cli.CliUrbanizacion) ? cli.CliUrbanizacion.ToUpper() : "";
            cliCalle = !string.IsNullOrEmpty(cli.CliCalle) ? cli.CliCalle.ToUpper() : "";
            cliCasa = !string.IsNullOrEmpty(cli.CliCasa) ? cli.CliCasa.ToUpper() : "";
            cliContactoFechaNacimiento = !string.IsNullOrEmpty(cli.CliContactoFechaNacimiento) ? cli.CliContactoFechaNacimiento.ToUpper() : "";
            cliTelefono = !string.IsNullOrEmpty(cli.CliTelefono) ? cli.CliTelefono.ToUpper() : "";
            cliFax = !string.IsNullOrEmpty(cli.CliFax) ? cli.CliFax.ToUpper() : "";
            cliEncargadoPago = !string.IsNullOrEmpty(cli.CliEncargadoPago) ? cli.CliEncargadoPago.ToUpper() : "";
            cliTipoComprobanteFAC = !string.IsNullOrEmpty(cli.CliTipoComprobanteFAC) ? cli.CliTipoComprobanteFAC.ToUpper() : "";

            if (!string.IsNullOrEmpty(sac.CliNombre) && !sac.CliNombre.ToUpper().Equals(cliNombre))
            {
                printer.DrawText("Nombre: " + sac.CliNombre, 45);
            };

            if (!string.IsNullOrEmpty(sac.CliRNC) && !sac.CliRNC.ToUpper().Equals(cliRNC))
            {
                printer.DrawText("RNC: " + sac.CliRNC);
            };

            if (!string.IsNullOrEmpty(sac.CliPropietario) && !sac.CliPropietario.ToUpper().Equals(cliPropietario)) 
            { 
                printer.DrawText("Propietario: " + sac.CliPropietario, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliCedulaPropietario) && !sac.CliCedulaPropietario.ToUpper().Equals(clicedulaPropietario)) 
            { 
                printer.DrawText("Cedula: " + sac.CliCedulaPropietario, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliPaginaWeb) && !sac.CliPaginaWeb.ToUpper().Equals(cliPaginaWeb)) 
            { 
                printer.DrawText("Sitio Web: " + sac.CliPaginaWeb, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliCorreoElectronico) && !sac.CliCorreoElectronico.ToUpper().Equals(cliCorreoElectronico)) 
            { 
                printer.DrawText("Email: " + sac.CliCorreoElectronico, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliContacto) && !sac.CliContacto.ToUpper().Equals(cliContacto)) 
            { 
                printer.DrawText("Contacto: " + sac.CliContacto, 45); 
            };

            if (!string.IsNullOrEmpty(provincia) && !sac.ProID.Equals(cli.ProID)) 
            { 
                printer.DrawText("Provincia: " + provincia, 45); 
            };

            if (!string.IsNullOrEmpty(municipio) && !sac.MunID.Equals(cli.MunID)) 
            { 
                printer.DrawText("Municipio: " + municipio, 45); 
            };

            if (!string.IsNullOrEmpty(sac.cliSector) && !sac.cliSector.ToUpper().Equals(cliSector)) 
            { 
                printer.DrawText("Sector: " + sac.cliSector, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliUrbanizacion) && !sac.CliUrbanizacion.ToUpper().Equals(cliUrbanizacion)) 
            { 
                printer.DrawText("Urb/Barrio: " + sac.CliUrbanizacion, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliCalle) && !sac.CliCalle.ToUpper().Equals(cliCalle)) 
            { 
                printer.DrawText("Calle: " + sac.CliCalle, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliCasa) && !sac.CliCasa.ToUpper().Equals(cliCasa)) 
            { 
                printer.DrawText("Local #: " + sac.CliCasa, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliTelefono) && !sac.CliTelefono.ToUpper().Equals(cliTelefono)) 
            { 
                printer.DrawText("Telefono: " + sac.CliTelefono, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliFax) && !sac.CliFax.ToUpper().Equals(cliFax)) 
            { 
                printer.DrawText("Whatsapp: " + sac.CliFax, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliEncargadoPago) && !sac.CliEncargadoPago.ToUpper().Equals(cliEncargadoPago)) 
            { 
                printer.DrawText("Encargado de pago: " + sac.CliEncargadoPago, 45); 
            };

            if (!string.IsNullOrEmpty(tipoNegocio) && !sac.TiNID.Equals(cli.TiNID)) 
            { 
                printer.DrawText("Tipo Negocio: " + tipoNegocio, 45); 
            };

            if (!string.IsNullOrEmpty(tipoLoc) && !sac.CliTipoLocal.Equals(cli.CliTipoLocal)) 
            { 
                printer.DrawText("Tipo Local: " + tipoLoc, 45); 
            };

            if (!string.IsNullOrEmpty(tipo) && !sac.CliTipoCliente.Equals(cli.CliTipoCliente)) 
            { 
                printer.DrawText("Tipo Cliente: " + tipo, 45); 
            };

            if (!string.IsNullOrEmpty(sac.CliTipoComprobanteFAC) && !sac.CliTipoComprobanteFAC.ToUpper().Equals(cliTipoComprobanteFAC)) 
            { 
                printer.DrawText("Tipo Comprobante: " + tipoCom, 45); 
            };

            if ((sac.CliLatitud != 0 && !sac.CliLatitud.Equals(cli.CliLatitud)) || (sac.CliLongitud != 0 && !sac.CliLongitud.Equals(cli.CliLongitud))) 
            { 
                printer.DrawText($"Latitud: {sac.CliLatitud}", 45); 
                printer.DrawText($"Longitud: {sac.CliLongitud}", 45); 
            };


            int numofFormat = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionSAC();

            string nota = new DS_TiposTransaccionReportesNotas().GetNotaXTipoTransaccionReporte(15, numofFormat == -1 ? 1 : numofFormat);

            if (nota != "")
            {
                printer.DrawText("NOTA: " + nota);
                printer.DrawText("");
            }
            printer.DrawText("");
            printer.DrawText("");
            printer.Font = PrinterFont.FOOTER;
            printer.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
            printer.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
            printer.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
            printer.DrawText("Formato SAC 1: Movilbusiness v." + Functions.AppVersion);
            printer.DrawText("");
            printer.Print();
        }


    }
}

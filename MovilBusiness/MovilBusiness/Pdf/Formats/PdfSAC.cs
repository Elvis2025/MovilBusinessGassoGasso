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
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Pdf.Formats
{
    public class PdfSAC : IPdfGenerator
    {
        private DS_SolicitudActualizacionClientes MySol;
        private DS_TiposTransaccionReportesNotas myTitRepNot;

        public PdfSAC()
        {
            MySol = new DS_SolicitudActualizacionClientes();
            myTitRepNot = new DS_TiposTransaccionReportesNotas();
        }

        public Task<string> GeneratePdf(int traSecuencia, bool confirmado = false)
        {
            return Formato1(traSecuencia, confirmado);
        }

        private Task<string> Formato1(int traSecuencia, bool confirmado)
        {
            return Task.Run(() =>
            {

                SolicitudActualizacionClientes sac = MySol.GetSACById(traSecuencia);
                Clientes cli = MySol.GetClienteById(sac.CliID);

                if (sac == null || cli == null)
                {
                    throw new Exception("Error cargando datos de la solicitud");
                }

                using (var manager = PdfManager.NewDocument((Arguments.CurrentUser.RepCodigo + "-SAC_No." + traSecuencia).Replace("/", "")))
                {
                    manager.PrintEmpresa();
                    manager.NewLine();
                    manager.Bold = true;
                    manager.Font = PrinterFont.TITLE;
                    manager.TextAlign = Justification.CENTER;
                    manager.DrawText("SOLICITUD DE ACTUALIZACION DE CLIENTE");
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();
                    manager.NewLine();
                    manager.Bold = false;
                    manager.Font = PrinterFont.BODY;
                    manager.DrawText("Solicitud No.: " + sac.RepCodigo + "-" + sac.SACSecuencia);
                    manager.DrawText("Fecha           : " + Functions.CurrentDate("dd/MM/yyyy"));
                    manager.DrawText("Cliente         : " + cli.CliCodigo +" - "+ cli.CliNombre);
                    manager.NewLine();
                    manager.TextAlign = Justification.CENTER;
                    manager.Bold = true;
                    manager.DrawText("DATOS SOLICITADOS MODIFICADOS");
                    manager.Bold = false;
                    manager.TextAlign = Justification.LEFT;
                    manager.NewLine();

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
                        cliSector, cliUrbanizacion, cliCalle, cliCasa, cliContactoFechaNacimiento, cliTelefono, cliFax, cliEncargadoPago, cliTipoComprobanteFAC = "";

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
                        manager.DrawText("Nombre:  " + sac.CliNombre);
                    };

                    if (!string.IsNullOrEmpty(sac.CliRNC) && !sac.CliRNC.ToUpper().Equals(cliRNC))
                    {
                        manager.DrawText("RNC:  " + sac.CliRNC);
                    };

                    if (!string.IsNullOrEmpty(sac.CliPropietario) && !sac.CliPropietario.ToUpper().Equals(cliPropietario))
                    {
                        manager.DrawText("Propietario:  " + sac.CliPropietario);
                    };

                    if (!string.IsNullOrEmpty(sac.CliCedulaPropietario) && !sac.CliCedulaPropietario.ToUpper().Equals(clicedulaPropietario))
                    {
                        manager.DrawText("Cedula:  " + sac.CliCedulaPropietario);
                    };

                    if (!string.IsNullOrEmpty(sac.CliPaginaWeb) && !sac.CliPaginaWeb.ToUpper().Equals(cliPaginaWeb))
                    {
                        manager.DrawText("Sitio Web:  " + sac.CliPaginaWeb);
                    };

                    if (!string.IsNullOrEmpty(sac.CliCorreoElectronico) && !sac.CliCorreoElectronico.ToUpper().Equals(cliCorreoElectronico))
                    {
                        manager.DrawText("Email:  " + sac.CliCorreoElectronico);
                    };

                    if (!string.IsNullOrEmpty(sac.CliContacto) && !sac.CliContacto.ToUpper().Equals(cliContacto))
                    {
                        manager.DrawText("Contacto:  " + sac.CliContacto);
                    };

                    if (!string.IsNullOrEmpty(provincia) && !sac.ProID.Equals(cli.ProID))
                    {
                        manager.DrawText("Provincia:  " + provincia);
                    };

                    if (!string.IsNullOrEmpty(municipio) && !sac.MunID.Equals(cli.MunID))
                    {
                        manager.DrawText("Municipio:  " + municipio);
                    };

                    if (!string.IsNullOrEmpty(sac.cliSector) && !sac.cliSector.ToUpper().Equals(cliSector))
                    {
                        manager.DrawText("Sector:  " + sac.cliSector);
                    };

                    if (!string.IsNullOrEmpty(sac.CliUrbanizacion) && !sac.CliUrbanizacion.ToUpper().Equals(cliUrbanizacion))
                    {
                        manager.DrawText("Urb/Barrio:  " + sac.CliUrbanizacion);
                    };

                    if (!string.IsNullOrEmpty(sac.CliCalle) && !sac.CliCalle.ToUpper().Equals(cliCalle))
                    {
                        manager.DrawText("Calle:  " + sac.CliCalle);
                    };

                    if (!string.IsNullOrEmpty(sac.CliCasa) && !sac.CliCasa.ToUpper().Equals(cliCasa))
                    {
                        manager.DrawText("Local #:  " + sac.CliCasa);
                    };

                    if (!string.IsNullOrEmpty(sac.CliTelefono) && !sac.CliTelefono.ToUpper().Equals(cliTelefono))
                    {
                        manager.DrawText("Telefono:  " + sac.CliTelefono);
                    };

                    if (!string.IsNullOrEmpty(sac.CliFax) && !sac.CliFax.ToUpper().Equals(cliFax))
                    {
                        manager.DrawText("Whatsapp:  " + sac.CliFax);
                    };

                    if (!string.IsNullOrEmpty(sac.CliEncargadoPago) && !sac.CliEncargadoPago.ToUpper().Equals(cliEncargadoPago))
                    {
                        manager.DrawText("Encargado de pago:  " + sac.CliEncargadoPago);
                    };

                    if (!string.IsNullOrEmpty(tipoNegocio) && !sac.TiNID.Equals(cli.TiNID))
                    {
                        manager.DrawText("Tipo Negocio:  " + tipoNegocio);
                    };

                    if (!string.IsNullOrEmpty(tipoLoc) && !sac.CliTipoLocal.Equals(cli.CliTipoLocal))
                    {
                        manager.DrawText("Tipo Local :  " + tipoLoc);
                    };

                    if (!string.IsNullOrEmpty(tipo) && !sac.CliTipoCliente.Equals(cli.CliTipoCliente))
                    {
                        manager.DrawText("Tipo Cliente:  " + tipo);
                    };

                    if (!string.IsNullOrEmpty(sac.CliTipoComprobanteFAC) && !sac.CliTipoComprobanteFAC.ToUpper().Equals(cliTipoComprobanteFAC))
                    {
                        manager.DrawText("Tipo Comprobante:  " + tipoCom);
                    };

                    if ((sac.CliLatitud != 0 && !sac.CliLatitud.Equals(cli.CliLatitud)) || (sac.CliLongitud != 0 && !sac.CliLongitud.Equals(cli.CliLongitud)))
                    {
                        manager.DrawText($"Latitud:  {sac.CliLatitud}");
                        manager.DrawText($"Longitud:  {sac.CliLongitud}");
                    };


                    int numofFormat = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionSAC();

                    string nota = new DS_TiposTransaccionReportesNotas().GetNotaXTipoTransaccionReporte(53, numofFormat == -1 ? 1 : numofFormat);

                    manager.DrawLine();
                    manager.NewLine();
                    manager.NewLine();
                    if (myTitRepNot.GetNotaXTipoTransaccionReporte(15, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionSAC()) != "")
                    {
                        manager.DrawText("NOTA: " + myTitRepNot.GetNotaXTipoTransaccionReporte(15, DS_RepresentantesParametros.GetInstance().GetFormatoImpresionSAC()));
                        manager.NewLine();
                    }
                    manager.NewLine();
                    manager.Font = PrinterFont.FOOTER;
                    manager.DrawText("Vendedor: " + Arguments.CurrentUser.RepNombre);
                    manager.DrawText("Celular: " + Arguments.CurrentUser.RepTelefono1);
                    manager.DrawText("Telefono: " + Arguments.CurrentUser.RepTelefono2);
                    manager.DrawText("Formato pdf SAC 1: movilbusiness " + Functions.AppVersion);
                    manager.DrawText("");

                    return manager.FilePath;
                }
            });
        }
    }
}

using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Representantes : DS_Controller
    {
        public static Representantes LogIn(string RepCodigo, string RepClave, bool isAuditor = false)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Representantes>("select ltrim(rtrim(RepCodigo)) as RepCodigo, ifnull(ltrim(rtrim(RepNombre)), 'Desconocido') as RepNombre, " +
                    "ifnull(RepCargo, '') as RepCargo, RutID, RepTelefono1, RepTelefono2, RepClave, " +
                    "RepIndicadorSupervisor, " +
                    "RepSupervisor, AlmID, " +
                    "RutID, VehID from " +
                    "Representantes where trim(RepCodigo) = ? and trim(RepClave) = ? " + (isAuditor ? " and trim(upper(RepCargo)) = 'AUDITOR'" : " and trim(upper(RepCargo)) != 'AUDITOR'"), new string[] { RepCodigo, RepClave });

                if (list != null && list.Count > 0)
                {
                    var rep = list[0];

                    if (rep != null)
                    {
                        rep.TipoRelacionClientes = DS_RepresentantesParametros.GetInstance().GetParTipoRelacionClientes();
                    }

                    return rep;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }
        public static Representantes LogInForHuella()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Representantes>("select ltrim(rtrim(RepCodigo)) as RepCodigo, ifnull(ltrim(rtrim(RepNombre)), 'Desconocido') as RepNombre, " +
                    "ifnull(RepCargo, '') as RepCargo, RutID, RepTelefono1, RepTelefono2, RepClave, " +
                    "RepIndicadorSupervisor, " +
                    "RepSupervisor, AlmID, " +
                    "RutID, VehID from " +
                    "Representantes where trim(upper(RepCargo)) <> 'AUDITOR'", new string[] {});

                if (list != null && list.Count > 0)
                {
                    var rep = list[0];

                    if (rep != null)
                    {
                        rep.TipoRelacionClientes = DS_RepresentantesParametros.GetInstance().GetParTipoRelacionClientes();
                    }

                    return rep;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        public static bool HasNCF(string repcodigo)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Representantes>("select RepCodigo from  " + (DS_RepresentantesParametros.GetInstance().GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + "  where RepCodigo = ? " +
                    " and cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) " +
                    "BETWEEN cast(strftime('%Y%m%d',DateTime('now')) as integer) and cast(strftime('%Y%m%d',ReDFechaVencimiento) as integer) ", 
                    new string[] { repcodigo.Trim() }).Count > 0;
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public List<Representantes> GetAllRepresentantes(int cliId = -1, bool sinLiquidador = false, string repCodigoExcluir = null)
        {
            string where = "";

            if(cliId != -1)
            {
                where = " and RepCodigo in (select RepCodigo from ClientesDetalle where CliID = "+cliId.ToString()+") ";
                if(DS_RepresentantesParametros.GetInstance().GetParTipoRelacionClientes() == 1)
                {
                    where = " and RepCodigo in (select RepCodigo from Clientes where CliID = "+cliId.ToString()+") ";
                }
            }

            if (sinLiquidador)
            {
                where += " and trim(upper(RepCargo)) != 'LIQUIDADOR' ";
            }

            if (!string.IsNullOrWhiteSpace(repCodigoExcluir))
            {
                where += " and trim(RepCodigo) != '"+repCodigoExcluir+"' ";
            }

            return SqliteManager.GetInstance().Query<Representantes>("select ltrim(rtrim(RepCodigo)) as RepCodigo, RepClave, ifnull(ltrim(rtrim(RepNombre)), 'Desconocido') as RepNombre, RepTelefono1, RepTelefono2, VehID, RepIndicadorSupervisor from Representantes where 1=1 "+where+" order by RepNombre", new string[] { });
        }

        public List<Representantes> GetAllRepresentantesFromClienteDetalle(string repCodigoToExclude = null)
        {
            var where = "";

            if (!string.IsNullOrWhiteSpace(repCodigoToExclude))
            {
                where = " and trim(RepCodigo) != '"+repCodigoToExclude+"' ";
            }

            return SqliteManager.GetInstance().Query<Representantes>("select ltrim(rtrim(RepCodigo)) as RepCodigo, RepClave, ifnull(ltrim(rtrim(RepNombre)), " +
                "'Desconocido') as RepNombre, RepTelefono1, RepTelefono2, VehID, RepIndicadorSupervisor from Representantes " +
                "where trim(RepCodigo) in (select distinct trim(RepCodigo) from ClientesDetalle) "+where+" order by RepNombre", new string[] { });
        }

        public string GetRepNombre(string repCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Representantes>("select ifnull(ltrim(rtrim(RepNombre)), '') as RepNombre from Representantes where ltrim(rtrim(Upper(RepCodigo))) = ?", new string[] { repCodigo.Trim().ToUpper() });

                if(list != null && list.Count > 0)
                {
                    return list[0].RepNombre;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return "";
        }

        public static Representantes RefrescarRepresentante(string RepCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Representantes>("select ltrim(rtrim(RepCodigo)) as RepCodigo, ifnull(ltrim(rtrim(RepNombre)), 'Desconocido') as RepNombre, " +
                    "ifnull(RepCargo, '') as RepCargo, RutID, RepTelefono1, RepTelefono2, RepClave, " +
                    "RepIndicadorSupervisor, " +
                    "RepSupervisor, AlmID, " +
                    "RutID, VehID from " +
                    "Representantes where trim(RepCodigo) = ? " , new string[] { RepCodigo });

                if (list != null && list.Count > 0)
                {
                    var rep = list[0];

                    if (rep != null)
                    {
                        rep.TipoRelacionClientes = DS_RepresentantesParametros.GetInstance().GetParTipoRelacionClientes();
                    }

                    return rep;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        public string GetRepCargoRepresentantes(string repcodigo)
        {
            return SqliteManager.GetInstance().Query<Representantes>
                ($"select repcargo from representantes where repcodigo = '{repcodigo}'").FirstOrDefault().RepCargo;
        }

        public static bool RepresentateIsInactive(string repcodigo)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Representantes>("select RepCodigo from  Representantes  where RepEstatus=0 and RepCodigo = ? ",
                    new string[] { repcodigo.Trim() }).Count > 0;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

    }
}

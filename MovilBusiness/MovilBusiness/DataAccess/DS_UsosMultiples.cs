

using Microsoft.AppCenter.Crashes;
using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
  
    public class DS_UsosMultiples: DS_Controller
    {

        public List<UsosMultiples> GetUsoByCodigoGrupo(string codigoGrupo, string orderBy = null, string codigoUso = null)
        {
            try
            {
                string query = "select ifnull(trim(CodigoGrupo), '') as CodigoGrupo, ifnull(CodigoUso, '') as CodigoUso, " +
                    "Descripcion from UsosMultiples where UPPER(trim(CodigoGrupo)) = ?";

                if (!string.IsNullOrWhiteSpace(codigoUso))
                {
                    query += " and trim(upper(CodigoUso)) = '"+codigoUso.Trim().ToUpper()+"' ";
                }

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    query += " order by " + orderBy;
                }
                else
                {
                    query += " order by CodigoUso";
                }

                return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { codigoGrupo.ToUpper().Trim() });

            }catch(Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);

                return new List<UsosMultiples>();
            }
        }

        public List<UsosMultiples> GetUsoByCodigoGrupoByEndLike(string codigoGrupo, string orderBy = "Descripcion")
        {
            try
            {
                string query = "select ifnull(trim(CodigoGrupo), '') as CodigoGrupo, ifnull(CodigoUso, '') as CodigoUso, " +
                    "Descripcion from UsosMultiples where UPPER(CodigoGrupo) LIKE '"+codigoGrupo.ToUpper()+"%'";

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    query += " order by " + orderBy;
                }
                else
                {
                    query += " order by CodigoUso";
                }

                return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);

                return new List<UsosMultiples>();
            }
        }

        public List<UsosMultiples> GetTipoVisita(string tipovisita = "")
        {
            var list = GetUsoByCodigoGrupo("TIPOVISITA");

            if(list == null || list.Count == 0)
            {
                list = new List<UsosMultiples>()
                {
                    new UsosMultiples(){CodigoUso = "1", Descripcion = "Presencial", CodigoGrupo = "TIPOVISITA" },
                    new UsosMultiples(){CodigoUso = "2", Descripcion = "Virtual", CodigoGrupo = "TIPOVISITA" }
                };
            }

            if (!string.IsNullOrEmpty(tipovisita))
            {
                list = list.Where(w => w.Descripcion == tipovisita).ToList();
            }

            return list;
        }

        public List<UsosMultiples> GetAllListaPrecios()
        {
            var list = new List<UsosMultiples>();

            if (DS_RepresentantesParametros.GetInstance().GetParLipCodigoClientes())
            {
                 list.AddRange(SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, Descripcion " +
                       "from UsosMultiples where UPPER(TRIM(CodigoGrupo)) = ? and codigouso in (SELECT DISTINCT LipCodigo from "+(myParametro.GetParTipoRelacionClientes() == 2? "ClientesDetalle" : "Clientes") +") order by Descripcion"
                       , new string[] {"LIPCODIGO"}));
            }else
            {
                 list = SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, Descripcion " +
                    "from UsosMultiples where UPPER(TRIM(CodigoGrupo)) = ? order by Descripcion", new string[] { "LIPCODIGO" });
            }


            if(list == null || list.Count == 0)
            {
                list = new List<UsosMultiples>()
                {
                    new UsosMultiples() { CodigoGrupo = "LIPCODIGO", CodigoUso = "*P.ProPrecio*", Descripcion = "Precio 1" },
                    new UsosMultiples() { CodigoGrupo = "LIPCODIGO", CodigoUso = "*P.ProPrecio2*", Descripcion = "Precio 2" },
                    new UsosMultiples() { CodigoGrupo = "LIPCODIGO", CodigoUso = "*P.ProPrecio3*", Descripcion = "Precio 3" }
                };
            }

            return list;
        }

        public List<UsosMultiples> GetTiposDepositos()
        {
            return GetUsoByCodigoGrupo("DEPTIPO");
        }

        public IEnumerable<UsosMultiples> GetTiposConexion()
        {
            List<UsosMultiples> list = new List<UsosMultiples>();
            UsosMultiples local = new UsosMultiples();
            local.CodigoUso = "1";
            local.Descripcion = "Local";
            UsosMultiples online = new UsosMultiples();
            online.CodigoUso = "2";
            online.Descripcion = "Online";

            list.Add(local);
            list.Add(online);

            return list;
        }

        public List<UsosMultiples> GetDevolucionAccion()
        {
            return SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, " +
                "Descripcion from UsosMultiples where trim(upper(CodigoGrupo)) = ? order by Descripcion", new string[] { "DEVACCION" });
        }

        public List<UsosMultiples> GetDevolucionCondicion()
        {
            return SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, " +
                "Descripcion from UsosMultiples where trim(upper(CodigoGrupo)) = ? order by Descripcion", new string[] { "DEVCONDICION" });
        }

        public string GetListaPreciosDescripcion(string lipCodigo)
        {

            List<UsosMultiples> list2 = new List<UsosMultiples>();

            List<UsosMultiples> list = SqliteManager.GetInstance().Query<UsosMultiples>("select ifnull(Descripcion, 'DEFAULT') as Descripcion from UsosMultiples " +
                "where trim(upper(CodigoGrupo)) = upper(?) and trim(upper(CodigoUso)) = upper(?)", new string[] { "LipCodigo", lipCodigo});

            if(list != null && list.Count > 0)
            {
                return list[0].Descripcion;
            }
            else
            {
                list2 = SqliteManager.GetInstance().Query<UsosMultiples>("select ifnull(Descripcion, 'DEFAULT') as Descripcion from UsosMultiples " +
                "where trim(upper(CodigoGrupo)) = upper(?) and trim(upper(CodigoUso)) = upper(?)", new string[] { "LISTAPRECIOS", lipCodigo });
            }

            if (list2 != null && list2.Count > 0)
            {
                return list2[0].Descripcion;
            }

            return null;
        }

        public string GetTipoComprobanteFacDescripcion(string tipoComprobante)
        {
            List<UsosMultiples> list = SqliteManager.GetInstance().Query<UsosMultiples>("select Descripcion from UsosMultiples " +
                "where trim(upper(CodigoGrupo)) = upper(?) and trim(upper(CodigoUso)) = upper(?)", new string[] { "NCFTIPO", tipoComprobante });

            if(list != null && list.Count > 0)
            {
                return list[0].Descripcion;
            }
            
            return null;
        }
        public string GetTipoComprobanteNCFDescripcion(string tipoComprobante)
        {
            List<UsosMultiples> list = SqliteManager.GetInstance().Query<UsosMultiples>("select Descripcion from UsosMultiples " +
                "where trim(upper(CodigoGrupo)) = upper(?) and trim(upper(CodigoUso)) = upper(?)", new string[] { "NCFTIPO2018", tipoComprobante });

            if(list != null && list.Count > 0)
            {
                return list[0].Descripcion;
            }
            
            return null;
        }

        public List<UsosMultiples> GetTipoComprobanteFacDescripciones(string tipoComprobante)
        {
            List<UsosMultiples> list = SqliteManager.GetInstance().Query<UsosMultiples>("select Descripcion from UsosMultiples " +
                "where trim(upper(CodigoGrupo)) = upper(?) and trim(upper(CodigoUso)) = upper(?)", new string[] { "NCFTIPO", tipoComprobante });

            if (list != null && list.Count > 0)
            {
                return list;
            }

            return null;
        }

        public List<UsosMultiples> GetCliCaracteristicas()
        {
            return GetUsoByCodigoGrupo("CLIDATOSOTROS", "Descripcion");
        }
     
        public List<UsosMultiples> GetInvAreas()
        {
            return GetUsoByCodigoGrupo("InvArea", "Descripcion");
        }

        public List<UsosMultiples> GetTiposGastos()
        {
            try
            {
                return GetUsoByCodigoGrupo("TIPOGASTOS", "Descripcion desc");
            }
            catch (Exception e)
            {
                //Crashes.TrackError(e);
                Console.Write(e.Message);
                return new List<UsosMultiples>();
            }
        }

        public List<UsosMultiples> GetFormasDePago()
        {
            try
            {
                return GetUsoByCodigoGrupo("FormaPago", "Descripcion desc");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<UsosMultiples>();
            }
        }

        public List<UsosMultiples> GetTiposNCF()
        {
            return GetUsoByCodigoGrupo("NCFTIPO");
        }

        public List<UsosMultiples> GetTiposTarjeta()
        {
            return GetUsoByCodigoGrupo("RefTarjetaPV");
        }

        public async Task<List<UsosMultiples>> GetTiposPresupuestos(string repCodigo,string repclave, int cliId ,bool IsOnline=false)
        {
            string query;
            try
            {
                if(IsOnline)
                {

                    ///Tipo 1: Presupuesto Cliente
                    ///Tipo 2: Presupuesto General
                    ///var url = new PreferenceManager().GetConnection().Url;
                   // api = ApiManager.GetInstance(url);

                    return await ApiManager.GetInstance(new PreferenceManager().GetConnection().Url).PresupuestosCombos<UsosMultiples>(repCodigo, repclave, cliId!=-1? 1:0, 1);

                    //query = "SELECT CodigoUso, Descripcion,CodigoGrupo FROM UsosMultiples WHERE(CodigoGrupo = 'PRETIPO') AND substr(CodigoUso, 5, 3) <> 'CLI' AND substr(CodigoUso, 4, 3) <> 'CTE' order by 2 ";
                }
                else { query = "SELECT distinct CodigoUso, Descripcion, CodigoGrupo FROM UsosMultiples U " +
                        "inner join Presupuestos P on trim(P.RepCodigo) = ? and PreTipo = CodigoUso " +
                        "where UPPER(trim(Codigogrupo)) = 'PRETIPO' and CodigoUso NOT LIKE '%CTE%' ORDER BY Descripcion ASC";}
                if (cliId != -1)
                {
                    query = "SELECT distinct CodigoUso, Descripcion, CodigoGrupo from UsosMultiples u " +
                       // "inner join Presupuestos p on trim(p.RepCodigo) = ? and PreTipo = CodigoUso " +
                        "where UPPER(trim(CodigoGrupo)) = 'PRETIPO' and CodigoUso like '%CTE%' order by Descripcion asc ";
                }

                return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { repCodigo.Trim() });
            }catch(Exception e)
            {
                Crashes.TrackError(e);
                Console.Write(e.Message);
            }

            return new List<UsosMultiples>();
        }

        public List<UsosMultiples> GetMotivosReclamaciones()
        {
            return GetUsoByCodigoGrupo("RECMOTIVO", "Descripcion");
        }

        public List<UsosMultiples> GetTiposPedidos()
        {
            return GetUsoByCodigoGrupo("PEDTIPOPEDIDO", "Descripcion");
        }

        public List<UsosMultiples> GetTiposCotizaciones()
        {
            return GetUsoByCodigoGrupo("CotTipo", "Descripcion");
        }

        public List<UsosMultiples> GetCentrosDeCostos()
        {
            return GetUsoByCodigoGrupo("GasCentroCosto", "Descripcion");
        }

        public List<UsosMultiples> GetTiposPagoCompras() { return GetUsoByCodigoGrupo("COMTIPOPAGO", "Descripcion"); }
        public List<UsosMultiples> GetTiposTransporte() { return GetUsoByCodigoGrupo("PedTipTrans", "Descripcion"); }
        public List<UsosMultiples> GetTiposCuentasBancarias() { return GetUsoByCodigoGrupo("TipoCtaBancaria", "Descripcion"); }
        public List<UsosMultiples> GetPedidosPrioridades() { return GetUsoByCodigoGrupo("PEDPRIORIDAD", "Descripcion"); }
        public List<UsosMultiples> GetCotizacionesPrioridades() { return GetUsoByCodigoGrupo("COTPRIORIDAD", "Descripcion"); }
        public List<UsosMultiples> GetPedidosCamposAdicionales()
        {
            /*return SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, Descripcion " +
                "from UsosMultiples where UPPER(TRIM(CodigoGrupo)) LIKE 'PEDOTROSDATOS%' order by Descripcion", new string[]{ });*/
            return GetCamposAdicionales(Modules.PEDIDOS);
        }

        public List<UsosMultiples> GetDevolucionesCamposAdicionales()
        {
            return GetCamposAdicionales(Modules.DEVOLUCIONES);
        }

        public List<UsosMultiples> GetCotizacionesCamposAdicionales()
        {
            List<UsosMultiples> result = null;
            if(Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                result = GetCamposAdicionales(Modules.COTIZACIONES);
            }

            if(result != null && result.Count == 0)
            {
                result = null;
            }

            return result;
        }

        private List<UsosMultiples> GetCamposAdicionales(Modules modulo)
        {
            var code = "-1";

            switch (modulo)
            {
                case Modules.PEDIDOS:
                    code = "PEDOTROSDATOS";
                    break;
                case Modules.DEVOLUCIONES:
                    code = "DEVOTROSDATOS";
                    break;
                case Modules.COTIZACIONES:
                    code = "COTOTROSDATOS";
                    break;
            }

            return SqliteManager.GetInstance().Query<UsosMultiples>("select CodigoGrupo, CodigoUso, Descripcion " +
                "from UsosMultiples where UPPER(TRIM(CodigoGrupo)) LIKE '" + code + "%' order by Descripcion", new string[] { });
        }

        public List<UsosMultiples> GetTiposClientes() { return GetUsoByCodigoGrupo("SOLTIPOCLIENTE", "Descripcion"); }
        public List<UsosMultiples> GetTiposLocales() { return GetUsoByCodigoGrupo("SOLTIPOLOCAL", "Descripcion"); }
        public List<UsosMultiples> GetTiposComprobante2018() { return GetUsoByCodigoGrupo("NCFTIPO2018", "Descripcion"); }
        public List<UsosMultiples> GetTiposReferenciasProspectos() { return GetUsoByCodigoGrupo("SOLREFTIPO", "Descripcion"); }

        public List<UsosMultiples> GetSeriesNCFGastos() { return GetUsoByCodigoGrupo("GASNCFSERIE", "Descripcion"); }

        public string GetFirstListaPrecio()
        {
            var LP = SqliteManager.GetInstance().Query<model.UsosMultiples>("Select * from UsosMultiples Where CodigoGrupo = 'LISTAPRECIOS' order by CodigoUso ", new string[] { });
            if (LP.Count > 0)
            {
                return LP[0].CodigoUso;
            }
            else
            {
                return null;
            }
        }

        public List<UsosMultiples> GetProductosTamanos()
        {
            string query = "select ifnull(trim(u.CodigoGrupo), '') as CodigoGrupo, ifnull(u.CodigoUso, '') as CodigoUso, " +
                    "u.Descripcion, u2.Descripcion as Orden from UsosMultiples u " +
                    "left join UsosMultiples u2 on u2.CodigoGrupo = 'PRODSIZEORDEN' and u2.CodigoUso = u.CodigoUso " +
                    " where UPPER(trim(u.CodigoGrupo)) = ?";


            return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { "PRODSIZE".ToUpper().Trim() });
        }
        public List<UsosMultiples> GetProductosUnidades()
        {
            string query = "select ifnull(trim(CodigoGrupo), '') as CodigoGrupo, ifnull(CodigoUso, '') as CodigoUso, " +
                    "Descripcion from UsosMultiples where CodigoGrupo = 'PRODUNDORDEN'";


            return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { "PRODSIZE".ToUpper().Trim() });
        }

        public List<UsosMultiples> GetProductosColores()
        {
            return GetUsoByCodigoGrupo("PRODCOLOR");
        }

        public List<UsosMultiples> GetFormatosImpresionPedidos()
        {
            return GetUsoByCodigoGrupo("FORMPED");
        }

        public List<UsosMultiples> GetMotivosExclusionClientes()
        {
            return GetUsoByCodigoGrupo("CLIEXCMOT");
        }

        public List<UsosMultiples> GetTiposDireccionesClientes()
        {
            return GetUsoByCodigoGrupo("CLDDIRTIPO");
        }

        public double GetPrecioDestino(string codigoGrupo, string orderBy = null, string codigoUso = null)
        {
            var LP = GetUsoByCodigoGrupo(codigoGrupo,orderBy,codigoUso);

            if (LP.Count > 0)
            {
                return Convert.ToDouble(LP[0].Descripcion);
            }
            else
            {
                return 0;
            }
        }

    }
}


using MovilBusiness.Configuration;

using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.structs;
using MovilBusiness.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
    public class DS_Presupuestos: DS_Controller
    {
        public ApiManager api { get; set; }

        public DS_Presupuestos()
        {
            api = ApiManager.GetInstance(new PreferenceManager().GetConnection().Url);
        }

        public async Task<List<KV>> GetPreAnyoByPreTipo(string preTipo, string repCodigo, string repclave, bool IsOnline)
        {
            var query = "select distinct PreAnio as Value from Presupuestos where upper(ltrim(rtrim(PreTipo))) = '" + preTipo.Trim().ToUpper() + "' and ltrim(rtrim(RepCodigo)) = '" + repCodigo.Trim() + "' order by PreAnio";

            if (IsOnline)
            {
                return await api.PresupuestosCombos<KV>(repCodigo, repclave, -1, 2, preTipo.Trim().ToUpper());
                // return api.RawQuery<KV>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, query);
            }

            return SqliteManager.GetInstance().Query<KV>(query, new string[] { });
        }

        public ClientesRebateData GetMontosRebate(int CliID)
        {
            string query = "Select  PrePresupuesto as Rebate, PreEjecutado as Acumulado " +
                " from Presupuestos " +
                "Where UPPER(trim(PreTipo)) = 'MNTDCLIREB' AND trim(PreReferencia) = '" + CliID + "'" +
                "AND (cast(PreMes as integer) = cast(strftime('%m', date()) as integer) " +
                "AND cast(PreAnio as integer) = cast(strftime('%Y', date()) as integer)) ";

            var list = SqliteManager.GetInstance().Query<ClientesRebateData>(query, new string[] { });

            if (list.Count > 0)
            {
                return new ClientesRebateData()
                {
                    Rebate = list[0].Rebate,
                    Acumulado = list[0].Acumulado
                };
            }

            return new ClientesRebateData()
            {
                Rebate = 0,
                Acumulado = 0
            };
        }

        public async Task<List<UsosMultiples>> GetPreMesByPreTipo(string preTipo, string preAnio, string repCodigo, string repclave, bool IsOnline, string CodigoGrupo)
        {
            string query = "Select distinct cast(PreMes as int) AS CodigoUso, " +
                    "u.Descripcion as Descripcion, " +
                    "PreAnio as CodigoGrupo from Presupuestos " +
                    "INNER JOIN UsosMultiples u on u.CodigoUso = cast(PreMes as int) and upper(u.CodigoGrupo) = '"+ CodigoGrupo + "' where ltrim(rtrim(upper(PreTipo))) = '" + preTipo.Trim().ToUpper() + "' and PreAnio = " + preAnio.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + repCodigo.Trim() + "' " +
                    "Order by Cast(PreMes as int);";

            //string query = "Select distinct cast(PreMes as int) AS CodigoUso, " +
            //        "(Case PreMes When 1 THEN 'Enero' When 2 THEN 'Febrero' When 3 THEN 'Marzo' WHEN 4 THEN 'Abril' WHEN 5 THEN 'Mayo' WHEN 6 THEN 'Junio' WHEN 7 THEN 'Julio' WHEN 8 THEN 'Agosto' WHEN 9 THEN 'Septiembre' WHEN 10 THEN 'Octubre' WHEN 11 THEN 'Noviembre' WHEN 12 THEN 'Diciembre' END) as Descripcion, " +
            //        "PreAnio as CodigoGrupo from Presupuestos where ltrim(rtrim(upper(PreTipo))) = '" + preTipo.Trim().ToUpper() + "' and PreAnio = " + preAnio.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + repCodigo.Trim() + "' " +
            //        "Order by Cast(PreMes as int);";

            if (IsOnline)
            {
                return await api.PresupuestosCombos<UsosMultiples>(repCodigo, repclave, -1, 3, preTipo.Trim().ToUpper(), preAnio);
                //return api.RawQuery<UsosMultiples>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, query);
            }

            return SqliteManager.GetInstance().Query<UsosMultiples>(query, new string[] { });
        }

        public async Task<double> GetPreMesByPreTipByDates(string preAnio, string preMes, string repCodigo, string Repclave)
        {
            if (!myParametro.GetParPresupuestosOnlineDefault())
            {
                string query = "Select PrePresupuesto from Presupuestos where ltrim(rtrim(upper(PreTipo))) = '" + myParametro.GetParPresupuestosForGetPreTipo().Trim().ToUpper() + "' and PreAnio = " + preAnio.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + repCodigo.Trim() + "' and PreMes = '" + preMes + "'";

                var presupuesto = SqliteManager.GetInstance().Query<Presupuestos>(query, new string[] { }).FirstOrDefault();

                return presupuesto != null ? presupuesto.PrePresupuesto : 0;
            }
            else
            {
                Presupuestos result = (await api.PresupuestosCargarOnline<Presupuestos>(repCodigo, Repclave, 0, myParametro.GetParPresupuestosForGetPreTipo().ToUpper().Trim(), preAnio, preMes)).FirstOrDefault();
                return result != null ? result.PrePresupuesto : 0.00;
            }
        }

        public async Task<List<Presupuestos>> GetPresupuestosByQuery(string query, string preTipo, string preAnio, string preMes, string repCodigo, string Repclave, bool IsOnline, int cliid = -1, string mayorque = "", string menorque = "")
        {
            if (!IsOnline)
            {
                if (string.IsNullOrEmpty(query))
                {
                    return new List<Presupuestos>();
                }

                if (!string.IsNullOrWhiteSpace(mayorque) && string.IsNullOrWhiteSpace(menorque))
                {
                    query += " AND ((PreEjecutado / PrePresupuesto) * 100)> " + mayorque + "";
                }

                if (!string.IsNullOrWhiteSpace(menorque) && string.IsNullOrWhiteSpace(mayorque))
                {
                    query += " AND ((PreEjecutado / PrePresupuesto) * 100)< " + menorque + "";
                }


                if (!string.IsNullOrWhiteSpace(menorque) && !string.IsNullOrWhiteSpace(mayorque))
                {
                    query += " AND ((PreEjecutado / PrePresupuesto) * 100) Between " + menorque + " And " + mayorque + "";
                }


                if (query.Contains("@RepCodigo"))
                {
                    query = query.Replace("@RepCodigo", "'" + repCodigo.Trim() + "'");
                }

                if (query.Contains("@PreAnio"))
                {
                    query = query.Replace("@PreAnio", preAnio);
                }

                if (query.Contains("@PreMes"))
                {
                    query = query.Replace("@PreMes", preMes);
                }

                if (query.Contains("@PreTipo"))
                {
                    query = query.Replace("@PreTipo", "'" + preTipo.ToUpper().Trim() + "'");
                }
                if (cliid != -1)
                {
                    
                        string codigoCliente = Arguments.Values.CurrentClient.CliCodigo;
                        int codigoInt = 0;
                        bool convertirAIntCodigoCliente = myParametro.GetParPresupuestosConvertirAIntCodigoCliente();
                        if (convertirAIntCodigoCliente)
                        {
                            int.TryParse(Arguments.Values.CurrentClient.CliCodigo, out codigoInt);
                            codigoCliente = codigoInt.ToString();
                        }
                    //query += "AND PreReferencia = '" + cliid + "'";
                    query += $"AND (PreReferencia = '{cliid}' OR PreReferencia LIKE '{codigoCliente}-%')";


                }
            }
            else
            {
                return await api.PresupuestosCargarOnline<Presupuestos>(repCodigo, Repclave, cliid, preTipo.ToUpper().Trim(), preAnio, preMes);
                //  return api.RawQuery<Presupuestos>(Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepClave, query);
            }

            return SqliteManager.GetInstance().Query<Presupuestos>(query.Replace("isnull", "ifnull").Replace("ISNULL", "IFNULL"), new string[] { });
        }
    }
}

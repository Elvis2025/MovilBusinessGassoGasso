
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Sectores : DS_Controller
    {
        public List<Sectores> GetSectoresByCliente(int cliId, string SecCodigo = "", bool orderByCode = false)
        {
            return SqliteManager.GetInstance().Query<Sectores>("select Distinct SecReferencia, SecDescripcion, s.SecCodigo as SecCodigo, c.ConID, " +
                "c.MonCodigo as MonCodigo, ifnull(c.AreaCtrlCredit, '0') as AreaCtrlCredit, ifnull("+(myParametro.GetParSectores() >= 2 ? "c.LiPCodigo" : "cli.LipCodigo")+", '') as LipCodigo, " +
                "c.CliIndicadorExonerado as CliIndicadorExonerado, estatus from Sectores s " +
                "inner join ClientesDetalle c on s.SecCodigo = c.SecCodigo " +
                "inner join Clientes cli on cli.CliID = c.CliID " +
                "where c.CliID = ? "+(!string.IsNullOrWhiteSpace(SecCodigo)? "AND S.SecCodigo = '" + SecCodigo + "'":"") + " order by " + (orderByCode ? "s.SecCodigo" : "s.SecDescripcion"), 
                new string[] { cliId.ToString() });
        }

        public List<Sectores> GetSectores(bool forRecibos = false)
        {
            return SqliteManager.GetInstance().Query<Sectores>("select SecReferencia, SecDescripcion, SecCodigo from sectores where 1=1 " + (forRecibos ? " " +
                "and SecCodigo in (select distinct SecCodigo from Recibos where ifnull(DepSecuencia, 0) = 0 " +
                "and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' union select distinct SecCodigo from RecibosConfirmados " +
                "where ifnull(DepSecuencia, 0) = 0 and trim(RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') " : "") + " " +
                "order by SecDescripcion", new string[] { });
        }

        public Sectores GetLastSectorVisitado(int cliId, bool validateCurrentVisit = false)
        {
            var list = SqliteManager.GetInstance().Query<Sectores>("select SecReferencia, SecDescripcion, SecCodigo from Sectores " +
                "where SecCodigo in (select SecCodigo from VisitasSectores s where ltrim(rtrim(RepCodigo)) = ? and VisSecuencia in (select VisSecuencia from Visitas where " +
                "("+(validateCurrentVisit ? "VisSecuencia = " + Arguments.Values.CurrentVisSecuencia.ToString() + " OR " : "")+"VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%') and CliID = ? " +
                "and VisSecuencia = s.VisSecuencia and ltrim(rtrim(RepCodigo)) = ? ) " +
                "order by VisSecuencia desc, VisPosicion desc limit 1)", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Sectores GetNextSectorAVisitar(int cliId, bool validateCurrentVisit = false, bool conEntregaDisponible = false)
        {
            /*
            se quito de la condicion de clientes detalle el repcodigo porque hay ocasiones en las que el cliente no 
            le pertenece al representante logeado. Ejemplo: replicaciones para repartidores
            */
            var where = "";

            if (!string.IsNullOrWhiteSpace(Arguments.Values.SecCodigoParaCrearVisita))
            {
                where = " OR trim(SecCodigo) = '"+Arguments.Values.SecCodigoParaCrearVisita.Trim()+"' ";

                Arguments.Values.SecCodigoParaCrearVisita = null;
            }

            var query = "select SecReferencia, SecDescripcion, SecCodigo from Sectores ss " +
               "where SecCodigo not in (select SecCodigo from VisitasSectores s where ltrim(rtrim(RepCodigo)) = ? and VisSecuencia " +
               "in (select VisSecuencia from Visitas where trim(RepCodigo) = ? and CliID = ? and (" + (validateCurrentVisit ? "VisSecuencia = " + Arguments.Values.CurrentVisSecuencia.ToString() + " OR " : "") + "VisFechaEntrada like '" + Functions.CurrentDate("yyyy-MM-dd") + "%'))) " +
               "and exists(select 1 from ClientesDetalle where   CliID = ? and SecCodigo = ss.SecCodigo) " + where;
               

            if (conEntregaDisponible)
            {
                var query2 = query + " and SecCodigo in (select SecCodigo from EntregasRepartidorTransacciones " +
                    "where CliID = ? and enrEstatusEntrega = 1 and trim(RepCodigo) = ? and SecCodigo = ss.SecCodigo limit 1)";

                var secEnt = SqliteManager.GetInstance().Query<Sectores>(query2, 
                    new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim(), cliId.ToString(), cliId.ToString(), cliId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                if(secEnt != null && secEnt.Count > 0)
                {
                    return secEnt[0];
                }else if (myParametro.GetParSectores() == 3)
                {
                    return null;
                }
            }

            query += " order by SecCodigo limit 1"; 

            return SqliteManager.GetInstance().Query<Sectores>(query,
               new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.CurrentUser.RepCodigo.Trim(), cliId.ToString(),
                     cliId.ToString() }).FirstOrDefault();
        }

        public Sectores GetSectorByCodigo(string SecCodigo, int cliId)
        {
            var list = SqliteManager.GetInstance().Query<Sectores>("select SecReferencia, SecDescripcion, s.SecCodigo as SecCodigo, c.ConID, " +
                 "c.MonCodigo as MonCodigo, ifnull(c.AreaCtrlCredit, '0') as AreaCtrlCredit, ifnull(" + (myParametro.GetParSectores() >= 2 ? "c.LiPCodigo" : "cli.LipCodigo") + ", '') as LipCodigo, " +
                 "c.CliIndicadorExonerado as CliIndicadorExonerado, estatus from Sectores s " +
                 "inner join ClientesDetalle c on s.SecCodigo = c.SecCodigo " +
                 "inner join Clientes cli on cli.CliID = c.CliID " +
                 "where c.CliID = ? and s.SecCodigo = ? " + (!string.IsNullOrWhiteSpace(SecCodigo) ? "AND S.SecCodigo = '" + SecCodigo + "'" : ""),
                 new string[] { cliId.ToString(), SecCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
       
        public Sectores GetSectorByCodigo(string SecCodigo)
        {
            return SqliteManager.GetInstance().
                Query<Sectores>($"select * from Sectores where SecCodigo = '{SecCodigo}'").FirstOrDefault();
        }       
       
        public Sectores GetSectorByRecibos(int recSecuencia)
        {
            return SqliteManager.GetInstance().
                Query<Sectores>($@"select * from Sectores where SecCodigo in 
                                (select seccodigo from recibos where recsecuencia = {recSecuencia})").FirstOrDefault();
        }       
    }
}

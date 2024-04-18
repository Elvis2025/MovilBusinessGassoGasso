using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Autorizaciones
    {
        public List<Autorizaciones> GetAutorizacionesByTitId(int titId)
        {
            try
            {
                return SqliteManager.GetInstance().Query<Autorizaciones>("select AutSecuencia, AutComentario, AutEstatus, " +
                    "ltrim(rtrim(ifnull(AutPin, ''))) as AutPin from Autorizaciones where ltrim(rtrim(RepCodigo)) = ? And TitID = ? " +
                    "And AutEstatus = 1 order by AutSecuencia asc limit 1", new string[] { Arguments.CurrentUser.RepCodigo, titId.ToString() });
            }
            catch (Exception e)
            {
                Console.Write(e);

            }
            return null;
        }

        public List<Autorizaciones> GetAutorizacionesActivas()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Autorizaciones>("select AutSecuencia, AutComentario, AutEstatus, " +
                    "ltrim(rtrim(ifnull(AutPin, ''))) as AutPin from Autorizaciones where ltrim(rtrim(RepCodigo)) = ? " +
                    "And AutEstatus = 1 order by AutSecuencia asc limit 1", new string[] { Arguments.CurrentUser.RepCodigo });
            }
            catch (Exception e)
            {
                Console.Write(e);

            }
            return null;
        }

        public List<Autorizaciones> GetAutorizacionesByTitIdfromLogin()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Autorizaciones>("select AutSecuencia, AutComentario, AutEstatus, " +
                    "ltrim(rtrim(ifnull(AutPin, ''))) as AutPin from Autorizaciones where " +
                    "AutEstatus = 1 order by AutSecuencia asc limit 1", new string[] {  });
            }
            catch (Exception e)
            {
                Console.Write(e);

            }
            return null;
        }

        public void MarkAutorizationAsUsed(int AutSecuencia, int TraSecuencia, int TitID, bool FromLogin = false)
        {
            Hash map = new Hash("Autorizaciones");
            map.Add("AutEstatus", 2);
            map.Add("TraSecuencia", TraSecuencia);
            map.Add("UsuInicioSesion", FromLogin ? "mdsoft" : Arguments.CurrentUser.RepCodigo);
            map.Add("AutFechaActualizacion", Functions.CurrentDate());
            map.Add("AutFechaAplicacion", Functions.CurrentDate());
            map.Add("TitID", TitID);

            map.ExecuteUpdate("AutSecuencia = " + AutSecuencia + (FromLogin ? " and TitID = 49" : " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "'"));
        }


        public List<double> GetPorcientoDescuentoByAutSecuencia(int autSecuencia)
        {
            try
            {
                List<Autorizaciones> list = SqliteManager.GetInstance().Query<Autorizaciones>("select AutReferencia from Autorizaciones where AutSecuencia = ?", new string[] { autSecuencia.ToString() });

                if (list != null && list.Count > 0)
                {
                    double resdes = double.Parse(list[0].AutReferencia.Split('-')[1]);
                      
                    List<double> result = new List<double>
                    {
                       resdes
                    };

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<double>();
        }
        public List<double> GetPorcientoDescuentoByAutSecuenciadoub(int autSecuencia)
        {
            try
            {
                List<Autorizaciones> list = SqliteManager.GetInstance().Query<Autorizaciones>("select AutReferencia from Autorizaciones where AutSecuencia = ?", new string[] { autSecuencia.ToString() });

                if (list != null && list.Count > 0)
                {
                    List<double> result = new List<double>
                    {
                        double.Parse(list[0].AutReferencia.Split('-')[1])
                    };

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<double>();
        }

        public double GetPorcientoDescuentoAutorizadoByReferencia(string Referencia)
        {
            try
            {
                List<Autorizaciones> list = SqliteManager.GetInstance().Query<Autorizaciones>("select a.AutReferencia from Autorizaciones a " +
                    "inner join CuentasxCobrar cxc on trim(cxc.cxcReferencia) = trim(substr(a.AutReferencia, 0, (length(trim(cxc.cxcReferencia)) + 1))) " +
                    "inner join RecibosDocumentosTemp r on r.AutID = a.AutSecuencia " +
                    "where trim(cxc.cxcReferencia) = trim(?) " +
                    "order by ((substr(a.AutReferencia, (length(trim(cxc.cxcReferencia))+1), length(a.AutReferencia) )))", new string[] { Referencia });

                if (list != null && list.Count > 0)
                {
                    return Convert.ToDouble(list[0].AutReferencia.Split('-')[1]);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public List<Autorizaciones> GetAutorizacionesByCxcDocumento(string cxcDocumento, int cliid)
        {
            return SqliteManager.GetInstance().Query<Autorizaciones>("select a.AutSecuencia as AutSecuencia, AutPin, AutComentario AS 'AutComentario', a.AutReferencia as AutReferencia  from Autorizaciones a " +
                   "inner join CuentasXCobrar cxc on trim(cxc.cxcDocumento) = trim (substr(a.AutReferencia, 0, (length(trim(cxc.cxcDocumento))+1) )) " +
                   "where a.AutEstatus = 1 AND cxc.clIID = ? AND trim(cxc.CxcDocumento) = trim(?)  " +
                   "order by ((substr(a.AutReferencia, (length(trim(cxc.cxcDocumento))+1), length(a.AutReferencia) ))) ", new string[] { cliid.ToString(), cxcDocumento });
        }

    }
}

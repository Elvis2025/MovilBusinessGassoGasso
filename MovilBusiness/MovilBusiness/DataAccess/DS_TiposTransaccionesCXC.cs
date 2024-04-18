
using MovilBusiness.model;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_TiposTransaccionesCXC
    {

        public string GetSiglaByReferencia(string sigla)
        {
            List<TiposTransaccionesCXC> list =  SqliteManager.GetInstance().Query<TiposTransaccionesCXC>("select ttcSigla from TiposTransaccionesCXC " +
                "where ltrim(rtrim(ttcReferencia)) = ?", new string[] { sigla });

            if(list.Count > 0)
            {
                return list[0].ttcSigla;
            }
            else
            {
                return "";
            }
        }

        public string GetCaracteristicaBySigla(string sigla)
        {
            List<TiposTransaccionesCXC> list = SqliteManager.GetInstance().Query<TiposTransaccionesCXC>("select ttcCaracteristicas from TiposTransaccionesCXC " +
                "where ltrim(rtrim(ttcSigla)) = ?", new string[] { sigla });

            if (list.Count > 0)
            {
                return list[0].ttcCaracteristicas;
            }
            else
            {
                return "";
            }
        }

        public bool ExistsSigla(string sigla)
        {
            return SqliteManager.GetInstance().Query<int>("select 1 from TiposTransaccionesCXC " +
                "where ltrim(rtrim(upper(ttcSigla))) = ?", new string[] { sigla.ToUpper() }).Count > 0;
        }

        public bool GetTipoTransaccionAplicaDescuento(string sigla)
        {
            if(string.IsNullOrEmpty(sigla))
            {
                return false;
            }

            var list = SqliteManager.GetInstance().Query<TiposTransaccionesCXC>("select ttcAplicaDescuento from TiposTransaccionesCXC " +
                "where ltrim(rtrim(upper(ttcSigla))) = ?", new string[] { sigla.ToUpper() });

            return list[0].ttcAplicaDescuento;
        }

       
    }
}

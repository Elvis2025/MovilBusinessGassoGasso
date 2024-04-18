using MovilBusiness.model;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_CuentasXCobrarAplicaciones
    {
        public DS_CuentasXCobrarAplicaciones()
        {

        }


        public List<CuentasXCobrarAplicaciones> GetCxcAplicaciones(string cxcReferencia)
        {
            var query = @"select cxca.CxcDocumento, ifnull(ttcxc.ttcSigla, '') as Sigla, cxca.CxcFechaAplicacion, cxca.CxcMonto, 
                         cxca.CXCNCF, ttcxc.ttcDescripcion from CuentasXCobrarAplicaciones cxca Left Join TiposTransaccionesCXC ttcxc On ttcxc.ttcID = cxca.CxcTipoTransaccion
                         where CXCReferencia = ?";

            return SqliteManager.GetInstance().Query<CuentasXCobrarAplicaciones>(query, new string[] { cxcReferencia });
        }

    }
}

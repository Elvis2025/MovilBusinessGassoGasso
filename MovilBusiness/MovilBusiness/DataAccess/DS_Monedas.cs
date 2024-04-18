
using MovilBusiness.Configuration;
using MovilBusiness.model;

using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Monedas
    {
        public List<Monedas> GetMonedas(string monCodigo = null, Monedas mainMoneda = null)
        {
            var query = "select MonCodigo, ifnull(MonNombre, '') || ' (' || ifnull(MonSigla, '') ||') - ' || " + (mainMoneda != null ? " case when " + mainMoneda.MonTasa.ToString("N2") + " > MonTasa then " + mainMoneda.MonTasa.ToString("N2") + " when MonTasa = " + mainMoneda.MonTasa.ToString("N2") + " then 1.00 else round(MonTasa,2) end " : "MonTasa") + "  as MonNombre, " +
                "ifnull(MonTasa, 0.0) as MonTasa, trim(upper(ifnull(MonSigla, ''))) as MonSigla from Monedas " + (monCodigo != null ? " " +
                "where trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "'))" : ""); 
            if (DS_RepresentantesParametros.GetInstance().GetParRecibosAutorizacionTazaFactura() > 0)
            {
                query = "select MonCodigo, ifnull(MonNombre, '') || ' (' || ifnull(MonSigla, '') ||') - ' || " + (mainMoneda != null ? " case when " + mainMoneda.MonTasa.ToString("N2") + " > MonTasa then " + mainMoneda.MonTasa.ToString("N2") + " when MonTasa <> " + mainMoneda.MonTasa.ToString("N2") + " or MonTasa = " + mainMoneda.MonTasa.ToString("N2") + "  then 1.00 else round(MonTasa,2) end " : "MonTasa") + "  as MonNombre, " +
                 (mainMoneda != null ? " case when MonSigla = 'USD' then ifnull(" + mainMoneda.MonTasa.ToString("N2") + ", 0.0) else ifnull(MonTasa,0.0) end as MonTasa " : " MonTasa ")+", trim(upper(ifnull(MonSigla, ''))) as MonSigla from Monedas " + (monCodigo != null ? " " +
               "where trim(upper(MonCodigo)) = trim(upper('" + monCodigo + "'))" : "");
            }

            return SqliteManager.GetInstance().Query<Monedas>(query, new string[] { });
        }

        public Monedas GetMoneda(string MonCodigo)
        {
            List<Monedas> list = GetMonedas(MonCodigo);

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<Monedas> GetMonedasSelect()
        {

            var where = "";

            if (DS_RepresentantesParametros.GetInstance().GetParMultiMonedaSoloMonedaCliente() && Arguments.Values.CurrentClient != null && !string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.MonCodigo))
            {
                where = " and MonCodigo = '"+Arguments.Values.CurrentClient.MonCodigo+"' ";
            }

            return SqliteManager.GetInstance().Query<Monedas>("Select MonCodigo, ifnull(MonNombre, '') ||' - Tasa:'||  ifnull(MonTasa, 0) as MonNombre" +
                " from Monedas where 1=1 "+where+" order by MonNombre", new string[] { });
        }

        public Monedas GetMonedaByMonCod(string MonCodigo)
        {
            return SqliteManager.GetInstance().Query<Monedas>("SELECT MonFechaActualizacion FROM Monedas where monCodigo = ? ", new string[] { MonCodigo }).FirstOrDefault();
        }
        public Monedas GetMonedaByMonCodForDep(string MonCodigo)
        {
            return SqliteManager.GetInstance().Query<Monedas>("SELECT MonSigla FROM Monedas where monCodigo = ? ", new string[] { MonCodigo }).FirstOrDefault();
        }

    }
}

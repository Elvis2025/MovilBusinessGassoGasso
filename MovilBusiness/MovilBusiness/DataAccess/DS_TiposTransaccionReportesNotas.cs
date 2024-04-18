using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    class DS_TiposTransaccionReportesNotas
    {
        public string GetNotaXTipoTransaccionReporte(int TitID, int TipFormato)
        {
            string resultado = "";

            try
            {
                var list = SqliteManager.GetInstance().Query<TiposTransaccionReportesNotas>("select ifnull(TipNota, '') as TipNota from TiposTransaccionReportesNotas " +
                       "where TitID = ? and TipFormato = ? ", new string[] { TitID.ToString(), TipFormato.ToString() });

                resultado = list.Count > 0 ? list[0].TipNota : "";
            }
            catch  
            {
                resultado = "";
            }
            return resultado;
        }
    }
}

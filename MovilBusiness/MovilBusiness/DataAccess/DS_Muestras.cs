using MovilBusiness.Configuration;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Muestras
    {

        public Muestras GetById(int mueSecuencia)
        {
            var list = SqliteManager.GetInstance().Query<Muestras>("select m.MueSecuencia as MueSecuencia, m.EstID as EstID, " +
                "m.CLIID as CLIID, m.CliNombre as CliNombre, m.MueFecha as MueFecha, " +
                "e.EstNombre as EstNombre from Muestras m " +
                "inner join Estudios e on e.EstID = m.EstID " +
                "where m.MueSecuencia = ? and trim(upper(m.RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim().ToUpper()+"'", new string[] { mueSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<MuestrasRespuestas> GetRespuestas(int mueSecuencia)
        {
            return SqliteManager.GetInstance().Query<MuestrasRespuestas>("select r.ResRespuesta as ResRespuesta, p.PreDescripcion as PreDescripcion from MuestrasRespuestas r " +
                "inner join Muestras m on m.MueSecuencia = r.MueSecuencia and m.RepCodigo = r.RepCodigo " +
                "inner join Preguntas p on p.PreID = r.PreID and p.EstID = m.EstID " +
                "where r.MueSecuencia = ? and trim(r.RepCodigo) = trim('"+Arguments.CurrentUser.RepCodigo+"') order by r.PreID", new string[] { mueSecuencia.ToString() });
        }
    }
}


using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args.services;

using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_SuscriptoresCambios
    {       
        public List<ExecuteQueryValues> GetAll(int limit = -1, string[] tablesToExclude = null)
        {
            var where = "";

            if (tablesToExclude != null && tablesToExclude.Length > 0)
            {
                string In = "";

                var firstStep = true;

                foreach(var table in tablesToExclude)
                {
                    In += (firstStep ? "'" + table.Trim().ToUpper() + "'" : ", '" + table.Trim().ToUpper() + "'");
                    firstStep = false;
                }

                where = " and trim(upper(Tabla)) not in ("+In+") ";
            }
            return SqliteManager.GetInstance().Query<ExecuteQueryValues>("select Script as Query, rowguid as rowguidtemp, Tabla as TableName, replace(RowguidTabla, '''', '') as rowguid, TipoScript as TipoScript, Posicion from SuscriptorCambios where 1=1 " + where + (limit != -1 ? " limit " + limit : ""), new object[] { });
        }

        public List<ExecuteQueryValues> GetPending30()
        {
            return GetAll((DS_RepresentantesParametros.GetInstance().GetCountCambios() > 0 ? DS_RepresentantesParametros.GetInstance().GetCountCambios() : 10));
        }

        public void Delete(List<ExecuteQueryValues> keys)
        {
            foreach (var cambio in keys)
            {
                SqliteManager.GetInstance().Execute("delete from SuscriptorCambios where Posicion = " + cambio.Posicion);
            }
        }

        public bool UpdateCambioEstadoInsertByRowguid(string traRowguid, int estatus)
        {
            var query = "select * from SuscriptorCambios " +
                "where trim(lower(RowguidTabla)) = lower('" + traRowguid.Trim() + "') and upper(TipoScript) = 'I'";

            var list = SqliteManager.GetInstance().Query<SuscriptorCambios>(query, new string[] { });

            if(list != null && list.Count > 0)
            {
                var cambio = list[0];

                var index = cambio.Script.IndexOf("VALUES(");

                if (index != -1)
                {
                    var estado = cambio.Script.Substring(index+7, 1);

                    cambio.Script = cambio.Script.Replace(") VALUES(" + estado.ToString() + ",", ") VALUES("+estatus.ToString()+",");

                    SqliteManager.GetInstance().Update(cambio);
                    return true;
                }
            }

            return false;
        }
    }
}

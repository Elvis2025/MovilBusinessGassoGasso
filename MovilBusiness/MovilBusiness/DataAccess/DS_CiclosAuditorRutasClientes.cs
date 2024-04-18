using MovilBusiness.Model;
using MovilBusiness.Utils;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_CiclosAuditorRutasClientes
    {
        public DS_CiclosAuditorRutasClientes() { }

        public void UpdateCiclosAuditor(int cliid)
        {
            var d = new Hash("CiclosAuditorRutasClientes")
            {
                { "CicCliIndicadorVisitado", 1 },
                {"CicFechaActualizacion",  Functions.CurrentDate()}
            };
            d.ExecuteUpdate("Cliid = " + cliid + "");
        }

        public bool GetResultOfCiclos(int cliid)
        {
            var list =  SqliteManager.GetInstance().Query<CiclosAuditorRutasClientes>("select CicCliIndicadorVisitado from CiclosAuditorRutasClientes where cliid = ?", new string[] { cliid.ToString() }).FirstOrDefault();
            return list != null && list.CicCliIndicadorVisitado > 0;
        }
    }
}

using MovilBusiness.Configuration;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_EntregasRepartidor: DS_Controller
    {
        private static DS_EntregasRepartidor Instance;

        public static DS_EntregasRepartidor GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DS_EntregasRepartidor();
            }

            return Instance;
        }
        public string GetEntregaRepartidorNumeroFactura(int cliId, int venSecuencia, string venNCF, bool confirmada)
        {
            string numeroFactura = "";
            //string tabla = confirmada ? "EntregasRepartidorTransaccionesConfirmados" : "EntregasRepartidorTransacciones";
            string query = $@"select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidor e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {venSecuencia} and e.VenNCF ='{venNCF}'
                            union
                            select ifnull(venNumeroERP,'') as venNumeroERP from  EntregasRepartidorConfirmados e  
                            where e.CliID = {cliId} and e.TraSecuencia =  {venSecuencia} and e.VenNCF ='{venNCF}' ";
            var entregaRepartidor = SqliteManager.GetInstance().Query<EntregasRepartidor>(query).FirstOrDefault();
            if (entregaRepartidor != null)
            {
                numeroFactura = entregaRepartidor.EnrNumeroERP;
            }
            return numeroFactura;
        }

        public EntregasRepartidor GetEntregaRepartidor(int enrSecuencia, bool confirmada)
        {
            return SqliteManager.GetInstance().Query<EntregasRepartidor>("select * from " + (confirmada ? "EntregasRepartidorConfirmados" : "EntregasRepartidor") + " e " +
                "inner join Clientes c on c.CliID = e.CliID " +
                "where e.EntSecuencia = ? and trim(e.RepCodigo) = ? ",
                new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).FirstOrDefault();
        }

        public EntregasRepartidor GetFechaEntregaRepartidor(int enrSecuencia, bool confirmada)
        {
            try
            {
                List<EntregasRepartidor> list = SqliteManager.GetInstance().Query<EntregasRepartidor>("select EnrFecha from " + (confirmada ? "EntregasRepartidorConfirmados" : "EntregasRepartidor") +" e " +
                "where e.EnrSecuencia = ? and trim(e.RepCodigo) = ? ", new string[] { enrSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return new EntregasRepartidor();
        }

    }
}

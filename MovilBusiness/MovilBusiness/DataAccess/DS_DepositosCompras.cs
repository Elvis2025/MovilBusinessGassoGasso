using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;

namespace MovilBusiness.DataAccess
{
    public class DS_DepositosCompras : DS_Controller
    {
        public int SaveDeposito(ComprasDepositarRango rango, double montoCajaChica)
        {
            int depSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("DepositosCompras");

            var dep = new Hash("DepositosCompras");
            dep.Add("DepEstatus", 1);
            dep.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            dep.Add("DepSecuencia", depSecuencia);
            dep.Add("DepMonto", rango.MontoComprado);
            dep.Add("DepCompraDesde", rango.MinComSecuencia);
            dep.Add("DepCompraHasta", rango.MaxComSecuencia);

            if (myParametro.GetParCuadres() > 0)
            {
                dep.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }           

            dep.Add("DepCantidadCompra", rango.CantidadCompras);
            dep.Add("DepMontoCajaChica", montoCajaChica);
            dep.Add("DepFecha", Functions.CurrentDate());           
            dep.Add("rowguid", Guid.NewGuid().ToString());
            dep.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            dep.Add("DepFechaActualizacion", Functions.CurrentDate());
            dep.Add("mbVersion", Functions.AppVersion);
            dep.ExecuteInsert();

            var com = new Hash("Compras");
            com.Add("DepSecuencia", depSecuencia);
            com.Add("ComFechaActualizacion", Functions.CurrentDate());
            com.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            com.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ifnull(ComEstatus, 0) <> 0 and ifnull(DepSecuencia, 0) = 0");

            DS_RepresentantesSecuencias.UpdateSecuencia("DepositosCompras", depSecuencia);
 
            return depSecuencia;
        }

        public void EstDepositos(string rowguid, int est)
        {
            Hash ped = new Hash("DepositosCompras");
            ped.Add("DepEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
            //ped.ExecuteUpdate("DepSecuencia = " + depSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public DepositosCompras GetBySecuencia(int depSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<DepositosCompras>("select RepCodigo, DepSecuencia, DepMonto, " +
                "DepCompraDesde, DepCompraHasta, DepCantidadCompra, DepMontoCajaChica, ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','' ), '') as DepFecha, DepEstatus from " +
                (confirmado? "DepositosComprasConfirmados" : "DepositosCompras") + " where DepSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", 
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
    }
}

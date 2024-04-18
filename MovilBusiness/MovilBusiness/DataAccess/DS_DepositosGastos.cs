using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_DepositosGastos
    {
        public int SaveDepositoGastos(List<Gastos> gastos)
        {
            var depSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("DepositosGastos");

            Hash dep = new Hash("DepositosGastos");
            dep.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            dep.Add("DepSecuencia", depSecuencia);
            dep.Add("DepFecha", Functions.CurrentDate());
            dep.Add("DepEstatus", 1);
            dep.Add("DepCantidadGastos", gastos.Count);

            var montoTotal = gastos.Sum(x => x.GasMontoTotal);
            var gastoDesde = gastos.Min(x => x.GasSecuencia);
            var gastoHasta = gastos.Max(x => x.GasSecuencia);

            dep.Add("DepMonto", montoTotal);
            dep.Add("DepGastoDesde", gastoDesde);
            dep.Add("DepGastoHasta", gastoHasta);
            dep.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            dep.Add("rowguid", Guid.NewGuid().ToString());
            dep.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            dep.Add("DepFechaActualizacion", Functions.CurrentDate());
            dep.Add("RepSupervisor", Functions.AppVersion);
            dep.ExecuteInsert();

            foreach(var gasto in gastos)
            {
                Hash gas = new Hash("Gastos");
                gas.Add("DepSecuencia", depSecuencia);
                gas.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                gas.Add("GasFechaActualizacion", Functions.CurrentDate());
                gas.ExecuteUpdate("GasSecuencia = " + gasto.GasSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("DepositosGastos", depSecuencia);

            return depSecuencia;
        }

        public DepositosGastos GetBySecuencia(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<DepositosGastos>("select RepCodigo, DepSecuencia, CAST(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','') as varchar) as DepFecha, DepCantidadGastos, DepMonto, DepGastoDesde, " +
                "DepGastoHasta from DepositosGastos where DepSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo }).FirstOrDefault();
        }
    }
}

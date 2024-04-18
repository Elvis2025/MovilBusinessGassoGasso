
using MovilBusiness.model;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_CuentasBancarias
    {
        public List<CuentasBancarias> GetCuentasBancarias()
        {
            return GetCuentasBancarias(null);
        }
        public List<CuentasBancarias> GetCuentasBancarias(string sector, string moncodigo = "")
        {
            string sql = "select CubNombre, CuBID from CuentasBancarias where ifnull(CubNombre, '') <> '' ";

            if(!string.IsNullOrWhiteSpace(sector))
            {
                sql += " and ltrim(rtrim(SecCodigo)) = '"+sector.Trim()+"'";
            }

            if (!string.IsNullOrWhiteSpace(moncodigo))
            {
                sql += " and MonCodigo = '" + moncodigo + "' ";
            }

            sql += " order by CuBNombre";

            return SqliteManager.GetInstance().Query<CuentasBancarias>(sql, new string[] { });
        }

        public int getCantidadCheque(int DepSecuencia, string Repcodigo)
        {
            int myResult = 0;
            string sql =
                    "SELECT count(*) from Recibos R " +                    
                    "WHERE trim(R.RepCodigo) = trim('" + Repcodigo + "') AND RecMontoCheque != 0 AND R.depsecuencia = " + DepSecuencia + " " +
                    "UNION ALL " +
                    "SELECT count(*) FROM RecibosConfirmados R " +
                    "WHERE trim(R.RepCodigo) = trim('" + Repcodigo + "')  AND RecMontoCheque != 0 AND R.depsecuencia = " + DepSecuencia + " ";
            var list = SqliteManager.GetInstance().Query<int>(sql, new string[] { });

            for (int v = 0; v < list.Count; v++)
            {
                myResult += list[0];
            }

            return myResult;
        }

        public string getNombreBanco(int CuBID)
        {
            var list = SqliteManager.GetInstance().Query<CuentasBancarias>("Select CuBNombre from cuentasbancarias where CuBID =" + CuBID, new string[] { });
            return list != null && list.Count > 0 ? list[0].CuBNombre : "";
        }

    }
}

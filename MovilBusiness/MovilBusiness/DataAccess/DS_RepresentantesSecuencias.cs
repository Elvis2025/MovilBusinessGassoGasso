using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_RepresentantesSecuencias
    {
        public static int GetLastSecuencia(string tableName)
        {
            int Secuencia = 1;

            try
            {
                List<RepresentantesSecuencias> list = SqliteManager.GetInstance().Query<RepresentantesSecuencias>("select ifnull(RepSecuencia, 0) as RepSecuencia from RepresentantesSecuencias where ltrim(rtrim(UPPER(RepTabla))) = ? and ltrim(rtrim(UPPER(RepCodigo))) = '" + Arguments.CurrentUser.RepCodigo.Trim().ToUpper() + "'", tableName.Trim().ToUpper());

                if (list.Count > 0)
                {
                    Secuencia = list[0].RepSecuencia + 1;
                }

                int secuenciaAlterna = VerificarSecuencia(tableName, Secuencia);

                if (secuenciaAlterna > Secuencia)
                {
                    Secuencia = secuenciaAlterna;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return Secuencia;
        }

        private static int VerificarSecuencia(string table, int secuencia)
        {
            string query = "";
            
            switch (table)
            {
                case "Pedidos":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(PedSecuencia), 0) as RepSecuencia from Pedidos where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(PedSecuencia), 0) as RepSecuencia from PedidosConfirmados where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Compras":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(ComSecuencia), 0) as RepSecuencia from Compras where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(ComSecuencia), 0) as RepSecuencia from ComprasConfirmados where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "NCDPP":
                    query = "select max(NCDSecuencia) as RepSecuencia from NCDPP where ltrim(rtrim(RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' ";
                    break;
                case "Cotizaciones":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(CotSecuencia), 0) as RepSecuencia from Cotizaciones where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(CotSecuencia), 0) as RepSecuencia from CotizacionesConfirmados where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Ventas":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(VenSecuencia), 0) as RepSecuencia from Ventas where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(VenSecuencia), 0) as RepSecuencia from VentasConfirmados where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Cuadres":
                    query = "select ifnull(max(CuaSecuencia), 0) as RepSecuencia from Cuadres where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "Devoluciones":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(DevSecuencia), 0) as RepSecuencia from Devoluciones where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(DevSecuencia), 0) as RepSecuencia from DevolucionesConfirmadas where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Recibos":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(RecSecuencia), 0) as RepSecuencia from Recibos where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(RecSecuencia), 0) as RepSecuencia from RecibosConfirmados where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Depositos":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(DepSecuencia), 0) as RepSecuencia from Depositos where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(DepSecuencia), 0) as RepSecuencia from DepositosConfirmados where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "EntregasDocumentos":
                    query = "select ifnull(max(EntSecuencia), 0) as RepSecuencia from (select ifnull(max(EntSecuencia), 0) as EntSecuencia EntregasDocumentos where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "' union select select ifnull(max(EntSecuencia), 0) as EntSecuencia EntregasDocumentosConfirmados where ltrim(rtrim(RepCodigo)) = '"+ Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "Visitas":
                    query = "select ifnull(max(VisSecuencia), 0) as RepSecuencia from Visitas where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "InventarioFisico":
                    query = "select ifnull(max(invSecuencia), 0) as RepSecuencia from InventarioFisico where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "RequisicionesInventario":
                    query = "select ifnull(max(ReqSecuencia),0) as RepSecuencia from  RequisicionesInventario where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "AuditoriasMercados":
                    query = "select ifnull(max(AudSecuencia), 0) as RepSecuencia from AuditoriasMercados where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "SolicitudActualizacionClientes":
                    query = "select ifnull(max(SACSecuencia), 0) as RepSecuencia from SolicitudActualizacionClientes where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "Gastos":
                    query = "select ifnull(max(GasSecuencia), 0) as RepSecuencia from Gastos where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "DepositosGastos":
                    query = "select ifnull(max(DepositosGastos), 0) as RepSecuencia from DepositosGastos where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "ReplicacionesSuscriptoresSincronizaciones":
                    query = "select ifnull(max(RssSecuencia), 0) as RepSecuencia from ReplicacionesSuscriptoresSincronizaciones where ltrim(rtrim(UsuInicioSesion)) = ? ";
                    break;
                case "Reclamaciones":
                    query = "select ifnull(max(RecSecuencia), 0) as RepSecuencia from Reclamaciones where ltrim(rtrim(RepCodigo)) = ? ";
                    break;
                case "DepositosCompras":
                    query = "select max(RepSecuencia) as RepSecuencia from (select ifnull(max(DepSecuencia), 0) as RepSecuencia from DepositosCompras where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' union select ifnull(max(DepSecuencia), 0) as RepSecuencia from DepositosComprasConfirmados where ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "') h ";
                    break;
                case "EntregasTransacciones":
                    query = "select max(EntSecuencia) as RepSecuencia from EntregasTransacciones where ltrim(rtrim(RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo+"'";
                    break;
                case "SolicitudExclusionClientes":
                    query = "select ifnull(max(SolSecuencia),0) as RepSecuencia from SolicitudExclusionClientes where trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'";
                    break;
                case "Conduces":
                    query = "select max(ConSecuencia) as RepSecuencia from Conduces where trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'";
                    break;
                case "TransaccionesCanastos":
                    query = "select ifnull(max(TraSecuencia),0) as TraSecuencia from TransaccionesCanastos";

                    if (string.IsNullOrEmpty(query))
                    {
                        return secuencia;
                    }

                    var list1 = SqliteManager.GetInstance().Query<TransaccionesCanastos>(query, Arguments.CurrentUser.RepCodigo.Trim());

                    if (list1.Count > 0)
                    {
                        if (list1[0].TraSecuencia >= secuencia)
                        {
                            return VerificarSecuencia(table, list1[0].TraSecuencia + 1);
                        }
                    }

                    return secuencia;
                    

            }

            if (string.IsNullOrEmpty(query))
            {
                return secuencia;
            }

            var list = SqliteManager.GetInstance().Query<RepresentantesSecuencias>(query, Arguments.CurrentUser.RepCodigo.Trim());

            if (list.Count > 0)
            {
                if(list[0].RepSecuencia >= secuencia)
                {
                    return VerificarSecuencia(table, list[0].RepSecuencia + 1);
                }
            }

            return secuencia;
        }

        public static void UpdateSecuencia(string table, int secuencia)
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesSecuencias>("select ifnull(RepSecuencia, 0) as RepSecuencia, rowguid from RepresentantesSecuencias " +
                "where ltrim(rtrim(UPPER(RepTabla))) = ? and ltrim(rtrim(UPPER(RepCodigo))) = ?", new string[] { table.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim().ToUpper() });

            Hash map = new Hash("RepresentantesSecuencias");

            if (list.Count > 0)
            { //si tiene algun valor
                if(ExistsOrLess(table, secuencia))
                {
                    map.Add("RepSecuencia", "ifnull(RepSecuencia, 0) + 1", true);
                }
                else
                {
                    map.Add("RepSecuencia", secuencia);
                }
               
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo.Trim());
                //map.ExecuteUpdate("rowguid = '"+list[0].rowguid.Trim()+"'", true);

                map.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { Value = list[0].rowguid.Trim(), IsText = true } }, true);

            }
            else
            { //si no existe insertarlo

                map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo.Trim());
                map.Add("RepSecuencia", 1);
                map.Add("RepTabla", table.Trim());
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo.Trim());
                map.Add("RepFechaActualizacion", Functions.CurrentDate());
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.ExecuteInsert();
            }
        }

        private static bool ExistsOrLess(string reptabla, int secuencia)
        {
            try
            {

                var list = SqliteManager.GetInstance().Query<RepresentantesSecuencias>("select RepSecuencia from RepresentantesSecuencias where ltrim(rtrim(upper(RepTabla))) = ? and ltrim(rtrim(RepCodigo)) = ? and ? <= RepSecuencia", new string[] {reptabla.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim(), secuencia.ToString() });

                return list != null && list.Count > 0;

            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public static bool ExistsTablaEnSecuencia(string reptabla)
        {
            try
            {

                var list = SqliteManager.GetInstance().Query<RepresentantesSecuencias>("select RepSecuencia from RepresentantesSecuencias where ltrim(rtrim(upper(RepTabla))) = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { reptabla.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

                return list != null && list.Count > 0;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }
    }
}

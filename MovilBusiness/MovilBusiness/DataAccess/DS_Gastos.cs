using MovilBusiness.Configuration;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Gastos
    {

        public void GuardarGastos(List<Gastos> gastos, DS_TransaccionesImagenes myTraImg, bool isEditing = false)
        {
            foreach (Gastos gasto in gastos)
            {
                int gasSecuencia;

                if (isEditing)
                {
                    gasSecuencia = gasto.GasSecuencia;
                }
                else
                {
                    gasSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Gastos");
                }

                if (gasto.GasFecha != null && gasto.GasFecha.Trim().Length == 0)
                {
                    gasto.GasFecha = null;
                }

                Hash map = new Hash("Gastos");
                
                map.Add("GasSecuencia", gasSecuencia);
                map.Add("GasFecha", gasto.GasFecha);
                map.Add("GasRNC", gasto.GasRNC);
                map.Add("GasNombreProveedor", gasto.GasNombreProveedor);
                map.Add("GasNCF", gasto.GasNCF);
                map.Add("GasNCFFechaVencimiento", gasto.GasNCFFechaVencimiento);
                map.Add("GasTipo", gasto.GasTipo);
                map.Add("FopID", gasto.FopID);
                map.Add("GasComentario", gasto.GasComentario);
                map.Add("GasMontoTotal", gasto.GasMontoTotal);
                map.Add("GasMontoItebis", gasto.GasMontoItebis);
                map.Add("GasItebis", gasto.GasItebis);
                map.Add("GasEstatus", 1);
                map.Add("GasPropina", gasto.GasPropina);
                map.Add("GasNoDocumento", gasto.GasNoDocumento);
                map.Add("FechaUltimaActualizacion", Functions.CurrentDate());  
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("GasFechaActualizacion", Functions.CurrentDate());
                map.Add("mbVersion", Functions.AppVersion);
                map.Add("GasCentroCosto", gasto.GasCentroCosto);
                map.Add("GasTipoComprobante", gasto.GasTipoComprobante);
                map.Add("GasBaseImponible", gasto.GasBaseImponible);
                map.Add("GasFechaDocumento", gasto.GasFechaDocumento);

                if (isEditing)
                {
                    map.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = gasto.rowguid} }, true);
                }
                else
                {
                    map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();

                    DS_RepresentantesSecuencias.UpdateSecuencia("Gastos", gasSecuencia);
                }

                myTraImg.MarkToSendToServer("Gastos", gasSecuencia.ToString());
                myTraImg.DeleteTemp(false);
            }
        }

   

        public List<Transaccion> GetGastosByEstado(int gasEstado)
        {
            return SqliteManager.GetInstance().Query<Transaccion>("select GasSecuencia as TransaccionID, v.RepCodigo as RepCodigo, " +
                "(GasSecuencia||' - '||ltrim(rtrim(ifnull(GasNombreProveedor, '')))||'. Fecha: '||ifnull(GasFecha, '')) as TransacionDescripcion " +
                "from Gastos v where ltrim(rtrim(v.RepCodigo)) = ? and v.GasEstatus = ? order by GasSecuencia DESC",
                new string[] { Arguments.CurrentUser.RepCodigo, gasEstado.ToString() });
        }

        public void AnularGasto(int gasSecuencia)
        {
            Hash map = new Hash("Gastos");
            map.Add("GasEstatus", 0);
            map.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
            map.Add("GasFechaActualizacion", Functions.CurrentDate());
            map.Add("FechaUltimaActualizacion", Functions.CurrentDate());
            map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and GasSecuencia = " + gasSecuencia);
        }

        public Gastos GetGastoBysecuencia(int gasSecuencia, bool confirmado)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Gastos>("select G.*, ifnull(UM.Descripcion, '') as GasTipoDescripcion from  " + (confirmado ? "GastosConfirmados G" : "Gastos G") + " " +
                    " Inner join UsosMultiples UM on G.GasTipo = UM.CodigoUso and UM.CodigoGrupo = 'TIPOGASTOS' where GasSecuencia = ? " +
                    "and ltrim(rtrim(RepCodigo)) = ?", new string[] { gasSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);

            }

            return null;
        }

        public List<Gastos> GetGastosParaDepositar()
        {
            return SqliteManager.GetInstance().Query<Gastos>("select CAST(replace(strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','') as varchar) as GasFecha, GasSecuencia, GasNoDocumento, GasNombreProveedor, GasMontoTotal, GasRNC, GasNCF, GasComentario " +
                "from Gastos where ifnull(DepSecuencia, 0) = 0 order by GasSecuencia", new string[] { });
        }

        public List<Gastos> GetByDepSecuencia(int depSecuencia, bool confirmados = false)
        {
            return SqliteManager.GetInstance().Query<Gastos>("select CAST(replace(strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','') as varchar) as GasFecha, GasSecuencia, GasNoDocumento, GasNombreProveedor, GasMontoTotal, GasRNC, GasNCF, GasComentario " +
                "from " + (confirmados ? "GastosConfirmados" : "Gastos") + " where DepSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<Gastos> GetByDepSecuenciaFormato2(int depSecuencia, bool confirmados = false)
        {
            return SqliteManager.GetInstance().Query<Gastos>("select CAST(replace(strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','') as varchar) as GasFecha, GasSecuencia, GasNoDocumento, GasNombreProveedor, GasMontoTotal, GasRNC, GasNCF, GasComentario, " +
                "GasFechaDocumento, GasBaseImponible, GasItebis, GasMontoItebis, u.Descripcion as GasTipoDescripcion, GasPropina " +
                "from " + (confirmados ? "GastosConfirmados" : "Gastos") + " as g " +
                "left join UsosMultiples u on trim(upper(u.CodigoGrupo)) = 'TIPOGASTOS' and trim(upper(CAST(CodigoUso as INT))) = trim(upper(CAST(g.GasTipo as INT))) where DepSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public bool HayGastosParaDepositar()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Gastos>("select GasSecuencia from Gastos where ifnull(DepSecuencia, 0) = 0 and ltrim(rtrim(RepCodigo)) = ? limit 1", new string[] { Arguments.CurrentUser.RepCodigo}).Count > 0;
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }
    }
}

using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_PushMoneyPagos : DS_Controller
    {
        public int GuardarRecibo(List<RecibosDocumentosTemp> aplicados, List<FormasPagoTemp> formasPagoAgregadas, string cldCedula, bool IsEditing = false, int EditingResecuencia = -1)
        {
            var RecStatus = 1;
            int RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("PushMoneyPagos"); 
            
            int lastSecuenciaVisitas = 0;

            if (IsEditing)
            {
                lastSecuenciaVisitas = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");
            }
            if (!IsEditing)
            {
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }

            Hash rec = new Hash("PushMoneyPagos");
            rec.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            rec.Add("pusEstatus", IsEditing ? 9 : RecStatus);
            rec.Add("pusSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
            rec.Add("CliID", Arguments.Values.CurrentClient.CliID);
            rec.Add("pusFecha", Functions.CurrentDate());
            rec.Add("CLDCedula", cldCedula);
            rec.Add("pusNumero", "");            
            rec.Add("pusMontoNcr", 0.0);
            rec.Add("pusMontoEfectivo", formasPagoAgregadas.Where(x => x.FormaPago.ToUpper() == "EFECTIVO").Sum(x => x.Prima));
            var montoBono = formasPagoAgregadas.Where(x => x.FormaPago.ToUpper() == "BONO").Sum(x => x.Prima);
            rec.Add("pusMontoBono", montoBono);
            rec.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            rec.Add("VisSecuencia", IsEditing ? lastSecuenciaVisitas : Arguments.Values.CurrentVisSecuencia);
            rec.Add("DepSecuencia", 0);
            rec.Add("pusDivision", 0);
            rec.Add("MonCodigo", "");
            rec.Add("OrvCodigo", "");
            rec.Add("OfvCodigo", "");
            rec.Add("pusCantidadDetalleAplicacion", aplicados.Count);
            rec.Add("pusCantidadDetalleFormaPago", formasPagoAgregadas.Count);
            rec.Add("pusCantidadImpresion", 0);
            rec.Add("pusMontoTotal", formasPagoAgregadas.Sum(x => x.Prima));
            rec.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            rec.Add("RecFechaActualizacion", Functions.CurrentDate());
            
            rec.Add("mbVersion", Functions.AppVersion);
            rec.Add("RecTasa", 1);

            var rowguid = Guid.NewGuid().ToString();

            if (IsEditing)
            {
                var edited = GetReciboBySecuencia(EditingResecuencia, false);

                if(edited != null)
                {
                    rowguid = edited.rowguid;
                }
            }

            //rec.Add("PHONE_ID", "");
            if (IsEditing)
            {
                rec.ExecuteUpdate("ltrim(rtrim(Repcodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' And pusSecuencia = " + EditingResecuencia + " ");
            }
            else
            {
                rec.Add("rowguid", rowguid);
                rec.ExecuteInsert();
            }

            int reaSecuencia = 0;

            foreach (var Aplicado in aplicados)
            {
                var ap = new Hash("PushMoneyPagosAplicacion");
                ap.Add("PushMoneyPagosrowguid", rowguid);
                ap.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                //ap.Add("pusSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
                reaSecuencia++;
                ap.Add("ppaSecuencia", reaSecuencia);
                //ap.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                ap.Add("pxpReferencia", Aplicado.Referencia);
                ap.Add("pxpDocumento", Aplicado.Documento);
                ap.Add("pxpValor", Math.Abs(Aplicado.Pendiente));

                ap.Add("repCodigo2", Aplicado.RepCodigo);
                ap.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                ap.Add("RecFechaActualizacion", Functions.CurrentDate());
                

                if (IsEditing) {
                    ap.ExecuteUpdate("ltrim(rtrim(Repcodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' And PushMoneyPagosrowguid = " + rowguid + " " + (IsEditing ? "And ppaSecuencia = " + reaSecuencia + " " : ""));
                } else {
                    ap.Add("rowguid", Guid.NewGuid().ToString());
                    ap.ExecuteInsert();
                }

            }
            
            foreach (var fp in formasPagoAgregadas)
            {

                Hash pago = new Hash("PushMoneyPagosFormaPago");
                pago.Add("PushMoneyPagosrowguid", rowguid);
                pago.Add("ppfSecuencia", fp.RefSecuencia);
                pago.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                pago.Add("ForID", fp.ForID);
                pago.Add("RefValor", fp.Valor);
                pago.Add("RecPrima", fp.Prima);
                pago.Add("RecTasa", fp.Tasa);
                pago.Add("MonCodigo", fp.MonCodigo);
                pago.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                pago.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                pago.Add("RecFechaActualizacion", Functions.CurrentDate());
                pago.Add("rowguid", Guid.NewGuid().ToString());
                pago.Add("DenId", fp.DenID);
                pago.Add("PusCantidad", fp.PusCantidad);
                pago.Add("PusBonoCantidad", fp.BonoCantidad);
                                
                pago.ExecuteInsert();
            }

          /*  if (myParametro.GetParRecibosValidarMontoCabeceraVSdetalle())
            {
                if (Math.Abs(Convert.ToDouble(rec["recmontoefectivo"])) - MontoTotalEfectivo > 1)
                {
                    throw new Exception("El monto efectivo no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontocheque"])) - MontoTotalCheque > 1)
                {
                    throw new Exception("El monto de cheque no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontochequef"])) - MontoTotalChequeFuturista > 1)
                {
                    throw new Exception("El monto de cheque futurista no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontotransferencia"])) - MontoTotalTransferencia > 1)
                {
                    throw new Exception("El monto de transferencia no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["rectotal"])) - MontoTotalAplicado > 1)
                {
                    throw new Exception("Hay una diferencia entre el total de la cabecera y el detalle del recibo");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontodescuento"])) - MontoTotalDescuento > 1)
                {
                    throw new Exception("hay una diferencia entre el descuento de la cabecera y el detalle del recibo");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontotarjeta"])) - MontoTotalTarjeta > 1)
                {
                    throw new Exception("El monto de tarjeta no concuerda");
                }
            }*/

            if (IsEditing)
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("Visita", lastSecuenciaVisitas);

            }
            else
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("PushMoneyPagos", RecSecuencia);

            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }


            //ClearTemps();

            return IsEditing ? EditingResecuencia : RecSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 52 as TitID, count(*) as VisCantidadTransacciones," +
                "sum(a.pxpValor) as VisMontoTotal, sum(a.pxpValor) as VisMontoSinItbis, '' as VisComentario " +
                "from PushMoneyPagos p " +
                "inner join PushMoneyPagosAplicacion a on a.PushMoneyPagosrowguid = p.rowguid " +
                "where p.VisSecuencia = ? and p.RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public PushMoneyPagos GetReciboBySecuencia(int RecSecuencia, bool reciboConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<PushMoneyPagos>("select pusSecuencia, r.rowguid as rowguid, R.SecCodigo, pusCantidadImpresion, pusFecha, CliCodigo, CliNombre, " + (reciboConfirmado ? 1 : 0) + " as Confirmado, pusEstatus, " +
                "ifnull(C.CliContacto, '') as CliContacto,ifnull(C.CliCalle, '') as CliCalle, ifnull(r.ClDCedula, '') as ClDCedula, ifnull(C.CliUrbanizacion, '') as CliUrbanizacion, ifnull(C.CliRNC, '') as CliRNC, ifnull(C.CliTelefono, '') as CliTelefono, " +
                "r.MonCodigo, cd.ClDNombre as ClDNombre from " + (reciboConfirmado ? "PushMoneyPagosConfirmados" : "PushMoneyPagos") + " r inner join Clientes c on c.CliID = r.CliID inner join ClientesDependientes cd on cd.CliID = r.CliID " +
                "where pusSecuencia = ? and r.RepCodigo = ? ", new string[] { RecSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<PushMoneyPagosAplicacion> GetRecibosAplicacionBySecuencia(string pushMoneyRowguid, bool ReciboConfirmados)
        {
            string query = "select ifnull(a.PxpDocumento,'') as PxpDocumento, ifnull(a.PxpReferencia,'') as PxpReferencia, a.pxpValor as pxpValor " +
               "from " + (ReciboConfirmados ? "PushMoneyPagosAplicacionConfirmados a " : "PushMoneyPagosAplicacion a ") + "INNER JOIN PushMoneyPorPagar cc ON a.PxpReferencia = cc.PxpReferencia " +
               "where a.PushMoneyPagosrowguid = '"+pushMoneyRowguid+"' and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ";


            return SqliteManager.GetInstance().Query<PushMoneyPagosAplicacion>(query, new string[] { });
        }

        public List<PushMoneyPagosFormaPago> GetRecibosFormasPagoBySecuencia(string pushMoneyRowguid, bool reciboConfirmado)
        {
            return SqliteManager.GetInstance().Query<PushMoneyPagosFormaPago>("select '' as BanNombre, R.DenId, PusCantidad, ForID, d.DenDescripcion as DenDescripcion, PusBonoCantidad, RefValor, " +
                "RecTasa, RecPrima from " + (reciboConfirmado ? "PushMoneyPagosFormaPagoConfirmados" : "PushMoneyPagosFormaPago") + " R " +
                "left join Denominaciones d on d.denid = R.DenId  " +
                "where PushMoneyPagosrowguid = ? and RepCodigo = ? " + " order by ppfSecuencia",
                new string[] { pushMoneyRowguid, Arguments.CurrentUser.RepCodigo });
        }

        public void ActualizarCantidadImpresion(int recSecuencia, bool confirmado)
        {
            if (!confirmado)
            {
                Hash map = new Hash("PushMoneyPagos")
                {
                    SaveScriptForServer = false
                };
                map.Add("pusCantidadImpresion", "pusCantidadImpresion + 1", true);
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and pusSecuencia = " + recSecuencia);
                map.SaveScriptForServer = true;
                map.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and pusSecuencia = " + recSecuencia);

            }
            else
            {
                Hash map2 = new Hash("PushMoneyPagosConfirmados");
                map2.Add("pusCantidadImpresion", "pusCantidadImpresion + 1", true);
                map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map2.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and pusSecuencia = " + recSecuencia);
            }
        }

        public void EstRecibos(string rowguid,int est)
        {
            Hash ped = new Hash("PushMoneyPagos");
            ped.Add("pusEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
            //ped.ExecuteUpdate("pusSecuencia = " + recSecuencia + " and DepSecuencia = 0 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' ");

        }


        public List<PushMoneyPagosAplicacion> GetPushMoneyAplicacionBySecuencia(string pushMoneyRowguid, bool ReciboConfirmados)
        {
            string query = "select ProCodigo, ProDescripcion, PxpCantidad, PxpCantidadDetalle, PxpPrecio, PxpItbis from " + 
                            (ReciboConfirmados ? "PushMoneyPagosAplicacionConfirmados a " : "PushMoneyPagosAplicacion a ") +
                            "INNER JOIN PushMoneyPorPagarDetalle cc ON a.PxpReferencia = cc.PxpReferencia inner join " +
                            "Productos p on p.proid = cc.Proid where a.PushMoneyPagosrowguid = '" + pushMoneyRowguid + "'" +
                            "and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";


            return SqliteManager.GetInstance().Query<PushMoneyPagosAplicacion>(query, new string[] { });
        }

        public List<PushMoneyPagosAplicacion> GetPushMoneyAplicacionByReferencia(string pxpReferencia, bool ReciboConfirmados)
        {
            string query = "select ProCodigo, ProDescripcion, PxpCantidad, PxpCantidadDetalle, PxpPrecio, PxpItbis from " +
                            (ReciboConfirmados ? "PushMoneyPagosAplicacionConfirmados a " : "PushMoneyPagosAplicacion a ") +
                            "INNER JOIN PushMoneyPorPagarDetalle cc ON a.PxpReferencia = cc.PxpReferencia inner join " +
                            "Productos p on p.proid = cc.Proid where cc.PxpReferencia = '" + pxpReferencia + "'" +
                            "and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "'";


            return SqliteManager.GetInstance().Query<PushMoneyPagosAplicacion>(query, new string[] { });
        }

    }
}

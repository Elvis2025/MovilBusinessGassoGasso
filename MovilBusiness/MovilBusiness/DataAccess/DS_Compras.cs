using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_Compras : DS_Controller
    {
        private DS_Productos myProd;

        public DS_Compras(DS_Productos myProd = null)
        {
            this.myProd = myProd;

            if(this.myProd == null)
            {
                this.myProd = new DS_Productos();
            }
        }

        public int SaveCompra(ClientesDependientes dependiente, string tipoPago, List<FormasPagoTemp> formasPago, bool IsEditing = false, int editedSecuencia = -1, double montoTotal = -1)
        {
            int comSecuencia;

            if (!IsEditing)
            {
                comSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Compras");

                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }
            else
            {
                comSecuencia = editedSecuencia;
            }

            var productos = myProd.GetResumenProductos((int)Modules.COMPRAS);
            var totales = myProd.GetTempTotales((int)Modules.COMPRAS);

            var c = new Hash("Compras");
            c.Add("ComEstatus", 1);
            c.Add("ComFecha", Functions.CurrentDate());            

            if (!IsEditing)
            {
                c.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                c.Add("ComSecuencia", comSecuencia);
                c.Add("CliID", Arguments.Values.CurrentClient.CliID);
                c.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
                c.Add("rowguid", Guid.NewGuid().ToString());
            }

            c.Add("ComCantidadDetalle", productos.Count);
            c.Add("ComTotal", totales.Total);
            c.Add("ConID", Arguments.Values.CurrentClient.ConID);
            c.Add("ComNCF", "");

            c.Add("ComReferencia", "");

            if (myParametro.GetParCuadres() > 0 && !IsEditing)
            {
                c.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }
            c.Add("ComCantidadCanastos", 1);

            c.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            if (!myParametro.GetParComprasNoUsarDependiente())
            {
                if (!myParametro.GetDependienteNoObligatorio())
                {
                    c.Add("CLDCedula", dependiente.ClDCedula);
                }
            }

            c.Add("ComFechaActualizacion", Functions.CurrentDate());

            if (!IsEditing && myParametro.GetParSectores() > 0 && Arguments.Values.CurrentSector != null)
            {
                c.Add("SecCodigo", Arguments.Values.CurrentSector.SecCodigo);
            }
            
            c.Add("mbVersion", Functions.AppVersion);

            if(Arguments.Values.IsPushMoneyRotacion)
            {
                c.Add("ComTipo", 2);
            }else
            {
                c.Add("ComTipo", 1);
            }

            if (myParametro.GetParComprasTipoPago())
            {
                c.Add("ComTipoPago", tipoPago);

                if (!string.IsNullOrWhiteSpace(tipoPago))
                {
                    c.Add("DepSecuencia", 0);
                }
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(Arguments.Values.CurrentClient.LipCodigoPM) && Arguments.Values.CurrentClient.LipCodigoPM.ToUpper().Trim() == "COMFACT")
                {
                    c.Add("ComTipoPago", 2);
                }
                else
                {
                    c.Add("ComTipoPago", 1);
                }
            }

            if (IsEditing)
            {
                c.ExecuteUpdate("ComSecuencia = " + comSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");

                //eliminando los productos que quito
                var proIds = GetProIdQueryForDeleteWhileEditing(comSecuencia, (int)Modules.COMPRAS);
                if (!string.IsNullOrWhiteSpace(proIds))
                {
                    var delete = new Hash("ComprasDetalle");
                    delete.ExecuteDelete("ComSecuencia = " + comSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ProID in (" + proIds + ")");
                }
            }
            else
            {
                c.Add("rowguid", Guid.NewGuid().ToString());
                c.ExecuteInsert();
            }

            var pos = 1;

            var isCompraFactura = Functions.IsCompraFactura;

            foreach (var det in productos.OrderByDescending(x=>x.Posicion))
            {
                var d = new Hash("ComprasDetalle");
                d.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                d.Add("ComSecuencia", comSecuencia);
                d.Add("ComPosicion", pos); pos++;
                d.Add("ProID", det.ProID);
                d.Add("ComCantidad", (int)det.Cantidad);
                d.Add("ComCantidadDetalle", det.CantidadDetalle);
                if (isCompraFactura)
                {
                    d.Add("ComPrecio", det.PrecioTemp);
                }
                else
                {
                    d.Add("ComPrecio", det.Precio);
                }
                
                d.Add("ComItbis", det.Itbis);
                d.Add("ComSelectivo", det.Selectivo);
                d.Add("d.ComAdValorem", det.AdValorem);
                d.Add("ComDescuento", det.Descuento);
                d.Add("ComTotalItbis", 0);
                d.Add("ComTotalDescuento", 0);
                d.Add("ComindicadorOferta", false);
                d.Add("ComFechaActualizacion", Functions.CurrentDate());
                d.Add("cxcDocumento", det.Documento);
                d.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

                if(IsEditing && ExistsDetalleInCompra(det.ProID, comSecuencia))
                {
                    d.ExecuteUpdate("ProID = " + det.ProID + " and ComSecuencia = " + comSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
                }
                else
                {
                    d.Add("rowguid", Guid.NewGuid().ToString());
                    d.ExecuteInsert();
                }  
            }

            var forIdAutomatico = myParametro.GetParComprasFormaPagoAutomatica();

            if(forIdAutomatico > 0)
            {
                formasPago = new List<FormasPagoTemp>()
                {
                    new FormasPagoTemp(){ ForID = forIdAutomatico, BonoCantidad = 0, BonoDenominacion = 0, Valor = montoTotal, Tasa = 1, Prima = montoTotal, 
                        NoCheque = 0, Fecha = null, BanID = 0, MonCodigo = Arguments.Values.CurrentClient.MonCodigo }
                };
            }

            if (formasPago != null)
            {
                if (IsEditing)
                {//si es editando eliminado las formas de pago para volverlas a insertar
                    var d = new Hash("ComprasFormaPago");
                    d.ExecuteDelete("RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' and ComSecuencia = " + comSecuencia);
                }

                pos = 1;
                foreach (var pago in formasPago)
                {
                    var p = new Hash("ComprasFormaPago");
                    p.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    p.Add("ComSecuencia", comSecuencia);
                    p.Add("CfpSecuencia", pos); pos++;
                    p.Add("ForID", pago.ForID);
                    p.Add("CfpBonoDenominacion", pago.BonoDenominacion);
                    p.Add("CfpBonoCantidad", pago.BonoCantidad);
                    p.Add("CfpMonto", pago.Valor);
                    p.Add("BanID", pago.BanID);
                    p.Add("cfpNumeroCheque", pago.NoCheque);
                    p.Add("CfpFecha", pago.Fecha);
                    p.Add("cfpNumeroTransferencia", pago.NoCheque);
                    p.Add("cfpPrima", pago.Prima);
                    p.Add("cfpTasa", pago.Tasa);
                    p.Add("MonCodigo", pago.MonCodigo);
                    p.Add("comFechaActualizacion", Functions.CurrentDate());
                    p.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    p.Add("rowguid", Guid.NewGuid().ToString());
                    p.ExecuteInsert();
                }
            }

            if (!IsEditing)
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("Compras", comSecuencia);
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            myProd.ClearTemp((int)Modules.COMPRAS);

            return comSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 11 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(((d.ComPrecio + d.ComAdValorem + d.ComSelectivo) - d.ComDescuento) * ((case when d.ComCantidadDetalle > 0 then d.ComCantidadDetalle / o.ProUnidades else 0 end) + d.ComCantidad)) as VisMontoSinItbis, sum(((d.ComItbis / 100.0) * ((d.ComPrecio + d.ComAdValorem + d.ComSelectivo) - d.ComDescuento)) * ((case when d.ComCantidadDetalle > 0 then d.ComCantidadDetalle / o.ProUnidades else 0 end) + d.ComCantidad)) as VisMontoItbis from Compras p " +
                "inner join ComprasDetalle d on d.RepCodigo = p.RepCodigo and d.ComSecuencia = p.ComSecuencia " +
                "inner join Productos o on o.ProID = d.ProID " +
                "where p.VisSecuencia = ? and p.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                var item = list[0];

                item.VisMontoTotal = item.VisMontoSinItbis + item.VisMontoItbis;
                item.VisComentario = "";

                new DS_Visitas().GuardarVisitasResultados(item);
            }
        }

        private string GetProIdQueryForDeleteWhileEditing(int comSecuencia, int titId)
        {
            var list = SqliteManager.GetInstance().Query<ComprasDetalle>("select ProID from ComprasDetalle " +
                "where ComSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and ProID not in (select distinct ProID from ProductosTemp where TitID = ?)",
                new string[] { comSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), titId.ToString() });

            if (list != null && list.Count > 0)
            {
                bool first = true;

                var proIds = "";

                foreach (var pro in list)
                {
                    if (first)
                    {
                        first = false;
                        proIds = pro.ProID.ToString();
                    }
                    else
                    {
                        proIds += ", " + pro.ProID.ToString();
                    }
                }

                return proIds;
            }

            return null;
        }

        private bool ExistsDetalleInCompra(int proId, int comSecuencia)
        {
            return SqliteManager.GetInstance().Query<Compras>("select ComSecuencia from ComprasDetalle where ProID = ? and ComSecuencia = ? " +
                "and ltrim(rtrim(RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' ", 
                new string[] { proId.ToString(), comSecuencia.ToString() }).Count > 0;
        }

        public Compras GetBySecuencia(int comSecuencia, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<Compras>("select c.CliID as CliID, ComSecuencia,ComEstatus, CLDCedula, u.Descripcion as TipoPagoDescripcion, p.ComCantidadDetalle as ComCantidadDetalle, p.rowguid as rowguid, CliNombre, " +
                "CliCodigo, CliCalle, ComFecha, ComCantidadImpresion, CliUrbanizacion from " + (confirmado ? "ComprasConfirmados" : "Compras") + " p " +
                    "inner join Clientes c on c.CliID = p.CliID " +
                    "left join UsosMultiples u on ltrim(rtrim(upper(u.CodigoGrupo))) = 'COMTIPOPAGO' and u.CodigoUso = p.ComTipoPago " +
                    "where ltrim(rtrim(p.RepCodigo)) = ? and p.ComSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), comSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<ComprasDetalle> GetDetalleBySecuencia(int comSecuencia, bool confirmado)
        {
            return SqliteManager.GetInstance().Query<ComprasDetalle>("select trim(ProDescripcion) as ProDescripcion, d.rowguid as rowguid, " +
                "d.ComDescuento as PedDescuento, ifnull(p.ProUnidades, 1) as ProUnidades, ifnull(ComCantidad, 0.0) as ComCantidad, " +
                "ifnull(ComCantidadDetalle, 0.0) as ComCantidadDetalle, trim(ifnull(p.ProCodigo, '')) as ProCodigo, d.ComItbis as ComItbis, " +
                "ifnull(ComPrecio, 0.0) as ComPrecio " +
                "from " + (confirmado ? "ComprasDetalleConfirmados" : "ComprasDetalle") + " d " +
                "inner join Productos p on p.ProID = d.ProID " +
                "where d.ComSecuencia = ? and ltrim(rtrim(d.RepCodigo)) = ? order by p.ProDescripcion", 
                new string[] { comSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public bool CompraFueDepositada(int comSecuencia)
        {
            return SqliteManager.GetInstance().Query<Compras>("select ComSecuencia from Compras where ComSecuencia = ? and ltrim(rtrim(RepCodigo)) = ? and IFNULL(DepSecuencia, 0) <> 0 limit 1", new string[] { comSecuencia.ToString(), Arguments.CurrentUser.RepCodigo }).Count > 0;
        }

        public void EstCompra(string rowguid, int est)
        {
            Hash ped = new Hash("Compras");
            ped.Add("ComEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
            ped.Add("ComFechaActualizacion", Functions.CurrentDate());

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            //ped.ExecuteUpdate("ComSecuencia = " + comSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }


        public void ActualizarComCantidadImpresion(string rowguid, bool confirmado)
        {
            Hash ped = new Hash((confirmado?"ComprasConfirmados": "Compras"));
            ped.Add("ComCantidadImpresion", "ifnull(ComCantidadImpresion, 0) + 1", true);
            ped.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            ped.Add("ComFechaActualizacion", Functions.CurrentDate());
            //ped.ExecuteUpdate("ComSecuencia = " + comSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }

        public void InsertarCompraInTemp(int pedSecuencia, bool confirmado)
        {
            myProd.ClearTemp((int)Modules.COMPRAS);

            var query = "select distinct "+ ((int)Modules.COMPRAS).ToString() + " as TitID, pd.ComPosicion as Posicion, pd.ComCantidad as Cantidad, pd.ComCantidadDetalle as CantidadDetalle, pd.cxcDocumento as Documento, pd.rowguid as rowguid, pd.ProID as ProID, pd.ComPrecio as Precio, " +
                "p.ProDescripcion as Descripcion, pd.ComItbis as Itbis, pd.ComSelectivo as Selectivo, ifnull(p.UnmCodigo, '') as UnmCodigo, " +
                "ifnull(pd.ComindicadorOferta, 0) as IndicadorOferta, pd.ComDescuento as Descuento "+(Application.Current.Properties.ContainsKey("SecCodigo") ? ", '"+ Application.Current.Properties["SecCodigo"] + "' as SecCodigo" : "") +" from " + (confirmado ? "ComprasDetalleConfirmados" : "ComprasDetalle") + " pd " +
                "inner join Productos p on p.ProID = pd.ProID where ltrim(rtrim(pd.RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and pd.ComSecuencia = ? order by p.ProDescripcion";

            var list = SqliteManager.GetInstance().Query<ProductosTemp>(query, new string[] { pedSecuencia.ToString() });

            SqliteManager.GetInstance().InsertAll(list);

        }

        public ComprasDepositarRango GetRangoComprasADepositar()
        {
            var query = " Select MIN(compras.minimo) as MinComSecuencia, Max(compras.maximo) as MaxComSecuencia, SUM(CantidadCompras) as CantidadCompras, sum(MontoComprado) as MontoComprado from (SELECT MIN(c.ComSecuencia) as minimo, MAX(c.ComSecuencia) as maximo, Count(c.ComSecuencia) as CantidadCompras, SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(d.ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) as MontoComprado FROM Compras c " +
                "inner join ComprasDetalle d on d.RepCodigo = c.RepCodigo and d.ComSecuencia = c.ComSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "WHERE ltrim(rtrim(c.RepCodigo)) = ? AND IFNULL(c.DepSecuencia, 0) = 0 and c.ComEstatus <> 0 " +
                    "Union  " +
                    "SELECT MIN(c.ComSecuencia) as minimo, MAX(c.ComSecuencia) as maximo, COUNT(c.ComSecuencia) as CantidadCompras, SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(d.ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) as MontoComprado FROM ComprasConfirmados c " +
                    "inner join ComprasDetalleConfirmados d on d.RepCodigo = c.RepCodigo and d.ComSecuencia = c.ComSecuencia " +
                    "inner join Productos p on p.ProID = d.ProID " +
                    "WHERE ltrim(rtrim(c.RepCodigo)) = ? AND IFNULL(c.DepSecuencia, 0) = 0 and c.ComEstatus <> 0) compras ";

            var list = SqliteManager.GetInstance().Query<ComprasDepositarRango>(query, new string[] { Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepCodigo });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public ComprasDepositarRango GetRangoComprasADepositarFromDetalle(int depsecuencia)
        {
            var query = " Select MIN(compras.minimo) as MinComSecuencia, Max(compras.maximo) as MaxComSecuencia, SUM(CantidadCompras) as CantidadCompras, sum(MontoComprado) as MontoComprado from (SELECT MIN(c.ComSecuencia) as minimo, MAX(c.ComSecuencia) as maximo, Count(c.ComSecuencia) as CantidadCompras, SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(d.ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) as MontoComprado FROM Compras c " +
                "inner join ComprasDetalle d on d.RepCodigo = c.RepCodigo and d.ComSecuencia = c.ComSecuencia " +
                "inner join Productos p on p.ProID = d.ProID " +
                "WHERE ltrim(rtrim(c.RepCodigo)) = ? AND c.DepSecuencia = " + depsecuencia + " and c.ComEstatus <> 0 " +
                    "Union  " +
                    "SELECT MIN(c.ComSecuencia) as minimo, MAX(c.ComSecuencia) as maximo, COUNT(c.ComSecuencia) as CantidadCompras, SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(d.ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) as MontoComprado FROM ComprasConfirmados c " +
                    "inner join ComprasDetalleConfirmados d on d.RepCodigo = c.RepCodigo and d.ComSecuencia = c.ComSecuencia " +
                    "inner join Productos p on p.ProID = d.ProID " +
                    "WHERE ltrim(rtrim(c.RepCodigo)) = ? AND c.DepSecuencia = " +depsecuencia+ " and c.ComEstatus <> 0) compras ";

            var list = SqliteManager.GetInstance().Query<ComprasDepositarRango>(query, new string[] { Arguments.CurrentUser.RepCodigo, Arguments.CurrentUser.RepCodigo });

            if(list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public bool HayComprasParaDepositar()
        {
            return SqliteManager.GetInstance().Query<Compras>("select ComSecuencia from Compras where ifnull(DepSecuencia, 0) = 0 and ComEstatus <> 0 and ltrim(rtrim(RepCodigo)) = ? limit 1", new string[] { Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
        }

        public List<Compras> GetComprasByDeposito(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<Compras>("select ComSecuencia, DepSecuencia, CliNombre, " +
                "(select SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) from ComprasDetalle d inner join Productos p on p.ProID = d.ProID where RepCodigo = com.RepCodigo and ComSecuencia = com.ComSecuencia) as ComTotal, " +
                "ComEstatus from Compras com " +
                "inner join Clientes c on c.CliID = com.CliID " +
                "where DepSecuencia = ? and ltrim(rtrim(com.RepCodigo)) = ? " +
                "union select ComSecuencia, DepSecuencia, CliNombre, " +
                "(select SUM((((ComPrecio - ComDescuento) + (ComPrecio - ComDescuento) * (ComItbis / 100))) * ((ifnull(ComCantidadDetalle, 0)  / case when ifnull(P.ProUnidades, 0) = 0 then 1 else ifnull(P.ProUnidades, 1) end) + ComCantidad)) from ComprasDetalleConfirmados d inner join Productos p on p.ProID = d.ProID where RepCodigo = com.RepCodigo and ComSecuencia = com.ComSecuencia) as ComTotal, " +
                "ComEstatus from ComprasConfirmados com " +
                "inner join Clientes c on c.CliID = com.CliID " +
                "where DepSecuencia = ? and ltrim(rtrim(com.RepCodigo)) = ? order by ComSecuencia", 
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }


        public List<ComprasDetalle> GetComprasByDepositoyProductos(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query <ComprasDetalle>("select  DISTINCT cm.Proid, ProCodigo, ProDescripcion,  Cm.ComCantidad as ComCantidad " +
                " from  Compras com " +
                " Inner join ComprasDetalle Cm " +
                " Inner join Productos P on P.Proid = Cm.Proid" +
                " where DepSecuencia = ? and ltrim(rtrim(com.RepCodigo)) = ?  And ComEstatus <>0 " +
                " GROUP by cm.Proid " +
                " UNION " +
                " Select  DISTINCT cm.Proid, ProCodigo, ProDescripcion, Cm.ComCantidad as ComCantidad " +
                " from comprasConfirmados com" +
                " Inner join ComprasDetalleConfirmados Cm " +
                " inner join Productos p on p.ProID = Cm.ProID" +
                " where DepSecuencia = ? and ltrim(rtrim(com.RepCodigo)) = ? And ComEstatus <>0 " +
                " GROUP by cm.Proid" +
                " order by ProDescripcion",
                new string[] { depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), depSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        public List<FormasPagoTemp> GetFormasPago(int comSecuencia)
        {
            try
            {
                return SqliteManager.GetInstance().Query<FormasPagoTemp>("select ForID, FopDescripcion as FormaPago, BanNombre as Banco, " +
                    "c.BanID as BanID, cfpNumeroCheque as NoCheque, CfpFecha as Fecha, CfpMonto as Valor, cfpTasa as Tasa, cfpPrima as Prima, " +
                    "c.rowguid as rowguid, c.MonCodigo as MonCodigo, CfpBonoCantidad as BonoCantidad, CfpBonoDenominacion as BonoDenominacion " +
                    "from ComprasFormaPago c " +
                    "left join Bancos b on b.BanID = c.BanID " +
                    "left join FormasPago f on f.FopID = c.ForID " +
                    "where c.ComSecuencia = ? and trim(c.RepCodigo) = ? ",
                    new string[] { comSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
            }catch(Exception)
            {
                return new List<FormasPagoTemp>();
            }
        }

        public bool FacturaYaFueUsada(string factura)
        {
            if (string.IsNullOrWhiteSpace(factura))
            {
                return false;
            }

            return SqliteManager.GetInstance().Query<Compras>("select cxcDocumento from ComprasDetalle d " +
                "inner join Compras c on c.RepCodigo = d.RepCodigo and d.ComSecuencia = c.ComSecuencia " +
                "where trim(c.RepCodigo) = ? and c.CliID = ? and trim(upper(ifnull(d.cxcDocumento, ''))) = ? " +
                "union " +
                "select cxcDocumento from ComprasDetalleConfirmados d " +
                "inner join ComprasConfirmados c on c.RepCodigo = d.RepCodigo and c.ComSecuencia = d.ComSecuencia " +
                "where trim(c.RepCodigo) = ? and c.CliID = ? and trim(upper(ifnull(d.cxcDocumento, ''))) = ? limit 1", 
                new string[] { Arguments.CurrentUser.RepCodigo.Trim(), Arguments.Values.CurrentClient.CliID.ToString(), factura.Trim().ToUpper(),
                Arguments.CurrentUser.RepCodigo.Trim(), Arguments.Values.CurrentClient.CliID.ToString(), factura.Trim().ToUpper() }).Count > 0;
        }

        public string GetFechaComprasDesdeHasta(int DepSecuencia)
        {
            var query = @"select ComFecha  from compras  
                           where depsecuencia = "+ DepSecuencia + "  order by ComFecha asc limit 1";

            var list = SqliteManager.GetInstance().Query<Compras>(query, new string[]{ });

            query = @"select ComFecha  from compras 
                      where depsecuencia = " + DepSecuencia + "  order by ComFecha desc limit 1";

            var list2 = SqliteManager.GetInstance().Query<Compras>(query, new string[] { });

            var ComFechaDesdeHasta = list[0].ComFecha + "-" + list2[0].ComFecha;
            return ComFechaDesdeHasta;
        }
    }
}

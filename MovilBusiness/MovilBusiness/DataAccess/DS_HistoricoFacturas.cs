using MovilBusiness.Configuration;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_HistoricoFacturas
    {

        public ObservableCollection<HistoricoFacturas> GetHistoricoByCliente(int cliId, int limit, int dias)
        {
            List<HistoricoFacturas> HistoricoFacturas = new List<HistoricoFacturas>();
            ObservableCollection<HistoricoFacturas> newHistoricoFacturas = new ObservableCollection<HistoricoFacturas>();

            string whereCondition = "";
            if (Arguments.Values.CurrentSector != null)
            {
                whereCondition += " and (SecCodigo = '" + Arguments.Values.CurrentSector.SecCodigo + "' or SecCodigo is NULL or SecCodigo = '') ";
            }

            string queryLimit = limit > 0 ? " limit " + limit.ToString() : "";

            string limiteDias = dias > 0 ? " and (cast(replace(cast(julianday(datetime('now')) - julianday(HifFecha) as integer),' ', '') as integer)) < " + dias.ToString() + " " : "";

            HistoricoFacturas = SqliteManager.GetInstance().Query<HistoricoFacturas>("select RepCodigo, idReferencia, HifDocumento, " +
                "HifMonto, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(HifFecha,1,10)),' ','' ), '') as HifFecha " +
                "from HistoricoFacturas where CliID = ? " + whereCondition + limiteDias + " ORDER BY HifDocumento DESC" + queryLimit, new string[] { cliId.ToString() });

            foreach(var fact in HistoricoFacturas)
            {
                newHistoricoFacturas.Add(fact);
            }
            return newHistoricoFacturas;

        }

        public ObservableCollection<HistoricoFacturas> GetHistoricoPedidos(int cliId)
        {
            List<HistoricoFacturas> HistoricoFacturas = new List<HistoricoFacturas>();
            ObservableCollection<HistoricoFacturas> newHistoricoFacturas = new ObservableCollection<HistoricoFacturas>();

            HistoricoFacturas =  SqliteManager.GetInstance().Query<HistoricoFacturas>("select RepCodigo, PedSecuencia as idReferencia, " +
                "PedTotal as HifMonto, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PedFecha,1,10)),' ','' ), '') as HifFecha " +
                "from PedidosConfirmados where CliID = ? order by PedFecha desc", new string[] { cliId.ToString() });
            foreach (var fact in HistoricoFacturas)
            {
                newHistoricoFacturas.Add(fact);
            }
            return newHistoricoFacturas;
        }


        public List<HistoricoFacturasDetalle> GetHistoricoFacturasDetalle(string idReferencia, string repCodigo)
        {
            return SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select p.ProCodigo||' - '|| p.ProDescripcion as ProDescripcion, "+( DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasTotalPrecioProducto() ? " (HiFPrecio / HiFCantidad) as HiFPrecio, " : " HiFPrecio, ") +
                "HiFCantidad, HifDesPorciento, (HifDescuento|| '('|| HifDesPorciento ||')') as HifDescuento, HifItbis, ProDatos2 as UnidadVenta, HifLote, h.ProID as ProID from HistoricoFacturasDetalle h " +
                "inner join Productos p on p.ProID = h.ProID where idReferencia = ? and h.RepCodigo = ? order by p.ProDescripcion", new string[] { idReferencia, repCodigo });

        }

        public double GetCantidadProductoByHistoricoFacturasDetalle(string hifDocumento, string repCodigo, int proid)
        {
            try
            {
                bool ofertasSeparadas = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesOfertasSeparadas();

                var list = SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select sum(HiFCantidad) HiFCantidad from HistoricoFacturas h " +
                "inner join HistoricoFacturasDetalle hd on hd.idReferencia = h.idReferencia and hd.RepCodigo=h.RepCodigo " +
                "inner join Productos p on p.ProID = hd.ProID where h.HifDocumento = ? and h.RepCodigo = ? and hd.ProID=? " + (ofertasSeparadas ? "and ifnull(hd.HifIndicadorOferta,0) = 0" : "") + " order by p.ProDescripcion", new string[] { hifDocumento, repCodigo, proid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].HiFCantidad;
                }

            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public double GetCantidadOfertaProductoByHistoricoFacturasDetalle(string hifDocumento, string repCodigo, int proid)
        {
            try
            {
                bool ofertasSeparadas = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesOfertasSeparadas();

                var list = SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select sum(HiFCantidad) HiFCantidad from HistoricoFacturas h " +
                "inner join HistoricoFacturasDetalle hd on hd.idReferencia = h.idReferencia and hd.RepCodigo=h.RepCodigo " +
                "inner join Productos p on p.ProID = hd.ProID where h.HifDocumento = ? and h.RepCodigo = ? and hd.ProID=? " + (ofertasSeparadas ? "and ifnull(hd.HifIndicadorOferta,0) = 1" : "") + " order by p.ProDescripcion", new string[] { hifDocumento, repCodigo, proid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].HiFCantidad;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public List<HistoricoFacturasDetalle> GetHistoricoFacturasDetalleByVentasDetallexNCF(string idReferencia, string repCodigo)
        {
            return SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select p.ProCodigo||' - '|| p.ProDescripcion as ProDescripcion, " + (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasTotalPrecioProducto() ? " (VenPrecio / VenCantidad) as HiFPrecio, " : " VenPrecio as HiFPrecio, ") +
                " VenCantidad as HiFCantidad, VenDescPorciento as HifDesPorciento, (VenDescuento|| '('|| VenDescPorciento ||')') as HifDescuento, VenItbis as HifItbis, ProDatos2 as UnidadVenta,VenLote as HifLote, h.ProID as ProID from VentasDetalle h " +
                "inner join Ventas v on v.Vensecuencia = h.Vensecuencia and v.RepCodigo=h.RepCodigo " +
                "inner join Productos p on p.ProID = h.ProID " +
                " where v.VenNCF = '" + idReferencia + "' and h.RepCodigo = " + repCodigo.ToString() + " ", new string[] { });

        }

        public List<HistoricoFacturasDetalle> GetHistoricoFacturasDetalleByVentasDetallexNumeroErp(string idReferencia, string repCodigo)
        {
            return SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select p.ProCodigo||' - '|| p.ProDescripcion as ProDescripcion, " + (DS_RepresentantesParametros.GetInstance().GetParHistoricoFacturasTotalPrecioProducto() ? " (VenPrecio / VenCantidad) as HiFPrecio, " : " VenPrecio as HiFPrecio, ") +
                " VenCantidad as HiFCantidad, VenDescPorciento as HifDesPorciento, (VenDescuento|| '('|| VenDescPorciento ||')') as HifDescuento, VenItbis as HifItbis, ProDatos2 as UnidadVenta,VenLote as HifLote, h.ProID as ProID from VentasDetalleConfirmados h " +
                "inner join VentasConfirmados v on v.Vensecuencia = h.Vensecuencia and v.RepCodigo=h.RepCodigo " +
                "inner join Productos p on p.ProID = h.ProID " +
                " where v.VenNumeroErp = " + idReferencia + " and h.RepCodigo = " + repCodigo.ToString() + " ", new string[] { });

        }

        public List<HistoricoFacturasDetalle> GetHistoricoPedidosDetalle(string idReferencia, string repCodigo)
        {
            return SqliteManager.GetInstance().Query<HistoricoFacturasDetalle>("select p.ProCodigo||' - '|| p.ProDescripcion as ProDescripcion, PedPrecio as HiFPrecio, " +
                "PedCantidad as HiFCantidad, PedDesPorciento|| '('|| PedDesPorciento ||')' as HifDescuento, PedDescuento as HifDescuento, PedItbis as HifItbis, ProDatos2 as UnidadVenta " +
                "from PedidosDetalleConfirmados d " +
                "inner join Productos p on p.ProID = d.ProID where d.PedSecuencia = ? and d.RepCodigo = ? order by p.ProDescripcion", new string[] { idReferencia, repCodigo });
        }

        //public void InsertProductosInTempOLD(string idReferencia, bool productoConCantidades, int titId)
        //{
        //    bool sinPrecio = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && !DS_RepresentantesParametros.GetInstance().GetParDevolucionesFacturaPrecioProducto();
        //    bool descuentoManual = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesDescuentoManual();
        //    bool ofertasSeparadas = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesOfertasSeparadas();

        //    var secCodigo = (DS_RepresentantesParametros.GetInstance().GetParSectores() > 0 && Application.Current.Properties.ContainsKey("SecCodigo") ? "'" + Application.Current.Properties["SecCodigo"] + "'" : "NULL");

        //    var query = "INSERT INTO ProductosTemp(TitId, rowguid, ProID, Cantidad, UnmCodigo, ProCodigo, Descripcion, ProDescripcion1, ProDescripcion2, ProDescripcion3, ProDatos1, ProDatos2, Precio, Itbis, Descuento, DesPorciento, Documento, FechaVencimiento, Lote, ProUnidades, SecCodigo " + (descuentoManual ? ",DesPorcientoManual" : " ") + " " + (ofertasSeparadas ? ",CantidadOferta" : " ") + ") " +
        //        "select "+titId.ToString()+", p.rowguid, p.ProID, "+(productoConCantidades? "sum(h.HiFCantidad)" : "0") + ", p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, p.ProDatos1, p.ProDatos2, " + (sinPrecio ? "0" : " h.HiFPrecio") +", h.HifItbis, h.HifDescuento, h.HifDesPorciento, g.HifDocumento, datetime('now'),HifLote, p.ProUnidades, "+secCodigo+ " " + (descuentoManual ? ",h.HifDesPorciento" : "") + " " + (ofertasSeparadas ? ",sum(h.HiFCantidadOferta)" : " ") + " " +
        //        "from HistoricoFacturasDetalle h " +
        //        "inner join HistoricoFacturas g on g.idReferencia = h.idReferencia and ltrim(rtrim(h.RepCodigo)) = ltrim(rtrim(g.RepCodigo)) " +
        //        "inner join Productos p on p.ProID = h.ProID " +
        //        "where h.idReferencia = ?  " + (ofertasSeparadas ? "and h.HiFPrecio > 0" : "") + "   " + // and ltrim(rtrim(h.RepCodigo)) = ?
        //        "group by p.rowguid, p.ProID, p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, ProDatos1, ProDatos2 ";

        //    SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? ", new string[] { titId.ToString() });

        //    SqliteManager.GetInstance().Execute(query, new string[] { idReferencia });// Arguments.CurrentUser.RepCodigo.Trim() 
        //}

        public void InsertProductosInTemp(string idReferencia, bool productoConCantidades, int titId)
        {
            bool sinPrecio = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && !DS_RepresentantesParametros.GetInstance().GetParDevolucionesFacturaPrecioProducto();
            bool descuentoManual = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesDescuentoManual();
            bool ofertasSeparadas = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetParDevolucionesOfertasSeparadas();

            var secCodigo = (DS_RepresentantesParametros.GetInstance().GetParSectores() > 0 && Application.Current.Properties.ContainsKey("SecCodigo") ? "'" + Application.Current.Properties["SecCodigo"] + "'" : "NULL");

            var query = "INSERT INTO ProductosTemp(TitId, rowguid, ProID, Cantidad, UnmCodigo, ProCodigo, Descripcion, ProDescripcion1, ProDescripcion2, ProDescripcion3, ProDatos1, ProDatos2, Precio, Itbis, Descuento, DesPorciento, Documento, FechaVencimiento, Lote, ProUnidades, SecCodigo " + (descuentoManual ? ",DesPorcientoManual" : " ") + " " + (ofertasSeparadas && productoConCantidades ? ",CantidadOferta" : " ") + ") " +
                "select " + titId.ToString() + ", p.rowguid, p.ProID, " + (productoConCantidades ? "sum(h.HiFCantidad)" : "0") + ", p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, p.ProDatos1, p.ProDatos2, " + (sinPrecio ? "0" : " h.HiFPrecio") + ", h.HifItbis, h.HifDescuento, h.HifDesPorciento, g.HifDocumento, datetime('now'),HifLote, " +
                "p.ProUnidades, " + secCodigo + " " + (descuentoManual ? ",h.HifDesPorciento" : "") + " " + (ofertasSeparadas && productoConCantidades ? ",(select sum(hd.HiFCantidad) from HistoricoFacturasDetalle hd where hd.idReferencia = h.idReferencia and ifnull(hd.HifIndicadorOferta,0) = 1 and hd.ProID =h.ProID)" : " ") + " " +
                "from HistoricoFacturasDetalle h " +
                "inner join HistoricoFacturas g on g.idReferencia = h.idReferencia and ltrim(rtrim(h.RepCodigo)) = ltrim(rtrim(g.RepCodigo)) " +
                "inner join Productos p on p.ProID = h.ProID " +
                "where h.idReferencia = ?  " + (ofertasSeparadas ? "and ifnull(h.HifIndicadorOferta,0) = 0" : "") + "   " + // and ltrim(rtrim(h.RepCodigo)) = ?
                "group by p.rowguid, p.ProID, p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, ProDatos1, ProDatos2 ";

            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? ", new string[] { titId.ToString() });

            SqliteManager.GetInstance().Execute(query, new string[] { idReferencia });// Arguments.CurrentUser.RepCodigo.Trim() 
        }

        public void InsertProductosInTempByVentasDetallexNCF(string idReferencia, bool productoConCantidades, int titId)
        {
            bool sinPrecio = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && !DS_RepresentantesParametros.GetInstance().GetParDevolucionesFacturaPrecioProducto();

            var query = "INSERT INTO ProductosTemp(TitId, rowguid, ProID, Cantidad, UnmCodigo, ProCodigo, Descripcion, ProDescripcion1, ProDescripcion2, ProDescripcion3, ProDatos1, ProDatos2, Precio, Itbis, Descuento, DesPorciento, Documento, FechaVencimiento, Lote, ProUnidades, SecCodigo) " +
                "select " + titId.ToString() + ", h.rowguid, p.ProID, " + (productoConCantidades ? "sum(h.VenCantidad)" : "0") + ", h.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, p.ProDatos1, p.ProDatos2, " + (sinPrecio ? "0" : " h.VenPrecio") + ", h.VenItbis, h.VenDescuento, h.VenDescPorciento, g.VenNCF , datetime('now'),VenLote, p.ProUnidades, NULL  " +
                "from VentasDetalle h " +
                "inner join Ventas g on g.VenSecuencia = h.VenSecuencia and ltrim(rtrim(h.RepCodigo)) = ltrim(rtrim(g.RepCodigo)) " +
                "inner join Productos p on p.ProID = h.ProID " +
                "where g.VenNCF = '" + idReferencia.ToString() + "'    " + 
                "group by p.rowguid, p.ProID, p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, ProDatos1, ProDatos2, h.VenLote ";


            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? ", new string[] { titId.ToString() });

            SqliteManager.GetInstance().Execute(query, new string[] {  });
        }

        public void InsertProductosInTempByVentasDetallexNumeroERP(string idReferencia, bool productoConCantidades, int titId)
        {
            bool sinPrecio = Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && !DS_RepresentantesParametros.GetInstance().GetParDevolucionesFacturaPrecioProducto();

            var query = "INSERT INTO ProductosTemp(TitId, rowguid, ProID, Cantidad, UnmCodigo, ProCodigo, Descripcion, ProDescripcion1, ProDescripcion2, ProDescripcion3, ProDatos1, ProDatos2, Precio, Itbis, Descuento, DesPorciento, Documento, FechaVencimiento, Lote, ProUnidades, SecCodigo) " +
                "select " + titId.ToString() + ", h.rowguid, p.ProID, " + (productoConCantidades ? "sum(h.VenCantidad)" : "0") + ", p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, p.ProDatos1, p.ProDatos2, " + (sinPrecio ? "0" : " h.VenPrecio") + ", h.VenItbis, h.VenDescuento, h.VenDescPorciento, g.VenNCF, datetime('now'), VenLote, p.ProUnidades, NULL  " +
                "from VentasDetalleConfirmados h " +
                "inner join VentasConfirmados g on g.VenSecuencia = h.VenSecuencia and ltrim(rtrim(h.RepCodigo)) = ltrim(rtrim(g.RepCodigo)) " +
                "inner join Productos p on p.ProID = h.ProID " +
                "where g.VenNumeroErp = " + idReferencia.ToString() + "    " + 
                "group by p.rowguid, p.ProID, p.UnmCodigo, p.ProCodigo, p.ProDescripcion, p.ProDescripcion1, p.ProDescripcion2, p.ProDescripcion3, ProDatos1, ProDatos2, h.VenLote ";


            SqliteManager.GetInstance().Execute("delete from ProductosTemp where TitID = ? ", new string[] { titId.ToString() });

            SqliteManager.GetInstance().Execute(query, new string[] { });
        }

        public HistoricoFacturas GetById(string Id, string repCodigo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<HistoricoFacturas>("select HifDocumento, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(HifFecha,1,10)),' ','' ), '') as HifFecha, idReferencia, RepCodigo, HiFNCF from HistoricoFacturas where idReferencia = ? and ltrim(rtrim(RepCodigo)) = ? ", new string[] { Id, repCodigo .Trim()});

                if(list != null && list.Count > 0)
                {
                    return list[0];
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        

        public ObservableCollection<HistoricoFacturas> GetHistoricoByClienteAndCuentasxCobrar(int cliId)
        {
            List<HistoricoFacturas> HistoricoFacturas = new List<HistoricoFacturas>();
            ObservableCollection<HistoricoFacturas> newHistoricoFacturas = new ObservableCollection<HistoricoFacturas>();

            HistoricoFacturas = SqliteManager.GetInstance().Query<HistoricoFacturas>("select RepCodigo, Replace(CxCReferencia,'FAT-',' ')  as idReferencia, CxcDocumento as HifDocumento, " +
                "CxcMontoTotal as HifMonto, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CxcFecha,1,10)),' ','' ), '') as HifFecha " +
                "from CuentasxCobrar where CliID = ? and cxcSigla= ? and CxcBalance > 0 and ConId <> ?  ORDER BY HifDocumento DESC", new string[] { cliId.ToString(), "FAT", DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado().ToString() });

            foreach (var fact in HistoricoFacturas)
            {
                newHistoricoFacturas.Add(fact);
            }
            return newHistoricoFacturas;

        }

        public double GetPorcientoDescuentoGeneralByFactura(DevolucionesArgs? args)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<HistoricoFacturas>("select ifnull(HifPorcientoDsctoGlobal,0) as HifPorcientoDsctoGlobal from HistoricoFacturas where idReferencia = ? ", new string[] { args?.Documento.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].HifPorcientoDsctoGlobal;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }


    }
}

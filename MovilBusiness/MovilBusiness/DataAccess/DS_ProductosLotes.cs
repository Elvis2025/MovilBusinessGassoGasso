
using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_ProductosLotes : DS_Controller
    {

        public List<ProductosLotes> GetLotesByProId(int proId, int almId = -1, bool isfecha = false)
        {
            if (isfecha)
            {
                if (almId != -1)
                {
                    return SqliteManager.GetInstance().Query<ProductosLotes>("select distinct InvLote as PrlLote, ProID " +
                        "from InventariosAlmacenesRepresentantes where ProID = ? and AlmID = ? and trim(RepCodigo) = ? and InvCantidad > 0.00" +
                        " and (strftime('%Y-%m-%d',PrlFechaVencimiento) <= '" + DateTime.Now.AddDays(myParametro.GetParCantidadDiasParaVencimiento() > 0 ? myParametro.GetParCantidadDiasParaVencimiento() : 30).ToString("yyyy-MM-dd") + "'" +
                        " or strftime('%Y-%m-%d',PrlFechaVencimiento) <= '" + DateTime.Now.ToString("yyyy-MM-dd") + "')",
                        new string[] { proId.ToString(), almId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
                }
                else
                {
                    string query = "select PrlLote, ProID, PrlFechaVencimiento " +
                    "from ProductosLotes where ProID = ? and (strftime('%Y-%m-%d',PrlFechaVencimiento)" +
                    " <= '" + DateTime.Now.AddDays(myParametro.GetParCantidadDiasParaVencimiento() > 0 ? myParametro.GetParCantidadDiasParaVencimiento() : 30).ToString("yyyy-MM-dd") + "' " +
                    "or strftime('%Y-%m-%d',PrlFechaVencimiento) <= '" + DateTime.Now.ToString("yyyy-MM-dd") + "')";

                    return SqliteManager.GetInstance().Query<ProductosLotes>(query, new string[] { proId.ToString() });
                }
            }
            else
            {
                if (almId != -1)
                {
                    return SqliteManager.GetInstance().Query<ProductosLotes>("select distinct InvLote as PrlLote, ProID " +
                        "from InventariosAlmacenesRepresentantes where ProID = ? and AlmID = ? and trim(RepCodigo) = ? and InvCantidad > 0.00",
                        new string[] { proId.ToString(), almId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
                }
                else
                {
                    return SqliteManager.GetInstance().Query<ProductosLotes>("select PrlLote, ProID, PrlFechaVencimiento " +
                    "from ProductosLotes where ProID = ?", new string[] { proId.ToString() });
                }
            }
        }

        public ProductosLotes GetByProIDAndLote(string Lote, int proId)
        {
            if (string.IsNullOrWhiteSpace(Lote))
            {
                return null;
            }

            List<ProductosLotes> list = SqliteManager.GetInstance().Query<ProductosLotes>("select PrlLote, ProID, PrlFechaVencimiento " +
                "from ProductosLotes where ProID = ? and trim(upper(PrlLote)) = ?", new string[] { proId.ToString(), Lote.Trim().ToUpper() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public string GetFechaVencimientoProductoLote(int proid, string Lote)
        {
            string query = @"Select ifnull(PrlFechaVencimiento, '1900-01-01') as PrlFechaVencimiento 
                    from ProductosLotes where Proid = " + proid + " and PrlLote = '" + Lote + "' ";
            var list = SqliteManager.GetInstance().Query<ProductosLotes>(query, new string[] { });

            return (list.Count > 0 ? list[0].PrlFechaVencimiento : "");
        }
        /*
        public int GetIndexLotesByProId(int proId, string Lote)
        {
            var list = SqliteManager.GetInstance().Query<ProductosLotes>("select PrlLote, ProID, PrlFechaVencimiento " +
                "from ProductosLotes where ProID = ?", new string[] { proId.ToString() });

            int index = 0;
            var x = 0;
            foreach (var pos in list)
            {
                x++;
                if (pos.PrlLote.Equals(Lote)) {
                    index = x;
                }
            }

            return index;
        }*/

        public bool GetLoteExistente(string Lote, int proId)
        {
            var list = SqliteManager.GetInstance().Query<ProductosLotes>("select PrlLote from ProductosLotes where ProID = ? " +
                "and upper(trim(PrlLote)) = '" + Lote.Trim().ToUpper() + "' ",
                new string[] { proId.ToString() });

            if (list.Count > 0)
            {
                return true;
            }

            return false;
        }

        public List<ProductosLotes> GetProductoLotesByCliente(int proId, int CliID)
        {
            return SqliteManager.GetInstance().Query<ProductosLotes>("select distinct CFPLLote as PrlLote, CliFechaActualizacion as ProFechaActualizacion, ProID, CFPLFechaVencimiento as PrlFechaVencimiento " +
               "from ClientesFacturasProductosLotes where ProID = ? and CliID = ? ", new string[] { proId.ToString(), CliID.ToString() });
        }

        public bool ValidarProductoFechaVencimiento(int proid, string lote, string fecha)
        {
            var list = SqliteManager.GetInstance().Query<ProductosLotes>("select 1 from ProductosLotes where " +
                "ProID = " + proid + " and PrlLote = '" + lote + "' and strftime('%Y-%m-%d',PrlFechaVencimiento) <= '" + fecha + "' ",
                new string[] { });
            if (list.Count > 0)
            {
                return true;
            }

            return false;
        }

        public ProductosLotes GetFechaVencimientoProductoXLote(int proid)
        {
            string query = @"Select PrlFechaVencimiento, ProFechaActualizacion, PrlLote,ProID
                    from ProductosLotes where Proid = '" + proid + "'";

            return SqliteManager.GetInstance().Query<ProductosLotes>(query, new string[] { }).FirstOrDefault();

        }

        public List<ClientesFacturasProductosLotes> GetFacturasProductosLotes(string lote, int proId)
        {
            return SqliteManager.GetInstance().Query<ClientesFacturasProductosLotes>("Select CFPLFActura, CFPLCantidadVendida, CFPLCantidadOferta " +
                "from ClientesFacturasProductosLotes where trim(CFPLLote) = trim(?) and CliID = ? and ProID = ? Order By CFPLFechaVencimiento DESC",
                new string[] { lote, Arguments.Values.CurrentClient.CliID.ToString(), proId.ToString() });
        }

        public List<ClientesFacturasProductosLotes> GetFacturasProductosbyProducto(int proId)
        {
            return SqliteManager.GetInstance().Query<ClientesFacturasProductosLotes>("Select CFPLFActura, CFPLCantidadVendida, CFPLCantidadOferta " +
                "from ClientesFacturasProductosLotes where  CliID = ? and ProID = ? Order By CFPLFechaVencimiento DESC",
                new string[] {  Arguments.Values.CurrentClient.CliID.ToString(), proId.ToString() });
        }

        public ClientesFacturasProductosLotes GetFacturasProductosLotesByFactura(string cfplFactura, string lote, int proid)
        {
            List<ClientesFacturasProductosLotes> list = SqliteManager.GetInstance().Query<ClientesFacturasProductosLotes>("Select CFPLFActura, CFPLCantidadVendida, CFPLCantidadOferta " +
                "from ClientesFacturasProductosLotes where trim(CFPLLote) = trim(?) and CliID = ? and ProID = ? and CFPLFActura = ? Order By CFPLFechaVencimiento DESC",
                new string[] { lote, Arguments.Values.CurrentClient.CliID.ToString(), proid.ToString(), cfplFactura.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public double GetCantidadByClientesFacturasProductosLotes(string cfplFactura, string lote, int proid)
        {
            try
            {

                var list = SqliteManager.GetInstance().Query<ClientesFacturasProductosLotes>("Select CFPLFActura, CFPLCantidadVendida, CFPLCantidadOferta " +
                "from ClientesFacturasProductosLotes where trim(CFPLFActura) = trim(?) and trim(CFPLLote) = trim(?) and CliID = ? and ProID = ? Order By CFPLFechaVencimiento DESC",
                new string[] { cfplFactura, lote, Arguments.Values.CurrentClient.CliID.ToString(), proid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].CFPLCantidadVendida;
                }

            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public double GetCantidadOfertaByClientesFacturasProductosLotes(string cfplFactura, string lote, int proid)
        {
            try
            {

                var list = SqliteManager.GetInstance().Query<ClientesFacturasProductosLotes>("Select CFPLFActura, CFPLCantidadVendida, CFPLCantidadOferta " +
                "from ClientesFacturasProductosLotes where trim(CFPLFActura) = trim(?) and trim(CFPLLote) = trim(?) and CliID = ? and ProID = ? Order By CFPLFechaVencimiento DESC",
                new string[] { cfplFactura, lote, Arguments.Values.CurrentClient.CliID.ToString(), proid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].CFPLCantidadOferta;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

    }
}

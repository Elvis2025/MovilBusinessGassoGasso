
using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_Inventarios : DS_Controller
    {
        private bool ParMultiAlmacenes = false;

       
        public DS_Inventarios()
        {
            ParMultiAlmacenes = myParametro.GetParUsarMultiAlmacenes();
        }

        public void RestarInventario(int proId, double cantidadARestar, double cantidadDetalleARestar, int almId = -1, string lote = "", bool isCantidadTotal = false)
        {
            var query = "select i.invCantidad as invCantidad, i.InvCantidadDetalle as InvCantidadDetalle, " +
                "p.ProUnidades as ProUnidades, p.ProDescripcion as ProDescripcion from Inventarios i " +
                "inner join Productos p on p.ProID = i.ProID " +
                "where i.ProID = ? and ltrim(rtrim(i.RepCodigo)) = ?";

            var args = new string[] { proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() };

            if(ParMultiAlmacenes && almId != -1)
            {
                query = "select i.invCantidad as invCantidad, 0 as InvCantidadDetalle, " +
                "p.ProUnidades as ProUnidades, p.ProDescripcion as ProDescripcion from InventariosAlmacenesRepresentantes i " +
                "inner join Productos p on p.ProID = i.ProID " +
                "where i.ProID = "+proId.ToString()+" and ltrim(rtrim(i.RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' " +
                "and i.AlmID = "+almId.ToString()+" " + (!string.IsNullOrWhiteSpace(lote) ? "and upper(trim(ifnull(i.InvLote, ''))) = trim(upper('" + lote + "')) " : "") + " " +
                "order by cast(i.invCantidad as real) desc ";

                args = new string[] {};
            }

            var list = SqliteManager.GetInstance().Query<Inventarios>(query, args);

            if ((list == null || list.Count == 0) && Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS)
            {
                throw new Exception("La cantidad solicitada para: " + new DS_Productos().GetProductCodigoAndDescByProId(proId) + " es mayor que la cantidad en inventario.");
            }

            list = list.OrderByDescending(x => x.invCantidad).ToList();

            var inv = list[0];

            var parConvertirCajasaUnidades = myParametro.GetParCajasUnidadesProductos();

            //var cantidadInventario = ((inv.invCantidad * ((inv.ProHolgura /100 )+1)) * inv.ProUnidades) + inv.InvCantidadDetalle;
            var cantidadInventario = !parConvertirCajasaUnidades ? (inv.invCantidad * inv.ProUnidades) + inv.InvCantidadDetalle : inv.invCantidad ;
            var cantidadRealRestar = !parConvertirCajasaUnidades ? (cantidadARestar * inv.ProUnidades) + cantidadDetalleARestar : cantidadARestar + cantidadDetalleARestar;

            if (isCantidadTotal)
            {
                cantidadRealRestar = !parConvertirCajasaUnidades ? (cantidadARestar * inv.ProUnidades) + cantidadDetalleARestar : cantidadARestar + cantidadDetalleARestar;
            }

            var parConteoFisicoPorAlmacenes = myParametro.GetParConteoFisicoMultiAlmacenAll();
            if (!parConteoFisicoPorAlmacenes || Arguments.Values.CurrentModule != Modules.CONTEOSFISICOS) {
                if (cantidadRealRestar > cantidadInventario)
                {
                    throw new Exception("La cantidad solicitada para: " + inv.ProDescripcion + " es mayor que la cantidad en inventario.");
                }
            }
      
            var resultRaw = cantidadInventario - (Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && parConteoFisicoPorAlmacenes && cantidadRealRestar > cantidadInventario ? cantidadInventario : cantidadRealRestar);

            var resultRaw2 = !parConvertirCajasaUnidades ? resultRaw / inv.ProUnidades : resultRaw;

            var invCantidad = Math.Truncate(resultRaw2);
            var invCantidadDetalle = (int)Math.Round((resultRaw2 - Math.Truncate(resultRaw2)) * inv.ProUnidades);

            if (myParametro.GetCantidadxLibras())
            {
                invCantidad = resultRaw2;
                invCantidadDetalle = 0;
            }

            if (invCantidad < 0)
            {
                invCantidad = 0;
            }

            if(invCantidadDetalle < 0)
            {
                invCantidadDetalle = 0;
            }

            invCantidad = Math.Round(invCantidad,2, MidpointRounding.AwayFromZero);

            var i = new Hash(ParMultiAlmacenes ? "InventariosAlmacenesRepresentantes" : "Inventarios");
            i.Add("invCantidad", invCantidad);
            i.Add("InvCantidadDetalle", invCantidadDetalle);
            i.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);

            //if (ParMultiAlmacenes)
            //{
            //    i.Add("InvLote", !string.IsNullOrWhiteSpace(lote) ? " and InvLote = '" + lote + "' " : "''");
            //}

            i.ExecuteUpdate( "ProID = " + proId.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'" + (ParMultiAlmacenes?" and AlmID = " + almId.ToString() + (!string.IsNullOrWhiteSpace(lote) ? " and InvLote = '" + lote + "' " : "") : ""));

        }

        public void AgregarInventario(int proId, double cantidad, double cantidadDetalle, int almId = -1, string lote = "", string loteFechaVencimiento= "")
        {
            var query = "select ifnull(ProUnidades, 1) as ProUnidades, ProDescripcion, invCantidad, InvCantidadDetalle from Productos p " +
                "left join Inventarios i on p.ProID = i.ProID and ltrim(rtrim(i.RepCodigo)) = ? where p.ProID = ? ";

            var args = new string[] { Arguments.CurrentUser.RepCodigo.Trim(), proId.ToString() };

            if (ParMultiAlmacenes)
            {
                query = "select ifnull(ProUnidades, 1) as ProUnidades, ifnull(i.InvLote, '') as InvLote, ProDescripcion, invCantidad, InvCantidadDetalle from Productos p " +
                    "left join InventariosAlmacenesRepresentantes i on i.ProID = p.ProID and i.AlmID = ? and trim(i.RepCodigo) = ? and upper(trim(ifnull(i.InvLote, ''))) = upper(trim('"+lote+"'))  " +
                    "where p.ProID = ?  ";
                args = new string[] { almId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), proId.ToString()};
            }

            var list = SqliteManager.GetInstance().Query<Inventarios>(query, args);

            if(list == null || list.Count == 0)
            {
                throw new Exception("Error no se pudo cargar la unidad minima de un producto");
            }

            var inv = list[0];

            var cantidadTotalDetallada = ((cantidad + inv.invCantidad) * inv.ProUnidades) + (cantidadDetalle + inv.InvCantidadDetalle);

            var cantidadRaw = cantidadTotalDetallada / inv.ProUnidades;

            var invCantidad = Math.Truncate(cantidadRaw);
            var invCantidadDetalle = (int)Math.Round((cantidadRaw - Math.Truncate(cantidadRaw)) * inv.ProUnidades);
            if (myParametro.GetCantidadxLibras())
            {
                invCantidad = cantidadRaw;
                invCantidadDetalle = 0;
            }

            var h = new Hash(ParMultiAlmacenes ? "InventariosAlmacenesRepresentantes" : "Inventarios");
            h.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            h.Add("InvFechaActualizacion", Functions.CurrentDate());
            h.Add("invCantidad", invCantidad);
            h.Add("InvCantidadDetalle", invCantidadDetalle);          

            if (ExistsOnInventory(proId, almId, lote))
            {
                h.ExecuteUpdate("ProID = " + proId.ToString() + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'" + (ParMultiAlmacenes?" and AlmID = " + almId.ToString() + " and InvLote = '" + lote + "'" : ""));
            }
            else
            {
                if (ParMultiAlmacenes)
                {
                    h.Add("AlmID", almId);
                    h.Add("InvLote", lote != null ? lote : "");
                    if(loteFechaVencimiento != null && loteFechaVencimiento != "" && myParametro.GetParAsignacionLotesByFechaVencimiento())
                    {
                        h.Add("InvLoteFechaVencimiento", DateTime.Parse(loteFechaVencimiento));
                    }
                    
                }

                h.Add("rowguid", Guid.NewGuid().ToString());
                h.Add("ProID", proId.ToString());
                h.Add("RepCodigo", Arguments.CurrentUser.RepCodigo.Trim());
                h.ExecuteInsert();
            }

        }

        private bool ExistsOnInventory(int proId, int almId = -1, string lote = "")
        {
            if(ParMultiAlmacenes && almId != -1)
            {
                return SqliteManager.GetInstance().Query<Inventarios>("select ProID from InventariosAlmacenesRepresentantes " +
                    "where ProID = ? and ltrim(rtrim(RepCodigo)) = ? and AlmID = ? "+(lote != "" ? " and InvLote = '"+lote+"' ": " ")+" limit 1",
                    new string[] { proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), almId.ToString() }).Count > 0;
            }
            else
            {
                return SqliteManager.GetInstance().Query<Inventarios>("select ProID from Inventarios " +
                "where ProID = ? and ltrim(rtrim(RepCodigo)) = ? limit 1", new string[] { proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() }).Count > 0;
            } 
        }

        public bool HayExistencia(int proId, double cantidad, double cantidadDetalle = 0, int almId = -1, bool isCantidadTotal = false, string lote = "", int currrentalmId = -1)
        {
            return HayExistencia(proId, cantidad, out Inventarios existencia, cantidadDetalle, almId, isCantidadTotal, lote, currrentalmId);
        }
        public bool HayExistencia(int proId, double cantidad, out Inventarios existencia, double cantidadDetalle = 0, int almId = -1, bool isCantidadTotal = false, string lote = "", int currrentalmId = -1)
        {
            existencia = null;
            var query = "select i.invCantidad as invCantidad, i.InvCantidadDetalle as InvCantidadDetalle, ifnull(p.ProUnidades, 1) as ProUnidades from Inventarios i " +
                "inner join Productos p on p.ProID = i.ProID where ltrim(rtrim(i.RepCodigo)) = ? and i.ProID = ? ";
            var args = new string[] { Arguments.CurrentUser.RepCodigo.Trim(), proId.ToString() };

            if(myParametro.GetParCantInvAlmacenes())
            {
                 query = "select i.invCantidad as invCantidad, i.InvCantidadDetalle as InvCantidadDetalle, ifnull(p.ProUnidades, 1) as ProUnidades from InventariosAlmacenes i " +
                            "inner join Productos p on p.ProID = i.ProID where i.ProID = ? and i.almid =? ";
                 args = new string[] {proId.ToString(), currrentalmId.ToString() };
            }

            if (ParMultiAlmacenes)
            {
                query = "select sum(i.invCantidad) as invCantidad, i.InvCantidadDetalle as InvCantidadDetalle, ifnull(p.ProUnidades, 1) as ProUnidades " +
                    "from InventariosAlmacenesRepresentantes i " +
                "inner join Productos p on p.ProID = i.ProID " +
                "where ltrim(rtrim(i.RepCodigo)) = ? and i.ProID = ? " + (almId != -1 ? " and i.AlmID = " + almId.ToString() : "") + " and (InvLote = '" + lote + "' or '"+ lote + "' ='') GROUP by i.ProID";

                args = new string[] { Arguments.CurrentUser.RepCodigo.Trim(), proId.ToString() };
            }

            var list = SqliteManager.GetInstance().Query<Inventarios>(query, 
                args);

            if (list != null && list.Count > 0)
            {
                var inv = list[0];
                var cantidadEnInventario = Convert.ToDouble(inv.invCantidad)  * (inv.ProUnidades <= 0? 1 : inv.ProUnidades) + inv.InvCantidadDetalle;

                double cantidadSolicitada = (cantidad * (inv.ProUnidades <= 0 ? 1 : inv.ProUnidades)) + cantidadDetalle;


                if (isCantidadTotal)
                {
                    cantidadSolicitada = cantidad;
                }

                existencia = new Inventarios()
                {
                    ProID = proId,
                    invCantidad = inv.invCantidad,
                    InvCantidadDetalle = inv.InvCantidadDetalle
                };

                return cantidadEnInventario >= cantidadSolicitada;
            }
            else
            {
                existencia = new Inventarios()
                {
                    ProID = proId,
                    invCantidad = 0,
                    InvCantidadDetalle = 0
                };
            }

            return (cantidad + cantidadDetalle) <= 0;
        }    

        public double GetCantidadTotalInventario(int proId, int almId = -1, string lote = "")
        {
            var where = "";
            
            if (ParMultiAlmacenes && lote == "")
            {

                where = " and i.AlmID = " + almId.ToString();
                
            }
            else if(ParMultiAlmacenes && lote != "")
            {
                where = " and i.AlmID = " + almId.ToString() + " and upper(trim(ifnull(i.InvLote, ''))) = '" + lote.ToUpper().Trim() + "' ";
            }

            var list = SqliteManager.GetInstance().Query<Inventarios>("select (ifnull(sum(i.invCantidad), 0) * ifnull(p.ProUnidades, 1)) + ifnull(sum(InvCantidadDetalle), 0) as invCantidad " +
                "from "+(ParMultiAlmacenes ? "InventariosAlmacenesRepresentantes" : "Inventarios")+" i " +
                "inner join Productos p on p.ProID = i.ProID " +
                "where i.ProID = ? "+where+ " and trim(i.RepCodigo) = ? GROUP by i.ProID ", new string[] { proId.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });

            if(list != null && list.Count > 0)
            {
                return list[0].invCantidad;
            }

            return 0;
        }

        public List<Inventarios> GetInventario(bool mayorQueCero = false, int almId = -1) //usado en la consulta de inventario
        {
            bool usuarCajasUnidades = myParametro.GetParInventariosMostrarCajasUnidades();
            string query = "select " + (usuarCajasUnidades ? "1 as UsarCajasUnidades, " : "") + " (p.ProCodigo ||'-'|| p.ProDescripcion) as ProDescripcion, " + (almId != -1 ? "i.AlmID," : "")+" " +
                "p.unmcodigo as UnmCodigo, i.invCantidad as invCantidad, i.InvCantidadDetalle as InvCantidadDetalle, "+(almId != -1 ? "ifnull(i.InvLote,'') as InvLote, " : "'' as InvLote, ")+ " p.ProID as ProID, p.ProUnidades as ProUnidades, " + (almId != -1 ? "a.AlmDescripcion" : "''")+" AlmDescripcion  " +
                ",i.rowguid from " + (almId != -1 ? "InventariosAlmacenesRepresentantes" : "Inventarios") + " i inner join Productos p on p.ProID = i.ProID " +
                (almId != -1 ? "inner join Almacenes a on a.AlmId = i.AlmId " : "") +   
                "where 1=1 " + (almId != -1 ? " and i.AlmID = " + almId.ToString() : "") + " " +
                (mayorQueCero ? " and (i.invCantidad > 0.0 or i.InvCantidadDetalle > 0)" : "") + " order by p.LinID, p.Cat1ID, p.ProID ";
            return SqliteManager.GetInstance().Query<Inventarios>(query, new string[] { });
        }

        public bool GrabarProductosTemporalesSinExistenciaInInventario()
        {
         
            string query = "Select ProID, Cantidad as InvCantidad, CantidadDetalle as InvCantidadDetalle from ProductosTemp where proID not in (select proid from Inventarios) ";
            var ProductoSinExistenciaLogica = SqliteManager.GetInstance().Query<Inventarios>(query, new string[] {  });
            foreach (var prod in ProductoSinExistenciaLogica) {

                var h = new Hash("Inventarios");
                h.Add("RepCodigo", Arguments.CurrentUser.RepCodigo.Trim());
                h.Add("ProID", prod.ProID.ToString());
                h.Add("invCantidad",0);
                h.Add("InvCantidadDetalle", 0);
                h.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                h.Add("InvFechaActualizacion", Functions.CurrentDate());
                h.Add("rowguid", Guid.NewGuid().ToString());
                h.ExecuteInsert();
            }
            return ProductoSinExistenciaLogica.Count > 0 ? true : false;
        }
     
        public List<Inventarios> GetInventarioAlmacenTotalForLot(int proId, int almId)
        {
            return SqliteManager.GetInstance().Query<Inventarios>("select ProID, sum(InvCantidad) as invCantidad, sum(InvCantidadDetalle) as InvCantidadDetalle, " +
                "InvLote " + (myParametro.GetParAsignacionLotesByFechaVencimiento() ? ", InvLoteFechaVencimiento" : "") + " from " +
                "InventariosAlmacenesRepresentantes where ProID = ? and AlmID = ? and ifnull(InvLote, '') != '' " +
                "and (ifnull(InvCantidad, 0) > 0) " +
                "group by ProID, InvLote", 
                new string[] { proId.ToString(), almId.ToString()});
        }

        public List<Inventarios> GetInventarioAlmacenTotalForLotByEntrega(int enrSecuencia, int traSecuencia, int indicadorOferta, int proId, int almId)
        {
            return SqliteManager.GetInstance().Query<Inventarios>("select i.ProID, sum(InvCantidad) as invCantidad, sum(InvCantidadDetalle) as InvCantidadDetalle, " +
                "InvLote " + (myParametro.GetParAsignacionLotesByFechaVencimiento() ? ", InvLoteFechaVencimiento" : "") + " from " +
                "InventariosAlmacenesRepresentantes i " +
                "Inner Join EntregasRepartidorTransaccionesDetalle e on e.ProID=i.ProID and ifnull(e.TraLote, '') = ifnull(i.InvLote, '') " +
                "where e.EnrSecuencia = ? and e.TraSecuencia= ? and TraIndicadorOferta= ? and  i.ProID = ? and i.AlmID = ? and ifnull(i.InvLote, '') != '' " +
                "and (ifnull(InvCantidad, 0) > 0) " +
                "group by i.ProID, InvLote",
                new string[] { enrSecuencia.ToString(),traSecuencia.ToString(), indicadorOferta.ToString(), proId.ToString(), almId.ToString() });
        }

        public void ActualizarParametroInventarioCamion(int almIdVenta)
        {
            Hash map = new Hash("RepresentantesParametros");
            map.Add("ParValor", almIdVenta);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate("ParReferencia = 'VENRANCALM' and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public void ActualizarParametroAlmacenesContar(int almIdVenta, int almIdMelma)
        {
            Hash map = new Hash("RepresentantesParametros");
            map.Add("ParValor", almIdVenta + "," + almIdMelma.ToString());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate("ParReferencia = 'CONTALMS' and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }
    }
}

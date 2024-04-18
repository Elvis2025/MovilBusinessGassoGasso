using MovilBusiness.model;
using MovilBusiness.Model.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_ListaPrecios : DS_Controller
    {

        public double GetLipPrecio(string lipCodigo, int proId)
        {
            var list = SqliteManager.GetInstance().Query<ListaPrecios>("select LipPrecio from ListaPrecios where trim(upper(LipCodigo)) = ? and ProID = ? ", new string[] { lipCodigo.Trim().ToUpper(), proId.ToString()});

            if(list != null && list.Count > 0)
            {
                return list[0].LipPrecio;
            }

            return 0;
        }
        public double GetLipPrecioCompl(int ProId, string lipCodigo, string unmCodigo)
        {
            var list = SqliteManager.GetInstance().Query<ListaPrecios>("select LipPrecio from ListaPrecios where ProID = ? and trim(upper(LipCodigo)) = ? and trim(upper(UnmCodigo)) = ? ", new string[] { ProId.ToString(), lipCodigo.ToUpper(), unmCodigo.ToUpper() });

            if (list != null && list.Count > 0)
            {
                return list[0].LipPrecio;
            }

            return 0;
        }

        public double GetLipDescuento(int ProId, string lipCodigo, string unmCodigo)
        {
            var list = SqliteManager.GetInstance().Query<ListaPrecios>("select LipDescuento from ListaPrecios where ProID = ? and trim(upper(LipCodigo)) = ? and trim(upper(UnmCodigo)) = ? ", new string[] { ProId.ToString(), lipCodigo.ToUpper(), unmCodigo.ToUpper() });

            if (list != null && list.Count > 0)
            {
                return list[0].LipDescuento;
            }

            return 0;
        }

        public List<RangoPrecioMinimo> GetRangoPrecioMinimo(int ProId, string lipCodigo, string unmCodigo)
        {
            var query = "select LipRangoPrecioMinimo from ListaPrecios where ProID = ? " +
                "and trim(upper(LipCodigo)) = ? and trim(upper(UnmCodigo)) = ? ";

            var item = SqliteManager.GetInstance().Query<ListaPrecios>(query, 
                new string[] { ProId.ToString(), lipCodigo.ToUpper(), unmCodigo.ToUpper() }).FirstOrDefault();

            if(item != null && !string.IsNullOrWhiteSpace(item.LipRangoPrecioMinimo))
            {
                return JsonConvert.DeserializeObject<List<RangoPrecioMinimo>>(item.LipRangoPrecioMinimo);
            }

            return new List<RangoPrecioMinimo>();
        }

        public double GetPrecioMinimo(int ProId, string lipCodigo, string unmCodigo)
        {
            var query = "select LipPrecioMinimo LipRangoPrecioMinimo from ListaPrecios where ProID = ? " +
                "and trim(upper(LipCodigo)) = ? and trim(upper(UnmCodigo)) = ? ";

            var item = SqliteManager.GetInstance().Query<ListaPrecios>(query,
                new string[] { ProId.ToString(), lipCodigo.ToUpper(), unmCodigo.ToUpper() }).FirstOrDefault();

            if (item != null && !string.IsNullOrWhiteSpace(item.LipRangoPrecioMinimo))
            {
                return JsonConvert.DeserializeObject<Double>(item.LipRangoPrecioMinimo);
            }

            return 0.00;
        }
        public List<ListaPrecios> GetLipUnmCodigo(string lipCodigo, int proId)
        {
            var list = SqliteManager.GetInstance().Query<ListaPrecios>("select UnmCodigo from ListaPrecios where trim(upper(LipCodigo)) = ? and ProID = ? ", new string[] { lipCodigo.Trim().ToUpper(), proId.ToString() });

            if (list != null && list.Count > 0)
            {
                return list;
            }

            return new List<ListaPrecios>();
        }

        public List<ListaPrecios> UpdatePrecioForDev(string porciento,int identificadorUpdate, int identificadorUpdateOLD)
        {
            var list = SqliteManager.GetInstance().Query<ListaPrecios>("UPDATE ListaPrecios set LipPrecio = LipPrecio" + porciento + ", LipAdValorem=" + identificadorUpdate.ToString() + "  where LipAdValorem =" + identificadorUpdateOLD.ToString() + " ", new string[] {});

            if (list != null && list.Count > 0)
            {
                return list;
            }

            return new List<ListaPrecios>();
        }


    }
}

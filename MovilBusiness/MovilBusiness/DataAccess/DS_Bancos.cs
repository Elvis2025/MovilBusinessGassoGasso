
using MovilBusiness.model;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Bancos
    {

        public List<Bancos> GetBancos()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Bancos>("select BanID, BanNombre, BanReferencia, BanTipo from Bancos order by BanNombre", new string[] { });

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<Bancos>();
        }


        public List<Bancos> GetBancosOrdenPago()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Bancos>("select BanID, BanNombre, BanReferencia, BanTipo from Bancos Where BanTipo = 2 order by BanNombre", new string[] { });

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<Bancos>();
        }


        public List<Bancos> GetFromCuentasBancarias()
        {
            try
            {
                return SqliteManager.GetInstance().Query<Bancos>("select CuBID as BanID, CuBNombre as BanNombre, CuBReferencia as BanReferencia from CuentasBancarias order by CuBNombre", new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return new List<Bancos>();
        }
    }
}

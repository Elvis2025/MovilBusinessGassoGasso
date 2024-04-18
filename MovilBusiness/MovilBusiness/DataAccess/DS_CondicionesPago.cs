
using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_CondicionesPago 
    {
        private static DS_CondicionesPago Instance;

        public static DS_CondicionesPago GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DS_CondicionesPago();
            }

            return Instance;
        }

        public ObservableCollection<CondicionesPago> GetAllCondicionesPago(bool ConIDClienteAndContado = false, int ConIDCliente = -1)
        {
            var where = "";
            if (ConIDClienteAndContado)
            {
                where = " where ConID = "+ ConIDCliente +" or ConDiasVencimiento = 0 ";
            }

            ObservableCollection<CondicionesPago> Condiciones = new ObservableCollection<CondicionesPago>();
            List<CondicionesPago> CP = SqliteManager.GetInstance().Query<CondicionesPago>("select ConID, ConReferencia, ConDescripcion, ConDiasVencimiento, " +
                "rowguid, UsuInicioSesion, ConFechaActualizacion from CondicionesPago " + where + " order by ConDiasVencimiento", new object[] { });
            foreach(var cp in CP)
            {
                Condiciones.Add(cp);
            }

            return Condiciones;
        }

        public CondicionesPago GetByConId(int conId)
        {
            try
            {
                List<CondicionesPago> list = SqliteManager.GetInstance().Query<CondicionesPago>("select ConID, ConReferencia, ConDescripcion, " +
                    "ConDiasVencimiento, rowguid, UsuInicioSesion, ConFechaActualizacion from CondicionesPago where ConID = ?", new string[] { conId.ToString() });
                if (list.Count > 0)
                {
                    return list[0];
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return new CondicionesPago();
        }

        public bool TieneFormasDePago()
        {
            try
            {
                List<CondicionesPago> list = SqliteManager.GetInstance().Query<CondicionesPago>("Select ConID from CondicionesPago Limit 1", new string[] { });

                return list != null && list.Count > 0;

            }catch(Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }

        public int GetConIdContado()
        {
            var list = SqliteManager.GetInstance().Query<CondicionesPago>("Select ConId from CondicionesPago where ConDiasVencimiento = 0 ", new string[] { });

            return list != null && list.Count == 1 ? list[0].ConID : DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
        }

        public int GetConIdCliente(int cliid)
        {
            var list = SqliteManager.GetInstance().Query<CondicionesPago>("Select ConId from Clientes where CliID = ?", new string[] { cliid.ToString() });

            return list != null && list.Count == 1 ? list[0].ConID : DS_RepresentantesParametros.GetInstance().GetParConIdFormaPagoContado();
        }

        public bool GetIfIsContado(int conid)
        {
            return SqliteManager.GetInstance().Query<CondicionesPago>($"select ConDiasVencimiento from CondicionesPago where ConId = {conid}").FirstOrDefault().ConDiasVencimiento == 0;
        }

        public double GetPorcientoDescuentoGeneralByCondicionPago(int conid)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<CondicionesPago>("select ifnull(ConPorcientoDsctoGlobal, 0) as ConPorcientoDsctoGlobal from CondicionesPago where ConId = ? ", new string[] { conid.ToString() });

                if (list != null && list.Count > 0)
                {
                    return list[0].ConPorcientoDsctoGlobal;
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

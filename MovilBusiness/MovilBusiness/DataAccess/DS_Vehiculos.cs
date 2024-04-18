using MovilBusiness.Configuration;

using MovilBusiness.model;
using MovilBusiness.Model;
using System;
using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_Vehiculos
    {
        public Vehiculos GetVehicleById(int VehId)
        {
            List<Vehiculos> list = SqliteManager.GetInstance().Query<Vehiculos>("select VehID, VehContador, VehKilometraje, " +
                "VehFicha from Vehiculos where VehID = ? and VehFicha is not null", new string[] { VehId.ToString() });

            if(list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<Vehiculos> GetAllVehiculos()
        {
            try
            {
                string query = @"select  VehID, v.MarID, v.ModID, VehAnio, VehChasis, VehFicha,  
                                VehReferencia, VehKilometraje, VehContador, VehVolumenMaximo, VehPesoMaximo, VehFechaActualizacion, 
                                v.UsuInicioSesion, v.rowguid, ModDescripcion, MarDescripcion from Vehiculos v 
                                inner join MarcasVehiculos m on v.MarID = m.MarID 
                                inner Join ModelosVehiculos  mv on v.MarID = mv.MarID  and mv.ModId = v.ModId
                                where VehFicha is not null GROUP by VehID  order by  VehFicha asc";


               // query = "select * from vehiculos";
                return SqliteManager.GetInstance().Query<Vehiculos>(query, new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<Vehiculos>();
        }

        public void ActualizarKilometraje(int VehId, int kilometraje)
        {
            Hash map = new Hash("Vehiculos");
            map.Add("VehKilometraje", kilometraje);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate("VehID = " + VehId);
        }

        public void ActualizarContador(int VehId, int Contador)
        {
            Hash map = new Hash("Vehiculos");
            map.Add("VehContador", Contador);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate("VehID = " + VehId);
        }

        public List<VehiculosCapacidad> GetCapacidadVehiculo(int vehId)
        {
            return SqliteManager.GetInstance().Query<VehiculosCapacidad>("select VehID, VCACantidad, VCACantidadDetalle, ProID " +
                "from VehiculosCapacidad where VehID = ? ", 
                new string[] { vehId.ToString() });
        }
    }
}

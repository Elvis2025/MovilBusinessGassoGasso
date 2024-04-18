using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_SolicitudActualizacionClienteDireccion
    {

        public void GuardarSolicitud(SolicitudActualizacionClienteDireccion data)
        {
            var map = new Hash("SolicitudActualizacionClienteDireccion");


            var solSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("SolicitudActualizacionClienteDireccion");

            map.Add("SolEstado", 1);
            map.Add("SolSecuencia", solSecuencia);
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("SolFecha", Functions.CurrentDate());
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("CldDirTipo", data.CldDirTipo);
            map.Add("CldCalle", data.CldCalle);
            map.Add("CldCasa", data.CldCasa);
            map.Add("CldSector", data.CldSector);
            map.Add("CldContacto", data.CldContacto);
            map.Add("PaiID", data.PaiID);
            if (data.ProID != -1)
            {
                map.Add("ProID", data.ProID);
            }

            if (data.MunID != -1)
            {
                map.Add("MunID", data.MunID);
            }
            map.Add("CldTelefono", data.CldTelefono);
            map.Add("CldWhatsapp", data.CldWhatsapp);

            if (data.CliLatitud != -1)
            {
                map.Add("CldLatitud", data.CliLatitud);
            }

            if (data.CliLongitud != -1)
            {
                map.Add("CldLongitud", data.CliLongitud);
            }
            map.Add("SolFechaActualizacion", Functions.CurrentDate());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.ExecuteInsert();

            DS_RepresentantesSecuencias.UpdateSecuencia("SolicitudActualizacionClienteDireccion", solSecuencia);

        }
    }
}

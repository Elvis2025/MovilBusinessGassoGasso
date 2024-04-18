using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_OperativosMedicos
    {
        
        public void GuardarOperativoMedico(List<OperativosDetalle> detalles, string cmnombre)
        {
            int opeID = DS_RepresentantesSecuencias.GetLastSecuencia("Operativos");
            bool parCargas = DS_RepresentantesParametros.GetInstance().GetParCargasInventario();
            var myInv = new DS_Inventarios();

            Hash map = new Hash("Operativos");
           // map.Add("CMID", detalles[0].CMID);
            map.Add("OpeEstado", 1);
            map.Add("OpeFecha", Functions.CurrentDate());
            map.Add("Repcodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("OpeID", opeID);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("OpeFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("CMNombre", cmnombre);
            map.ExecuteInsert();

            int pos = 1;
            foreach (var detalle in detalles)
            {
                Hash det = new Hash("OperativosDetalle");
                det.Add("CliID", detalle.CliID);
                det.Add("OpeID", opeID);

                var nombre = detalle.OpePacienteNombre;

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    nombre = "";
                }

                det.Add("OpePacienteNombre", nombre);
                det.Add("OpePacienteTelefono", detalle.OpePacienteTelefono);
                det.Add("OpePacienteEmail", detalle.OpePacienteEmail);
                det.Add("OpeSector", detalle.OpeSector);
                det.Add("EspID", detalle.EspID);
                det.Add("OpeSecuencia", pos);
                det.Add("Repcodigo", Arguments.CurrentUser.RepCodigo);
                det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                det.Add("OpeFechaActualizacion", Functions.CurrentDate());
                det.Add("OpeNombreDoctor", detalle.OpeNombreDoctor);

                var rowguid = detalle.rowguid;

                if (string.IsNullOrWhiteSpace(rowguid))
                {
                    rowguid = Guid.NewGuid().ToString();
                }

                det.Add("rowguid", rowguid);
                det.ExecuteInsert();

                foreach (var muestra in detalle.Productos)
                {
                    Hash p = new Hash("OperativosDetalleProductos");
                    p.Add("OpeID", opeID);
                    p.Add("OpeSecuencia", pos);
                    p.Add("ProID", muestra.ProID);
                    p.Add("OpeProductoCantidad", muestra.OpeProductoCantidad);
                    p.Add("OpeProductoCantidadPrescrita", muestra.OpeProductoCantidadPrescrita);
                    p.Add("Repcodigo", Arguments.CurrentUser.RepCodigo);
                    p.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    p.Add("OpeFechaActualizacion", Functions.CurrentDate());
                    p.Add("rowguid", Guid.NewGuid().ToString());
                    p.ExecuteInsert();

                    if (parCargas)
                    {
                        myInv.RestarInventario(muestra.ProID, muestra.OpeProductoCantidad, 0);
                    }
                }
                pos++;

            }
           // myTraImg.MarkToSendToServer("Operativos", opeID);

            DS_RepresentantesSecuencias.UpdateSecuencia("Operativos", opeID);
        }
        

        public List<Operativos> GetOperativosByEstado(int opeEstado)
        {
            return SqliteManager.GetInstance().Query<Operativos>("select OpeID as OpeID, v.RepCodigo as RepCodigo, OpeFecha, CMID " +
               // "(OpeID||' - '||ltrim(rtrim(ifnull(CMNombre, '')))||'. Fecha: '||ifnull(replace(strftime('%d-%m-%Y-%H:%M', OpeFecha),' ','' ), '')) as Title " +
                "from Operativos v " +
                //"inner join CentrosMedicos c on c.CMID = v.CMID " +
                "where ltrim(rtrim(v.RepCodigo)) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and v.OpeEstado = ? order by OpeFecha ASC",
                new string[] { opeEstado.ToString() });
        }

        public List<OperativosDetalleProductos> GetProductosForOperativos(bool withInventory = false, List<Productos> filtrados = null)
        {
            var query = "select ifnull(ProDescripcion, '') as ProDescripcion, p.ProID as ProID from Productos p ";

            if (withInventory)
            {
                query += " inner join Inventarios i on i.ProID = p.ProID and trim(i.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and ifnull(i.invCantidad, 0) > 0 ";
            }

            query += " where 1=1 ";

            if(filtrados != null && filtrados.Count > 0)
            {
                var queryIn = " and p.ProID in (";

                var first = true;
                foreach(var fil in filtrados)
                {
                    queryIn += (first ? fil.ProID.ToString() : ", " + fil.ProID.ToString());

                    if (first)
                    {
                        first = false;
                    }
                }

                queryIn += ") ";

                query += queryIn;
            }

            query += " order by p.ProDescripcion";

            return SqliteManager.GetInstance().Query<OperativosDetalleProductos>(query, new string[] { });
        }

    }
}

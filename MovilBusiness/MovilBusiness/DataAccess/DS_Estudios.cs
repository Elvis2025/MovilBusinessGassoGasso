using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.DataAccess
{
    public class DS_Estudios : DS_Controller
    {
        public List<Estudios> GetEncuestasVigentes(int cliId, bool limit = false, bool onewaybycliid = false, bool onewaybyvisid = false)
        {
            var where = " and not exists (select 1 from Muestras where  EstID = e.EstID AND VisSecuencia = " + Arguments.Values.CurrentVisSecuencia + ") ";// " and VisSecuencia = " + Arguments.Values.CurrentVisSecuencia;
                       
            if(onewaybyvisid)
            {
                where = " and exists (select 1 from Muestras where  EstID = e.EstID AND VisSecuencia = " + Arguments.Values.CurrentVisSecuencia + ") ";// " and VisSecuencia = " + Arguments.Values.CurrentVisSecuencia;
            }                     

            if (onewaybycliid)
            {
                where = " and exists (select 1 from Muestras where CliID = " + cliId.ToString() + " and EstID = e.EstID) ";
            }


            if (myParametro.GetParEncuestasSoloUnaVezPorCliente() && cliId != -1)
            {
                where = " and not exists (select 1 from Muestras where CliID = " + cliId.ToString() + " and EstID = e.EstID) ";
            }

            if (cliId == -1)
            {
                where = " AND (ifnull(CliID, 0) = 0 AND ifnull(GrcCodigo, '0') = '0')";
            }
            else
            {
                where += " and (((ifnull(Cliid, 0) = " + cliId.ToString() + " or ifnull(Cliid, 0) = 0)  and ifnull(GrcCodigo, '0') = '0') " +
                        " OR( (ifnull(Cliid, 0) = 0 and " + cliId.ToString() + " in (select CliId from GrupoClientesDetalle where GrcCodigo = e.GrcCodigo)))) ";
            }

            var query = "select EstID, ltrim(rtrim(EstNombre)) as EstNombre, EstCantidadPreguntas, EstTipoMuestra from Estudios e where STRFTIME('%Y-%m-%d %H:%M:%S', datetime('now')) " +
                "BETWEEN STRFTIME('%Y-%m-%d %H:%M:%S', EstFechaDesde) AND STRFTIME('%Y-%m-%d %H:%M:%S', EstFechaHasta) " + where +
                "order by EstNombre " + (limit ? " limit 1" : "");

            return SqliteManager.GetInstance().Query<Estudios>(query, new string[] { });
        }

        public bool HayEncuestasVigentesForEstTipoMuestra(int cliid)
        {
            var vigentes = GetEncuestasVigentes(cliid);
            int muestraxCliente = vigentes.Count(e => e.EstTipoMuestra == "1");
            int muestraxVisitas = vigentes.Count(e => e.EstTipoMuestra == "2");

            int countMuestraxCliente = GetEncuestasVigentes(cliid, onewaybycliid:true).Count(e => e.EstTipoMuestra == "1"); 
            int countMuestraxVisita = GetEncuestasVigentes(cliid, onewaybyvisid: true).Count(e => e.EstTipoMuestra == "2"); ;

            return (countMuestraxCliente <= 0 && muestraxCliente > 0) 
                || (countMuestraxVisita <= 0  && muestraxVisitas > 0);

        //Forma anterior - se cambia porque no se entendia bien lo realizado y no estaba funcionando correctamente
            //var vigentes = GetEncuestasVigentes(cliid, onewaybyvisid: true);
            //var vigentes2 = GetEncuestasVigentes(cliid);
            //int count1 = GetEncuestasVigentes(cliid, onewaybycliid: true).Count(e => e.EstTipoMuestra == "1");
            //int count2 = vigentes2.Count(e => e.EstTipoMuestra == "1");
            //int count3 = vigentes.Count(e => e.EstTipoMuestra == "2"); ;

            //return (count1 <= 0 && count2 > 0)
            //    || (vigentes2.Count(e => e.EstTipoMuestra == "2") <= 0 && count3 > 0);

        }

        public List<Preguntas> GetPreguntasEncuesta(int EstId)
        {
            return SqliteManager.GetInstance().Query<Preguntas>("select EstID, PreID, PreTipoPregunta, '¿'||replace(replace(PreDescripcion, '?', ''), '¿', '')||'?' as PreDescripcion, PreIdPMultiple, PreIndicadorAleatorio from Preguntas where EstID = ? order by PreOrden", new string[] { EstId.ToString() });
        }

        public List<PreguntasOpciones> GetPreguntasOpciones(int estId, int preId)
        {
            return SqliteManager.GetInstance().Query<PreguntasOpciones>("select EstID, PreID, PreNumOpcion, PreDescripcion from PreguntasOpciones where EstID = ? and PreID = ? order by PreNumOpcion", new string[] { estId.ToString(), preId.ToString() });
        }

        public void GuardarEncuesta(List<MuestrasRespuestas> respuestas, int estId, string comentario, EncuestaInfoCliente infoCliente)
        {
            var mueSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Muestras");

            var m = new Hash("Muestras");
            m.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            m.Add("MueSecuencia", mueSecuencia);
            m.Add("MueFecha", Functions.CurrentDate());
            m.Add("EstID", estId);
            m.Add("MueEstatus", 1);

            if (Arguments.Values.CurrentClient != null)
            {
                m.Add("CLIID", Arguments.Values.CurrentClient.CliID);
                m.Add("MueCliTipo", 1);
                m.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            }
            else
            {
                m.Add("MueCliTipo", 2);
            }

            m.Add("CliNombre", infoCliente.CliNombre);
            m.Add("CliContacto", infoCliente.CliContacto);
            m.Add("CliTelefono", infoCliente.CliTelefono);
            m.Add("CliWhatsapp", infoCliente.CliWhatsapp);
            m.Add("CliDireccion", infoCliente.CliDireccion);
            m.Add("CliSector", infoCliente.CliSector);
            m.Add("CliCorreoElectronico", infoCliente.CliCorreoElectronico);

            m.Add("MueComentario", comentario);
            m.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            m.Add("MueFechaActualizacion", Functions.CurrentDate());
            m.Add("rowguid", Guid.NewGuid().ToString());
            m.ExecuteInsert();

            foreach (var res in respuestas)
            {
                var r = new Hash("MuestrasRespuestas");
                r.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                r.Add("MueSecuencia", mueSecuencia);
                r.Add("PreID", res.PreID);
                r.Add("ResRespuesta", res.ResRespuesta);
                r.Add("ResFecha", Functions.CurrentDate());
                r.Add("ResHora", Functions.CurrentDate());
                r.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                r.Add("MueFechaActualizacion", Functions.CurrentDate());
                r.Add("rowguid", Guid.NewGuid().ToString());
                r.ExecuteInsert();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("Muestras", mueSecuencia);
        }
    }
}

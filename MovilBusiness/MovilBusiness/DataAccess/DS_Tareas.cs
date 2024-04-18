using MovilBusiness.Configuration;
using MovilBusiness.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.DataAccess
{
   public class DS_Tareas
    {


        public List<Tareas> GetPendingTask()
        {
            return SqliteManager.GetInstance().Query<Tareas>("select TarSecuencia, TarFecha, TarAsunto, TarDescripcion, TarEstado, EstDescripcion as TarEstadoDescripcion " +
                " from  Tareas t inner join Estados e on t.TarEstado = e.EstEstado and EstTabla = 'Tareas' where RepCodigo = ? And Cliid = ? and TarEstado <> 3  order by TarFecha desc",
                new string[] { Arguments.CurrentUser.RepCodigo, Arguments.Values.CurrentClient.CliID.ToString() });
        }


        public Tareas GetPendingTaskBySecuencia(int tarSecuencia)
        {
             var list= SqliteManager.GetInstance().Query<Tareas>("select TarSecuencia, TarFecha, TarAsunto, TarDescripcion, TarEstado, EstDescripcion as TarEstadoDescripcion, TarFechaLimite  " +
                " from  Tareas t inner join Estados e on t.TarEstado = e.EstEstado and EstTabla = 'Tareas' where RepCodigo = ? And TarSecuencia = ? ",
                new string[] { Arguments.CurrentUser.RepCodigo, tarSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            } 
            return null;
        }


        public void ActualizarTarea(int TarSecuencia = -1, string estado = "", int TitId = -1)
        {
            Hash map = new Hash("Tareas");
            map.Add("TarEstado", estado);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            map.Add("TarFechaActualizacion", Functions.CurrentDate());
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.ExecuteUpdate($"ltrim(rtrim(RepCodigo)) = '{Arguments.CurrentUser.RepCodigo}' {(TitId != -1 ? $" and CliID = {Arguments.Values.CurrentClient.CliID} and TitId = {TitId}" : $" and TarSecuencia = {TarSecuencia}")}");
        }

        public int getTareasPendientes()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Tareas>("select count(TarSecuencia) as TarSecuencia " +
                    "from Tareas where RepCodigo = ? and TarEstado in (1,2) And Cliid = ?",
                    new string[] { Arguments.CurrentUser.RepCodigo, Arguments.Values.CurrentClient.CliID.ToString()});

                if (list != null && list.Count > 0)
                {
                    return list[0].TarSecuencia;
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

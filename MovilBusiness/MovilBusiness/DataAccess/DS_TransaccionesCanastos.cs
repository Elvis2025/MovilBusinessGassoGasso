using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Printer.Formats;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
    public class DS_TransaccionesCanastos : DS_Controller
    {

        public int SaveCanastos(TipoCapturaCanastos tipo, int cantidadCanastos, List<string> detalles)
        {
            var traSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("TransaccionesCanastos");

            var map = new Hash("TransaccionesCanastos");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);

            var traId = tipo == TipoCapturaCanastos.ENTREGARCANASTOS ? 21 : 18;

            map.Add("TraID", traId);
            map.Add("TraSecuencia", traSecuencia);
            map.Add("TraFecha", Functions.CurrentDate());
            map.Add("RepVendedor", "");
            map.Add("CliID", Arguments.Values.CurrentClient.CliID);
            map.Add("TitOrigen", tipo == TipoCapturaCanastos.ENTREGARCANASTOS ? -1 : 1);
            map.Add("TitID", traId);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("TraFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.Add("mbVersion", Functions.AppVersion);
            map.Add("TraCantidadCanastos", cantidadCanastos);
            map.Add("TraCantidaddetalle", cantidadCanastos);
            map.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
           // map.Add("TrcTipo", "General");

            var parNoUsarDetalle = myParametro.GetParCanastosNoDetalle();

            if(parNoUsarDetalle)
            {
               // map.Add("TraCantidadCanastos", cantidadCanastos);
                //map.Add("TraCantidaddetalle", cantidadCanastos);
                map.Add("TrcTipo", "General");
            }
            else
            {
                map.Add("TrcTipo", "Detalle");
            }

            if (Arguments.Values.CurrentCuaSecuencia != -1)
            {
                map.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            }

            map.ExecuteInsert();

            if (!parNoUsarDetalle)
            {
                var pos = 1;
                foreach(var serie in detalles){
                    var det = new Hash("TransaccionesCanastosDetalle");
                    det.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    det.Add("TraID", traId);
                    det.Add("TraSecuencia", traSecuencia);
                    det.Add("TraPosicion", pos); pos++;
                    det.Add("RecSerie", serie);
                    det.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    det.Add("TraFechaActualizacion", Functions.CurrentDate());
                    det.Add("rowguid", Guid.NewGuid().ToString());
                    det.ExecuteInsert();

                    if(tipo == TipoCapturaCanastos.ENTREGARCANASTOS)
                    {
                        DeleteFromInventario(serie);
                    }
                    else
                    {
                        AgregarToInventario(serie);
                    }
                }
            }
            DS_RepresentantesSecuencias.UpdateSecuencia("TransaccionesCanastos", traSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados(tipo);
            }


            return traSecuencia;
        }

        private void ActualizarVisitasResultados(TipoCapturaCanastos tipo)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<VisitasResultados>("select count(*) as VisCantidadTransacciones, '' as VisComentario " +
                    "from TransaccionesCanastos where RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and VisSecuencia = ? and TitOrigen = " + (tipo == TipoCapturaCanastos.ENTREGARCANASTOS ? -1 : 1) + " ",
                    new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

                if (list != null && list.Count > 0)
                {
                    var item = list[0];
                    item.TitID = tipo == TipoCapturaCanastos.ENTREGARCANASTOS ? 21 : 18;
                    new DS_Visitas().GuardarVisitasResultados(item);
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void DeleteFromInventario(string serie)
        {
            var map = new Hash("InventarioCanastos");
            map.ExecuteDelete("ltrim(rtrim(upper(RecSerie))) = '" + serie.Trim().ToUpper() + "' and RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'");
        }

        private void AgregarToInventario(string serie)
        {
            if (ExisteInventarioCanasto(serie))
            {
                return;
            }

            var map = new Hash("InventarioCanastos");
            map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            map.Add("RecSerie", serie);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            map.Add("InvFechaActualizacion", Functions.CurrentDate());
            map.Add("rowguid", Guid.NewGuid().ToString());
            map.ExecuteInsert();
        }


        private bool ExisteInventarioCanasto(string serie)
        {
            var list = SqliteManager.GetInstance().Query<TransaccionesCanastos>("select RepCodigo from InventarioCanastos " +
                "where trim(upper(RecSerie)) = '"+serie.Trim().ToUpper()+"' and trim(RepCodigo) = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { });

            return list != null && list.Count > 0;
        }

        public async Task AceptarImpresion(int traSecuencia, int Copias)
        {
            try
            {
                var Printer = new TransaccionesCanastosFormats();

                for (int x = 0; x < Copias; x++)
                {
                    await Task.Run(() =>
                    {
                        Printer.Print(traSecuencia, false, new Printer.PrinterManager());
                    });

                    if (Copias > 1 && x != Copias - 1)
                    {
                        await Functions.DisplayAlert("Copia impresión", "Corte la copia actual de la impresora para continuar", "Imprimir");
                    }

                }
            }
            catch (Exception e)
            {
                await Functions.DisplayAlert("Error imprimiendo carga de inventario", e.Message, "Aceptar");
            }
        }

        public TransaccionesCanastos GetBySecuencia(int traSecuencia)
        {
            return SqliteManager.GetInstance().Query<TransaccionesCanastos>("select t.*, c.CliNombre as CliNombre, c.CliCodigo as CliCodigo, " +
                "c.CliCalle as CliCalle, c.CliUrbanizacion as CliUrbanizacion from TransaccionesCanastos t " +
                "inner join Clientes c on c.CliID = t.CliID " +
                "where t.RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and t.TraSecuencia = " + traSecuencia.ToString(),
                new string[] { }).FirstOrDefault();
        }

        public List<TransaccionesCanastosDetalle> GetDetalleBySecuencia(int traSecuencia) {
            return SqliteManager.GetInstance().Query<TransaccionesCanastosDetalle>("select * from TransaccionesCanastosDetalle " +
                "where TraSecuencia = " + traSecuencia + " and RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", new string[] { });
        }

    }
}

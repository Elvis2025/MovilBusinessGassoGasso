using MovilBusiness.Configuration;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_TransaccionesImagenes
    {
        public void SaveImagenInTemp(byte[] image, string tabla, string repTablaKey, bool forFirma = false, int titId = -1)
        {
            TransaccionesImagenesTemp temp = new TransaccionesImagenesTemp
            {
                Rowguid = Guid.NewGuid().ToString(),
                RepTabla = tabla,
                ///TraSecuencia = traSecuencia,
                TitId = titId,
                RepCodigo = Arguments.CurrentUser.RepCodigo,
                RepTablaKey = repTablaKey,
                TraImagen = image,
                TraFormato = "JPEG",
                TraTamano = "Auto",
                ForFirma = forFirma,
                TraPosicion = GetLastPosition(tabla, repTablaKey, forFirma)
            };

            SqliteManager.GetInstance().InsertOrReplace(temp);

        }

        public void DeleteById(string rowguid)
        {
            try
            {
                SqliteManager.GetInstance().Execute("delete from TransaccionesImagenesTemp Where Rowguid = '" + rowguid + "'");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private int GetLastPosition(string tabla, string repTablaKey, bool forFirma)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<TransaccionesImagenesTemp>("select ifnull(max(TraPosicion), 0) as TraPosicion from " +
                    "TransaccionesImagenesTemp where RepTabla = ? and RepTablaKey = ? and ForFirma = " + (forFirma?"1":"0"), new string[] { tabla, repTablaKey });

                if (list != null && list.Count > 0)
                {
                    return list[0].TraPosicion + 1;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 1;
        }

        public List<TransaccionesImagenesTemp> GetImagenesInTemp(string tabla, string repTablaKey, bool forFirma)
        {
            return SqliteManager.GetInstance().Table<TransaccionesImagenesTemp>().Where(x => x.RepTabla == tabla && x.RepTablaKey == repTablaKey && x.ForFirma == forFirma).ToList();
        }

        public int GetCantidadImagenesInTemp(string tabla, string key)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Totales>("select count(RepCodigo) as CantidadTotal " +
                    "from TransaccionesImagenesTemp where RepTabla = ? and RepTablaKey = ? ", new string[] { tabla, key });

                if(list != null && list.Count > 0)
                {
                    return list[0].CantidadTotal;
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
            return 0;
        }

        public void DeleteTemp(bool forFirma, string tabla = null, string repTablaKey = null, int position = -1, bool marked = false)
        {
            try
            {
                string where = " where ForFirma = " + (forFirma?"1":"0") + " ";

                if (tabla != null) { where += " and RepTabla = '" + tabla + "'"; }

                if (repTablaKey != null)
                {
                    where += " and RepTablaKey = '" + repTablaKey + "'";
                }

                if (position != -1)
                {
                    where +=" and TraPosicion = " + position;
                }

                if (!marked)
                {
                    where += " and Ready = 0";
                }

                SqliteManager.GetInstance().Execute("delete from TransaccionesImagenesTemp " + where);

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public int MarkToSendToServer(string RepTabla, string repTablaKey, bool forFirma = false)
        {
            try
            {
                return SqliteManager.GetInstance().Execute("update TransaccionesImagenesTemp set Ready = 1 where RepTabla = ? and RepTablaKey = ? and ForFirma = " + (forFirma?"1":"0"), new string[] { RepTabla, repTablaKey });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public List<TransaccionesImagenesTemp> GetImagesReadyToSend()
        {
            try
            {

                /*SQLiteCommand cmd = SqliteManager.GetInstance().CreateCommand("select * from TransaccionesImagenesTemp where Ready = 1", new string[] { });
                //cmd.CommandText = "SELECT Data FROM Images WHERE Id=1";
                var data = cmd.ExecuteScalar<TransaccionesImagenesTemp>();

                return new List<TransaccionesImagenesTemp>(data);*/

                string query = "SELECT Rowguid, RepCodigo, RepTabla, RepTablaKey, TraImagen, TraPosicion, TraFormato, TraTamano, TitId, ForFirma FROM TransaccionesImagenesTemp WHERE Ready=1;";
                /*string conString = @" Data Source = \Program Files\Users.s3db ";
                SQLiteConnection con = new SQLiteConnection(conString);*/
                SQLiteCommand cmd = SqliteManager.GetInstance().CreateCommand(query, new string[] { });//new SQLiteCommand(query, con);


                var list = cmd.ExecuteQuery<TransaccionesImagenesTemp>();
                /*try
                {
                    while (rdr.Read())
                    {
                        byte[] a = (System.Byte[])rdr[0];
                        pictureBox1.Image = ByteToImage(a);
                    }
                }
                catch (Exception exc) { MessageBox.Show(exc.Message); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            con.Close();*/

                return list;

                //   return SqliteManager.GetInstance().Table<TransaccionesImagenesTemp>().Where(x => x.Ready == 1).ToList();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return new List<TransaccionesImagenesTemp>();
        }

        public  TransaccionesImagenesTemp GetFirmaByTransaccion(int TitId, string RepTablaKey)
        {
            TransaccionesImagenesTemp resultado = null;
            try
            {          
                string query = $"SELECT Rowguid, RepCodigo, RepTabla, RepTablaKey, TraImagen, TraPosicion, TraFormato, TraTamano, TitId, ForFirma FROM TransaccionesImagenesTemp WHERE ForFirma = 1 AND TitId ={TitId} AND RepTablaKey = '{RepTablaKey}';";
                SQLiteCommand cmd = SqliteManager.GetInstance().CreateCommand(query, new string[] { });//new SQLiteCommand(query, con);

                resultado = cmd.ExecuteQuery<TransaccionesImagenesTemp>().FirstOrDefault();              
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return resultado;

        }

        public int GetCountImgByVis()
        {
            string query = $"SELECT * FROM TransaccionesImagenesTemp WHERE RepTablaKey = {Arguments.Values.CurrentVisSecuencia}";
            SQLiteCommand cmd = SqliteManager.GetInstance().CreateCommand(query, new string[] { });//new SQLiteCommand(query, con);

            var img = cmd.ExecuteQuery<TransaccionesImagenesTemp>();

            if(img != null)
            {
                return img.Count();
            }

            return 0;
        }

    }
}

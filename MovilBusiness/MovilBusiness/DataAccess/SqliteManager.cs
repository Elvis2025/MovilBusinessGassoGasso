using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class SqliteManager : SQLiteConnection
    {
        private static SqliteManager instance;
        private static object collitionlock = new object();
        private static string KeyUrl;

        private SqliteManager() : 
            base(new SQLiteConnectionString(databasePath:DbPath(),
                openFlags:SQLiteOpenFlags.ReadWrite
                | SQLiteOpenFlags.Create 
                | SQLiteOpenFlags.FullMutex,
                storeDateTimeAsTicks:false, 
                key:KeyUrl)) {}

        public static SqliteManager GetInstance(bool isFromInstance = false)
        {
            lock (collitionlock)
            {
                if (instance == null || isFromInstance)
                {
                    string urltgive = new PreferenceManager().Get("KeyUrl");

                    KeyUrl = string.IsNullOrEmpty(urltgive)? "mdsoft09052008" : urltgive;

                    instance = new SqliteManager();
                    instance.CreateTempTables();
                }
                return instance;
            }
        }

        public static string DbPath()
        {
            var dbName = "MovilBusiness.db3";

            var info = DependencyService.Get<IAppInfo>();

            var path = Path.Combine(System.Environment.
              GetFolderPath(System.Environment.
              SpecialFolder.Personal), dbName);


            if(Device.RuntimePlatform == Device.Android)
            {
                path = Path.Combine(info.DatabasePath() + dbName);
            }  

            return path;
        }

        public static string DbName { get => "MovilBusiness.db3"; }

        /*public static string ZipDbPath()
        {
            var dbName = "MovilBusinessDb.zip";

            var info = DependencyService.Get<IAppInfo>();

            var path = Path.Combine(System.Environment.
              GetFolderPath(System.Environment.
              SpecialFolder.Personal), dbName);

            if (Device.RuntimePlatform == Device.Android)
            {
                path = Path.Combine(info.DatabasePath() + dbName);
            }

            return path;
        }*/

        public static string DbDirectoryPath
        {
            get
            {
                var info = DependencyService.Get<IAppInfo>();

                /*var path = System.Environment.
                  GetFolderPath(System.Environment.
                  SpecialFolder.Personal);

                if (Device.RuntimePlatform == Device.Android)
                {
                    path = info.DatabasePath();
                }*/

                return info.DatabasePath();
            }
            
        }

        public static bool ExistsTable(string tableName)
        {
            try
            {
                return GetInstance().Query<Totales>("select name as Total from sqlite_master where lower(type) = 'table' and trim(upper(name)) = trim(upper('"+tableName+"')) limit 1", new string[] { }).Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteDatabase()
        {
            if (instance != null)
            {
                Close();
            }

            instance = null;

            if (File.Exists(DbPath()))
            {
                File.Delete(DbPath());
            }
        }

        private void CreateTempTables()
        {
            CreateTable(typeof(SuscriptorCambios), CreateFlags.AutoIncPK);
            CreateTable(typeof(SuscriptorCambiosRecibidos), CreateFlags.AutoIncPK);

            try
            {
                CreateTable(typeof(ProductosTemp), CreateFlags.None);
            }
            catch (Exception)
            {
                DropTable<ProductosTemp>();
                CreateTable(typeof(ProductosTemp), CreateFlags.None);
            }


            try
            {
                Query<EntregasDetalleTemp>("select rowguid from EntregasDetalleTemp", new string[] { });
            }catch(Exception)
            {
                DropTable<EntregasDetalleTemp>();
            }

            CreateTable(typeof(ErrorLog), CreateFlags.None);
            CreateTable(typeof(RecibosDocumentosTemp), CreateFlags.None);
            CreateTable(typeof(DocumentosAplicadosTemp), CreateFlags.None);
            CreateTable(typeof(FormasPagoTemp), CreateFlags.None);
            CreateTable(typeof(TransaccionesImagenesTemp), CreateFlags.None);
            CreateTable(typeof(AuditoriasMercadosTemp), CreateFlags.None);
            CreateTable(typeof(ReplicacionesSuscriptores), CreateFlags.None);
            CreateTable(typeof(AsignacionRutasTemp), CreateFlags.None);
            //CreateTable(typeof(ProductosValidosOfertas), CreateFlags.None);
            CreateTable(typeof(EntregasDetalleTemp), CreateFlags.None);
            CreateTable(typeof(NoticiasTemp), CreateFlags.None);

            var mdc = new SQLiteCommand(this);
            BeginTransaction();

            try
            {
                mdc.CommandText = "drop table if exists ProductosValidosOfertas";
                mdc.ExecuteNonQuery();
            }
            catch (Exception) { }

            if (DS_RepresentantesParametros.GetInstance().GetOfertasyDescuentosbyUnidadMedida())
            {
                //mdc.CommandText = "create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean, UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, TitId integer, primary key(ProID, CliID, UnmCodigo, UnmCodigoOferta))";
                mdc.CommandText = "create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean, UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, TitId integer, primary key(ProID, CliID, UnmCodigo, UnmCodigoOferta,TitId))";
                mdc.ExecuteNonQuery();
                //Execute("create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean, UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, primary key(ProID, CliID, UnmCodigo, UnmCodigoOferta))");
            }
            else
            {
                //mdc.CommandText = "create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean , UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, TitId integer, primary key(ProID, CliID, UnmCodigo))";
                mdc.CommandText = "create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean , UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, TitId integer, primary key(ProID, CliID, UnmCodigo,TitId))";
                mdc.ExecuteNonQuery();
                //Execute("create table if not exists ProductosValidosOfertas(ProID integer, CliID integer, TieneOferta boolean, TieneDescuento boolean, PorcientoDescuento double, TieneDescuentoEscala boolean , UnmCodigo string ,UnmCodigoOferta string, OfeCaracteristicas string, primary key(ProID, CliID, OfeCaracteristicas))");
            }

            Commit();
        }

        public void GenerateDatabaseIndexs()
        {
            try
            {
                Execute("CREATE INDEX IF NOT EXISTS index_ProId ON Productos(ProID);", new string[] { });
                Execute("CREATE INDEX IF NOT EXISTS index_ProDescripcion ON Productos(ProID);", new string[] { });
                Execute("CREATE INDEX IF NOT EXISTS index_ProCodigo ON Productos(ProID);", new string[] { });

                Execute("CREATE INDEX IF NOT EXISTS index_CliNombre ON Clientes(CliNombre);", new string[] { });
                Execute("CREATE INDEX IF NOT EXISTS index_CliCodigo ON Clientes(CliCodigo);", new string[] { });

                Execute("CREATE INDEX IF NOT EXISTS index_GrpCodigo ON GrupoProductosDetalle(GrpCodigo, ProID);", new string[] { });
                Execute("CREATE INDEX IF NOT EXISTS index_GrcCodigo ON GrupoClientesDetalle(GrcCodigo, CliID);", new string[] { });

                Execute("CREATE INDEX IF NOT EXISTS index_prod_validosOfertas ON ProductosValidosOfertas(ProID, CliID)");

                Execute("CREATE INDEX IF NOT EXISTS index_OfeId ON Ofertas(OfeID);", new string[] { });
                
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void GenerateDatabaseIndexsRowGuid(List<ReplicacionesTablas> tablas)
        {
            try
            {
                if (tablas.Count > 0)
                {
                    //var db = SqliteManager.GetInstance();
                    //var args = new string[] { };

                    foreach (ReplicacionesTablas tabla in tablas.Where(x => x.RepTabla.Trim().ToUpper() != "ReplicacionesSuscriptores".Trim().ToUpper()).ToList())
                    {
                        Execute("CREATE INDEX IF NOT EXISTS IDX" + tabla.RepTabla + "rowguid ON " + tabla.RepTabla + "(rowguid);", new string[] { });
                    }

                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void AddVirtualColumns()
        {
            try
            {
                Execute("ALTER TABLE VISITAS ADD COLUMN VisSecuenciaOrigen integer", new string[] { });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            try
            {
                Execute("ALTER TABLE DEPOSITOS ADD COLUMN TieneFoto TEXT", new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public List<DbInfo> GetDatabaseRegistryInfo()
        {
            var tables = Query<DbInfo>("select name as TableName from sqlite_master where type='table' and name <> 'sqlite_sequence'", new string[] { });

            bool firstTime = true;

            string query = "";

            if(tables == null)
            {
                return new List<DbInfo>();
            }

            foreach(var table in tables)
            {
                if (firstTime)
                {
                    firstTime = false;
                    query = "select count(*) as RegistryCount, '"+table.TableName+"' as TableName from " + table.TableName;
                }
                else
                {
                    query += " union select count(*) as RegistryCount, '"+ table.TableName + "' as TableName from " + table.TableName;
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<DbInfo>();
            }

            query += " order by TableName";

            return Query<DbInfo>(query, new string[] { });

        }
    }
}
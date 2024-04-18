using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace MovilBusiness.DataAccess
{
    public class Hash : Dictionary<string, object>
    {
        private readonly string TABLE_NAME;
        private List<TableColumns> tableColumnsSchema;
        private readonly string[] dTypeQuotes = { "char", "varchar", "text", "uniqueidentifier", "date", "datetime", "datetimeoffset", "smalldatetime", "time", "image" };
        private readonly string[] numericTypes = { "int", "smallint", "double", "decimal", "bit", "money", "real", "float", "bigint", "numeric", "smallmoney", "tinyint", "mediumint", "integer" };
        public bool SaveScriptForServer { get; set; } = true;
        public bool SaveScriptInTransaccionesForServer { get; set; } = false;

        public Hash(string tableName)
        {
            TABLE_NAME = tableName;
            if (!SqliteManager.ExistsTable(tableName))
            {
                throw new Exception("La tabla " + tableName + " no existe!");
            }

            loadTableColumnsSchema();
        }
        public Hash(){}
        private void loadTableColumnsSchema()
        {
            tableColumnsSchema = SqliteManager.GetInstance().Query<TableColumns>("pragma table_info(" + TABLE_NAME + ")", new object[] { });
        }

        /*private bool existsTabla(string table)
        {
            try
            {
                return SqliteManager.GetInstance().Query<TableColumns>("pragma table_info(" + TABLE_NAME + ")", new object[] { })[0] != null;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }*/

        public new void Add(string key, object value)
        {
            Add(key, value, false);
        }

        public void Add(string key, object value, bool IsFunction)
        {
            Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture("en-US");

            if (value != null && value.GetType() == typeof(bool))
            {
                value = value.ToString().Equals("True") ? "1" : "0";
            }

            var columnSchema = GetColumnSchema(key);

            if (columnSchema == null)
            {
                Console.Write("La tabla " + TABLE_NAME + " no tiene la columna: " + key);
                return; //la tabla no tiene esta columna!
            }

            if (ContainsKey(key.ToLower()))
            {
                Remove(key.ToLower());
            }

            if (value == null)
            {
                value = "NULL";
                base.Add(key.ToLower(), value);
                return;
            }

            /*if (isNumberColumn(columnSchema.type) && value != null && value.ToString().Contains(","))
            {
                value = value.ToString().Replace(",", "");
            }*/

            if ((IsTextColumn(columnSchema.type) || (isNumberColumn(columnSchema.type) && value.ToString().Trim().Length == 0)) && !IsFunction)
            {
                value = "'" + value.ToString().Replace("'", "") + "'";

                if (columnSchema.type.ToLower().Contains("datetime") || columnSchema.type.ToLower().Contains("date"))
                {
                    value = "[DATETYPE]" + value;
                }
            }

            base.Add(key.ToLower(), value);

        }

        private TableColumns GetColumnSchema(string columnName)
        {
            return tableColumnsSchema.Where(x => x.name.Trim().ToUpper() == columnName.Trim().ToUpper()).FirstOrDefault();
            /*
            foreach (var col in tableColumnsSchema)
            {
                if (col.name.Trim().ToUpper() == columnName.Trim().ToUpper())
                {
                    return col;
                }
            }
            return null;*/
        }

        private bool IsTextColumn(string dataType)
        {
            dataType = dataType.ToLower();

            foreach (var type in dTypeQuotes)
            {
                if (dataType.Contains(type))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isNumberColumn(string dataType)
        {
            dataType = dataType.ToLower();

            foreach (var type in numericTypes)
            {
                if (dataType.Contains(type))
                {
                    return true;
                }
            }
            return false;
        }

        public int ExecuteInsert()
        {

            StringBuilder query = new StringBuilder("INSERT INTO " + TABLE_NAME + " (");

            try
            {

                if (Count == 0)
                {
                    return 0;
                }

                StringBuilder queryForSqlServer = new StringBuilder("INSERT INTO " + TABLE_NAME + " (");

                StringBuilder columnsBuilder = new StringBuilder();
                StringBuilder valuesBuilder = new StringBuilder();
                StringBuilder valuesForSqlServerBuilder = new StringBuilder();

                bool first = true;
                foreach (var data in this)
                {
                    columnsBuilder.Append((!first ? "," : "") + data.Key);
                    valuesBuilder.Append((!first ? "," : "") + data.Value.ToString().Replace("[DATETYPE]", ""));
                    valuesForSqlServerBuilder.Append((!first ? "," : "") + (data.Value.ToString().Contains("[DATETYPE]") ? "CONVERT(datetime, " + data.Value.ToString().Replace("[DATETYPE]", "").Replace("T", " ") + ", 120)" : data.Value.ToString()));
                    if (first)
                    {
                        first = false;
                    }
                }

                query.Append(columnsBuilder.ToString() + ") VALUES(" + valuesBuilder.ToString() + ")");
                queryForSqlServer.Append(columnsBuilder.ToString() + ") VALUES(" + valuesForSqlServerBuilder.ToString() + ")");

                // var quer = query.ToString();

                var result = !SaveScriptInTransaccionesForServer? SqliteManager.GetInstance().CreateCommand(query.ToString(), new object[] { }).ExecuteNonQuery() : -1;

                if (SaveScriptForServer)
                {
                    Functions.SaveSuscriptorCambioScript(queryForSqlServer.ToString().Replace("ifnull", "isnull").Replace("IFNULL", "ISNULL") + ";", TABLE_NAME, this["rowguid"].ToString().Replace("'", ""), "I");
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Constraint"))
                {
                    var tableColumnsSchem = tableColumnsSchema.Where(x => x.pk != 0).ToList();

                    string querys = "";
                    bool first = true;
                    foreach (var tableclumn in tableColumnsSchem)
                    {
                        var sdt = this.Where(x => x.Key == tableclumn.name.ToLower()).FirstOrDefault();
                        querys += (!first ? "," : "") + sdt.Value + " ";
                        if (first)
                        {
                            first = false;
                        }
                    }

                    querys = querys.Replace("'","");
                    ex.Data.Add("query", $"No puede ser insertado este valor ({querys}) en la tabla {TABLE_NAME}");
                    throw ex;
                }
                throw ex;
            }
            
        }


        public string ExecuteUpdate(string[] columns, DbUpdateValue[] values, bool trimed)
        {
            if(columns.Count() != values.Count())
            {
                throw new Exception("Las columnas no son iguales a los valores para realizar el update");
            }

            if(columns.Count() == 0)
            {
                throw new Exception("Las columnas no pueden estar vacias para realizar el update");
            }

            if(values.Count() == 0)
            {
                throw new Exception("Los valores no pueden estar vacios para realizar el update");
            }

            string whereSqlite = "";
            string where = "";

            for(var i = 0; i < columns.Count(); i++)
            {
                whereSqlite += (i > 0 ? " and " : "") + (trimed ? " trim(" : "") + columns[i] + (trimed ? ")" : "") + " = " + (trimed ? "trim(" : "") + (values[i].IsText ? "'" : "") + values[i].Value + (values[i].IsText ? "'" : "") + (trimed ? ")" : "");
                where += (i > 0 ? " and " : "") + columns[i] + " = " + (values[i].IsText ? "'" : "") + values[i].Value + (values[i].IsText ? "'" : "");
            }

            return ExecuteUpdate(where, whereSqlite);

        }

        public string ExecuteUpdate(string whereCondition, string whereForSqlite = null)
        {
            if (this.Count == 0)
            {
                return "";
            }

            whereCondition = whereCondition.Replace("WHERE", "").Replace("where", "");

            if (!string.IsNullOrWhiteSpace(whereForSqlite))
            {
                whereForSqlite = whereForSqlite.Replace("WHERE", "").Replace("where", "");
            }

            StringBuilder query = new StringBuilder("UPDATE " + TABLE_NAME + " SET ");
            StringBuilder queryForSqlServer = new StringBuilder("UPDATE " + TABLE_NAME + " SET ");

            bool first = true;
            foreach (var data in this)
            {
                query.Append((!first ? "," : "") + data.Key + " = " + data.Value.ToString().Replace("[DATETYPE]", ""));
                queryForSqlServer.Append((!first ? ", " : "") + data.Key + " = " + (data.Value.ToString().Contains("[DATETYPE]") ? "CONVERT(datetime, " + data.Value.ToString().Replace("[DATETYPE]", "") + ", 120)" : data.Value.ToString().Replace("DATETYPE", "")));
                if (first)
                {
                    first = false;
                }
            }

            if (whereCondition != null && whereCondition.Trim().Length > 0)
            {
                query.Append(" WHERE " + (string.IsNullOrWhiteSpace(whereForSqlite) ? whereCondition : whereForSqlite));
                queryForSqlServer.Append(" WHERE " + whereCondition);
            }

            if (query.ToString().Length == 0)
            {
                return "";
            }

            try
            {
                SqliteManager.GetInstance().Execute(query.ToString(), new string[] { });
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
           /* var cmd = SqliteManager.GetInstance().CreateCommand(query.ToString(), new object[] { });//.ExecuteNonQuery();

            cmd.ExecuteNonQuery();*/

            if (SaveScriptForServer)
            {
                Functions.SaveSuscriptorCambioScript(queryForSqlServer.ToString().Replace("ifnull", "isnull").Replace("IFNULL", "ISNULL") + ";", TABLE_NAME, null, "U");
            }

            return query.ToString();
        }

        public void ExecuteDelete(string whereCondition)
        {
            if (string.IsNullOrEmpty(whereCondition))
            {
                return;
            }

            whereCondition = whereCondition.Replace("WHERE", "").Replace("where", "");

            StringBuilder query = new StringBuilder("DELETE FROM " + TABLE_NAME + " WHERE " + whereCondition);

            //SqliteManager.GetInstance().Execute(query.ToString(), new string[] { });
            SqliteManager.GetInstance().CreateCommand(query.ToString(), new object[] { }).ExecuteNonQuery();

            if (SaveScriptForServer)
            {
                Functions.SaveSuscriptorCambioScript(query.ToString().Replace("ifnull", "isnull").Replace("IFNULL", "ISNULL")+";", TABLE_NAME, null, "U");
            }
        }

        public bool ExistColumnsSchema(string table_name, string columnName)
        {
           return SqliteManager.GetInstance().Query<TableColumns>("pragma table_info(" + table_name + ")", new object[] { })
                         .Where(x => x.name.Trim().ToUpper() == columnName.Trim().ToUpper()).FirstOrDefault() != null;
        }


        private sealed class TableColumns
        {
            public int cid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public bool notnull { get; set; }
            public string dflt_value { get; set; }
            public int pk { get; set; }
        }

    }
}

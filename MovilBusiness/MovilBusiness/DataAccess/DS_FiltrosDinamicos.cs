using MovilBusiness.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_FiltrosDinamicos
    {
        private List<FiltrosDinamicos> GetFiltrosDinamicos(string filKey)
        {
            var query = SqliteManager.
                        GetInstance().
                        Query<FiltrosDinamicos>(
                            @"SELECT name FROM sqlite_master 
                            WHERE type = 'table' AND upper (name) = upper ('FiltrosDinamicos')");
            
            if (query.Count == 0)
                return default(List<FiltrosDinamicos>);
            
            return SqliteManager.
                    GetInstance().
                    Query<FiltrosDinamicos>(
                    "select distinct FilKey, FilCampo, " +
                    "FilDescripcion, ifnull(UPPER(ltrim(rtrim(FilCondicion))), '') " +
                    "as FilCondicion, FilTipo, FilComboSelect, FilIndicadorDefault from " +
                    "FiltrosDinamicos WHERE ltrim(rtrim(upper(FilKey))) = ? " +
                    "order by ifnull(FilOrden, FilDescripcion)", new string[] { filKey.Trim().ToUpper() });
        }

        public List<FiltrosDinamicos> GetFiltrosProductos()
        {
            var list = GetFiltrosDinamicos("PRODFILTRO");

            if(list == null || list.Count == 0)
            {
                list = new List<FiltrosDinamicos>()
                {
                    new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProDescripcion", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Descripción" },
                    new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Código" },
                    new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.LinID", FilTipo = 2, FilDescripcion = "Linea", FilComboSelect = "select LinID as Key, LinDescripcion as Value FROM Lineas Order by LinDescripcion", FilCondicion = "=" }
                };
            }

            if (DS_RepresentantesParametros.GetInstance().GetParBusquedaCombinadaPorDefault())
                list.Add(new FiltrosDinamicos() { FilKey = "PRODFILTRO", FilCampo = "p.ProDescripcion, p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Combinada" });
                    

            return list;
        }

        public List<FiltrosDinamicos> GetFiltroOrdenarProductos()
        {
            try
            {
                return GetFiltrosDinamicos("PRODORDEN");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        public List<FiltrosDinamicos> GetFiltrosClientes()
        {
            try
            {
                var list = GetFiltrosDinamicos("CLIFILTRO");

                if (list == null || list.Count == 0)
                {
                    list = new List<FiltrosDinamicos>()
                    {
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliNombre", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Nombre" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Código" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo, c.CliNombre, c.CliNombreComercial", FilTipo = 1, FilDescripcion = "Combinada", FilCondicion = "LIKE" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo", FilTipo = 1, FilDescripcion = "Código barra", FilCondicion = "=" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliDatosOtros", FilCondicion = "LIKE", FilTipo = 2, FilDescripcion = "Caracteristicas", FilComboSelect = "select CodigoUso as Key, Descripcion as Value FROM UsosMultiples where trim(upper(CodigoGrupo)) = 'CLIDATOSOTROS' Order by Descripcion"}
                    };
                }

                return list;
            }catch(Exception e)
            {
                Console.Write(e.Message);

                return new List<FiltrosDinamicos>()
                    {
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliNombre", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Nombre" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Código" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo, c.CliNombre, c.CliNombreComercial", FilTipo = 1, FilDescripcion = "Combinada", FilCondicion = "LIKE" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliCodigo", FilTipo = 1, FilDescripcion = "Código barra", FilCondicion = "=" },
                        new FiltrosDinamicos() { FilKey = "CLIFILTRO", FilCampo = "c.CliDatosOtros", FilCondicion = "LIKE", FilTipo = 2, FilDescripcion = "Caracteristicas", FilComboSelect = "select CodigoUso as Key, Descripcion as Value FROM UsosMultiples where trim(upper(CodigoGrupo)) = 'CLIDATOSOTROS' Order by Descripcion"}
                    };
            }
        }


        public List<FiltrosDinamicos> GetFiltrosProductosAuditoriasPrecios()
        {
            var list = GetFiltrosDinamicos("PRODAUDFILTRO");

            if (list == null || list.Count == 0)
            {
                list = new List<FiltrosDinamicos>()
                {
                    new FiltrosDinamicos() { FilKey = "PRODAUDFILTRO", FilCampo = "p.ProDescripcion", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Descripción" },
                    new FiltrosDinamicos() { FilKey = "PRODAUDFILTRO", FilCampo = "p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Código" },
                };
            }

            if (DS_RepresentantesParametros.GetInstance().GetParBusquedaCombinadaPorDefault())
                list.Add(new FiltrosDinamicos() { FilKey = "PRODAUDFILTRO", FilCampo = "p.ProDescripcion, p.ProCodigo", FilTipo = 1, FilCondicion = "LIKE", FilDescripcion = "Combinada" });


            return list;
        }
    }
}

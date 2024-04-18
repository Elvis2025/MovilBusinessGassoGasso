using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MovilBusiness.DataAccess
{
    public class DS_AuditoriasMercados
    {
        public bool HasProductsInTemp()
        {
            try
            {

                return SqliteManager.GetInstance().Query<AuditoriasMercadosTemp>("select 1 as AudGondolaSuelo from AuditoriasMercadosTemp limit 1", new string[] { }).Count > 0;

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return false;
        }

        public List<AuditoriasMercadosTemp> GetTemp()
        {
            return SqliteManager.GetInstance().Query<AuditoriasMercadosTemp>("select * from AuditoriasMercadosTemp", new string[] { });
        }

        public List<Categorias1AuditoriasMercado> GetCategorias1Auditorias()
        {
            return SqliteManager.GetInstance().Query<Categorias1AuditoriasMercado>("select Ca1Codigo, Ca1Descripcion from Categorias1AuditoriasMercado " +
                "order by Ca1Descripcion", new string[] { });
        }

        public List<Categorias2AuditoriasMercado> GetCategorias2ByCa1Codigo(string ca1Codigo)
        {
            return SqliteManager.GetInstance().Query<Categorias2AuditoriasMercado>("select Ca2Codigo, Ca2Descripcion, Ca1Codigo from Categorias2AuditoriasMercado " +
                "where Ca1Codigo = ? order by Ca2Descripcion", new string[] { ca1Codigo });
        }

        public List<Categorias2AuditoriasMercado> GetCategorias2()
        {
            return SqliteManager.GetInstance().Query<Categorias2AuditoriasMercado>("select Ca2Codigo, Ca2Descripcion, Ca1Codigo from Categorias2AuditoriasMercado " +
                "order by Ca2Descripcion", new string[] {});
        }

        public List<MarcasAuditoriasMercado> GetMarcasAuditorias(string ca1Codigo, string ca2Codigo)
        {
            return SqliteManager.GetInstance().Query<MarcasAuditoriasMercado>("select MarCodigo, MarDescripcion, Ca1Codigo, Ca2Codigo, ifnull(MarFragancia, '') as MarFragancia from MarcasAuditoriasMercado " +
                "where Ca1Codigo= ? and Ca2Codigo = ? order by MarDescripcion", new string[] { ca1Codigo, ca2Codigo });
        }
        public List<MarcasAuditoriasMercado> GetMarcasAuditoriaswithout()
        {
            return SqliteManager.GetInstance().Query<MarcasAuditoriasMercado>("select MarCodigo, MarDescripcion, Ca1Codigo, Ca2Codigo, MarFragancia from MarcasAuditoriasMercado " +
                "order by MarDescripcion", new string[] {});
        }

        public List<PresentacionesMarcasAuditoriasMercado> GetPresentacionesAuditorias(string ca1Codigo, string ca2Codigo, string marCodigo)
        {
            return SqliteManager.GetInstance().Query<PresentacionesMarcasAuditoriasMercado>("select PreCodigo, PreDescripcion, Ca1Codigo, Ca2Codigo, MarCodigo from PresentacionesMarcasAuditoriasMercado " +
                "where Ca1Codigo = ? and Ca2Codigo = ? and MarCodigo = ? order by PreDescripcion", new string[] { ca1Codigo, ca2Codigo, marCodigo });
        }

        public List<EmpaquesAuditoriasMercados> GetEmpaquesAuditorias(string ca1Codigo, string ca2Codigo, string marCodigo, string preCodigo)
        { 
             return SqliteManager.GetInstance().Query<EmpaquesAuditoriasMercados>("select EmpCodigo, EmpDescripcion, Ca1Codigo, Ca2Codigo, MarCodigo, PreCodigo from EmpaquesAuditoriasMercados " +
                "where ca1Codigo = ? and Ca2Codigo = ? and MarCodigo = ? and PreCodigo = ? order by EmpDescripcion", new string[] { ca1Codigo, ca2Codigo, marCodigo, preCodigo });
        }

        public List<UnidadDeMedidasAuditoriasMercados> GetUnidadesMedidasAuditorias(string ca1Codigo, string ca2Codigo, string marCodigo, string preCodigo, string empCodigo)
        {
            return SqliteManager.GetInstance().Query<UnidadDeMedidasAuditoriasMercados>("select Ca1Codigo, Ca2Codigo, MarCodigo, PreCodigo, EmpCodigo, UnidCodigo, UnidDescripcion from UnidadDeMedidasAuditoriasMercados " +
                "where Ca1Codigo = ? and Ca2Codigo = ? and MarCodigo = ? and PreCodigo = ? and EmpCodigo = ? order by UnidDescripcion", new string[] { ca1Codigo, ca2Codigo, marCodigo, preCodigo, empCodigo });
        }

        public List<UsosMultiples> GetAuditoriaTamanos()
        {
            return SqliteManager.GetInstance().Query<UsosMultiples>("select ifnull(trim(CodigoUso), '') as CodigoUso, Descripcion from UsosMultiples " +
                "where UPPER(ltrim(rtrim(CodigoGrupo))) = ? order by Descripcion", new string[] { "AUDTAMANO" });
        }
        public List<PresentacionesMarcasAuditoriasMercado> GetPresentacionesAuditoriasGenericas()
        {
            return SqliteManager.GetInstance().Query<PresentacionesMarcasAuditoriasMercado>("select ifnull(trim(CodigoUso), '') as PreCodigo, Descripcion as PreDescripcion from UsosMultiples " +
                "where UPPER(ltrim(rtrim(CodigoGrupo))) = ? order by Descripcion", new string[] { "AutPresentacion".ToUpper() });
        }

        public List<PresentacionesMarcasAuditoriasMercado> GetPresentacionesAuditoriasGenericaswithout()
        {
            return SqliteManager.GetInstance().Query<PresentacionesMarcasAuditoriasMercado>("select PreCodigo, PreDescripcion from PresentacionesMarcasAuditoriasMercado " +
                "order by PreDescripcion", new string[] {});
        }

        public List<EmpaquesAuditoriasMercados> GetEmpaquesAuditoriasGenericos()
        {
            return SqliteManager.GetInstance().Query<EmpaquesAuditoriasMercados>("select ifnull(trim(CodigoUso), '') as EmpCodigo, Descripcion as EmpDescripcion from UsosMultiples " +
                "where UPPER(ltrim(rtrim(CodigoGrupo))) = ? order by Descripcion", new string[] { "AudEmpaque".ToUpper() });
        }
        public List<EmpaquesAuditoriasMercados> GetEmpaquesAuditoriasGenericosWithOut()
        {
            return SqliteManager.GetInstance().Query<EmpaquesAuditoriasMercados>("select EmpCodigo, EmpDescripcion from EmpaquesAuditoriasMercados " +
                "order by EmpDescripcion", new string[] {});
        }
        
        public List<Categorias1AuditoriasMercado> GetCategoriasAuditoriasGenericas()
        {
            return SqliteManager.GetInstance().Query<Categorias1AuditoriasMercado>("select ifnull(trim(CodigoUso), '') as Ca1Codigo, Descripcion as Ca1Descripcion from UsosMultiples " +
                "where UPPER(ltrim(rtrim(CodigoGrupo))) = ? order by Descripcion", new string[] { "LINCOMP".ToUpper() });
        }       
        public List<Categorias1AuditoriasMercado> GetCategoriasAuditoriasGenericaswithout()
        {
            return SqliteManager.GetInstance().Query<Categorias1AuditoriasMercado>("select Ca1Codigo, Ca1Descripcion from Categorias1AuditoriasMercado " +
                " order by Ca1Descripcion", new string[] {});
        }

        public List<UnidadDeMedidasAuditoriasMercados> GetUnidadesMedidasAuditoriasGenericas()
        {
            return SqliteManager.GetInstance().Query<UnidadDeMedidasAuditoriasMercados>("select ifnull(trim(CodigoUso), '') as UnidCodigo, Descripcion as UnidDescripcion from UsosMultiples " +
                "where UPPER(ltrim(rtrim(CodigoGrupo))) = ? order by Descripcion", new string[] { "UnmCodigo".ToUpper() });
        }
        public List<UnidadDeMedidasAuditoriasMercados> GetUnidadesMedidasAuditoriasGenericasWhitOut()
        {
            return SqliteManager.GetInstance().Query<UnidadDeMedidasAuditoriasMercados>("select UnidCodigo, UnidDescripcion from UnidadDeMedidasAuditoriasMercados " +
                "order by UnidDescripcion", new string[] {});
        }

        private bool ExistsInTemp(AuditoriasMercadosTemp data)
        {
            var list = SqliteManager.GetInstance().Query<AuditoriasMercadosTemp>("select Ca1Codigo from AuditoriasMercadosTemp where " +
                "ifnull(AudEmpaque, '') = ? and ifnull(UnmCodigo, '') = ? and ifnull(AudPresentacion, '') = ? " +
                "and ifnull(AudVariedad, '') = ? and ifnull(AudCapacidad, 0) = ? " +
                "and Ca1Codigo = ? and Ca2Codigo = ? and MarCodigo = ? limit 1", 
                new string[] { data.AudEmpaque, data.UnmCodigo, data.AudPresentacion, data.AudVariedad, data.AudCapacidad.ToString(), data.Ca1Codigo, data.Ca2Codigo, data.MarCodigo });

            return list != null && list.Count > 0;
        }

        public void InsertTemp(AuditoriasMercadosTemp data, bool IsEditing)
        {
            if (!IsEditing && ExistsInTemp(data))
            {
                throw new Exception("Ya existe un articulo con estas caracteristicas, no puede agregar otro");
            }

            if (IsEditing)
            {
                DeleteTempByRowguid(data.rowguid);
            }

            SqliteManager.GetInstance().InsertOrReplace(data);
        }

        public void SaveAuditoria(ObservableCollection<AuditoriasMercadosTemp> productos, string tamano, int cajasRegistradoras)
        {
            var audSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("AuditoriasMercados");

            Hash aud = new Hash("AuditoriasMercados");
            aud.Add("Repcodigo", Arguments.CurrentUser.RepCodigo);
            aud.Add("AudSecuencia", audSecuencia);
            aud.Add("AudFecha", Functions.CurrentDate());
            aud.Add("CliID", Arguments.Values.CurrentClient.CliID);
            aud.Add("AudEstatus", 1);
            aud.Add("VisSecuencia", Arguments.Values.CurrentVisSecuencia);
            aud.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            aud.Add("AudTamano", tamano);
            aud.Add("AudCajasRegistradoras", cajasRegistradoras);
            aud.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            aud.Add("AudFechaActualizacion", Functions.CurrentDate());
            aud.Add("rowguid", Guid.NewGuid().ToString());
            aud.Add("mbVersion", Functions.AppVersion);
            aud.ExecuteInsert();

            int pos = 1;

            var montoTotal = 0.0;
            foreach(var prod in productos)
            {
                Hash d = new Hash("AuditoriasMercadosDetalle");
                d.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                d.Add("AudSecuencia", audSecuencia);
                d.Add("AudPosicion", pos); pos++;
                d.Add("ProID", 0);
                d.Add("AudEmpaque", prod.AudEmpaque);
                d.Add("UnmCodigo", prod.UnmCodigo);
                d.Add("AudCapacidad", prod.AudCapacidad);
                d.Add("AudUnidadVenta", prod.AudUnidadVenta);
                d.Add("AudPrecioPublico", prod.AudPrecioPublico);

                montoTotal += prod.AudPrecioPublico;

                d.Add("AudPrecioOferta", prod.AudPrecioOferta);
                d.Add("AudGondolaSuelo", prod.AudGondolaSuelo);
                d.Add("AudGondolaManos", prod.AudGondolaManos);
                d.Add("AudGondolaOjos", prod.AudGondolaOjos);
                d.Add("AudGondolaTecho", prod.AudGondolaTecho);
                d.Add("AudEspacioCabecera", prod.AudEspacioCabecera);
                d.Add("AudEspacioIsla", prod.AudEspacioIsla);
                d.Add("AudEspacioCajas", prod.AudEspacioCajas);
                d.Add("AudEspacioFrio", prod.AudEspacioFrio);
                d.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                d.Add("AudFechaActualizacion", Functions.CurrentDate());
                d.Add("rowguid", Guid.NewGuid().ToString());
                d.Add("AutPresentacion", prod.AudPresentacion);
                d.Add("Ca1Codigo", prod.Ca1Codigo);
                d.Add("Ca2Codigo", prod.Ca2Codigo);
                d.Add("MarCodigo", prod.MarCodigo);
                d.Add("AudPrecioCompra", prod.AudPrecioCompra);
                d.Add("AudVariedad", prod.AudVariedad);
                d.ExecuteInsert();
            }

            DS_RepresentantesSecuencias.UpdateSecuencia("AuditoriasMercados", audSecuencia);

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }
            

            DeleteTemp();

        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select 56 as TitID, count(*) as VisCantidadTransacciones, '' as VisComentario from AuditoriasMercados " +
                "where RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"' and VisSecuencia = ? ", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public void DeleteTemp(AuditoriasMercadosTemp item = null)
        {
            try
            {
                if (item != null)
                {
                    SqliteManager.GetInstance().Delete(item);
                    return;
                }

                SqliteManager.GetInstance().Execute("delete from AuditoriasMercadosTemp", new string[] { });

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void DeleteTempByRowguid(string rowguid)
        {
            SqliteManager.GetInstance().Execute("delete from AuditoriasMercadosTemp where ltrim(rtrim(UPPER(rowguid))) = ? ", new string[] { rowguid.Trim().ToUpper() });
        }
    }
}

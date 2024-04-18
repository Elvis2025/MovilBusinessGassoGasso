using MovilBusiness.Configuration;
using MovilBusiness.Model;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class ReferenciaValor
    {
        public string ParReferencia { get; set; }
        public string ParValor { get; set; }
    }
    public class DS_TablasRelaciones : DS_Controller
    {

        public void GetTablasRelaciones(string tableName1, string tableName2)
        {
            TablasRelaciones result = SqliteManager.GetInstance().Query<TablasRelaciones>($@"select TabPk2 from TablasRelaciones where TabNombre1 = '{tableName1}' and 
                                                    TabNombre2 ='{tableName2}' and
                                                 TabPk1 = '{Arguments.Values.CurrentClient.CanID}'").FirstOrDefault();
            if(result != null)
            {
                var valores = JsonConvert.DeserializeObject<List<ReferenciaValor>>(result.TabPk2);
                if (valores != null && valores.Count > 0){
                    foreach (var valor in valores)
                    {
                        if(!string.IsNullOrWhiteSpace(valor.ParReferencia) && !string.IsNullOrWhiteSpace(valor.ParValor))
                        {

                        SqliteManager.GetInstance().Execute($@"Update RepresentantesParametros set ParValor = '{valor.ParValor}'
                                         where UPPER(ltrim(rtrim(ParReferencia))) = '{valor.ParReferencia}'");
                        }
                    }
                }
                //SqliteManager.GetInstance().Execute($@"Update RepresentantesParametros set ParValor = '{result.TabPk2}'
                //                         where UPPER(ltrim(rtrim(ParReferencia))) = 'UNMCODIGO'");
            }
        }

    }
}


using MovilBusiness.model;

using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_Categorias
    {
        public List<Categoria1> GetCategorias1()
        {
            return SqliteManager.GetInstance().Query<Categoria1>("SELECT Cat1Descripcion, Cat1ID FROM Categoria1 " +
                "WHERE Cat1ID in (SELECT DISTINCT Cat1ID from Productos) order by Cat1Descripcion", new string[] { });
        }

        public List<Categoria2> GetCategorias2()
        {
            return SqliteManager.GetInstance().Query<Categoria2>("SELECT Cat2Descripcion, Cat2ID " +
                "FROM Categoria2 ORDER BY Cat2Descripcion", new string[] { });
        }
    }
}


using MovilBusiness.model;

using System.Collections.Generic;


namespace MovilBusiness.DataAccess
{
    public class DS_TiposMensaje
    {
        public List<TiposMensaje> GetTipoMensaje(int TraId)
        {
            return SqliteManager.GetInstance().Query<TiposMensaje>("select TraID, MenID, ifnull(MenDescripcion, '') as MenDescripcion " +
                "from TiposMensaje where TraID = ? and ltrim(rtrim(replace(upper(ifnull(MenDescripcion, '')), 'S', ''))) <> 'OTRO'" +
                "union select "+TraId+" as TraID, -1, 'Otros' as MenDescripcion order by MenDescripcion", new string[] { TraId.ToString() });
        }

        public List<TiposMensaje> GetTipoMensajeSinOtros(int TraId)
        {
            return SqliteManager.GetInstance().Query<TiposMensaje>("select TraID, MenID, ifnull(MenDescripcion, '') as MenDescripcion " +
                "from TiposMensaje where TraID = ? and ltrim(rtrim(replace(upper(ifnull(MenDescripcion, '')), 'S', ''))) <> 'OTRO'" +
                " order by MenDescripcion", new string[] { TraId.ToString() });
        }
    }
}

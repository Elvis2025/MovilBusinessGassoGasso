using MovilBusiness.model;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Empresa
    {
        public Empresa GetEmpresa(string seccodigo="")
        {
            try
            {

                List<Empresa> list = SqliteManager.GetInstance().Query<Empresa>("select EmpNombre, EmpDireccion, EmpDireccion2, EmpDireccion3, EmpRNC, EmpTelefono, EmpFax, EmpLogo " +
                    "from Empresa order by EmpID", new string[] { });

                int i;
                bool result = int.TryParse(seccodigo, out i);

                //if (result && !string.IsNullOrWhiteSpace(seccodigo))
                //{
                //    list = SqliteManager.GetInstance().Query<Empresa>("select EmpNombre, EmpDireccion, EmpDireccion2, EmpDireccion3, EmpRNC, EmpFax, EmpTelefono, EmpLogo " +
                //   "from Empresa  where EmpID = ? or ifnull(OrvCodigo,'') = ? order by EmpID", new string[] { seccodigo, seccodigo });
                //}
                if (!string.IsNullOrWhiteSpace(seccodigo))
                {
                    if (result)
                    {
                        list = SqliteManager.GetInstance().Query<Empresa>("select EmpNombre, EmpDireccion, EmpDireccion2, EmpDireccion3, EmpRNC, EmpFax, EmpTelefono, EmpLogo " +
                  "from Empresa  where EmpID = ?  order by EmpID", new string[] { seccodigo });
                    }
                     
                    if(list == null || list.Count == 0)
                    {
                        list = SqliteManager.GetInstance().Query<Empresa>("select EmpNombre, EmpDireccion, EmpDireccion2, EmpDireccion3, EmpRNC, EmpFax, EmpTelefono, EmpLogo " +
                                    "from Empresa  where  ifnull(OrvCodigo,'') = ? order by EmpID", new string[] { seccodigo });
                    }
                }

                if (list != null && list.Count > 0)
                {
                    return list[0];
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }
    }
}

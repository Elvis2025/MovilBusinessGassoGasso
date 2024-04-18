using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal.Structs.services
{
    public class UpdateInfo
    {
        public int versionNumber
        {
            get
            {
                int.TryParse(versionName.Replace(".", ""), out int version);

                return version;
            }
        }
        public string fileName { get; set; }
        public string downloadURL { get; set; }
        public string versionName { get; set; }
    }
}

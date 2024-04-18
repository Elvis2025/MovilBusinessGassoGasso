using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesHorarios
    {
        public int CliID { get; set; }
        public int ClhSecuencia { get; set; }
        public string clhHorarioApertura { get; set; }
        public string clhHorarioCierre { get; set; }
        public string ClhDia { get; set; }

        public string GetHorarioFormat(string value)
        {
            if(TimeSpan.TryParse(value, out TimeSpan timeSpan))
            {
                return DateTime.Today.Add(timeSpan).ToString("hh:mm tt");
            }

            return "";
        }

    }
}

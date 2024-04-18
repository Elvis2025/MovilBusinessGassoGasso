using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class SolicitudActualizacionClientesHorarios
    {
        [JsonIgnore]public string ClhDiaDescripcion { get; set; }
        [JsonIgnore]public string ClhHorarioAperturaAMPM { get; set; }
        [JsonIgnore] public string ClhHorarioCierreAMPM { get; set; }

        public string ClhDia { get; set; }
        public string ClhHorarioApertura { get; set; }
        public string ClhHorarioCierre { get; set; }
    }
}

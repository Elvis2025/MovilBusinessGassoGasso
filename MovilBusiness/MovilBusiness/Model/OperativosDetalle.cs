using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class OperativosDetalle
    {
        public string RepCodigo { get; set; }
        public int OpeID { get; set; }
        public int OpeSecuencia { get; set; }
        public int CliID { get; set; }
        public int ProID { get; set; }
        public int OpeProductoCantidad { get; set; }
        public string OpePacienteNombre { get; set; }
        public string OpePacienteTelefono { get; set; }
        public string OpePacienteEmail { get; set; }
        public string OpeSector { get; set; }

        public int EspID { get; set; }
        public string rowguid { get; set; }

        public string OpeNombreDoctor { get; set; }

        public List<OperativosDetalleProductos> Productos { get; set; }

        [Ignore]public string ProductosDesc { get; set; }
    }
}

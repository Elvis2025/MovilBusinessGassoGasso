
using SQLite;

namespace MovilBusiness.model
{
    public class Representantes
    {
        public string RepCodigo { get; set; }
        public string RepClave { get; set; }
        public string RepNombre { get; set; }
        public string RepCargo { get; set; }
        public bool RepIndicadorSupervisor { get; set; }
        public string RepSupervisor { get; set; }
        public int RepEstatus { get; set; }
        public int VehID { get; set; }
        public int RutID { get; set; }
        public string RepTelefono1 { get; set; }
        public string RepTelefono2 { get; set; }

        [Ignore] public int TipoRelacionClientes { get; set; } = 1;
        [Ignore] public string RepTitulo { get => RepCodigo + "-" + RepNombre; }

        public bool IsAuditor { get { return !string.IsNullOrWhiteSpace(RepCargo) && RepCargo.Trim().ToUpper().Equals("AUDITORRUTAS"); } }

        public override string ToString()
        {
            return RepNombre;
        }
    }
}

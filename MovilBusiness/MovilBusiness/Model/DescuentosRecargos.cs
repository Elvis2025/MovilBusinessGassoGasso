
namespace MovilBusiness.model
{
    public class DescuentosRecargos
    {
        public int DesID { get; set; }
        public int DoROperacion { get; set; }
        public int ProID { get; set; }
        public string GrpCodigo { get; set; }
        public int CliID { get; set; }
        public string GrcCodigo { get; set; }
        public string DesDescripcion { get; set; }
        public int DesMetodo { get; set; }
        public string DesFechaInicio { get; set; }
        public string DesFechaFin { get; set; }
        public int DesCantidadSKU { get; set; }
        public int DescNivel { get; set; }
        public int DesTipo { get; set; }
        public string GrpCodigoDescuento { get; set; }
        public int DesOrden { get; set; }
        public int ConID { get; set; }
        public string DesFuente { get; set; }

        public string DesTipoDescripcion { get; set; }
        public string ConIdDescripcion { get; set; }
        public string UnmCodigo { get; set; }
        public bool IsConsultaGeneral { get; set; }

        public bool IsMancomunado { get => DesMetodo == 6 || DesMetodo == 2 || DesMetodo == 3; }
    }
}

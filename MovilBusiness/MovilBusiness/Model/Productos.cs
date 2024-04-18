
namespace MovilBusiness.model
{
    public class Productos
    {
        public int ProID { get; set; }
        public int LinID { get; set; }
        public string ProDescripcion { get; set; }
        public double ProPrecio { get; set; }
        public double ProPrecio2{ get; set; }
        public double ProPrecio3{ get; set; }
        public double ProPrecioMin{ get; set; }
        public string ProCodigo{ get; set; }
        public string ProReferencia{ get; set; }
        public double ProUnidades{ get; set; }
        public double ProCantidad{ get; set; }
        public bool ProIndicadorDetalle{ get; set; }
        public string ProDatos3{ get; set; }
        public string UnmCodigo{ get; set; }
        public string rowguid{ get; set; }
        public double ProItbis{ get; set; }
        public double ProSelectivo{ get; set; }
        public double ProAdValorem{ get; set; }
        public double ProCantidadDetalle { get; set; }
        public double ProDescuentoMaximo { get; set; }
        public double ProHolgura { get; set; }

        public bool IsSelected { get; set; }

        /*public Productos GetRawObject()
        {
            return this;
        }

        public string GetSubTitle()
        {
            return "Precio: "+ProPrecio+"      Cantidad: "+ProCantidad;
        }

        public string GetTitle()
        {
            return ProDescripcion;
        }*/
    }
}

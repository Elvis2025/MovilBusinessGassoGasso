

using MovilBusiness.Abstraction;

namespace MovilBusiness.model
{
    public class ResumenCuadres
    {
        public int mCantidadClientesAVisitar{ get; set; }
        public int mCantidadClientesVisitados { get; set; }
        public int mCantidadVisitasPositivas {  get; set; }
        public string mTotalTiempoRuta {  get; set; }
        public string mTiempoPromRuta { get; set; }
        public string mTiempoPromVisitas {  get; set; }
        public int mNumFacturasGeneradas {  get; set; }
        public double mEfectividad { get; set; }
        public double mPromVentasPorVisitas { get; set; }
    }
}

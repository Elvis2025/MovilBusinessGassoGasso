
using System;


namespace MovilBusiness.model.Internal
{
    public class ProductosArgs : EventArgs
    {
        public string valueToSearch { get; set; }
        public FiltrosDinamicos filter { get; set; }
        public string lipCodigo { get; set; }
        public string secondFilter { get; set; }
        public string IdFactura { get; set; } = "-1";
        public bool FiltrarProductosPorSector { get; set; } = false;
        public bool NotUseTemp { get; set; } = false;
        public string MonCodigo { get; set; }

        public bool useAlmacenDespacho { get; set; }
        public string orderBy { get; set; } = null;

        public bool IsEntregandoTraspaso { get; set; }

        public int ProID { get; set; } = -1;

        public string referenceSplit { get; set; }

        public string ProCodigo { get; set; }
        public string ProUndMedidas { get; set; }

        public bool precioMayorQueCero { get; set; } = true;
    }
}

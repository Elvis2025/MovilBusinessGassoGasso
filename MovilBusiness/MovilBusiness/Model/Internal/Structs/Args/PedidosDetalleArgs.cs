using MovilBusiness.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using System;
using System.Collections.Generic;

namespace MovilBusiness.Model.Internal.Structs.Args
{
    public class PedidosDetalleArgs
    {
        public DateTime FechaEntrega = DateTime.Now;
        public int ConId;
        public int DisenoDelRow;
        public string PedOrdenCompra;
        public bool IsEditing = false;
        public bool FromCopy = false;
        public int TipoPedido = 1;
        public DevolucionesArgs? devArgs = null;
        public int InvArea = -1;
        public int EditedTraSecuencia = -1;
        public int PedTipoTrans = -1;
        public string ComTipoPago = null;
        public ClientesDependientes CompraDependiente = null;
        public int Prioridad = 0;
        public string CldDirTipo = null;
        public string PedCamposAdicionales;
        public string RepAuditor { get; set; }
        public string CedCodigo;
        public Almacenes CurrentAlmacenConteo = null;
        public List<FormasPagoTemp> comprasFormasPago = new List<FormasPagoTemp>();

        public EntregasRepartidorTransacciones CurrentEntrega = null;

        public bool EnEspera = false;

        public int CliIDMaster = -1;
        public string MonCodigo { get; set; }

        public string RepCodigoTraspaso { get; set; }
        public bool IsEntregandoTraspaso { get; set; }
        public int VenCantidadCanastos { get; set; }
        public int motivodevolucion { get; set; }

        public double PorcientoDescuentoManual { get; set; }

        public bool IsMultiEntrega { get; set; }
    }
}

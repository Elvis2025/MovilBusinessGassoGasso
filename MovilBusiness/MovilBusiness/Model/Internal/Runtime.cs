using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using System.Collections.Generic;

namespace MovilBusiness.model.Internal
{
    public class Runtime
    {
        public int CurrentVisSecuencia { get; set; } = -1;
        public Location CurrentLocation { get; set; } = null;
        public int CurrentCuaSecuencia { get; set; } = -1;
        public int CurrentCountDes { get; set; } = -1;
        public int CurrentTraSecuencia { get; set; } = -1;
        public Clientes CurrentClient { get; set; } = null;
        public Sectores CurrentSector { get; set; } = null;
        public Modules CurrentModule = Modules.NULL;

        public Modules ANTSMODULES = Modules.NULL;
        public RecibosDocumentosTemp CurrenRecDocumentosTemp { get; set; }
        public string SecCodigoParaCrearVisita { get; set; } = null;
        public string ProidForExclurIdOfertas { get; set; } = "";
        public string AlmRef { get; set; } = null;
        public int AlmId { get; set; } = -1;
        public bool IsUpdatePrecioForDev { get; set; } = false;

        public bool IsPushMoneyRotacion { get; set; } = false;

        public List<UsosMultiples> CliDatosOtros { get; set; } //se usa para en la lista de clientes mostrar la descripcion de CliDatosOtros, en el cual cada letra significa algo
       
        public string RutaImagenes { get; set; } = DS_RepresentantesParametros.GetInstance().RutaImagenesProductos() + DS_RepresentantesParametros.GetInstance().GetParPrefijoFotos();

        public int CurrentTipoVisita { get; set; } = -1;
        public void Clear()
        {
            CurrentVisSecuencia = -1;
            CurrentLocation = null;
            CurrentCuaSecuencia = -1;
            CurrentTraSecuencia = -1;
            CurrentClient = null;
            CurrentSector = null;
            CurrentModule = Modules.NULL;
            ANTSMODULES = Modules.NULL;
            CliDatosOtros = null;
            SecCodigoParaCrearVisita = null;
            CurrentTipoVisita = -1;
            AlmRef = null;
            AlmId = -1;
            CurrentCountDes = -1;
            ProidForExclurIdOfertas = "";
            IsPedidoAutorizado = false;
            IsUpdatePrecioForDev = false;
        }

        public void ClearAlm()
        {
            AlmRef = null;
            AlmId = -1;
        }

        public bool IsPedidoAutorizado { get; set; } = false;
    }
}

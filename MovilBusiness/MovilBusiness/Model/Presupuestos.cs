using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model
{
    public class Presupuestos
    {
        public string RepCodigo { get; set; }
        public string PreTipo { get; set; }
        public int PreAnio { get; set; }
        public int PreMes { get; set; }
        public string PreReferencia { get; set; }
        public double PrePresupuesto { get; set; }
        public double PreEjecutado { get; set; }
        public string RepSupervisor { get; set; }
        public string rowguid { get; set; }
        public string Descripcion { get; set; }

        public int ProID { get; set; }

        public bool isVisibleAdd { get; set; }

        [JsonIgnore] [Ignore] public double Cumplimiento { get => PrePresupuesto == 0 ? 0 : (PreEjecutado / PrePresupuesto) * 100; }
        [JsonIgnore] [Ignore] public string CumplimientoString { get => PrePresupuesto == 0 ? "0%" : ((PreEjecutado / PrePresupuesto) * 100).ToString("N2") + "%"; }
        [JsonIgnore] [Ignore] public string PrePresupuestoString { get => string.IsNullOrWhiteSpace(PreTipo) ? PrePresupuesto.ToString("N2") : PreTipo.ToUpper().Contains("MNT") ? "RD$ " + PrePresupuesto.ToString("N2") : PrePresupuesto.ToString("N2"); }
        [JsonIgnore] [Ignore] public string PreEjecutadoString { get => string.IsNullOrWhiteSpace(PreTipo) ? PreEjecutado.ToString("N2") : PreTipo.ToUpper().Contains("MNT") ? "RD$ " + PreEjecutado.ToString("N2") : PreEjecutado.ToString("N2"); }
        [JsonIgnore] [Ignore] public double Faltante { get => (PrePresupuesto - PreEjecutado) > 0 ? (PrePresupuesto - PreEjecutado) : 0 ; }
        [JsonIgnore] [Ignore] public string FaltanteString { get => string.IsNullOrWhiteSpace(PreTipo) ? Faltante.ToString("N2") : PreTipo.ToUpper().Contains("MNT") ? "RD$ " + Faltante.ToString("N2") : Faltante.ToString("N2"); }
    }
}

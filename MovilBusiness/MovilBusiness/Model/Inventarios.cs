using MovilBusiness.Views.Components.TemplateSelector;
using SQLite;
using System;

namespace MovilBusiness.Model
{
    public class Inventarios : RowLinker
    {
        public string RepCodigo { get; set; }
        public int ProID { get; set; }
        public double invCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public double ProUnidades { get; set; }
        public string UnmCodigo { get; set; }
        public string InvLote { get; set; }

        public bool FormatForMoney { get; set; }
        public int AlmId { get; set; }
        public string rowguid { get; set; }
        public int ForID { get; set; }

        public string AlmDescripcion { get; set; }
        public string InvLoteFechaVencimiento { get; set; }
        public bool UsarCajasUnidades { get; set; }
        //[Ignore]public string InvCantidadLabel { get => FormatForMoney ? invCantidad.ToString("C2") : invCantidad.ToString() + (InvCantidadDetalle > 0 ? "/" + InvCantidadDetalle.ToString() : ""); }
        [Ignore]
        public string InvCantidadLabel
        {
            get
            {
                if (FormatForMoney)
                    return invCantidad.ToString("C2");
                else if (UsarCajasUnidades)
                {
                    int cajas = (int)(invCantidad / (ProUnidades > 0 ? ProUnidades : 1.0));
                    int unidades = (int)(invCantidad - (cajas * (ProUnidades > 0 ? ProUnidades : 1.0)));

                    return $"{cajas}/{unidades}";
                }
                else
                    return invCantidad.ToString() + (InvCantidadDetalle > 0 ? "/" + InvCantidadDetalle.ToString() : "");
            }
        }
        [Ignore]public string ProIndicator { get => invCantidad > 0 ? "#1976D2" : "LightGray"; }
        [Ignore]public bool UsaLote { get => !string.IsNullOrWhiteSpace(InvLote); }
        [Ignore] public string CantidadLabel { get => UsarCajasUnidades ? "Cajas/Und" : "Disponible"; }
    }
}

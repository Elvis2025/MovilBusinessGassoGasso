using MovilBusiness.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
    public class CargasDetalle
    {
        public string RepCodigo { get; set; }
        public int CarSecuencia { get; set; }
        public int CarPosicion { get; set; }
        public int ProID { get; set; }
        public double CarCantidad { get; set; }
        public int CarCantidadDetalle { get; set; }
        public string ProDescripcion { get; set; }
        public string ProCodigo { get; set; }
        public string ProDatos3 { get; set; }
        public double ProUnidades { get; set; }
        public double InvCantidad { get; set; }
        public double InvCantidadDetalle { get; set; }
        
        public string CarLote { get; set; }

        public bool UsarCajasUnidades { get; set; }
        public string CarLoteFechaVencimiento { get; set; }


        [Ignore] public string CantidadUnidades { 
            get 
            {
                if (UsarCajasUnidades)
                {
                    try
                    {
                        //var cantidadTotal = CarCantidad / (ProUnidades > 0 ? ProUnidades : 1.0);
                        //var decimales = cantidadTotal - (int)cantidadTotal;

                        //return ((int)cantidadTotal) + "/" + ((int)decimales);

                        int cajas = (int)(CarCantidad / (ProUnidades > 0 ? ProUnidades : 1.0));
                        int unidades = (int)(CarCantidad - (cajas * (ProUnidades > 0 ? ProUnidades : 1.0))) ;

                        return $"{cajas}/{unidades}";
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
                return CarCantidad + "/" + CarCantidadDetalle; 
            } 
        } 
        [Ignore] public bool UsaLote { get => !string.IsNullOrWhiteSpace(CarLote); }

        [Ignore] public string CantidadLabel { get => UsarCajasUnidades ? "Cajas/Und" : "Cantidad"; }
    }
}

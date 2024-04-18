using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model
{
   public class PedidosDescuentos
    {
      public int DesID { get; set; }
      public int ProID { get; set; }
      public int CliID { get; set; }
      public int DesEscalon { get; set; }
      public double PedDescuento { get; set; }
      public int DesMetodo { get; set; }      
      public double PedDesPorciento { get; set; }
      public int PedPosicionDescuento { get; set; }
    }
}

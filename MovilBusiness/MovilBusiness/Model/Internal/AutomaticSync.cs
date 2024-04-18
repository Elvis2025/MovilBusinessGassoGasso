using MovilBusiness.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class AutomaticSync
    {
        public MomentToSync Moment { get; set; }
        public int QuantityOfPendingToSync { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Model
{
    public class BTDevice
    {
        public string Address { get; set; }
        public string Name { get; set; }


        public override string ToString()
        {
            return Address;
        }
    }
}

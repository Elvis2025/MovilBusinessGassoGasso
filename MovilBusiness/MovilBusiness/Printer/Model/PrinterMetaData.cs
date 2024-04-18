using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Model
{
    public class PrinterMetaData
    {
        public string PrinterLanguage { get; set; } = "CPCL";
        public int PrinterSize { get; set; } = 3;
        public string PrinterMac { get; set; }
    }
}

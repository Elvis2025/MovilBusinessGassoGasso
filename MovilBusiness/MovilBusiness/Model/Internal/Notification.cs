using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class Notification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool Indeterminate { get; set; }
        public bool IsCancelable { get; set; }

        public bool SuccessIcon { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class DbUpdateValue
    {
        public object Value { get; set; }
        public bool IsText { get; set; } = true;
    }
}

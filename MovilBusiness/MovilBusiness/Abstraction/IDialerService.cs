using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Abstraction
{
   public interface IDialerService
    {
        bool Call(string Number);
    }
}

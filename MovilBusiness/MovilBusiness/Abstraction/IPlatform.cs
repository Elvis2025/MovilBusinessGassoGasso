using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Abstraction
{
    public interface IPlatform
    {
        IPublicClientApplication GetIdentityClient(string applicationId);
    }
}

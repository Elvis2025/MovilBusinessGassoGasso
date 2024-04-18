using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MovilBusiness.Abstraction
{
    public interface iOSEscPosPrinterConn
    {
        bool IsConnected { get; }
        void Initialize(string mac);
        Task Close();
        Task Open();
        Task Write(byte[] data);
    }
}

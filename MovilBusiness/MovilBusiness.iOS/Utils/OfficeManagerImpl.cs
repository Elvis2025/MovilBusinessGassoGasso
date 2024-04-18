using System;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(OfficeManagerImpl))]
namespace MovilBusiness.iOS.Utils
{
    public class OfficeManagerImpl : IOfficeManager
    {
        public void OpenPowerPoint(string fileName)
        {
            try
            {

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}
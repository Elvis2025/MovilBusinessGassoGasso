
namespace MovilBusiness.Abstraction
{
    public interface IAppInfo
    {
        string AppVersion();
        string ProductsImagePath();
        string DocumentsPath();
        string DatabasePath();

        double BatteryLevel();

        byte[] ReadCertificate();
    }
}


using System.Threading.Tasks;

namespace MovilBusiness.Abstraction
{
    public interface IPlatformImageConverter
    {
        object Create(byte[] image, int width, int height);
        object CreateESCPOS(byte[] image, int width, int height);
        void DecodeForEscPos(byte[] image, int width, int height);

    }
}

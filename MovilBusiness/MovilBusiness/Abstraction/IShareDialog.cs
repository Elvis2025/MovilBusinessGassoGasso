using System.Threading.Tasks;

namespace MovilBusiness.Abstraction
{
    public interface IShareDialog
    {
        Task Show(string title, string message, string filePath);
    }
}

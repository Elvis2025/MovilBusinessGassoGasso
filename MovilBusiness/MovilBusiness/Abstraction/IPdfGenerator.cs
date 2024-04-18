using System.Threading.Tasks;

namespace MovilBusiness.Abstraction
{
    public interface IPdfGenerator
    {
        Task<string> GeneratePdf(int traSecuencia, bool confirmado = false);
    }
}

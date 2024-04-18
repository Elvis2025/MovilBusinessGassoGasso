using MovilBusiness.Printer;

namespace MovilBusiness.Abstraction
{
    public interface IPrinterFormatter
    {
        void Print(int traSecuencia, bool confirmado, PrinterManager printer, string rowguid = "", int forceFormat = -1, int traSecuencia2 = -1);
    }
}

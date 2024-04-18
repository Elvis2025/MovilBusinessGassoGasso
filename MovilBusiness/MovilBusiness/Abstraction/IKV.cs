
namespace MovilBusiness.Abstraction
{
    public interface IKV
    {
        /*void SetKey(string value);
        void SetValue(string value);*/
        string GetKey();
        string GetValue();
        string ToString();
    }
}

using MovilBusiness.Model.Internal;

namespace MovilBusiness.Abstraction
{
    public interface INotificationManager
    {
        void Notify(Notification notification);
        void CancelAll();
    }
}

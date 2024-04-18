namespace MovilBusiness.DataAccess
{
    public class DS_Controller
    {
        protected DS_RepresentantesParametros myParametro;

        public DS_Controller()
        {
            myParametro = DS_RepresentantesParametros.GetInstance();
        }
    }
}

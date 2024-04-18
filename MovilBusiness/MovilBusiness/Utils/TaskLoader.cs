
using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.Utils
{
    public class TaskLoader
    {
        public bool SqlTransactionWhenRun { get; set; } = false;

        public TaskLoader() { }

        public async Task Execute(Action func)
        {
            var screen = DependencyService.Get<IScreen>();

            try
            {
                screen?.KeepLightsOn(true);

                await Task.Run(() => {
                    try
                    {
                        if (SqlTransactionWhenRun)
                        {
                            SqliteManager.GetInstance().BeginTransaction();
                        }

                        func();

                        if (SqlTransactionWhenRun)
                        {
                            SqliteManager.GetInstance().Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        if (SqlTransactionWhenRun)
                        {
                            SqliteManager.GetInstance().Rollback();
                        }
                        throw e;
                    }
                });

                screen?.KeepLightsOn(false);
            }
            catch (Exception e)
            {
                screen?.KeepLightsOn(false);
                throw e;
            }
        }

    }
}

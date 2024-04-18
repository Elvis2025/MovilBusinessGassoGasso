

using System;
using System.Threading;

namespace MovilBusiness.Abstraction
{
    public interface IApplicationUpgrader
    {
        void DownloadFile(string url, string fileName, Action<double> progressUpdated, CancellationTokenSource cancelToken);
    }
}

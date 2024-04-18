
using SQLite;

namespace MovilBusiness.Model.Internal
{
    public class ErrorLog
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerSource { get; set; }
        public string InnerMessage { get; set; }
        public string InnerStackTrace { get; set; }

        [PrimaryKey]public string ErrorGuid { get; set; }
        public string ErrorFecha { get; set; }
    }
}

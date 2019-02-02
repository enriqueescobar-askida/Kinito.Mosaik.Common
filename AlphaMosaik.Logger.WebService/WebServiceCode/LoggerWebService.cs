using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace AlphaMosaik.Logger
{
    [WebService(Namespace = "http://alphamosaik.com/Logger")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]  // Allow the Web Service to be called from script (ASP.Net Ajax)
    public class LoggerWebService : System.Web.Services.WebService
    {
        [WebMethod]
        public void AddEntry(LogEntry entry)
        {
        }
    }
}

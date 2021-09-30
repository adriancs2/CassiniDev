using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CassiniDev;


namespace CORSHeaderPlugin
{
    public class Plugin
    {
        public void ProcessRequest(Request request)
        {
            request.SetResponseHeader("Access-Control-Allow-Origin", "*");
            request.SetResponseHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            request.SetResponseHeader("Access-Control-Allow-Headers", "X-Requested-With, Content-Type");
            request.SetResponseHeader("Access-Control-Max-Age", "1728000");



        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using IT2_backend.Classes;
using System.IO;

namespace IT2_backend
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

            ConnectionString.connString = File.ReadAllText(Server.MapPath("/connstring.cs"));
            PushNotification.userHash = File.ReadAllText(Server.MapPath("/user.cs"));
            PushNotification.tokenHash = File.ReadAllText(Server.MapPath("/token.cs"));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;

namespace IT2_backend.RoastIO
{
    public partial class ReceiveCurrentTemperature : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            double currentTemperature;
            if (double.TryParse(Request.QueryString["temperature"], out currentTemperature))
            {
                var activeRoast = new Roast();
                activeRoast.CurrentTemp = currentTemperature;
                activeRoast.Save();
            }
        }
    }
}
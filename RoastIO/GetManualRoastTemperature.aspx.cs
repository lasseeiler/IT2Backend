using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;

namespace IT2_backend.RoastIO
{
    public partial class GetManualRoastTemperature : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var roast = new Roast();
            TemperatureLiteral.Text = "{" + Math.Floor((roast.CurrentTargetTemp ?? 0)).ToString() + "}";
        }
    }
}
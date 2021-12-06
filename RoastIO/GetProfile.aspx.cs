using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;

namespace IT2_backend.RoastIO
{
    public partial class GetProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var activeRoast = new Roast();
            if (activeRoast.ProfileId.HasValue)
            {
                var activeProfile = new Profile(activeRoast.ProfileId.Value);
                ProfileLiteral.Text = activeProfile.ProfileText;
            }
            else
            {
                ProfileLiteral.Text = "0";
                activeRoast.StatusId = (int)Classes.RoastStatus.ErrorUnspecified;
                activeRoast.Save();
            }            
        }
    }
}
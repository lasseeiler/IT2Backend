using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;

namespace IT2_backend.RoastIO
{
    public partial class RoasterStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int receivedCode;
            int.TryParse(Request.QueryString["code"], out receivedCode);

            var roast = new Roast();

            switch (receivedCode)
            {
                case 105: // Load profile
                case 115: // Start roasting
                case 205: // Start manual roasting
                case 305:
                    roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    break;
                case 30: // Roaster online - ready
                    if (roast.StatusId == (int) RoastStatus.NoCommunicationYet ||
                        roast.StatusId == (int) RoastStatus.RemoveProfile ||
                        roast.StatusId == (int) RoastStatus.EndingRoast ||
                        roast.StatusId == (int) RoastStatus.ResetEgEndRoastEjectBulb ||
                        roast.StatusId == receivedCode)
                    {
                        if ((roast.StatusId == (int) RoastStatus.EndingRoast ||
                             roast.StatusId == (int) RoastStatus.ResetEgEndRoastEjectBulb) &&
                            roast.EndTime == null)
                        {
                            roast.EndTime = DateTime.Now;
                            roast.Save();
                            roast.CreateNewRoast();
                        }
                        
                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.LoadProfile ||
                        roast.StatusId == (int)RoastStatus.StartManualRoasting)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;   
                    }
                    break;
                case 106: // Started loading profile
                    if (roast.StatusId == (int)RoastStatus.LoadProfile ||
                        roast.StatusId == receivedCode)
                    {
                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 110: // Profile loaded - ready for roasting with profile
                    if (roast.StatusId == (int) RoastStatus.LoadProfile ||
                        roast.StatusId ==  (int) RoastStatus.StartedLoadingProfile ||
                        roast.StatusId == receivedCode)
                    {
                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.StartRoasting ||
                        roast.StatusId == (int)RoastStatus.RemoveProfile ||
                        roast.StatusId == (int)RoastStatus.StartManualRoasting ||
                        roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;   
                    }
                    break;
                case 120: // Initiating profile roast (inserting bulb)
                    if (roast.StatusId == (int)RoastStatus.StartRoasting ||
                        roast.StatusId == receivedCode)
                    {
                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 130: // Roasting with profile
                    if (roast.StatusId == (int)RoastStatus.StartRoasting ||
                        roast.StatusId == (int)RoastStatus.InitiatingProfileRoastInsertingBulb ||
                        roast.StatusId == receivedCode)
                    {
                        if (roast.StartTime == null)
                            roast.StartTime = DateTime.Now;

                        if(String.IsNullOrEmpty(roast.ProfileText))
                        {
                            Profile profile = new Profile((int)roast.ProfileId);
                            roast.ProfileText = profile.ProfileText;
                        }

                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.StartManualRoasting ||
                        roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 210: // Initiating manual roast (inserting bulb)
                    if (roast.StatusId == (int)RoastStatus.StartManualRoasting ||
                        roast.StatusId == receivedCode)
                    {
                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 220: // Roasting with manual control
                    if (roast.StatusId == (int)RoastStatus.StartManualRoasting ||
                        roast.StatusId == (int)RoastStatus.InitiatingManualRoastInsertingBulb ||
                        roast.StatusId == receivedCode)
                    {
                        if (roast.StartTime == null)
                            roast.StartTime = DateTime.Now;
                        if (roast.ManualControlStartTime == null)
                            roast.ManualControlStartTime = DateTime.Now;

                        roast.StatusId = receivedCode;
                    }
                    else if (roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb)
                    {
                        // roast status correct - already set - send to roaster
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 310: // Ending roast (will return to 30)
                    if (roast.StatusId == (int)RoastStatus.ResetEgEndRoastEjectBulb ||
                        roast.StatusId == receivedCode)
                    {
                        roast.StatusId = receivedCode;
                    }
                    else
                    {
                        roast.StatusId = (int)RoastStatus.ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster;
                    }
                    break;
                case 400: // Error - unspecified
                case 401: // Error - Current status is not valid for the current state of the roaster
                case 402: // Error - Roaster not online
                    roast.StatusId = receivedCode;
                    break;
            }
            
            roast.Save();

            StatusLiteral.Text = "{" + roast.StatusId + "}"; // for now: send back the current status code
            
        }
    }
}
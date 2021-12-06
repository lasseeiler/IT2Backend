using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;
using System.Web.Script.Serialization;
using System.Data.SqlClient;


namespace IT2_backend.RoastIO
{
    enum InterfaceAction
    {
        SetStatus = 10,
        SaveProfile = 20,
        GetProfile = 30,
        GetProfiles = 40,
        SetProfile = 50,
        GetRoastData = 60,
        GetStatus = 70,
        SetManualRoastTemperature = 80,
        GetManualRoastTemperature = 90,
        DeleteProfile = 100,
        MasterReset = 110, 
        RemoveProfile = 120,
        SetFirstCrack = 130,
        SetSecondCrack = 140
    }

    public class InterfaceObject
    {
        public int Action { get; set; }
        public int StatusCode { get; set; }
        public Profile Profile { get; set; }
        public int ProfileId { get; set; }
        public List<Profile> Profiles { get; set; }
        public double? CurrentTemp { get; set; }
        public long ElapsedTime { get; set; }
        public double? ManualTargetTemp { get; set; }
    }

    public partial class InterfaceCom : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var jsonString = Request.Form["data"];
            if(String.IsNullOrEmpty(jsonString))
            {
                return;
            }
            var interfaceObject = new JavaScriptSerializer().Deserialize<InterfaceObject>(jsonString);
            //var interfaceObject = new InterfaceObject();

            var roast = new Roast();

            switch (interfaceObject.Action)
            {
                case (int)InterfaceAction.SetStatus:
                    if (IsNewStatusValid(roast, interfaceObject.StatusCode))
                    {
                        if (interfaceObject.StatusCode == (int)RoastStatus.StartManualRoasting &&
                            roast.CurrentTemp.HasValue)
                            roast.CurrentTargetTemp = Math.Floor(roast.CurrentTemp.Value);

                        roast.StatusId = interfaceObject.StatusCode;
                        roast.Save();
                    }
                    break;
                case (int)InterfaceAction.SaveProfile:
                    interfaceObject.Profile.Save();
                    break;
                case (int)InterfaceAction.GetProfile:
                    var profileId = 0;
                    if (interfaceObject.ProfileId > 0)
                        profileId = interfaceObject.ProfileId;
                    else if (roast.ProfileId.HasValue)
                        profileId = roast.ProfileId.Value;
                    interfaceObject.Profile = new Profile(profileId);
                    break;
                case (int)InterfaceAction.GetProfiles:
                    interfaceObject.Profiles = LoadProfilesFromDb();
                    break;
                case (int)InterfaceAction.SetProfile:
                    if (GetElapsedTime(roast.Id ?? 0) == 0 && 
                        (roast.StatusId == (int)RoastStatus.RoasterOnlineReady))
                    {
                        roast.StatusId = (int) RoastStatus.LoadProfile;
                        roast.ProfileId = interfaceObject.ProfileId;
                        roast.Save();
                    }                                           
                    break;
                case (int)InterfaceAction.GetStatus:
                    interfaceObject.StatusCode = roast.StatusId ?? 0;
                    interfaceObject.CurrentTemp = roast.CurrentTemp;
                    interfaceObject.ElapsedTime = GetElapsedTime(roast.Id ?? 0);
                    interfaceObject.ProfileId = roast.ProfileId ?? 0;
                    if (roast.StatusId == (int) RoastStatus.StartManualRoasting ||
                        roast.StatusId == (int) RoastStatus.InitiatingManualRoastInsertingBulb ||
                        roast.StatusId == (int) RoastStatus.RoastingWithManualControl)
                    {
                        interfaceObject.ManualTargetTemp = roast.CurrentTargetTemp;
                    }
                    break;
                case (int)InterfaceAction.SetManualRoastTemperature:
                    roast.CurrentTargetTemp = interfaceObject.ManualTargetTemp;
                    roast.Save();
                    break;
                case (int)InterfaceAction.GetManualRoastTemperature:
                    interfaceObject.ManualTargetTemp = roast.CurrentTargetTemp;
                    break;
                case (int)InterfaceAction.DeleteProfile:
                    var profile = new Profile(interfaceObject.ProfileId);                    
                    profile.Delete();                   
                    break;
                case (int)InterfaceAction.MasterReset:
                    roast.CreateNewRoast();
                    break;
                case (int)InterfaceAction.RemoveProfile:
                    if (roast.StatusId == (int)RoastStatus.ProfileLoadedReadyForRoastingWithProfile)
                    {
                        roast.ProfileId = null;
                        roast.StatusId = (int) RoastStatus.RemoveProfile;
                        roast.Save();
                    }
                    break;
                case (int)InterfaceAction.SetFirstCrack:
                    int FirstElapsed = (int)GetElapsedTime(roast.Id ?? 0);
                    if(FirstElapsed > 0)
                    {
                        roast.FirstCrackElapsed = FirstElapsed;
                        roast.FirstCrackTemp = GetTemperatureAverage(roast.Id ?? 0, 3);
                        roast.Save();
                    }
                    break;
                case (int)InterfaceAction.SetSecondCrack:
                    int SecondElapsed = (int)GetElapsedTime(roast.Id ?? 0);
                    if (SecondElapsed > 0)
                    {
                        roast.SecondCrackElapsed = SecondElapsed;
                        roast.SecondCrackTemp = GetTemperatureAverage(roast.Id ?? 0, 3);
                        roast.Save();
                    }
                    break;
            }

            var json = new JavaScriptSerializer().Serialize(interfaceObject);
            StatusLiteral.Text = json;
        }

        private double GetTemperatureAverage(int roastId, int count)
        {
            var conn = new SqlConnection(ConnectionString.connString);
            var command =
                @"SELECT AVG(Temperature) AS Temperature
                FROM RoastLog
                WHERE 
                (Id IN (
                    SELECT TOP (3) Id
                    FROM RoastLog AS RoastLog_1
                    WHERE (RoastId = @RoastId)
                    ORDER BY ElapsedTime DESC
                ))";

            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue("RoastId", roastId);

            conn.Open();
            sqlCommand.Connection = conn;
            var sdr = sqlCommand.ExecuteReader();
            double temperature = 0;
            while (sdr.Read())
            {
                temperature = (double)sdr["Temperature"];
            }
            conn.Close();

            temperature = Math.Round(temperature, 1);
            return temperature;
        }
        private long GetElapsedTime(int roastId)
        {
            var conn = new SqlConnection(ConnectionString.connString);
            var command =
                @"SELECT Top (1) LogTime, ElapsedTime
                FROM RoastLog
                WHERE (RoastId = @RoastId) 
                ORDER BY Id";

            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue("RoastId", roastId);

            conn.Open();
            sqlCommand.Connection = conn;
            var sdr = sqlCommand.ExecuteReader();
            long elapsedTime = 0;
            while (sdr.Read())
            {
                var firstLogTime = DateTime.Parse(sdr["LogTime"].ToString());
                var elapsedSecondsOnFirstLogTime = (int)sdr["ElapsedTime"];
                elapsedTime = (long)(DateTime.Now - firstLogTime.AddSeconds(-elapsedSecondsOnFirstLogTime)).TotalSeconds;                
            }
            conn.Close();

            return elapsedTime;
        }

        private List<Profile> LoadProfilesFromDb()
        {
            var conn = new SqlConnection(ConnectionString.connString);
            var command =
                @"SELECT Id, Name, ProfileText
                FROM Profile 
                WHERE (Active = 1) 
                ORDER BY Name";

            var sqlCommand = new SqlCommand(command);

            conn.Open();
            sqlCommand.Connection = conn;
            var sdr = sqlCommand.ExecuteReader();
            var profiles = new List<Profile>();
            while (sdr.Read())
            {
                profiles.Add(new Profile
                {
                    Id = ((int)sdr["Id"]), 
                    Name = ((string)sdr["Name"]),
                    ProfileText = ((string)sdr["ProfileText"])                        
                });
            }
            conn.Close();

            return profiles;
        }

        private static bool IsNewStatusValid(Roast roast, int newStatusCode)
        {
            switch (newStatusCode)
            {
                case 105:
                    return (roast.StatusId == (int) RoastStatus.RoasterOnlineReady ||
                            roast.StatusId == newStatusCode);
                case 115:
                    return (roast.StatusId == (int)RoastStatus.ProfileLoadedReadyForRoastingWithProfile ||
                            roast.StatusId == newStatusCode);
                case 145:
                    return (roast.StatusId == (int)RoastStatus.ProfileLoadedReadyForRoastingWithProfile ||
                            roast.StatusId == newStatusCode);
                case 205:
                    return (roast.StatusId == (int)RoastStatus.RoasterOnlineReady ||
                        roast.StatusId == (int)RoastStatus.ProfileLoadedReadyForRoastingWithProfile ||
                        roast.StatusId == (int)RoastStatus.RoastingWithProfile ||
                            roast.StatusId == newStatusCode);
                case 305:
                    return (roast.StatusId == (int)RoastStatus.LoadProfile ||
                        roast.StatusId == (int)RoastStatus.StartedLoadingProfile ||
                        roast.StatusId == (int)RoastStatus.ProfileLoadedReadyForRoastingWithProfile ||
                        roast.StatusId == (int)RoastStatus.StartRoasting ||
                        roast.StatusId == (int)RoastStatus.InitiatingProfileRoastInsertingBulb ||
                        roast.StatusId == (int)RoastStatus.RoastingWithProfile ||
                        roast.StatusId == (int)RoastStatus.StartManualRoasting ||
                        roast.StatusId == (int)RoastStatus.InitiatingManualRoastInsertingBulb ||
                        roast.StatusId == (int)RoastStatus.RoastingWithManualControl ||
                            roast.StatusId == newStatusCode);

            }

            return false;
        }
    }
}
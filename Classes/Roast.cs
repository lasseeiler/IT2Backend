using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Text;
using System.Web.Caching;

namespace IT2_backend.Classes
{
    enum RoastStatus
    {
        NoCommunicationYet = 0,
        RoasterOnlineWait = 20,
        RoasterOnlineReady = 30,
        LoadProfile = 105,
        StartedLoadingProfile = 106,
        ProfileLoadedReadyForRoastingWithProfile = 110,
        StartRoasting = 115,
        InitiatingProfileRoastInsertingBulb = 120,
        RoastingWithProfile = 130,
        RemoveProfile = 145,
        StartManualRoasting = 205,
        InitiatingManualRoastInsertingBulb = 210,
        RoastingWithManualControl = 220,
        ResetEgEndRoastEjectBulb = 305,
        EndingRoast = 310,
        ErrorUnspecified = 400,
        ErrorCurrentStatusIsNotValidForTheCurrentStateOfTheRoaster = 401,
        ErrorRoasterNotOnline = 402
    };

    public class Roast
    {

        public int? Id { get; set; }
        public int? ProfileId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ManualControlStartTime { get; set; }
        public int? CurrentStep { get; set; }
        public double? CurrentTargetTemp { get; set; }
        public double? CurrentTemp { get; set; }
        public int? ElapsedTotalTime { get; set; }
        public DateTime? LastUpdate { get; set; }

        private readonly SqlConnection _conn;

        private string connectionString =
            "";

        public Roast()
        {
            _conn = new SqlConnection(connectionString);

            LoadOrCreateActiveRoast();
        }

        public Roast(int id)
        {
            _conn = new SqlConnection(connectionString);

            Id = id;

            // get object from database
            LoadRoastObjectFromDb(Id.Value);
        }

        private void LoadRoastObjectFromDb(int id)
        {
            var command = 
            @"SELECT TOP (1) Id, ProfileId, StatusId, StartTime, EndTime, ManualControlStartTime, CurrentStep, CurrentTargetTemp, CurrentTemp, ElapsedTotalTime, LastUpdate  
            FROM Roast 
            WHERE (Id = @RoastID)";

            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue("RoastID", id);

            _conn.Open();
            sqlCommand.Connection = _conn;
            var sdr = sqlCommand.ExecuteReader();
            while (sdr.Read())
            {
                Id = id;
                ProfileId = (sdr["ProfileId"] == DBNull.Value) ? null : (int?)sdr["ProfileId"];
                StatusId = (int)sdr["StatusId"];
                StartTime = (!string.IsNullOrEmpty(sdr["StartTime"].ToString()) ? DateTime.Parse(sdr["StartTime"].ToString()) : (DateTime?)null);
                EndTime = (!string.IsNullOrEmpty(sdr["EndTime"].ToString()) ? DateTime.Parse(sdr["EndTime"].ToString()) : (DateTime?)null);
                ManualControlStartTime = (!string.IsNullOrEmpty(sdr["ManualControlStartTime"].ToString()) ? DateTime.Parse(sdr["ManualControlStartTime"].ToString()) : (DateTime?)null);
                CurrentStep = (sdr["CurrentStep"] == DBNull.Value) ? null : (int?)sdr["CurrentStep"];
                CurrentTargetTemp = (sdr["CurrentTargetTemp"] == DBNull.Value) ? null : (double?)sdr["CurrentTargetTemp"];
                CurrentTemp = (sdr["CurrentTemp"] == DBNull.Value) ? null : (double?)sdr["CurrentTemp"];
                ElapsedTotalTime = (sdr["ElapsedTotalTime"] == DBNull.Value) ? null : (int?)sdr["ElapsedTotalTime"];
                LastUpdate = (!string.IsNullOrEmpty(sdr["LastUpdate"].ToString()) ? DateTime.Parse(sdr["LastUpdate"].ToString()) : (DateTime?)null);
            }
            _conn.Close();
        }

        private void LoadOrCreateActiveRoast()
        {
            Id = (int?)HttpContext.Current.Cache["ActiveRoastID"];
            if (Id.HasValue)
            {
                LoadRoastObjectFromDb(Id.Value);
                if (EndTime.HasValue || (StartTime.HasValue && StartTime.Value.AddHours(2) < DateTime.Now))
                {
                    CreateNewRoast();
                    HttpContext.Current.Cache["ActiveRoastID"] = Id;
                }
            }
            else
            {
                CreateNewRoast();
                HttpContext.Current.Cache["ActiveRoastID"] = Id;
            }
        }

        public void CreateNewRoast()
        {
            Id = null;
            ProfileId = null;
            StatusId = (int)RoastStatus.NoCommunicationYet;
            StartTime = null;
            EndTime = null;
            ManualControlStartTime = null;
            CurrentStep = null;
            CurrentTargetTemp = null;
            CurrentTemp = null;
            ElapsedTotalTime = null;
            LastUpdate = null;

            Save();
            HttpContext.Current.Cache["ActiveRoastID"] = Id;
        }

        public void Save()
        {
            if (!Id.HasValue)
            {
                var command =
                    @"SET NOCOUNT ON; INSERT INTO Roast(ProfileId, StatusId, StartTime, EndTime, ManualControlStartTime, CurrentStep, CurrentTargetTemp, CurrentTemp, ElapsedTotalTime, LastUpdate)
                    VALUES
                    (@ProfileId, @StatusId, @StartTime, @EndTime, @ManualControlStartTime, @CurrentStep, @CurrentTargetTemp, @CurrentTemp, @ElapsedTotalTime, @LastUpdate); SELECT CAST(SCOPE_IDENTITY() AS int) AS lastId";
            
                var sqlCommand = new SqlCommand(command);
                sqlCommand.Parameters.AddWithValue("@ProfileId", ((object)ProfileId) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StatusId", StatusId);
                sqlCommand.Parameters.AddWithValue("@StartTime", ((object)StartTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@EndTime", ((object)EndTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ManualControlStartTime", ((object)ManualControlStartTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentStep", ((object)CurrentStep) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentTargetTemp", ((object)CurrentTargetTemp) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentTemp", ((object)CurrentTemp) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ElapsedTotalTime", ((object)ElapsedTotalTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@LastUpdate", ((object)LastUpdate) ?? DBNull.Value);
            
                _conn.Open();
                sqlCommand.Connection = _conn;
                var sdr = sqlCommand.ExecuteReader();
                int lastId = 0;
                if (sdr.Read())
                    lastId = (int)sdr["lastId"];

                _conn.Close();

                if(lastId > 0)
                    LoadRoastObjectFromDb(lastId);
            }
            else
            {
                var command =
                    @"UPDATE Roast SET ProfileId = @ProfileId, StatusId = @StatusId, StartTime = @StartTime, EndTime = @EndTime, ManualControlStartTime = @ManualControlStartTime, CurrentStep = @CurrentStep, CurrentTargetTemp = @CurrentTargetTemp, CurrentTemp = @CurrentTemp, ElapsedTotalTime = @ElapsedTotalTime, LastUpdate = @LastUpdate WHERE (Id = @RoastId)";

                var sqlCommand = new SqlCommand(command);
                sqlCommand.Parameters.AddWithValue("@RoastId", ((object)Id.Value));
                sqlCommand.Parameters.AddWithValue("@ProfileId", ((object)ProfileId) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StatusId", StatusId);
                sqlCommand.Parameters.AddWithValue("@StartTime", ((object)StartTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@EndTime", ((object)EndTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ManualControlStartTime", ((object)ManualControlStartTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentStep", ((object)CurrentStep) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentTargetTemp", ((object)CurrentTargetTemp) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@CurrentTemp", ((object)CurrentTemp) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ElapsedTotalTime", ((object)ElapsedTotalTime) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@LastUpdate", ((object)LastUpdate) ?? DBNull.Value);

                _conn.Open();
                sqlCommand.Connection = _conn;
                var affectedRows = sqlCommand.ExecuteNonQuery();

                _conn.Close();

                if (affectedRows == 1)
                    LoadRoastObjectFromDb(Id.Value);
                else
                    throw new Exception("Could not update Roast");               
            }
        }
    }
}
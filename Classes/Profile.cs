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
    public class Profile
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ProfileText { get; set; }

        private readonly SqlConnection _conn;

        private string connectionString =
            "";

        public Profile()
        {
            _conn = new SqlConnection(connectionString);
        }

        public Profile(int id)
        {
            _conn = new SqlConnection(connectionString);

            Id = id;
            LoadProfileObjectFromDb(Id.Value);
        }

        private void LoadProfileObjectFromDb(int id)
        {
            var command =
                @"SELECT TOP (1) Id, Name, ProfileText
                FROM Profile 
                WHERE (Id = @ProfileID)";

            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue("ProfileID", id);

            _conn.Open();
            sqlCommand.Connection = _conn;
            var sdr = sqlCommand.ExecuteReader();
            while (sdr.Read())
            {
                Id = id;
                Name = ((string)sdr["Name"]);
                ProfileText = ((string)sdr["ProfileText"]);
            }
            _conn.Close();
        }

        public void Save()
        {
            if (!Id.HasValue || Id.Value == 0)
            {
                var command =
                    @"SET NOCOUNT ON; INSERT INTO Profile(Name, ProfileText)
                    VALUES
                    (@Name, @ProfileText); SELECT CAST(SCOPE_IDENTITY() AS int) AS lastId";

                var sqlCommand = new SqlCommand(command);
                sqlCommand.Parameters.AddWithValue("@Name", ((object)Name) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ProfileText", ((object)ProfileText) ?? DBNull.Value);

                _conn.Open();
                sqlCommand.Connection = _conn;
                var sdr = sqlCommand.ExecuteReader();
                int lastId = 0;
                if (sdr.Read())
                    lastId = (int)sdr["lastId"];

                _conn.Close();

                if (lastId > 0)
                    LoadProfileObjectFromDb(lastId);
            }
            else
            {
                var command =
                    @"UPDATE Profile SET Name = @Name, ProfileText = @ProfileText WHERE (Id = @ProfileId)";

                var sqlCommand = new SqlCommand(command);
                sqlCommand.Parameters.AddWithValue("@ProfileId", ((object)Id.Value));
                sqlCommand.Parameters.AddWithValue("@Name", ((object)Name) ?? DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@ProfileText", ((object)ProfileText) ?? DBNull.Value);

                _conn.Open();
                sqlCommand.Connection = _conn;
                var affectedRows = sqlCommand.ExecuteNonQuery();

                _conn.Close();

                if (affectedRows == 1)
                    LoadProfileObjectFromDb(Id.Value);
                else
                    throw new Exception("Could not update Profile");
            }
        }

        public void Delete()
        {
            if (Id.HasValue)
            {
                //var command = @"DELETE FROM Profile WHERE (Id = @ProfileId)";
                var command = @"UPDATE Profile SET Active = 0 WHERE (Id = @ProfileId)";

                var sqlCommand = new SqlCommand(command);
                sqlCommand.Parameters.AddWithValue("@ProfileId", ((object)Id.Value));

                _conn.Open();
                sqlCommand.Connection = _conn;
                var affectedRows = sqlCommand.ExecuteNonQuery();

                _conn.Close();

                if (affectedRows == 1)
                    LoadProfileObjectFromDb(Id.Value);
                else
                    throw new Exception("Could not delete Profile");

                Id = null;
            }
        }
    }
}
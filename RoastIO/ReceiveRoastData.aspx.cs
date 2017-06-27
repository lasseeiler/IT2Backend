using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT2_backend.Classes;

namespace IT2_backend.RoastIO
{
    public partial class ReceiveRoastData : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {
            var roast = new Roast();

            var roastdata = Request.QueryString["roastdata"];
            var dataArray = roastdata.Split(';');
            var elapsed = int.Parse(dataArray[0]);
            var temperature = float.Parse(dataArray[1]);
            var debugstring = dataArray[2];

            var command =
                    @"INSERT INTO RoastLog(RoastId, ElapsedTime, Temperature, DebugData)
                    VALUES
                    (@RoastId, @ElapsedTime, @Temperature, @DebugData)";

            var _conn = new SqlConnection(ConnectionString.connString);

            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue("@RoastId", ((object)roast.Id) ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@ElapsedTime", elapsed);
            sqlCommand.Parameters.AddWithValue("@Temperature", temperature);
            sqlCommand.Parameters.AddWithValue("@DebugData", debugstring);

            _conn.Open();
            sqlCommand.Connection = _conn;
            sqlCommand.ExecuteNonQuery();
            _conn.Close();
        }
    }
}
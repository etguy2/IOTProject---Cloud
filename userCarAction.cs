using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
using CarSharing.Cryptography;

namespace carSharing.userCarAction
{
    public static class userCarAction
    {
        [FunctionName("userCarAction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Const Vars
            const int permit_validity_time = 100; // No. of minutes until permit expiration time.

            // parse query parameter
            string action = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "action", true) == 0)
                .Value;
            string login_hash = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "login_hash", true) == 0)
                .Value;
            string user_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "user_id", true) == 0)
                .Value;
            string vehicle_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "vehicle_id", true) == 0)
                .Value;

            response response;

            // Verify user
            if (!utilitles.validateUser(System.Convert.ToInt32(user_id), login_hash)) {
                response = new response(-1, "Invalid user Credentials");
                return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
            }

            bool status = false;
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");

            // Defines tiem time treshold for the permit
            DateTime permit_expiration_treshold = DateTime.Now;
            permit_expiration_treshold.AddHours(-3); // Subs 3 hours because Azure are stupid.
            permit_expiration_treshold.AddMinutes(permit_validity_time);

            // Look for the right Permit in the DB
            string permit_query =  "SELECT COUNT(*) FROM Permits "
                                    + "INNER JOIN Users ON Users.id = Permits.user_id AND Permits.user_id = @user_id "
                                    + "INNER JOIN Vehicles ON Vehicles.id = Permits.vehicle_id AND Permits.vehicle_id = @vehicle_id "
                                    + "AND Permits.time <= Convert(datetime, @permit_expiration_treshold )";

            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(permit_query, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                command.Parameters.AddWithValue("@permit_expiration_treshold", permit_expiration_treshold);
                int rows = (int) command.ExecuteScalar();
                if (rows >= 1) 
                    status = true;
                
                conn.Close();
            }
            if (!status) {
                response = new response(-2, "No permit");
            } else {
                response = new response(1, "Approved");
            }
            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }

    }
}

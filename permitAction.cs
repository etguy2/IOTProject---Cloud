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
using CarSharing.Exceptions;

namespace carSharing.PermitAction
{
    public static class permitAction
    {
        [FunctionName("permitAction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response;

            try {
                // parse query parameter
                int permit_id = Convert.ToInt32 ( utilitles.getURLVar(req, "permit_id") );
                int user_id = Convert.ToInt32 ( utilitles.getURLVar(req, "user_id") );
                string login_hash = utilitles.getURLVar(req, "login_hash");
                int action =  Convert.ToInt32 ( utilitles.getURLVar(req, "action") );

                // Validates user identity.
                utilitles.validateUser( user_id, login_hash );
                
                if (action != 0 && action != 1)
                    throw new InvalidInputException("action");

                string[] new_status = new string[] { "DENIED", "APPROVED" };

                update_permit_status(permit_id, user_id, new_status[action]);
                response = new response(1, "Permit " + new_status[action]);

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static void update_permit_status(int permit_id, int user_id, string new_status) {
            DateTime checkin = DateTime.Now;
            string update_checkin = "UPDATE Permits SET status = @status WHERE id = @permit_id AND user_id = @user_id";
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(update_checkin, conn);
                command.Parameters.AddWithValue("@status", new_status);
                command.Parameters.AddWithValue("@permit_id", permit_id);
                command.Parameters.AddWithValue("@user_id", user_id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
}

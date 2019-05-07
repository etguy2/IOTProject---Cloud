using System;    
using System.Text;  
using System.Linq;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace CarSharing.Exceptions
{
    [ComVisible(true)]
    public class CarSharingException : System.IO.IOException
    {
        public int status_code;
        public string info;

        public CarSharingException() { this.status_code = 0; }
        public CarSharingException(int status_code, string info) {
            this.status_code = status_code;
            this.info = info;
        }
        public CarSharingException(string message, Exception innerException) {}
        public CarSharingException(string message, string fileName) {}
        public CarSharingException(string message, string fileName, Exception innerException) {}
        protected CarSharingException(SerializationInfo info, StreamingContext context) {}

        public override string Message { get; }
        public string FileName { get; }
        public string FusionLog { get; }

    }
    class CarSharingException2 : System.Exception {
        public int status {
            get { return status; }
            protected set { status = value; }
        }
        public CarSharingException2(int status, string message) : base(message) { this.status = status; }
    }
    class InvalidInputException : CarSharingException {

        public InvalidInputException() : base(-1, "invalid Input") {}
        public InvalidInputException(string message) : base(-1, "invalid Input, no '"+message+"'") {}
    }

    class UserNotVerified : CarSharingException2 {
        public UserNotVerified() : base(-2, "User not verified") {}
        public UserNotVerified(string message) : base(-2, "user no. '"+message+"' could not be verified") {}
    }

    class VehicleNotFound : CarSharingException {
        public VehicleNotFound() : base(-3, "Vehicle not found.") {}
        public VehicleNotFound(string message) : base(-3, "Vehicle no. '"+message+"' not found in the DB.") {}
    }
}
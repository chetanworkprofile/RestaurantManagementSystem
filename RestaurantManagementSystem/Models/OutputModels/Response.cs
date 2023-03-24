using System.Security.Permissions;

namespace RestaurantManagementSystem.Models.OutputModels
{
    public class Response
    {
        public int statusCode { get; set; } = 200;
        public string message { get; set; } = "Ok";
        public Object data { get; set; } = new Object();
        public bool success { get; set; } = true;

        public Response() { }
        public Response(int statusCode, string message, Object data, bool success)
        {
            this.statusCode = statusCode;
            this.message = message;
            this.data = data;
            this.success = success;
        }
    }
}

//response model for api output
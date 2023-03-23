namespace RestaurantManagementSystem.Models.OutputModels
{
    public class ResponseWithoutData
    {
        public int statusCode { get; set; } = 200;
        public string message { get; set; } = "Ok";
        public bool success { get; set; } = true;

        public ResponseWithoutData() { }
        public ResponseWithoutData(int status, string message, bool success)
        {
            this.statusCode = status;
            this.message = message;
            this.success = success;
        }
    }
}

//this is alternative to response model when we don't retrun data
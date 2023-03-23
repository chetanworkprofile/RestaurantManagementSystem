namespace RestaurantManagementSystem.Models.OutputModels
{
    public class Response
    {
        public int statusCode { get; set; } = 200;
        public string message { get; set; } = "Ok";
        public Object data { get; set; } = new Object();
        public bool success { get; set; } = true;
    }
}

//response model for api output
namespace RestaurantManagementSystem.Models.OutputModels
{
    public class PicUploadResponse
    {
        public ResponseUser? User { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PathToPic { get; set; } = string.Empty;
    }
}

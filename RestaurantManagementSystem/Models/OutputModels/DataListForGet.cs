namespace RestaurantManagementSystem.Models.OutputModels
{
    public class DataListForGet
    {
        public int totalAvailableRecords { get; set; } = 0;
        public Object list { get; set; } = new Object();
        public DataListForGet() { }
        public DataListForGet(int totalAvailableRecords, object list)
        {
            this.totalAvailableRecords = totalAvailableRecords;
            this.list = list;
        }
    }
}

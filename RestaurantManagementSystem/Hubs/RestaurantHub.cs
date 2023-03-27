using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.OutputModels;

namespace RestaurantManagementSystem.Hubs
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestaurantHub : Hub
    {
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        //TokenUser tokenUser = new TokenUser();
        object result = new object();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<string> _logger;


        public RestaurantHub(RestaurantDbContext dbContext, ILogger<string> logger)
        {
            //this._configuration = configuration;
            DbContext = dbContext;
            _logger = logger;
        }
    }
}

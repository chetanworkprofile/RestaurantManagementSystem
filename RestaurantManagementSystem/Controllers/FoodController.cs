using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Services;
using System.Data;

namespace RestaurantManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        IFoodService foodService;               //service dependency
        Response response = new Response();     //response model instance
        ResponseWithoutData response2 = new ResponseWithoutData();      //response model in case we don't return data
        object result = new object();                                   //object to match both response models in return values from function
        private readonly ILogger<AdminController> _logger;

        public FoodController(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AdminController> logger)          //constructor
        {
            foodService = new FoodService(configuration, dbContext);
            _logger = logger;
        }

        [HttpPost, Authorize(Roles = "admin,chef")]
        [Route("/api/v1/admin/addFood")]
        public IActionResult AddFood(AddFood inpFood)
        {
            if (!ModelState.IsValid)
            {   //checks for validation of model
                response2 = new ResponseWithoutData(400, "Invalid Input/One or more fields are invalid", false);
                return BadRequest(response2);
            }
            try
            {
                _logger.LogInformation("Adding new food attempt");
                result = foodService.AddFood(inpFood).Result; ;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }
    }
}

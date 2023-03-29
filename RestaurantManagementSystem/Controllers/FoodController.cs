using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Services;
using System.Data;
using System.Security.Claims;

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
        [Route("/api/v1/food/addFood")]
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
                int statusCode = 0;
                result = foodService.AddFood(inpFood, out statusCode); ;
                return StatusCode(statusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        [HttpGet, Authorize(Roles = "user,chef,admin")]
        [Route("/api/v1/food/get")]
        public IActionResult GetFoods(Guid? foodId = null, string? searchString = null, string? category = "all", String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 15, int PageNumber = 1)          // sort order   ===   e1 for ascending  -1 for descending
        {
            _logger.LogInformation("Get foods method started");
            try
            {
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                int statusCode = 200;
                result = foodService.GetFoods(foodId, userId, token, searchString, category, OrderBy, SortOrder, RecordsPerPage, PageNumber, out statusCode);
                //result = adminService.GetUsers(userId,userType, token, UserId, searchString, Email, Phone, OrderBy, SortOrder, RecordsPerPage, PageNumber, out statusCode);
                return StatusCode(statusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        } 
        /*  
        [HttpPut, Authorize(Roles = "user,chef,admin")]
        [Route("/api/v1/update")]
        public IActionResult UpdateUser(UpdateUser u)
        {
            _logger.LogInformation("Update user method started");
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int statusCode = 0;
                result = userService.UpdateUser(userId, u, token, out statusCode);

                return StatusCode(statusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        [HttpDelete, Authorize(Roles = "user,chef")]
        [Route("/api/v1/usernchef/delete")]
        public IActionResult DeleteUser(string Password)
        {
            _logger.LogInformation("Delete Student method started");
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int statusCode = 0;
                result = userService.DeleteUser(userId, token, Password, out statusCode);
                return StatusCode(statusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }*/

    }
}

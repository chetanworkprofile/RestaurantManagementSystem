using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Services;
using System.Data;
using System.Security.Claims;

namespace RestaurantManagementSystem.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        IAdminService adminService;               //service dependency
        Response response = new Response();     //response model instance
        ResponseWithoutData response2 = new ResponseWithoutData();      //response model in case we don't return data
        object result = new object();                                   //object to match both response models in return values from function
        private readonly ILogger<AdminController> _logger;

        public AdminController(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AdminController> logger)          //constructor
        {
            adminService = new AdminService(configuration, dbContext, logger);
            _logger = logger;
        }

        [HttpPost, Authorize(Roles = "admin")]
        [Route("/api/v1/admin/addChef")]
        public IActionResult AddChef([FromBody] RegisterUser inpUser)             //add chef uses service 
        {
            if (!ModelState.IsValid)
            {   //checks for validation of model
                response2 = new ResponseWithoutData(400, "Invalid Input/One or more fields are invalid", false);
                return BadRequest(response2);
            }
            try
            {
                _logger.LogInformation("Adding new chef attempt");
                result = adminService.AddChef(inpUser).Result; ;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        //getusers api to get list of other users and details
        //[HttpGet, Authorize(Roles = "admin")]
        [HttpGet, Authorize(Roles = "admin")]
        [Route("/api/v1/admin/get")]
        public IActionResult GetUsers(Guid? UserId = null, string? searchString = null, string? Email = null, long Phone = -1, String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 100, int PageNumber = 0)          // sort order   ===   e1 for ascending  -1 for descending
        {
            _logger.LogInformation("Get users method started");
            try
            {
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                result = adminService.GetUsers(userId, token, UserId, searchString, Email, Phone, OrderBy, SortOrder, RecordsPerPage, PageNumber);
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

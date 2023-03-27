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
    public class UserController : ControllerBase
    {
        IUserService userService;                   //service dependency
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        object result = new object();
        private readonly ILogger<AuthController> _logger;

        public UserController(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AuthController> logger)
        {
            userService = new UserService(configuration, dbContext, logger);
            _logger = logger;
        }

        [HttpGet, Authorize(Roles = "user,chef,admin")]
        [Route("/api/v1/getYourself")]
        public IActionResult GetYourself()                  // api for user to get data of himself for proifile details
        {
            _logger.LogInformation("Get yourself method started");
            try
            {
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                result = userService.GetYourself(userId, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        [HttpPut, Authorize(Roles = "user,chef,admin")]
        [Route("/api/v1/update")]
        public IActionResult UpdateUser(UpdateUser u)
        {
            _logger.LogInformation("Update user method started");
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = userService.UpdateUser(userId, u, token).Result;

                return Ok(result);
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
                result = userService.DeleteUser(userId, token, Password).Result;
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

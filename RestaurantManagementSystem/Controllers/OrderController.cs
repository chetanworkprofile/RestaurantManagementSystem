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
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        IOrderService orderService;               //service dependency
        Response response = new Response();     //response model instance
        ResponseWithoutData response2 = new ResponseWithoutData();      //response model in case we don't return data
        object result = new object();                                   //object to match both response models in return values from function
        private readonly ILogger<AdminController> _logger;

        public OrderController(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AdminController> logger)          //constructor
        {
            orderService = new OrderService(configuration, dbContext);
            _logger = logger;
        }


        [HttpGet, Authorize(Roles = "admin")]
        //[HttpGet]
        [Route("/api/v1/order/adminGet")]
        public IActionResult GetOrdersAsAdmin(Guid? orderId = null, string? userId = null, string? status = "all", String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 20, int PageNumber = 1)          // sort order   ===   e1 for ascending  -1 for descending
        {
            _logger.LogInformation("Get foods method started");
            try
            {
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? loggedInUserId = User.FindFirstValue(ClaimTypes.Sid);
                int statusCode = 200;
                result = orderService.GetOrdersAsAdmin(orderId, loggedInUserId, token, userId, status, OrderBy, SortOrder, RecordsPerPage, PageNumber, out statusCode);
                return StatusCode(statusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        [HttpGet, Authorize(Roles = "user")]
        //[HttpGet]
        [Route("/api/v1/order/userGet")]
        public IActionResult GetOrdersAsUser(Guid? orderId = null, string? status = "all", String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 20, int PageNumber = 1)          // sort order   ===   e1 for ascending  -1 for descending
        {
            _logger.LogInformation("Get foods method started");
            try
            {
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? loggedInUserId = User.FindFirstValue(ClaimTypes.Sid);
                int statusCode = 200;
                result = orderService.GetOrdersAsUser(orderId, loggedInUserId, token, status, OrderBy, SortOrder, RecordsPerPage, PageNumber, out statusCode);
                return StatusCode(statusCode, result);
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

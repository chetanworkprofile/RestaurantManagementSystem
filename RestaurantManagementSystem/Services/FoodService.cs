using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Models;
using System.Text.RegularExpressions;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Controllers;

namespace RestaurantManagementSystem.Services
{
    public class FoodService : IFoodService
    {
        Response response = new Response();                             //response models/objects
        ResponseWithoutData response2 = new ResponseWithoutData();
        CreateToken tokenUser = new CreateToken();              // model to create token
        object result = new object();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;

        public FoodService(IConfiguration configuration, RestaurantDbContext dbContext)
        {
            this._configuration = configuration;
            DbContext = dbContext;
        }
        public async Task<Object> AddFood(AddFood inpFood)
        {
            var DbFood = DbContext.Foods;
            //-----------------------------------------------------------------------------------------------------------------//
            //-----------------model validations--------------------------------------//

            string regexPatternFirstName = "^[A-Z][a-zA-Z]*$";
            if (!Regex.IsMatch(inpFood.foodName, regexPatternFirstName))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Name", false);
                return response2;
            }
            string regexPatternPhone = "^[0-9]*$";
            if (!Regex.IsMatch(inpFood.price.ToString(), regexPatternPhone))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Price", false);
                return response2;
            }
            /*List<string> categories = new List<string> { };
            if (!categories.Contains(inpFood.category))
            {
                response2 = new ResponseWithoutData(400, "Please Enter valid category", false);
                return response2;
            }*/
            //-----------------------------------------------------------------------------------------------------------------//

            //create new food object to add into database
            var food = new Food(Guid.NewGuid(), inpFood.foodName, inpFood.price, inpFood.timeToPrepare, false, inpFood.category, inpFood.pathToPic);

            await DbContext.Foods.AddAsync(food);
            await DbContext.SaveChangesAsync();

            //response object
            //RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, token);
            response = new Response(200, "Food added Successfully", food, true);
            return response;
        }
    }
}

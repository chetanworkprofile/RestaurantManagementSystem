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
        public Object AddFood(AddFood inpFood, out int code)
        {
            var DbFood = DbContext.Foods;
            //-----------------------------------------------------------------------------------------------------------------//
            //-----------------model validations--------------------------------------//

            /*string regexPatternFirstName = "^[A-Z][a-zA-Z]*$";
            if (!Regex.IsMatch(inpFood.foodName, regexPatternFirstName))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Name", false);
                code = 400;
                return response2;
            }*/
            string regexPatternPhone = "^[0-9]*$";
            if (!Regex.IsMatch(inpFood.price.ToString(), regexPatternPhone))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Price", false);
                code = 400;
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

            DbContext.Foods.Add(food);
            DbContext.SaveChanges();
            FoodResponse res = new FoodResponse(food);
            response = new Response(200, "Food added Successfully", res, true);
            code = 200;
            return response;
        }

        public Object GetFoods(Guid? foodId, string userId,string token, string? searchString, string? category, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code)          // sort order   ===   e1 for ascending   -1 for descending
        {
            //get logged in user from database
            Guid id = new Guid(userId);
            var userLoggedIn = DbContext.Users.Find(id);
            var foods = DbContext.Foods.AsQueryable();

            foods = foods.Where(t => t.isDeleted == false);     //remove deleted foods from list

            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }

            //--------------------------filtering based on userId,searchString, Email, or Phone---------------------------------//

            if (foodId != null) { foods = foods.Where(s => (s.foodId == foodId)); }
            if (searchString != null) { foods = foods.Where(s => EF.Functions.Like(s.foodName, "%" + searchString + "%") || EF.Functions.Like(s.category, "%" + searchString + "%")); }
            //if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if ( category != "all") { foods = foods.Where(s => (s.category == category)); }

            var foodsList = foods.ToList();

            // delegate used to create orderby depending on user input
            Func<Food, Object> orderBy = s => s.foodId;
            if (OrderBy == "FoodId" || OrderBy == "ID" || OrderBy == "Id" || OrderBy == "foodId" || OrderBy == "foodid")
            {
                orderBy = x => x.foodId;
            }
            else if (OrderBy == "Name" || OrderBy == "name" || OrderBy == "FoodName" || OrderBy == "foodName" || OrderBy == "foodname")
            {
                orderBy = x => x.foodName;
            }
            else if (OrderBy == "Category" || OrderBy == "category")
            {
                orderBy = x => x.category;
            }
            else if (OrderBy == "price" || OrderBy == "Price" )
            {
                orderBy = x => x.price;
            }
            else if (OrderBy == "status" || OrderBy == "Status")
            {
                orderBy = x => x.status;
            }
            else if (OrderBy == "timetoprepare" || OrderBy == "time" || OrderBy == "Time" || OrderBy == "timeToPrepare" || OrderBy == "TimeToPrepare")
            {
                orderBy = x => x.timeToPrepare;
            }

            // sort according to input based on orderby
            if (SortOrder == 1)
            {
                foodsList = foodsList.OrderBy(orderBy).Select(c => (c)).ToList();
            }
            else
            {
                foodsList = foodsList.OrderByDescending(orderBy).Select(c => (c)).ToList();
            }

            //pagination
            foodsList = foodsList.Skip((PageNumber - 1) * RecordsPerPage)
                                  .Take(RecordsPerPage).ToList();

            List<FoodResponse> res = new List<FoodResponse>();

            foreach (var food in foodsList)
            {
                FoodResponse r = new FoodResponse(food);
                res.Add(r);
            }

            if (!res.Any())
            {
                response2 = new ResponseWithoutData(404, "No food found.", true);
                code = 404;
                return response2;
            }
            response = new Response(200, "Foods list fetched", res, true);
            code = 200;
            return response;
        }
    }
}

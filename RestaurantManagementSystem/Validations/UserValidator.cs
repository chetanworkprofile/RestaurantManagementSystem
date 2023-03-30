using FluentValidation;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Validations
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(p => p.firstName).NotEmpty()
                .WithErrorCode("name_required")
                .WithMessage("FirstName name cannot be empty");

            RuleFor(p => p.phone).GreaterThan(0)
                .WithErrorCode("Phone_invalid")
                .WithMessage("Quantity must be greater than 0");
        }
    }
}

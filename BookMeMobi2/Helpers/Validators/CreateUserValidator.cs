using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Models;
using FluentValidation;

namespace BookMeMobi2.Helpers.Validators
{
    public class CreateUserValidator : AbstractValidator<UserRegisterDto>
    {
        public CreateUserValidator()
        {
            RuleFor(m => m.UserName).NotEmpty().WithMessage("Username can't be empty.")
                                    .Length(5,30).WithMessage("Too short or too long username.")
                                    .Matches("^[A-Za-z]+.*").WithMessage("User name has to start with a letter.");
            RuleFor(m => m.Email).EmailAddress();
            RuleFor(m => m.FirstName).NotEmpty().WithMessage("First name can't be empty.")
                                     .Matches("[A-Za-z]+").WithMessage("First name can contain only letters.");
            RuleFor(m => m.LastName).NotEmpty().WithMessage("Last name can't be empty.")
                                    .Matches("[A-Za-z]+").WithMessage("Last name can contain only letters");
            RuleFor(m => m.KindleEmail).EmailAddress();
        }
    }
}

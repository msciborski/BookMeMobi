using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BookMeMobi2.Models.User;

namespace BookMeMobi2.Helpers.Validators
{
    public class UserResetPasswordValidator : AbstractValidator<UserResetPasswordDto>
    {
        public UserResetPasswordValidator()
        {
            RuleFor(m => m.Token).NotNull().NotEmpty();
            RuleFor(m => m.NewPassword).NotNull().NotEmpty()
                .Must(predicate: s => Regex.IsMatch(s, "\\d+") && Regex.IsMatch(s, "[A-Z]+")).Length(6, 20);
        }
    }
}

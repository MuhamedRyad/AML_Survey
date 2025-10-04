using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMLSurvey.Core.DTOS.Auth;
using FluentValidation;

namespace AMLSurvey.Core.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("كلمة المرور وتأكيدها غير متطابقين.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب.")
                .MaximumLength(100).WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("الاسم الأخير مطلوب.")
                .MaximumLength(100).WithMessage("الاسم الأخير يجب ألا يتجاوز 100 حرف.");

        }
    }
}

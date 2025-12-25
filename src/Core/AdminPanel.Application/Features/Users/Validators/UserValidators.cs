using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Domain.Constants;
using FluentValidation;

namespace AdminPanel.Application.Features.Users.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(Messages.Validation.UsernameRequired)
            .MinimumLength(3).WithMessage(Messages.Validation.UsernameMinLength)
            .MaximumLength(50).WithMessage(Messages.Validation.UsernameMaxLength)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage(Messages.Validation.UsernameFormat);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
            .EmailAddress().WithMessage(Messages.Validation.InvalidEmail);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
            .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength)
            .MaximumLength(100).WithMessage(Messages.Validation.PasswordMaxLength)
            .Matches("[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
            .Matches("[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
            .Matches("[0-9]").WithMessage(Messages.Validation.PasswordDigit);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Messages.Validation.FullNameRequired)
            .MinimumLength(2).WithMessage(Messages.Validation.FullNameMinLength)
            .MaximumLength(100).WithMessage(Messages.Validation.FullNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^05\d{8}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage(Messages.Validation.InvalidPhone);
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(Messages.Validation.InvalidEmail);

        RuleFor(x => x.FullName)
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage(Messages.Validation.FullNameMinLength)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage(Messages.Validation.FullNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^05\d{8}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage(Messages.Validation.InvalidPhone);
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
            .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength)
            .MaximumLength(100).WithMessage(Messages.Validation.PasswordMaxLength)
            .Matches("[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
            .Matches("[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
            .Matches("[0-9]").WithMessage(Messages.Validation.PasswordDigit);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage(Messages.Validation.PasswordConfirmMismatch);
    }
}

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(Messages.Error.UserNotFound);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
            .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength)
            .MaximumLength(100).WithMessage(Messages.Validation.PasswordMaxLength)
            .Matches("[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
            .Matches("[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
            .Matches("[0-9]").WithMessage(Messages.Validation.PasswordDigit);
    }
}

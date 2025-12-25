using AdminPanel.Application.Features.Auth.DTOs;
using AdminPanel.Domain.Constants;
using FluentValidation;

namespace AdminPanel.Application.Features.Auth.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(Messages.Validation.UsernameRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired);
    }
}

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
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

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(Messages.Validation.PasswordConfirmMismatch);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Messages.Validation.FullNameRequired)
            .MinimumLength(2).WithMessage(Messages.Validation.FullNameMinLength)
            .MaximumLength(100).WithMessage(Messages.Validation.FullNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^05\d{8}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage(Messages.Validation.InvalidPhone);
    }
}

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
            .EmailAddress().WithMessage(Messages.Validation.InvalidEmail);
    }
}

public class ResetPasswordWithTokenValidator : AbstractValidator<ResetPasswordWithTokenDto>
{
    public ResetPasswordWithTokenValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
            .EmailAddress().WithMessage(Messages.Validation.InvalidEmail);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(Messages.Error.InvalidToken);

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

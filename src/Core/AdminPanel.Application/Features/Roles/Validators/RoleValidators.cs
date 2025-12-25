using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Domain.Constants;
using FluentValidation;

namespace AdminPanel.Application.Features.Roles.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Messages.Validation.RoleNameRequired)
            .MinimumLength(2).WithMessage("اسم الدور يجب أن يكون حرفين على الأقل")
            .MaximumLength(50).WithMessage("اسم الدور يجب ألا يتجاوز 50 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("الوصف يجب ألا يتجاوز 200 حرف");
    }
}

public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("اسم الدور يجب أن يكون حرفين على الأقل")
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("اسم الدور يجب ألا يتجاوز 50 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("الوصف يجب ألا يتجاوز 200 حرف");
    }
}

public class AssignPermissionsValidator : AbstractValidator<AssignPermissionsDto>
{
    public AssignPermissionsValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage(Messages.Error.RoleNotFound);

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("الصلاحيات مطلوبة");
    }
}

using AdminPanel.Application.Features.AuditLogs.DTOs;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Application.Features.Tenants.DTOs;
using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Domain.Entities.Logging;
using AdminPanel.Domain.Entities.Tenancy;
using AutoMapper;

namespace AdminPanel.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s =>
                s.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList()));

        CreateMap<User, UserListDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s =>
                s.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList()));

        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Role Mappings
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.UsersCount, opt => opt.MapFrom(s => s.UserRoles.Count))
            .ForMember(d => d.PermissionsCount, opt => opt.MapFrom(s => s.RolePermissions.Count(rp => rp.IsGranted)));

        CreateMap<Role, RoleListDto>()
            .ForMember(d => d.UsersCount, opt => opt.MapFrom(s => s.UserRoles.Count))
            .ForMember(d => d.PermissionsCount, opt => opt.MapFrom(s => s.RolePermissions.Count(rp => rp.IsGranted)));

        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Permission Mappings
        CreateMap<Permission, PermissionDto>();

        // Tenant Mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(d => d.UsersCount, opt => opt.MapFrom(s => s.Users != null ? s.Users.Count : 0));

        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // AuditLog Mappings
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.ActionName, opt => opt.MapFrom(s => s.Action.ToString()));

        CreateMap<AuditLog, AuditLogDetailDto>()
            .ForMember(d => d.ActionName, opt => opt.MapFrom(s => s.Action.ToString()));
    }
}

namespace AdminPanel.Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
        public const string ManageRoles = "Users.ManageRoles";
        public const string ResetPassword = "Users.ResetPassword";
        public const string Export = "Users.Export";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Edit = "Roles.Edit";
        public const string Delete = "Roles.Delete";
        public const string ManagePermissions = "Roles.ManagePermissions";
    }

    public static class AuditLogs
    {
        public const string View = "AuditLogs.View";
        public const string Export = "AuditLogs.Export";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Edit = "Settings.Edit";
    }

    public static class Tenants
    {
        public const string View = "Tenants.View";
        public const string Create = "Tenants.Create";
        public const string Edit = "Tenants.Edit";
        public const string Delete = "Tenants.Delete";
    }

    public static List<(string Code, string Module, string Action, string DisplayNameAr)> GetAllPermissions()
    {
        return new List<(string, string, string, string)>
        {
            // Users
            (Users.View, "Users", "View", "عرض المستخدمين"),
            (Users.Create, "Users", "Create", "إنشاء مستخدم"),
            (Users.Edit, "Users", "Edit", "تعديل مستخدم"),
            (Users.Delete, "Users", "Delete", "حذف مستخدم"),
            (Users.ManageRoles, "Users", "ManageRoles", "إدارة أدوار المستخدم"),
            (Users.ResetPassword, "Users", "ResetPassword", "إعادة تعيين كلمة المرور"),
            (Users.Export, "Users", "Export", "تصدير المستخدمين"),

            // Roles
            (Roles.View, "Roles", "View", "عرض الأدوار"),
            (Roles.Create, "Roles", "Create", "إنشاء دور"),
            (Roles.Edit, "Roles", "Edit", "تعديل دور"),
            (Roles.Delete, "Roles", "Delete", "حذف دور"),
            (Roles.ManagePermissions, "Roles", "ManagePermissions", "إدارة صلاحيات الدور"),

            // AuditLogs
            (AuditLogs.View, "AuditLogs", "View", "عرض سجل التدقيق"),
            (AuditLogs.Export, "AuditLogs", "Export", "تصدير سجل التدقيق"),

            // Settings
            (Settings.View, "Settings", "View", "عرض الإعدادات"),
            (Settings.Edit, "Settings", "Edit", "تعديل الإعدادات"),

            // Tenants
            (Tenants.View, "Tenants", "View", "عرض المستأجرين"),
            (Tenants.Create, "Tenants", "Create", "إنشاء مستأجر"),
            (Tenants.Edit, "Tenants", "Edit", "تعديل مستأجر"),
            (Tenants.Delete, "Tenants", "Delete", "حذف مستأجر"),
        };
    }
}

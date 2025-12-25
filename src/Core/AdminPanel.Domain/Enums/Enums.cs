namespace AdminPanel.Domain.Enums;

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    View = 6,
    Export = 7,
    Import = 8
}

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

public enum NotificationType
{
    Info = 1,
    Success = 2,
    Warning = 3,
    Error = 4
}

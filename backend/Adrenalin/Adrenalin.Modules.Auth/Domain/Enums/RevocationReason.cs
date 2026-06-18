namespace Adrenalin.Modules.Auth.Domain.Enums;

public enum RevocationReason
{
    Logout,
    
    AdminForce,
    RoleChange,
    LogoutAll,
    SessionExpired ,
    PasswordReset,
    PasswordChanged,
    TokenReuseDetected
}
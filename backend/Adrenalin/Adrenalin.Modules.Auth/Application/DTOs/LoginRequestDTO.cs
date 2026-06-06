namespace Adrenalin.Modules.Auth.Application.DTOs
{
    public  sealed  record LoginRequestDTO(
        string Email,
        string Password
    );
}
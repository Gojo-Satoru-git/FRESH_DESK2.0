using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class Contact : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string? Phone { get; private set; }

    public bool IsAuthorized { get; private set; }

    public bool AutoCreated { get; private set; }

    public Company Company { get; private set; } = null!;
    public bool CanViewAllTickets { get; private set; }

    private Contact() { }
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static Contact Create(Guid companyId, string email, string name, bool autoCreated = true, bool isAuthorized = true, Guid? userId = null,bool canViewAllTickets = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Contact
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Email = NormalizeEmail(email),
            Name = name.Trim(),
            AutoCreated = autoCreated,
            IsAuthorized = isAuthorized,
            UserId = userId,
            CanViewAllTickets = canViewAllTickets
        };
    }

    public void Update(string name, string email, string? phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Name = name.Trim();
        Email = NormalizeEmail(email);
        Phone = phone?.Trim();
    }

    public void Authorize()
    {
        if (IsAuthorized) return;

        IsAuthorized = true;
    }

    public void RevokeAuthorization()
    {
        if (!IsAuthorized) return;

        IsAuthorized = false;
    }

    public void LinkUser(Guid userId)
    {
        UserId = userId;
    }

    public void UnlinkUser()
    {
        UserId = null;
    }
    public void GrantTicketVisibility()
{
    CanViewAllTickets = true;
}

public void RevokeTicketVisibility()
{
    CanViewAllTickets = false;
}
}

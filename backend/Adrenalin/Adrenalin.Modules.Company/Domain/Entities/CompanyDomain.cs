using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Company.Domain.Entities;

public sealed class CompanyDomain : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }

    public string Domain { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    public bool IsVerified { get; private set; }

    public DateTimeOffset? VerifiedAt { get; private set; }

    public Company Company { get; private set; } = null!;

    private CompanyDomain() { }

    public static CompanyDomain Create(Guid companyId, string domain, bool isPrimary = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        domain = NormalizeDomain(domain);

        return new CompanyDomain
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Domain = domain,
            IsPrimary = isPrimary,
            IsVerified = false
        };
    }

    public void SetPrimary()
    {
        IsPrimary = true;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
    }

    public void Verify()
    {
        if (IsVerified) return;

        IsVerified = true;
        VerifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDomain(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        Domain = NormalizeDomain(domain);

        IsVerified = false;
        VerifiedAt = null;
    }

    private static string NormalizeDomain(string domain)
    {
        domain = domain.Trim().ToLowerInvariant();

        if (domain.StartsWith("@")) domain = domain[1..];

        return domain;
    }
}

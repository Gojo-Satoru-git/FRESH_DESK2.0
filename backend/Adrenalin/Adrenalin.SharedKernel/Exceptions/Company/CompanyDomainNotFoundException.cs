using System;

namespace Adrenalin.SharedKernel.Exceptions.Company;

public class CompanyDomainNotFoundException : NotFoundException
{
    public CompanyDomainNotFoundException(Guid companyId, Guid domainId) 
        : base($"Domain '{domainId}' not found for company '{companyId}'.")
    {
    }
}

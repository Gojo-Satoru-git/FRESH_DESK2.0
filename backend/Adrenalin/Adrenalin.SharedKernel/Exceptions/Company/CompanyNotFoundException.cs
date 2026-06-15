using System;

namespace Adrenalin.SharedKernel.Exceptions.Company;

public class CompanyNotFoundException : NotFoundException
{
    public CompanyNotFoundException(Guid companyId) 
        : base($"Company with ID '{companyId}' was not found.")
    {
    }
}

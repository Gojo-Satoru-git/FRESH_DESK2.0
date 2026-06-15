using System;

namespace Adrenalin.SharedKernel.Exceptions.Company;

public class CompanyContactLimitNotConfiguredException : DomainException
{
    public CompanyContactLimitNotConfiguredException() 
        : base("Contact limit has not been configured for this company. Please contact an administrator.")
    {
    }
}

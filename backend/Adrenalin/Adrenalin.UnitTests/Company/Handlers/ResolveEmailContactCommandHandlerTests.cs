using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Company.Application.Handlers;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CompanyEntity = Adrenalin.Modules.Company.Domain.Entities.Company;

namespace Adrenalin.UnitTests.Company.Handlers;

public class ResolveEmailContactCommandHandlerTests
{
    private readonly Mock<IContactRepository> _contactRepoMock;
    private readonly Mock<ICompanyRepository> _companyRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ResolveEmailContactCommandHandler>> _loggerMock;
    private readonly ResolveEmailContactCommandHandler _handler;

    public ResolveEmailContactCommandHandlerTests()
    {
        _contactRepoMock = new Mock<IContactRepository>();
        _companyRepoMock = new Mock<ICompanyRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ResolveEmailContactCommandHandler>>();

        _handler = new ResolveEmailContactCommandHandler(
            _contactRepoMock.Object,
            _companyRepoMock.Object,
            _uowMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ExternalSenderWithNullUserId_CreatesContactWithNullUserId()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = CompanyEntity.Create("Acme", "US", "Standard");
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.Id))!.SetValue(company, companyId);
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.AllowAutoContactCreation))!.SetValue(company, true);
        
        _contactRepoMock.Setup(x => x.GetByEmailAsync("john@acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);
            
        _companyRepoMock.Setup(x => x.GetByDomainAsync("acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var command = new ResolveEmailContactContractCommand("john@acme.com", "John Doe", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.AutoCreated);
        
        _contactRepoMock.Verify(x => x.AddAsync(It.Is<Contact>(c => 
            c.Email == "john@acme.com" && 
            c.UserId == null && 
            c.AutoCreated == true
        ), It.IsAny<CancellationToken>()), Times.Once);
        
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GuidEmptySupplied_ConvertsToNullUserId()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = CompanyEntity.Create("Acme", "US", "Standard");
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.Id))!.SetValue(company, companyId);
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.AllowAutoContactCreation))!.SetValue(company, true);
        
        _contactRepoMock.Setup(x => x.GetByEmailAsync("john@acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);
            
        _companyRepoMock.Setup(x => x.GetByDomainAsync("acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var command = new ResolveEmailContactContractCommand("john@acme.com", "John Doe", Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.AutoCreated);
        
        _contactRepoMock.Verify(x => x.AddAsync(It.Is<Contact>(c => 
            c.UserId == null
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidAuthenticatedUser_PreservesUserId()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var validUserId = Guid.NewGuid();
        var company = CompanyEntity.Create("Acme", "US", "Standard");
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.Id))!.SetValue(company, companyId);
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.AllowAutoContactCreation))!.SetValue(company, true);
        
        _contactRepoMock.Setup(x => x.GetByEmailAsync("john@acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);
            
        _companyRepoMock.Setup(x => x.GetByDomainAsync("acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var command = new ResolveEmailContactContractCommand("john@acme.com", "John Doe", validUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _contactRepoMock.Verify(x => x.AddAsync(It.Is<Contact>(c => 
            c.UserId == validUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DomainMatchButAutoCreateDisabled_ReturnsCompanyWithoutContact()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = CompanyEntity.Create("Acme", "US", "Standard");
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.Id))!.SetValue(company, companyId);
        typeof(CompanyEntity).GetProperty(nameof(CompanyEntity.AllowAutoContactCreation))!.SetValue(company, false);
        
        _contactRepoMock.Setup(x => x.GetByEmailAsync("john@acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);
            
        _companyRepoMock.Setup(x => x.GetByDomainAsync("acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var command = new ResolveEmailContactContractCommand("john@acme.com", "John Doe", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.AutoCreated);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Null(result.Value.ContactId);
        
        _contactRepoMock.Verify(x => x.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

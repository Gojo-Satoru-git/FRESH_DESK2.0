using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.UnitTests.Fakes;

// No-op implementation of IUnitOfWork. Records whether SaveChangesAsync was called.
public class FakeUnitOfWork : IUnitOfWork
{
    public bool SaveChangesCalled { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalled = true;
        return Task.FromResult(1);
    }
}

// In-memory fake for IFileStorageService. Stores nothing; returns predictable URLs.
public class FakeFileStorageService : IFileStorageService
{
    public Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
        => Task.FromResult($"http://fakeurl/{folder}/{fileName}");

    public Task<Stream> OpenReadAsync(string fileUrl, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new MemoryStream());

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}

// In-memory fake for ITicketAttachmentRepository.
public class FakeTicketAttachmentRepository : ITicketAttachmentRepository
{
    public TicketAttachment? AttachmentToReturn { get; set; }

    public Task AddAsync(TicketAttachment attachment, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<TicketAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(AttachmentToReturn);

    public void Remove(TicketAttachment attachment) { }
}

// Fake implementation of ICurrentUserService. Set UserId to simulate an authenticated user.
public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

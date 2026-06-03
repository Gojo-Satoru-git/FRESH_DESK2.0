using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
    public sealed class UserSession : BaseEntity
    {
        public Guid UserId { get; private set; }

        public Guid? RefreshTokenId { get; private set; }

        public string? DeviceName { get; private set; }

        public string? IpAddress { get; private set; }

        public string? GeoLocation { get; private set; }

        public DateTimeOffset StartedAt { get; private set; }

        public DateTimeOffset LastActiveAt { get; private set; }

        public DateTimeOffset? EndedAt { get; private set; }

        public bool IsActive { get; private set; }
        public User User { get; private set; } = null!;

        public RefreshToken? RefreshToken { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Persistence.Context;
using Adrenalin.Persistence.Repositories;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Lookup.Application.DTOs;
using Adrenalin.Modules.Lookup.Domain.Entities;
using Adrenalin.Modules.Lookup.Application.Queries;
using System.Text.Json;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.UnitTests.Lookup.Application;

public class LookupQueryServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DbContextOptions<AdrenalinDbContext> _dbOptions;

    public LookupQueryServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _dbOptions = new DbContextOptionsBuilder<AdrenalinDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetGeoRegionsAsync_ShouldReturnFromCache_WhenCacheHit()
    {
        // Arrange
        var cachedRegions = new List<GeoRegionDto>
        {
            new GeoRegionDto(Guid.NewGuid(), "NA", "North America", "EST", new TimeOnly(9, 0), new TimeOnly(17, 0), "1111100")
        };
        _cacheServiceMock.Setup(x => x.GetOrSetAsync<List<GeoRegionDto>>(
                "lookups:regions", 
                It.IsAny<Func<Task<List<GeoRegionDto>>>>(), 
                It.IsAny<TimeSpan?>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedRegions);

        using var context = new AdrenalinDbContext(_dbOptions, new Mock<IPublisher>().Object);
        var service = new LookupQueryService(context, _cacheServiceMock.Object);

        // Act
        var result = await service.GetGeoRegionsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Code.Should().Be("NA");
        _cacheServiceMock.Verify(x => x.GetOrSetAsync<List<GeoRegionDto>>(
            "lookups:regions", 
            It.IsAny<Func<Task<List<GeoRegionDto>>>>(), 
            It.IsAny<TimeSpan?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

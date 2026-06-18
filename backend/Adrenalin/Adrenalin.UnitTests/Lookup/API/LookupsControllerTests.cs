using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using FluentAssertions;
using Adrenalin.unify.API.Controllers;
using Adrenalin.Modules.Lookup.Application.Queries;
using Adrenalin.Modules.Lookup.Application.DTOs;

namespace Adrenalin.UnitTests.Lookup.API;

public class LookupsControllerTests
{
    private readonly Mock<ILookupQueryService> _lookupQueryServiceMock;
    private readonly LookupsController _controller;

    public LookupsControllerTests()
    {
        _lookupQueryServiceMock = new Mock<ILookupQueryService>();
        _controller = new LookupsController(_lookupQueryServiceMock.Object);
    }

    [Fact]
    public async Task GetGeoRegions_ShouldReturnOkWithRegions()
    {
        // Arrange
        var expectedRegions = new List<GeoRegionDto>
        {
            new GeoRegionDto(Guid.NewGuid(), "EMEA", "Europe, Middle East, Africa", "UTC", new TimeOnly(9, 0), new TimeOnly(17, 0), "1111100")
        };
        _lookupQueryServiceMock.Setup(s => s.GetGeoRegionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRegions);

        // Act
        var result = await _controller.GetGeoRegions(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var regions = okResult.Value.Should().BeAssignableTo<IEnumerable<GeoRegionDto>>().Subject;
        regions.Should().HaveCount(1);
    }
}

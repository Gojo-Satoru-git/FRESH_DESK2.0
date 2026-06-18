using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class GetCompanyTicketMetricsQueryHandler : IRequestHandler<GetCompanyTicketMetricsQuery, Result<CompanyTicketMetricsDto>>
{
    private readonly ITicketDashboardRepository _dashboardRepo;

    public GetCompanyTicketMetricsQueryHandler(ITicketDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<Result<CompanyTicketMetricsDto>> Handle(GetCompanyTicketMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var isAuthorized = await _dashboardRepo.IsCompanyMemberOrAdminAsync(request.CompanyId, request.ActorId, cancellationToken);
            if (!isAuthorized)
            {
                return Result<CompanyTicketMetricsDto>.Failure("User is not authorized to view this company's metrics.");
            }

            var metrics = await _dashboardRepo.GetCompanyTicketMetricsAsync(request.CompanyId, cancellationToken);
            return Result<CompanyTicketMetricsDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            return Result<CompanyTicketMetricsDto>.Failure(ex.Message);
        }
    }
}

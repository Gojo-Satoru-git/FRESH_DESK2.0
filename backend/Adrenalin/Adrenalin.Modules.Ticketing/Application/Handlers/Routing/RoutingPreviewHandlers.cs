using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Routing;

public sealed class GetCompanyRoutingPreviewQueryHandler : IRequestHandler<GetCompanyRoutingPreviewQuery, Result<CompanyRoutingPreviewDto>>
{
    private readonly ITicketRoutingContextRepository _routingContext;
    private readonly IRoutingRuleRepository _ruleRepository;

    public GetCompanyRoutingPreviewQueryHandler(ITicketRoutingContextRepository routingContext, IRoutingRuleRepository ruleRepository)
    {
        _routingContext = routingContext;
        _ruleRepository = ruleRepository;
    }

    public async Task<Result<CompanyRoutingPreviewDto>> Handle(GetCompanyRoutingPreviewQuery query, CancellationToken ct)
    {
        try
        {
            var companyName = await _routingContext.GetCompanyNameAsync(query.CompanyId, ct);
            if (companyName == null)
            {
                return Result<CompanyRoutingPreviewDto>.Failure($"Company {query.CompanyId} not found");
            }

            var defaultGroupId = await _routingContext.GetCompanyDefaultGroupAsync(query.CompanyId, ct);
            string? defaultGroupName = null;
            if (defaultGroupId.HasValue)
            {
                defaultGroupName = await _routingContext.GetGroupNameAsync(defaultGroupId.Value, ct);
            }

            var rules = await _ruleRepository.GetByCompanyOrderedAsync(query.CompanyId, ct);
            
            // Extract group ids to fetch names
            var groupIds = rules.Select(r => r.GroupId).Distinct().ToList();
            var groupNames = await _routingContext.GetGroupNamesAsync(groupIds, ct);

            var explicitRules = rules.Select(r => new RoutingRuleDto(
                r.Id,
                r.CompanyId,
                companyName,
                r.GroupId,
                groupNames.GetValueOrDefault(r.GroupId, "Unknown"),
                r.ModuleId,
                null, // ModuleName left null for preview
                r.RegionCode,
                r.TierCode,
                r.Priority,
                r.TicketType,
                r.Keywords,
                r.RulePriority,
                r.IsDefault,
                r.CreatedAt,
                r.UpdatedAt
            )).ToList();

            var warnings = new List<string>();

            if (!defaultGroupId.HasValue && rules.Count == 0)
                warnings.Add("Missing default group and no explicit routing rules. Tickets for this company will fall back to the system global queue.");
            else if (!defaultGroupId.HasValue)
                warnings.Add("Missing default group. Unmatched tickets will fall back to the system global queue.");

            var inactiveGroupIds = explicitRules
                .Where(r => r.GroupName == "Unknown")
                .Select(r => r.GroupId)
                .Distinct()
                .ToList();

            if (inactiveGroupIds.Any())
                warnings.Add($"Broken routing rules detected. Groups might be deleted or inactive: {string.Join(", ", inactiveGroupIds)}");

            var preview = new CompanyRoutingPreviewDto(
                query.CompanyId,
                companyName,
                defaultGroupId,
                defaultGroupName,
                explicitRules,
                warnings
            );

            return Result<CompanyRoutingPreviewDto>.Success(preview);
        }
        catch (Exception ex)
        {
            return Result<CompanyRoutingPreviewDto>.Failure(ex.Message);
        }
    }
}

public sealed class GetGroupRoutingPreviewQueryHandler : IRequestHandler<GetGroupRoutingPreviewQuery, Result<GroupRoutingPreviewDto>>
{
    private readonly ITicketRoutingContextRepository _routingContext;
    private readonly IRoutingRuleRepository _ruleRepository;

    public GetGroupRoutingPreviewQueryHandler(ITicketRoutingContextRepository routingContext, IRoutingRuleRepository ruleRepository)
    {
        _routingContext = routingContext;
        _ruleRepository = ruleRepository;
    }

    public async Task<Result<GroupRoutingPreviewDto>> Handle(GetGroupRoutingPreviewQuery query, CancellationToken ct)
    {
        try
        {
            var groupName = await _routingContext.GetGroupNameAsync(query.GroupId, ct);
            if (groupName == null)
            {
                return Result<GroupRoutingPreviewDto>.Failure($"Group {query.GroupId} not found");
            }

            var defaultRoutes = await _routingContext.GetCompaniesWithDefaultGroupAsync(query.GroupId, ct);

            var rules = await _ruleRepository.GetByGroupAsync(query.GroupId, ct);

            var companyIds = rules.Select(r => r.CompanyId).Distinct().ToList();
            var companyNames = await _routingContext.GetCompanyNamesAsync(companyIds, ct);

            var explicitRules = rules.Select(r => new RoutingRuleDto(
                r.Id,
                r.CompanyId,
                companyNames.GetValueOrDefault(r.CompanyId, "Unknown"),
                r.GroupId,
                groupName,
                r.ModuleId,
                null,
                r.RegionCode,
                r.TierCode,
                r.Priority,
                r.TicketType,
                r.Keywords,
                r.RulePriority,
                r.IsDefault,
                r.CreatedAt,
                r.UpdatedAt
            )).ToList();

            var preview = new GroupRoutingPreviewDto(
                query.GroupId,
                groupName,
                defaultRoutes,
                explicitRules
            );

            return Result<GroupRoutingPreviewDto>.Success(preview);
        }
        catch (Exception ex)
        {
            return Result<GroupRoutingPreviewDto>.Failure(ex.Message);
        }
    }
}

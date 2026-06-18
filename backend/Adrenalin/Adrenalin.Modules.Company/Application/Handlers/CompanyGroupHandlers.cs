using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.Modules.Company.Application.Queries;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Handlers;

// ═══ ASSIGN COMPANY TO GROUP ═════════════════════════════════════════════════

public sealed class AssignCompanyToGroupCommandHandler
    : IRequestHandler<AssignCompanyToGroupCommand, Result>
{
    private readonly ICompanyRepository _companies;
    private readonly ICompanyGroupRepository _companyGroups;

    public AssignCompanyToGroupCommandHandler(
        ICompanyRepository companies,
        ICompanyGroupRepository companyGroups)
    {
        _companies = companies;
        _companyGroups = companyGroups;
    }

    public async Task<Result> Handle(AssignCompanyToGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var company = await _companies.GetByIdAsync(cmd.CompanyId, ct);
            if (company is null)
                return Result.Failure($"Company {cmd.CompanyId} not found.");
            if (!company.IsActive)
                return Result.Failure("Cannot assign groups to a deactivated company.");

            // Validate group exists using the cross-module lookup
            var groupExists = await _companyGroups.GroupExistsAsync(cmd.GroupId, ct);
            if (!groupExists)
                return Result.Failure($"Group {cmd.GroupId} not found.");

            // Check if mapping already exists (active)
            var existing = await _companyGroups.GetAsync(cmd.CompanyId, cmd.GroupId, ct);
            if (existing is not null)
                return Result.Success(); // idempotent

            // Check for soft-deleted mapping (restore pattern)
            var deleted = await _companyGroups.GetIncludingDeletedAsync(cmd.CompanyId, cmd.GroupId, ct);
            if (deleted is not null)
            {
                if (cmd.IsDefault)
                    await _companyGroups.ClearDefaultForCompanyAsync(cmd.CompanyId, cmd.ActorId, ct);
                deleted.Restore(cmd.IsDefault, cmd.Priority, cmd.ActorId);
                _companyGroups.Update(deleted);
            }
            else
            {
                if (cmd.IsDefault)
                    await _companyGroups.ClearDefaultForCompanyAsync(cmd.CompanyId, cmd.ActorId, ct);
                _companyGroups.Add(CompanyGroup.Create(cmd.CompanyId, cmd.GroupId, cmd.IsDefault, cmd.Priority, cmd.ActorId));
            }

            await _companyGroups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ REMOVE COMPANY FROM GROUP ═══════════════════════════════════════════════

public sealed class RemoveCompanyFromGroupCommandHandler
    : IRequestHandler<RemoveCompanyFromGroupCommand, Result>
{
    private readonly ICompanyGroupRepository _companyGroups;

    public RemoveCompanyFromGroupCommandHandler(ICompanyGroupRepository companyGroups)
        => _companyGroups = companyGroups;

    public async Task<Result> Handle(RemoveCompanyFromGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var cg = await _companyGroups.GetAsync(cmd.CompanyId, cmd.GroupId, ct);
            if (cg is null)
                return Result.Failure("Company is not assigned to this group.");

            if (cg.IsDefault)
            {
                var hasRoutingRules = await _companyGroups.HasRoutingRulesAsync(cmd.CompanyId, ct);
                if (!hasRoutingRules)
                    return Result.Failure("Cannot remove the default group because the company has no other routing rules. A company must always have at least one routing path.");
            }

            cg.SoftDelete(cmd.ActorId);
            _companyGroups.Update(cg);
            await _companyGroups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ SET DEFAULT COMPANY GROUP ═══════════════════════════════════════════════

public sealed class SetDefaultCompanyGroupCommandHandler
    : IRequestHandler<SetDefaultCompanyGroupCommand, Result>
{
    private readonly ICompanyGroupRepository _companyGroups;

    public SetDefaultCompanyGroupCommandHandler(ICompanyGroupRepository companyGroups)
        => _companyGroups = companyGroups;

    public async Task<Result> Handle(SetDefaultCompanyGroupCommand cmd, CancellationToken ct)
    {
        try
        {
            var cg = await _companyGroups.GetAsync(cmd.CompanyId, cmd.GroupId, ct);
            if (cg is null)
                return Result.Failure("Company is not assigned to this group.");

            // Clear existing default for this company, then set new one
            await _companyGroups.ClearDefaultForCompanyAsync(cmd.CompanyId, cmd.ActorId, ct);
            cg.SetDefault(true, cmd.ActorId);
            _companyGroups.Update(cg);
            await _companyGroups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ═══ QUERY: GET COMPANY GROUPS ═══════════════════════════════════════════════

public sealed class GetCompanyGroupsQueryHandler
    : IRequestHandler<GetCompanyGroupsQuery, Result<IReadOnlyList<CompanyGroupDto>>>
{
    private readonly ICompanyGroupRepository _companyGroups;
    private readonly ICompanyRepository _companies;

    public GetCompanyGroupsQueryHandler(
        ICompanyGroupRepository companyGroups,
        ICompanyRepository companies)
    {
        _companyGroups = companyGroups;
        _companies = companies;
    }

    public async Task<Result<IReadOnlyList<CompanyGroupDto>>> Handle(
        GetCompanyGroupsQuery query, CancellationToken ct)
    {
        try
        {
            var company = await _companies.GetByIdAsync(query.CompanyId, ct);
            if (company is null)
                return Result<IReadOnlyList<CompanyGroupDto>>.Failure($"Company {query.CompanyId} not found.");

            var companyGroups = await _companyGroups.GetByCompanyAsync(query.CompanyId, ct);
            if (!companyGroups.Any()) return Result<IReadOnlyList<CompanyGroupDto>>.Success(new List<CompanyGroupDto>());

            var groupIds = companyGroups.Select(cg => cg.GroupId).ToList();
            var groupNames = await _companyGroups.GetGroupNamesAsync(groupIds, ct);

            var dtos = companyGroups.Select(cg => new CompanyGroupDto(
                cg.CompanyId,
                company.Name,
                cg.GroupId,
                groupNames.TryGetValue(cg.GroupId, out var name) ? name : "Unknown",
                cg.IsDefault,
                cg.Priority,
                cg.CreatedAt
            )).ToList();

            return Result<IReadOnlyList<CompanyGroupDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<CompanyGroupDto>>.Failure(ex.Message); }
    }
}

// ═══ QUERY: GET GROUP COMPANIES ═══════════════════════════════════════════════

public sealed class GetGroupCompaniesQueryHandler
    : IRequestHandler<GetGroupCompaniesQuery, Result<IReadOnlyList<CompanyGroupDto>>>
{
    private readonly ICompanyGroupRepository _companyGroups;
    private readonly ICompanyRepository _companies;

    public GetGroupCompaniesQueryHandler(
        ICompanyGroupRepository companyGroups,
        ICompanyRepository companies)
    {
        _companyGroups = companyGroups;
        _companies = companies;
    }

    public async Task<Result<IReadOnlyList<CompanyGroupDto>>> Handle(
        GetGroupCompaniesQuery query, CancellationToken ct)
    {
        try
        {
            var groupExists = await _companyGroups.GroupExistsAsync(query.GroupId, ct);
            if (!groupExists)
                return Result<IReadOnlyList<CompanyGroupDto>>.Failure($"Group {query.GroupId} not found.");

            var mappings = await _companyGroups.GetByGroupAsync(query.GroupId, ct);
            var groupNames = await _companyGroups.GetGroupNamesAsync(new List<Guid> { query.GroupId }, ct);
            var groupName = groupNames.GetValueOrDefault(query.GroupId, "Unknown");

            var dtos = new List<CompanyGroupDto>();
            foreach (var cg in mappings)
            {
                var company = await _companies.GetByIdAsync(cg.CompanyId, ct);
                dtos.Add(new CompanyGroupDto(
                    cg.CompanyId,
                    company?.Name ?? "Unknown",
                    cg.GroupId,
                    groupName,
                    cg.IsDefault,
                    cg.Priority,
                    cg.CreatedAt
                ));
            }

            return Result<IReadOnlyList<CompanyGroupDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<CompanyGroupDto>>.Failure(ex.Message); }
    }
}

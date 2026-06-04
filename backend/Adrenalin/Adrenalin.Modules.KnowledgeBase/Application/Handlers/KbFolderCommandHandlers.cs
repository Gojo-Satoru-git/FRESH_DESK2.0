using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Domain.Entities;
using Adrenalin.Modules.KB.Domain.Interfaces;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using Adrenalin.SharedKernel.Results;
using MediatR;

namespace Adrenalin.Modules.KB.Application.Handlers;

// ─── CreateKbFolderCommandHandler ─────────────────────────────────────────────

public sealed class CreateKbFolderCommandHandler
    : IRequestHandler<CreateKbFolderCommand, Result<Guid>>
{
    private readonly IKbFolderRepository _repo;

    public CreateKbFolderCommandHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(
        CreateKbFolderCommand cmd, CancellationToken ct)
    {
        try
        {
            int parentDepth = 0;

            if (cmd.ParentId.HasValue)
            {
                var parent = await _repo.GetByIdAsync(cmd.ParentId.Value, ct);
                if (parent is null)
                    return Result<Guid>.Failure($"Parent folder {cmd.ParentId} not found.");
                if (parent.IsDeleted)
                    return Result<Guid>.Failure("Cannot create a child folder under a deleted folder.");

                parentDepth = await _repo.GetDepthAsync(cmd.ParentId.Value, ct);
            }

            var folder = KbFolder.Create(
                FolderName.Create(cmd.Name),
                cmd.ParentId,
                cmd.DisplayOrder,
                cmd.ActorId,
                currentParentDepth: parentDepth);

            _repo.Add(folder);
            await _repo.SaveChangesAsync(ct);

            return Result<Guid>.Success(folder.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// ─── RenameKbFolderCommandHandler ─────────────────────────────────────────────

public sealed class RenameKbFolderCommandHandler
    : IRequestHandler<RenameKbFolderCommand, Result>
{
    private readonly IKbFolderRepository _repo;

    public RenameKbFolderCommandHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result> Handle(RenameKbFolderCommand cmd, CancellationToken ct)
    {
        try
        {
            var folder = await _repo.GetByIdAsync(cmd.FolderId, ct);
            if (folder is null)
                return Result.Failure($"Folder {cmd.FolderId} not found.");

            folder.Rename(FolderName.Create(cmd.NewName), cmd.ActorId);
            _repo.Update(folder);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

// ─── ReorderKbFolderCommandHandler ────────────────────────────────────────────

public sealed class ReorderKbFolderCommandHandler
    : IRequestHandler<ReorderKbFolderCommand, Result>
{
    private readonly IKbFolderRepository _repo;

    public ReorderKbFolderCommandHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result> Handle(ReorderKbFolderCommand cmd, CancellationToken ct)
    {
        try
        {
            var folder = await _repo.GetByIdAsync(cmd.FolderId, ct);
            if (folder is null)
                return Result.Failure($"Folder {cmd.FolderId} not found.");

            folder.Reorder(cmd.NewDisplayOrder, cmd.ActorId);
            _repo.Update(folder);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

// ─── DeleteKbFolderCommandHandler ─────────────────────────────────────────────

public sealed class DeleteKbFolderCommandHandler
    : IRequestHandler<DeleteKbFolderCommand, Result>
{
    private readonly IKbFolderRepository _repo;

    public DeleteKbFolderCommandHandler(IKbFolderRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteKbFolderCommand cmd, CancellationToken ct)
    {
        try
        {
            var folder = await _repo.GetByIdAsync(cmd.FolderId, ct);
            if (folder is null)
                return Result.Failure($"Folder {cmd.FolderId} not found.");

            bool hasArticles = await _repo.HasArticlesAsync(cmd.FolderId, ct);
            if (hasArticles)
                return Result.Failure(
                    "Cannot delete a folder that still contains articles. Move or delete articles first.");

            folder.SoftDelete(cmd.ActorId);
            _repo.Update(folder);
            await _repo.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

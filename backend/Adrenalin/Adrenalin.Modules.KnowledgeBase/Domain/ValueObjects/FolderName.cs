namespace Adrenalin.Modules.KB.Domain.ValueObjects;

/// <summary>
/// Encapsulates kb.kb_folders.name — max 150 chars, cannot be blank.
/// </summary>
public sealed class FolderName
{
    public const int MaxLength = 150;

    public string Value { get; }

    private FolderName(string value) => Value = value;

    public static FolderName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be blank.", nameof(name));

        if (name.Length > MaxLength)
            throw new ArgumentException($"Folder name cannot exceed {MaxLength} characters.", nameof(name));

        return new FolderName(name.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(FolderName f) => f.Value;
}

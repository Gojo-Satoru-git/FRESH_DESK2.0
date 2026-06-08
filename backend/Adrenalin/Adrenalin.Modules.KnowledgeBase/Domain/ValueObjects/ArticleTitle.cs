namespace Adrenalin.Modules.KB.Domain.ValueObjects;

/// <summary>
/// Encapsulates kb.kb_articles.title — max 300 chars, cannot be blank.
/// </summary>
public sealed class ArticleTitle
{
    public const int MaxLength = 300;

    public string Value { get; }

    private ArticleTitle(string value) => Value = value;

    public static ArticleTitle Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Article title cannot be blank.", nameof(title));

        if (title.Length > MaxLength)
            throw new ArgumentException($"Article title cannot exceed {MaxLength} characters.", nameof(title));

        return new ArticleTitle(title.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(ArticleTitle t) => t.Value;
}

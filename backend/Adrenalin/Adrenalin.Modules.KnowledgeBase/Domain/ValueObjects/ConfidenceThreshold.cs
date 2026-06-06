namespace Adrenalin.Modules.KB.Domain.ValueObjects;

/// <summary>
/// Encapsulates kb.kb_articles.confidence_threshold — BETWEEN 0.500 AND 1.000.
/// Default 0.850 per schema addendum v8. Learning loop raises it via Raise().
/// </summary>
public sealed class ConfidenceThreshold
{
    public const decimal Minimum = 0.500m;
    public const decimal Maximum = 1.000m;
    public const decimal Default  = 0.850m;

    public decimal Value { get; }

    private ConfidenceThreshold(decimal value) => Value = value;

    public static ConfidenceThreshold Create(decimal value)
    {
        if (value < Minimum || value > Maximum)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Confidence threshold must be between {Minimum} and {Maximum}.");

        return new ConfidenceThreshold(value);
    }

    public static ConfidenceThreshold CreateDefault() => new(Default);

    /// <summary>
    /// Raises threshold by <paramref name="delta"/>, capped at Maximum.
    /// Called by the learning loop when reopen rate is high.
    /// </summary>
    public ConfidenceThreshold Raise(decimal delta) =>
        new(Math.Min(Value + delta, Maximum));

    public override string ToString() => Value.ToString("F3");
    public static implicit operator decimal(ConfidenceThreshold c) => c.Value;
}

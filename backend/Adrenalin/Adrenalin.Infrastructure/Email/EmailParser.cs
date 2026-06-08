using System;
using System.Text.RegularExpressions;

namespace Adrenalin.Infrastructure.Email;

public static class EmailParser
{
    private static readonly Regex SubjectPrefixRegex = new(
        @"^(?:re|fwd|fw|aw|vs|antwort|ref):\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string CleanSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return "No Subject";

        var cleaned = subject.Trim();
        
        // Loop to strip multiple nested prefixes
        string previous;
        do
        {
            previous = cleaned;
            cleaned = SubjectPrefixRegex.Replace(cleaned, string.Empty);
        } while (cleaned != previous);

        cleaned = cleaned.Trim();
        
        return cleaned.Length switch
        {
            0 => "No Subject",
            > 100 => cleaned[..97] + "...",
            _ => cleaned
        };
    }

    public static string CleanBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "No Description Provided";

        var cleaned = body.Trim();
        if (cleaned.Length > 5000)
        {
            cleaned = cleaned[..4997] + "...";
        }
        return cleaned;
    }
}

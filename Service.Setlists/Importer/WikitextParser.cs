using System.Text.RegularExpressions;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Importer;

/// <summary>
/// Class to parse the source wikitext from Linkinpedia
/// </summary>
public partial class WikitextParser : IWikitextParser
{
    [GeneratedRegex(@"{{Setlist[\s\S-[}]]*}}", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ExtractSetlistFromSourceRegex { get; }
    
    /// <summary>
    /// Regex to read a line form the setlist source and find the index of it. This can be used to group multiple lines.
    ///
    /// Example input: | ActNote12 = w/ "Castle Of Glass" Vocals
    /// </summary>
    [GeneratedRegex(@"\|\s+[A-Za-z]+(?<index>\d+)\s=.*", RegexOptions.Multiline, "en-US")]
    private static partial Regex ExtractIndexFromSetlistLine { get; }
    
    /// <summary>
    /// Regex to read the key (without index!) and assigned value from a line of the setlist source
    ///
    /// Example input: | ActNote12 = w/ "Castle Of Glass" Vocals
    /// Output: ActNote -> w/ "Castle Of Glass" Vocals
    /// </summary>
    [GeneratedRegex(@"\|\s+(?<key>[A-Za-z]+)(?<index>\d+)\s*=\s*(?<value>.*)$", RegexOptions.Multiline, "en-US")]
    private static partial Regex ExtractKeyAndValueFromSetlistLine { get; }
    
    public WikiSetlistEntry[] GetEntries(string setlistSource)
    {
        var relevantLines = setlistSource
            .Trim()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(l => l.StartsWith("| "))
            .ToArray();

        var groupedLines = relevantLines
            .GroupBy(l =>
            {
                var match = ExtractIndexFromSetlistLine.Match(l);
                _ = uint.TryParse(match.Groups["index"].Value, out var index);
                return index;
            });

        var wikiSetlistEntries = groupedLines
            .SelectMany(group =>
            {
                // these are all lines grouped by the number in the key names
                var linesInGroup = group.ToArray();
                
                //HOWEVER: Acts and songs might have the same number! So if acts were found, we need to split this
                var actLines = linesInGroup.Where(l => l.StartsWith("| Act")).ToArray();
                var remainingLines = linesInGroup.Where(l => !l.StartsWith("| Act")).ToArray();

                List<WikiSetlistEntry?> entries = [];
                if (actLines.Length > 0)
                {
                    var act = LinesToWikiSetlistEntry(actLines);
                    entries.Add(act);
                }
                
                var entry = LinesToWikiSetlistEntry(remainingLines);
                entries.Add(entry);
                return entries;
            })
            .Where(e => e is not null)
            .Cast<WikiSetlistEntry>();
        
        var finishedEntries = SetActNumbers(wikiSetlistEntries.ToArray());
        return finishedEntries;
    }

    /// <summary>
    /// Extract the part of the source that renders the setlist
    /// </summary>
    /// <param name="pageSource">Source of the whole wiki page</param>
    /// <returns>the setlist part or null if no setlist was found</returns>
    public string? ExtractSetlistSource(string pageSource)
    {
        var matches = ExtractSetlistFromSourceRegex.Matches(pageSource);
        return matches
            .FirstOrDefault(m => !m.Value.Contains("Type = Rehearsal", StringComparison.InvariantCultureIgnoreCase))
            ?.Value;
    }

    private static WikiSetlistEntry? LinesToWikiSetlistEntry(string[] lines)
    {
        // if any of the lines in this group start with Act, we know it's an act
        if (lines.Any(l => l.StartsWith("| Act")))
        {
            var dictionary = lines.Select(ExtractSetlistLineKeyValue).ToDictionary(kv => kv.key, kv => kv.value);
            return new ActWikiSetlistEntry
            {
                ActNumber = uint.Parse(dictionary["ActNo"]),
                Name = dictionary.GetValueOrDefault("ActName"),
                Note = dictionary.GetValueOrDefault("ActNote"),
            };
        }

        if (lines.Any(l => l.StartsWith("| Song")))
        {
            // if any line started with Song, we know it's a song
            var dictionary = lines.Select(ExtractSetlistLineKeyValue).ToDictionary(kv => kv.key, kv => kv.value);
            var index = ExtractSetlistLineKeyValue(lines.First()).index; // since lines.Any() was true, we have at least one line
            return new SongWikiSetlistEntry
            {
                SongNumber = index ?? 0,
                Name = dictionary.GetValueOrDefault("Song"),
                Note = dictionary.GetValueOrDefault("Note")
            };
        }

        // anything else is not implemented and will be discarded
        return null;
    }

    /// <summary>
    /// Read a line from a setlist and extracts the key and value
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private static (string key, string value, uint? index) ExtractSetlistLineKeyValue(string line)
    {
        var match = ExtractKeyAndValueFromSetlistLine.Match(line);
        _ = uint.TryParse(match.Groups["index"].Value, out var index);
        return (match.Groups["key"].Value, match.Groups["value"].Value, index);
    }
    
    /// <summary>
    /// Makes sure that all <see cref="SongWikiSetlistEntry"/> in the <paramref name="entries"/> have the <see cref="SongWikiSetlistEntry.ActNumber"/> set.
    /// </summary>
    /// <param name="entries">setlist entries</param>
    /// <returns>entries with act numbers</returns>
    private WikiSetlistEntry[] SetActNumbers(WikiSetlistEntry[] entries)
    {
        uint? currentActNumber = null;
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case ActWikiSetlistEntry act:
                    currentActNumber = act.ActNumber;
                    break;
                case SongWikiSetlistEntry song:
                    song.ActNumber ??= currentActNumber;
                    break;
            }
        }
        
        return entries;
    }
}
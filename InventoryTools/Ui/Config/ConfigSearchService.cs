using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic;

namespace InventoryTools.Ui.Config;

public class ConfigSearchService
{
    private List<ConfigSearchEntry> _entries = [];

    public void BuildIndex(IEnumerable<IConfigPage> pages)
    {
        _entries = pages.SelectMany(c => c.GetSearchEntries()).ToList();
    }

    public int IndexedCount => _entries.Count;

    public IReadOnlyList<ConfigSearchEntry> Search(string query, int limit = 40)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var trimmed = query.Trim();

        return _entries
            .Select(c => (Entry: c, Score: Score(c, trimmed)))
            .Where(c => c.Score > 0)
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Entry.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .Select(c => c.Entry)
            .ToList();
    }

    private int Score(ConfigSearchEntry entry, string query)
    {
        if (entry.DisplayName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return 4;
        }

        if (entry.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (entry.Breadcrumb.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (entry.HelpText.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 0;
    }
}
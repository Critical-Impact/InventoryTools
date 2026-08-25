using System;

namespace InventoryTools.Ui.Config;

public class ConfigNavigationState
{
    private static readonly TimeSpan HighlightDuration = TimeSpan.FromSeconds(2.5);

    private Type? _scrollTarget;
    private Type? _highlightTarget;
    private DateTime _highlightUntil;

    public void RequestScrollTo(Type settingType, bool highlight = true)
    {
        _scrollTarget = settingType;
        if (!highlight)
        {
            return;
        }

        _highlightTarget = settingType;
        _highlightUntil = DateTime.UtcNow + HighlightDuration;
    }

    public bool ShouldScrollTo(Type settingType)
    {
        if (_scrollTarget != settingType)
        {
            return false;
        }

        _scrollTarget = null;
        return true;
    }

    public bool IsHighlighted(Type settingType)
    {
        if (_highlightTarget != settingType)
        {
            return false;
        }

        if (DateTime.UtcNow > _highlightUntil)
        {
            _highlightTarget = null;
            return false;
        }

        return true;
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryToolsMock;

public class IconBrowserWindow : GenericWindow
{
    public IconBrowserWindow(ILogger<IconBrowserWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "Icon Browser") : base(logger, mediator, imGuiService, configuration, name)
    {
    }

    public bool iconBrowserOpen = false;
    public bool doPasteIcon = false;
    public int pasteIcon = 0;

    private bool _tabExists = false;
    private int _i, _columns;
    private string _name;
    private float _iconSize;
    private string _tooltip;
    private bool _useLowQuality = false;
    private List<(int, int)> _iconList;
    private bool _displayOutsideMain = true;

    private const int iconMax = 200_000;
    private HashSet<int> _iconExistsCache;
    private readonly Dictionary<string, List<int>> _iconCache = new();

    public void ToggleIconBrowser() => iconBrowserOpen = !iconBrowserOpen;

    public override void Initialize()
    {
        WindowName = "Icon Browser";
        Key = "iconbrowser";
    }

    public override string GenericKey { get; } = "iconbrowser";
    public override string GenericName { get; } = "Icon Browser";
    public override bool DestroyOnClose { get; }
    public override bool SaveState { get; }
    public override Vector2? DefaultSize { get; } = new Vector2(300, 300);
    public override Vector2? MaxSize { get; } = new Vector2(2000, 2000);
    public override Vector2? MinSize { get; } = new Vector2(300, 300);

    /// <summary>
    /// Taken from Umbra
    /// </summary>
    private static Dictionary<string, List<(uint, uint)>> Categories { get; } = new() {
        {
            "General", [
                (0, 94), (101, 125), (651, 652), (66001, 66001), (66021, 66023), (66031, 66033),
                (66041, 66043), (66051, 66053), (66061, 66063), (66071, 66073), (66081, 66083), (66101, 66105),
                (66121, 66125), (66141, 66145), (66161, 66171), (66181, 66191), (66301, 66341), (66401, 66423),
                (66452, 66473), (60001, 6004), (60011, 60013), (60026, 60048), (60071, 60074), (61471, 61489),
                (61501, 61548), (61551, 61598), (61751, 61768), (61801, 61850), (61875, 61880)
            ]
        }, {
            "Jobs", [
                (0, 0), (62001, 62042), (62801, 62842), (62226, 62267), (62101, 62142), (62301, 62320), (62401, 62422)
            ]
        }, {
            "Quests", [
                (0, 0),
                (71001, 71006), (71021, 71025), (71041, 71045), (71061, 70165), (71081, 70185), (71101, 71102),
                (71121, 71125), (71141, 71145), (71201, 71205), (71221, 71225), (61721, 61723), (61731, 61733),
                (63875, 63892), (63900, 63977), (63979, 63987)
            ]
        }, {
            "Avatars", [
                (0, 0), (82009, 82010), (62145, 62146), (72556, 72608), (72001, 72059), (73001, 73279), (88001, 88444)
            ]
        }, {
            "Emotes", [
                (0, 0), (64001, 64080), (64082, 64099), (64101, 64133), (64151, 64154), (64326, 64349), (64352, 64455),
                (64457, 64459)
            ]
        }, {
            "Rewards", [
                (0, 0), (65001, 65115), (65122, 65126), (65130, 65130)
            ]
        }, {
            "MapMarkers", [
                (0, 0),
                (60401, 60408), (60412, 60482), (60501, 60508), (60511, 60515), (60550, 60565), (60567, 60583),
                (60585, 60611), (60640, 60649), (60651, 60662), (60751, 60792), (60901, 60999)
            ]
        }, {
            "Shapes", [
                (0, 0),
                (82091, 82093), (90001, 90004), (90200, 90263), (90401, 90463), (61901, 61918),
                (230131, 230143),                   // group pose birthday
                (230201, 230215),                   // group pose decoration small
                (230301, 230317),                   // group pose decoration large
                (230401, 230433),                   // group pose stamp symbol
                (230701, 230715),                   // group pose stamp nation
                (230626, 230629), (230631, 230641), // Group pose facial motion
                (180021, 180028),                   // Ranking Symbols
            ]
        }, {
            "Minions", [
                (0, 0), (4401, 4521), (4523, 4611), (4613, 4891), (4893, 4928), (4930, 4935), (4938, 4939),
                (59401, 59521), (59523, 59611), (59613, 59891),
                (59893, 59926)
            ]
        }, {
            "Mounts", [
                (0, 0), (4001, 4045), (4047, 4098), (4101, 4276), (4278, 4297), (4301, 4301), (4303, 4304)
            ]
        },
    };

    public override void Draw()
    {
        var iconSize = 64 * ImGuiHelpers.GlobalScale;

        if (ImGui.Button("RebuildIconCache"))
        {
            BuildCache(true);
        }

        if (ImGui.BeginTabBar("Icon Tabs", ImGuiTabBarFlags.NoTooltip))
        {
            foreach (var category in Categories)
            {
                if (BeginIconList(category.Key, iconSize))
                {
                    foreach (var iconSet in category.Value)
                    {
                        AddIcons((int)iconSet.Item1, (int)iconSet.Item2, $"{iconSet.Item1}-{iconSet.Item2}");
                    }
                }

                EndIconList();
            }
            // if (BeginIconList(" ★ ", iconSize))
            // {
            //     AddIcons(0, 100, "System");
            //     AddIcons(62_000, 62_600, "Class/Job");
            //     AddIcons(62_800, 62_900, "Gearsets");
            //     AddIcons(66_000, 66_400, "Macros");
            //     AddIcons(90_000, 100_000, "FC Crests/Symbols");
            //     AddIcons(114_000, 114_100, "New Game+");
            // }
            // EndIconList();
            //
            // if (BeginIconList("Misc", iconSize))
            // {
            //     AddIcons(60_000, 61_000, "UI");
            //     AddIcons(61_200, 61_250, "Markers");
            //     AddIcons(61_290, 61_390, "Markers 2");
            //     AddIcons(61_390, 62_000, "UI 2");
            //     AddIcons(62_600, 62_620, "HQ FC Banners");
            //     AddIcons(63_900, 64_000, "Map Markers");
            //     AddIcons(64_500, 64_600, "Stamps");
            //     AddIcons(65_000, 65_900, "Currencies");
            //     AddIcons(76_300, 78_000, "Group Pose");
            //     AddIcons(180_000, 180_060, "Stamps/Chocobo Racing");
            // }
            // EndIconList();
            //
            // if (BeginIconList("Misc 2", iconSize))
            // {
            //     AddIcons(62_900, 63_200, "Achievements/Hunting Log");
            //     AddIcons(65_900, 66_000, "Fishing");
            //     AddIcons(66_400, 66_500, "Tags");
            //     AddIcons(67_000, 68_000, "Fashion Log");
            //     AddIcons(71_000, 71_500, "Quests");
            //     AddIcons(72_000, 72_500, "BLU UI");
            //     AddIcons(72_500, 76_000, "Bozja UI");
            //     AddIcons(76_000, 76_200, "Mahjong");
            //     AddIcons(80_000, 80_200, "Quest Log");
            //     AddIcons(80_730, 81_000, "Relic Log");
            //     AddIcons(83_000, 84_000, "FC Ranks");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Actions", iconSize))
            // {
            //     AddIcons(100, 4_000, "Classes/Jobs");
            //     AddIcons(5_100, 8_000, "Traits");
            //     AddIcons(8_000, 9_000, "Fashion");
            //     AddIcons(9_000, 10_000, "PvP");
            //     AddIcons(61_100, 61_200, "Event");
            //     AddIcons(61_250, 61_290, "Duties/Trials");
            //     AddIcons(64_000, 64_200, "Emotes");
            //     AddIcons(64_200, 64_325, "FC");
            //     AddIcons(64_325, 64_500, "Emotes 2");
            //     AddIcons(64_600, 64_800, "Eureka");
            //     AddIcons(64_800, 65_000, "NPC");
            //     AddIcons(70_000, 70_200, "Chocobo Racing");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Mounts & Minions", iconSize))
            // {
            //     AddIcons(4_000, 4_400, "Mounts");
            //     AddIcons(4_400, 5_100, "Minions");
            //     AddIcons(59_000, 59_400, "Mounts... again?");
            //     AddIcons(59_400, 60_000, "Minion Items");
            //     AddIcons(68_000, 68_400, "Mounts Log");
            //     AddIcons(68_400, 69_000, "Minions Log");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Items", iconSize))
            // {
            //     AddIcons(20_000, 30_000, "General");
            //     AddIcons(50_000, 54_400, "Housing");
            //     AddIcons(58_000, 59_000, "Fashion");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Equipment", iconSize))
            // {
            //     AddIcons(30_000, 50_000, "Equipment");
            //     AddIcons(54_400, 58_000, "Special Equipment");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Aesthetics", iconSize))
            // {
            //     AddIcons(130_000, 142_000);
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Statuses", iconSize))
            // {
            //     AddIcons(10_000, 20_000);
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Garbage", iconSize, true))
            // {
            //     AddIcons(61_000, 61_100, "Splash Logos");
            //     AddIcons(62_620, 62_800, "World Map");
            //     AddIcons(63_200, 63_900, "Zone Maps");
            //     AddIcons(66_500, 67_000, "Gardening Log");
            //     AddIcons(69_000, 70_000, "Mount/Minion Footprints");
            //     AddIcons(70_200, 71_000, "DoH/DoL Logs");
            //     AddIcons(76_200, 76_300, "Fan Festival");
            //     AddIcons(78_000, 80_000, "Fishing Log");
            //     AddIcons(80_200, 80_730, "Notebooks");
            //     AddIcons(81_000, 82_060, "Notebooks 2");
            //     AddIcons(84_000, 85_000, "Hunts");
            //     AddIcons(85_000, 90_000, "UI 3");
            //     AddIcons(150_000, 170_000, "Tutorials");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Spoilers", iconSize, true))
            // {
            //     AddIcons(82_100, 83_000, "Triple Triad"); // Out of order because people might want to use these
            //     AddIcons(82_060, 82_100, "Trusts");
            //     AddIcons(120_000, 130_000, "Popup Texts");
            //     AddIcons(142_000, 150_000, "Japanese Popup Texts");
            //     AddIcons(180_060, 180_100, "Trusts Names");
            //     AddIcons(181_000, 181_500, "Boss Titles");
            //     AddIcons(181_500, iconMax, "Placeholder");
            // }
            //
            // EndIconList();
            //
            // if (BeginIconList("Spoilers 2", iconSize, true))
            // {
            //     AddIcons(71_500, 72_000, "Credits");
            //     AddIcons(100_000, 114_000, "Quest Images");
            //     AddIcons(114_100, 120_000, "New Game+");
            // }
            //
            // EndIconList();

            ImGui.EndTabBar();
        }

    }

    public override void Invalidate()
    {

    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;

    private bool BeginIconList(string name, float iconSize, bool useLowQuality = false)
    {
        _tooltip = "Contains:";
        if (ImGui.BeginTabItem(name))
        {
            _name = name;
            _tabExists = true;
            _i = 0;
            _columns = (int)((ImGui.GetContentRegionAvail().X - ImGui.GetStyle().WindowPadding.X) / (iconSize + ImGui.GetStyle().ItemSpacing.X)); // WHYYYYYYYYYYYYYYYYYYYYY
            _iconSize = iconSize;
            _iconList = new List<(int, int)>();

            if (useLowQuality)
                _useLowQuality = true;
        }
        else
        {
            _tabExists = false;
        }

        return _tabExists;
    }

    private void EndIconList()
    {
        if (_tabExists)
        {
            if (!string.IsNullOrEmpty(_tooltip))
                OtterGui.ImGuiUtil.HoverTooltip(_tooltip);
            BuildTabCache();
            ImGui.EndTabItem();
        }
        else if (!string.IsNullOrEmpty(_tooltip))
        {
            OtterGui.ImGuiUtil.HoverTooltip(_tooltip);
        }
    }

    private void AddIcons(int start, int end, string desc = "")
    {
        var count = 0;
        for (int index = start; index < end; index++)
        {
            if (ImGuiService.TextureProvider.TryGetFromGameIcon(new GameIconLookup((uint)index), out var icon))
            {
                ImGui.Image(icon.GetWrapOrEmpty().Handle, new Vector2(64, 64));

                ImGuiUtil.HoverTooltip(index.ToString());

                if (count % 10 != 0)
                {
                    ImGui.SameLine();
                }

                count++;
            }

        }
    }


    private void DrawIconList()
    {
        if (_columns <= 0) return;

        ImGui.BeginChild($"{_name}##IconList");

        var cache = _iconCache[_name];

        ImGuiListClipperPtr clipper;
        unsafe { clipper = ImGui.ImGuiListClipper(); }
        clipper.Begin((cache.Count - 1) / _columns + 1, _iconSize + ImGui.GetStyle().ItemSpacing.Y);

        var iconSize = new Vector2(_iconSize);
        while (clipper.Step())
        {
            for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var start = row * _columns;
                var end = Math.Min(start + _columns, cache.Count);
                for (int i = start; i < end; i++)
                {
                    var icon = cache[i];
                    if(ImGuiService.TextureProvider.TryGetFromGameIcon(new GameIconLookup((uint)icon), out var texture))
                    {
                        ImGui.Image(texture.GetWrapOrEmpty().Handle, iconSize);
                    }
                    else
                    {
                        ImGui.Dummy(iconSize);
                    }
                    if (ImGui.IsItemClicked())
                    {
                        doPasteIcon = true;
                        pasteIcon = icon;
                        ImGui.SetClipboardText($"::{icon}");
                    }

                    if (ImGui.IsItemHovered())
                    {
                        if (!ImGui.IsMouseDown(ImGuiMouseButton.Right))
                            ImGui.SetTooltip($"{icon}");
                    }
                    if (_i % _columns != _columns - 1)
                        ImGui.SameLine();
                    _i++;
                }
            }
        }

        clipper.Destroy();

        ImGui.EndChild();
    }

    private void BuildTabCache()
    {
        _iconExistsCache = new HashSet<int>();
        if (_iconCache.ContainsKey(_name)) return;
        Logger.LogDebug($"Building Icon Browser cache for tab \"{_name}\"");

        var cache = _iconCache[_name] = new();
        foreach (var (start, end) in _iconList)
        {
            for (int icon = start; icon < end; icon++)
            {
                if (_iconExistsCache.Contains(icon))
                    cache.Add(icon);
            }
        }

        Logger.LogInformation($"Done building tab cache! {cache.Count} icons found.");
    }

    public void BuildCache(bool rebuild)
    {
        Logger.LogInformation("Building Icon Browser cache");

        _iconCache.Clear();
        _iconExistsCache = new();

        if (_iconExistsCache.Count == 0)
        {
            for (int i = 0; i < iconMax; i++)
            {
                _iconExistsCache.Add(i);
            }

            _iconExistsCache.Remove(125052); // Remove broken image (TextureFormat R8G8B8X8 is not supported for image conversion)
        }

        Logger.LogInformation($"Done building cache! {_iconExistsCache.Count} icons found.");
    }
}
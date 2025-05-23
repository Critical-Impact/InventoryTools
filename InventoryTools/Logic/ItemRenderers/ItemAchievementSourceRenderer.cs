using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAchievementSourceRenderer : ItemInfoRenderer<ItemAchievementSource>
{
    public ItemAchievementSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Achievement;
    public override string SingularName => "Achievement";

    public override string? PluralName => "Achievements";
    public override string HelpText => "Can the item be earned via an achievement?";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        ImGui.Text(this.GetDescription(source));
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var achievementSource = AsSource(source);
        return achievementSource.Achievement.Value.Name.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.AchievementCertIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return
            $"{asSource.Achievement.Value.Name.ExtractText()} ({asSource.Achievement.Value.AchievementCategory.Value.Name.ExtractText()})";
    };
}
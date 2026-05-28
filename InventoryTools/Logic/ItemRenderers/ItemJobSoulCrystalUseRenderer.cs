using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemJobSoulCrystalUseRenderer : ItemInfoRenderer<ItemJobSoulCrystalUse>
{
    public ItemJobSoulCrystalUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.JobSoulCrystal;
    public override string SingularName => "Job Soul Crystal";
    public override string PluralName => "Job Soul Crystals";
    public override string HelpText => "Equipped to either switch from a Class to Job or used by a Job?";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        if (asSource.ParentClassJob != null)
        {
            ImGui.Text("Converts " + asSource.ParentClassJob.Base.Name.ExtractText() + " into " + asSource.ClassJob.Base.Name.ExtractText());
        }
        else
        {
            ImGui.Text("Provides the " + asSource.ClassJob.Base.Name.ExtractText() + " job");
        }
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        return asSource.ClassJob.Base.Name.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        var asSource = AsSource(source);
        return asSource.ClassJob.Icon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        if (asSource.ParentClassJob != null)
        {
            return "Converts " + asSource.ParentClassJob.Base.Name.ExtractText() + " into " + asSource.ClassJob.Base.Name.ExtractText();
        }

        return "Provides the " + asSource.ClassJob.Base.Name.ExtractText() + " job";
    };
}
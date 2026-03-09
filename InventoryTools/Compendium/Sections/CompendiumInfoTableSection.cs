using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Compendium.Sections;

public class CompendiumInfoTableSection : CompendiumViewSection
{
    private readonly CompendiumInfoTableSectionOptions _options;

    public delegate CompendiumInfoTableSection Factory(CompendiumInfoTableSectionOptions options);

    public CompendiumInfoTableSection(ImGuiService imGuiService, CompendiumInfoTableSectionOptions options) : base(imGuiService)
    {
        _options = options;
    }

    public override string SectionName => _options.SectionName;

    public override void DrawSection(SectionState sectionState)
    {
        using (var table = ImRaii.Table("##" + _options.SectionName, _options.Items.Count(c => c.IsVisible)))
        {
            if (table)
            {
                foreach (var item in _options.Items.Where(c => c.IsVisible))
                {
                    ImGui.TableSetupColumn(item.Header, ImGuiTableColumnFlags.WidthStretch);
                }

                foreach (var item in _options.Items.Where(c => c.IsVisible))
                {
                    ImGui.TableNextColumn();
                    ImGuiUtil.CenterWrappedText(item.Header);
                }

                ImGui.TableNextRow();
                foreach (var item in _options.Items.Where(c => c.IsVisible))
                {
                    ImGui.TableNextColumn();
                    ImGuiUtil.CenterWrappedText(item.Value);
                }
            }
        }
    }
}
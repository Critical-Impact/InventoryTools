using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public class SupportDumpWindow : GenericWindow
    {
        private readonly SupportDumpService _supportDumpService;
        private readonly IFileDialogManager _fileDialogManager;
        private readonly IChatUtilities _chatUtilities;

        public SupportDumpWindow(ILogger<SupportDumpWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, SupportDumpService supportDumpService, IFileDialogManager fileDialogManager, IChatUtilities chatUtilities, string name = "Generate Support Dump") : base(logger, mediator, imGuiService, configuration, name)
        {
            _supportDumpService = supportDumpService;
            _fileDialogManager = fileDialogManager;
            _chatUtilities = chatUtilities;
        }

        public override void Initialize()
        {
            WindowName = "Generate Support Dump";
            Key = "supportdump";
        }

        public override bool SaveState => false;
        public override Vector2? DefaultSize { get; } = new Vector2(500, 200);
        public override Vector2? MaxSize { get; } = new Vector2(800, 400);
        public override Vector2? MinSize { get; } = new Vector2(300, 150);
        public override string GenericKey { get; } = "supportdump";
        public override string GenericName { get; } = "Generate Support Dump";
        public override bool DestroyOnClose => true;

        public override void DrawWindow()
        {
            ImGui.PushTextWrapPos();
            ImGui.TextWrapped(
                "Only press this if you have been instructed to, it will generate a zip file containing your inventory, the allagan tools configuration, and your logs. If you are attempting to provide this information to help fix a bug, turn on Verbose Logging in File, replicate the bug and then generate this dump. Proceed?");
            ImGui.PopTextWrapPos();
            ImGui.NewLine();

            if (ImGui.Button("Proceed"))
            {
                _fileDialogManager.SaveFileDialog("Save support dump", "*.zip", "support_dump.zip", ".zip",
                    (success, path) =>
                    {
                        if (success)
                        {
                            GenerateDump(path);
                        }
                    }, null, true);
            }

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                this.IsOpen = false;
            }
        }

        private void GenerateDump(string path)
        {
            var result = _supportDumpService.GenerateDump(path);
            if (result.Success)
            {
                var included = result.IncludedFiles.Any() ? string.Join(", ", result.IncludedFiles) : "no files";
                _chatUtilities.Print($"Support dump saved to {result.ZipPath} ({included}).");
                if (result.MissingFiles.Any())
                {
                    _chatUtilities.PrintError($"The following files could not be included in the support dump: {string.Join(", ", result.MissingFiles)}.");
                }
            }
            else
            {
                _chatUtilities.PrintError("Failed to generate the support dump. Please check your Dalamud log for details.");
            }

            this.IsOpen = false;
        }

        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {

        }
    }
}

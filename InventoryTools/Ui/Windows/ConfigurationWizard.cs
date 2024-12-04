using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Features;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class ConfigurationWizard : GenericWindow
{
    private readonly ConfigurationWizardService _configurationWizardService;
    private readonly InventoryToolsConfiguration _configuration;

    public ConfigurationWizard(ILogger<ConfigurationWizard> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ConfigurationWizardService configurationWizardService, string name = "Configuration Wizard") : base(logger, mediator, imGuiService, configuration, name)
    {
        _configurationWizardService = configurationWizardService;
        _configuration = configuration;
    }
    private List<IFeature> _availableFeatures = new();
    private int _currentFeature;
    public override void Initialize()
    {
        WindowName = "Configuration Wizard";
        Key = "wizard";
        _availableFeatures = _configurationWizardService.GetNewFeatures();
    }

    public override string GenericKey => "wizard";
    public override string GenericName => "Configuration Wizard";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new(750, 500);
    public override Vector2? MaxSize { get; } = new(1000, 1000);
    public override Vector2? MinSize { get; } = new(750, 350);

    private bool CanGoPrevious => _currentFeature != 0;
    private bool CanGoNext => _availableFeatures.Count != 0 && _currentFeature != _availableFeatures.Count;

    private void NextStep()
    {
        if (_currentFeature == 0)
        {
            _currentFeature = 1;
        }
        else if(_currentFeature == _availableFeatures.Count)
        {

        }
        else
        {
            _currentFeature++;
        }
    }

    private void PreviousStep()
    {
        if (_currentFeature != 0)
        {
            _currentFeature--;
        }
    }

    public override void Draw()
    {
        using (var sideBar = ImRaii.Child("sideBar", new Vector2(150, 0) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (sideBar)
            {
                using (var sideBarMenu = ImRaii.Child("sideBarMenu",
                           new Vector2(150, -120) * ImGui.GetIO().FontGlobalScale, false))
                {
                    if (sideBarMenu)
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                                   _currentFeature == 0))
                        {
                            ImGui.Text("Welcome");
                        }

                        for (var index = 0; index < _availableFeatures.Count; index++)
                        {
                            var feature = _availableFeatures[index];
                            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                                       index + 1 == _currentFeature))
                            {
                                ImGui.Text((index + 1) + ". " + feature.Name);
                            }
                        }
                    }
                }
                using (var sideBarImage = ImRaii.Child("sideBarImage",
                           new Vector2(150, 0) * ImGui.GetIO().FontGlobalScale, false))
                {
                    if (sideBarImage)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 12.5f);
                        ImGui.Image(ImGuiService.GetImageTexture("icon").ImGuiHandle, new (100,100));
                    }
                }
            }
        }
        ImGui.SameLine();
        using (var mainWindow = ImRaii.Child("mainWindow", new Vector2(0, 0)))
        {
            if (mainWindow)
            {
                using (var mainContainer = ImRaii.Child("mainContainer", new Vector2(-1, -80) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (mainContainer)
                    {
                        if (_currentFeature == 0)
                        {
                            if (_configurationWizardService.ConfiguredOnce)
                            {
                                ImGui.TextWrapped("Welcome back to the Allagan Tools configuration wizard.");
                                ImGui.Separator();
                                ImGui.TextWrapped(
                                    "There are new features available to configure and you elected to show this window when that occurs.");
                                ImGui.NewLine();
                            }
                            else
                            {
                                ImGui.TextWrapped("Welcome to the Allagan Tools configuration wizard.");
                                ImGui.Separator();
                                ImGui.TextWrapped(
                                    "This will guide you through the setup of the most commonly used features. This wizard, with your permission will show itself again when a new feature gets released as features are normally left for the user to configure and activate.");
                                ImGui.NewLine();
                                ImGui.TextWrapped("If this is your first time using Allagan Tools, I'd recommend opening the help window and reading the General section. It gives you a run down of what the plugin can do.");
                                ImGui.TextWrapped("If you are a returning user feel free to close this window.");
                                if (ImGui.Button("Open Help"))
                                {
                                    MediatorService.Publish(new ToggleGenericWindowMessage(typeof(HelpWindow)));
                                }
                                ImGui.NewLine();
                            }


                        }
                        else
                        {
                            for (var index = 0; index < _availableFeatures.Count; index++)
                            {
                                var feature = _availableFeatures[index];
                                if (_currentFeature - 1 == index)
                                {
                                    ImGui.Text(feature.Name);
                                    ImGui.Separator();
                                    ImGui.PushTextWrapPos();
                                    ImGui.Text(feature.Description);
                                    ImGui.PopTextWrapPos();
                                    ImGui.Separator();
                                    foreach (var setting in _configurationWizardService.GetApplicableSettings(feature))
                                    {
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                                        setting.Draw(_configuration, setting.WizardName, true, true);
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                                    }
                                }
                            }
                        }
                    }
                }

                using (var nextPrevBar = ImRaii.Child("nextPrevBar", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (nextPrevBar)
                    {
                        if (_currentFeature == 0)
                        {
                            if (_configurationWizardService.ConfiguredOnce)
                            {
                                if (ImGui.Button("Continue"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                if (ImGui.Button("Close (and show next time the plugin loads)"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = true;
                                }
                            }
                            else
                            {
                                if (ImGui.Button("Continue (and show on new features)"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Continue (and never show the wizard again)"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = false;
                                }

                                if (ImGui.Button("Close (and show next time the plugin loads)"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Close (and never show the wizard again)"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = false;
                                }
                            }
                        }
                        else
                        {
                            var canGoPrevious = CanGoPrevious;
                            if (!canGoPrevious)
                            {
                                ImGui.BeginDisabled();
                            }

                            if (ImGui.Button("Previous"))
                            {
                                PreviousStep();
                            }

                            if (!canGoPrevious)
                            {
                                ImGui.EndDisabled();
                            }

                            ImGui.SameLine();
                            var canGoNext = CanGoNext;

                            if (canGoNext && ImGui.Button("Next"))
                            {
                                NextStep();
                            }

                            if (!canGoNext && ImGui.Button("Finish"))
                            {
                                Finish();
                            }

                        }
                    }
                }
            }
        }
    }

    private void Finish()
    {
        this.Close();
        _currentFeature = 0;
        foreach (var feature in _availableFeatures)
        {
            feature.OnFinish();
        }

        if (!_configurationWizardService.ConfiguredOnce)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }
        _configurationWizardService.MarkFeaturesSeen();

    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}
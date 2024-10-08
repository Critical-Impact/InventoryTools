using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Misc;
using InventoryTools.Overlays;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using Tetris.GameEngine;

namespace InventoryTools.Ui
{
    public class TetrisWindow : GenericWindow, IMenuWindow
    {
        private readonly TetrisGame _tetrisGame;
        private readonly TetrisOverlay _tetrisOverlay;

        public TetrisWindow(ILogger<TetrisWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, TetrisGame tetrisGame, TetrisOverlay tetrisOverlay, string name = "Tetris Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _tetrisGame = tetrisGame;
            _tetrisOverlay = tetrisOverlay;
        }
        public override void Initialize()
        {
            WindowName = "Tetris";
            Key = "tetris";
        }

        public override bool SaveState => false;

        public override void Draw()
        {
            var tetrisGame = _tetrisGame.Game;
            ImGui.PushTextWrapPos();
            ImGui.TextUnformatted("Welcome to Tetris. ");
            ImGui.TextUnformatted("Please turn on the tetris overlay, this will overwrite the contents of your main inventory window.");
            ImGui.TextUnformatted("Please make sure your inventory is set to 'Open All'. While the overlay is active, you will not be able to access your inventory.");
            ImGui.Separator();
            ImGui.TextUnformatted("Controls:");
            ImGui.TextUnformatted("Up: Smash Down");
            ImGui.TextUnformatted("Left: Move Left");
            ImGui.TextUnformatted("Right: Move Right");
            ImGui.TextUnformatted("Down: Move Down");
            ImGui.TextUnformatted("Z: Rotate Left");
            ImGui.TextUnformatted("X: Rotate Right");
            ImGui.PopTextWrapPos();
            ImGui.Separator();

            if (ImGui.Button(_tetrisGame.TetrisEnabled ? "Disable Tetris Overlay" : "Enable Tetris Overlay"))
            {
                if (_tetrisGame.TetrisEnabled)
                {
                    _tetrisOverlay.Enabled = false;
                    _tetrisOverlay.Clear();
                }
                else
                {
                    if (_tetrisGame.Game == null)
                    {
                        _tetrisGame.Restart();
                    }
                    _tetrisOverlay.Enabled = true;
                }
                _tetrisGame.ToggleTetris();
            }

            if (tetrisGame == null)
            {
                return;
            }

            ImGui.TextUnformatted("Overlay: " + (_tetrisGame.TetrisEnabled ? "Enabled" : "Disabled"));
            ImGui.TextUnformatted("Current Status: " + tetrisGame.Status.ToString());
            if ((tetrisGame.Status == Game.GameStatus.ReadyToStart || tetrisGame.Status == Game.GameStatus.Finished) && ImGui.Button("Start"))
            {
                if (tetrisGame.Status == Game.GameStatus.Finished)
                {
                    _tetrisGame.Restart();
                }
                else
                {
                    tetrisGame.Start();
                }
            }
            if (tetrisGame.Status == Game.GameStatus.InProgress && ImGui.Button("Pause"))
            {
                tetrisGame.Pause();
            }
            if (tetrisGame.Status == Game.GameStatus.Paused && ImGui.Button("Resume"))
            {
                tetrisGame.Pause();
            }
            if ((tetrisGame.Status == Game.GameStatus.InProgress || tetrisGame.Status == Game.GameStatus.Paused) && ImGui.Button("End Game"))
            {
                tetrisGame.GameOver();
            }
        }

        public override Vector2? DefaultSize { get; } = new Vector2(800, 300);
        public override Vector2? MaxSize => new Vector2(5000, 5000);
        public override Vector2? MinSize => new Vector2(300, 300);

        public override void Invalidate()
        {

        }
        public override FilterConfiguration? SelectedConfiguration => null;
        public override string GenericKey { get; } = "tetris";
        public override string GenericName { get; } = "Tetris";
        public override bool DestroyOnClose => true;
    }
}
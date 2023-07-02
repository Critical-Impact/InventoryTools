using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Misc;
using Tetris.GameEngine;

namespace InventoryTools.Ui
{
    public class TetrisWindow : Window
    {
        public override bool SaveState => false;

        public TetrisWindow(string name = "Allagan Tools - Tetris") : base(name)
        {
            
        }
        public TetrisWindow() : base("Allagan Tools - Tetris")
        {
            
        }

        public override void Draw()
        {
            var tetrisGame = Misc.TetrisGame.Instance.Game;
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

            if (ImGui.Button(TetrisGame.TetrisEnabled ? "Disable Tetris Overlay" : "Enable Tetris Overlay"))
            {
                TetrisGame.ToggleTetris();
            }
            
            ImGui.TextUnformatted("Overlay: " + (TetrisGame.TetrisEnabled ? "Enabled" : "Disabled"));
            ImGui.TextUnformatted("Current Status: " + tetrisGame.Status.ToString());
            if ((tetrisGame.Status == Game.GameStatus.ReadyToStart || tetrisGame.Status == Game.GameStatus.Finished) && ImGui.Button("Start"))
            {
                if (tetrisGame.Status == Game.GameStatus.Finished)
                {
                    TetrisGame.Restart();
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

        public override Vector2 DefaultSize { get; } = new Vector2(800, 300);
        public override Vector2 MaxSize => new Vector2(5000, 5000);
        public override Vector2 MinSize => new Vector2(300, 300);

        public override void Invalidate()
        {
            
        }
        public override FilterConfiguration? SelectedConfiguration => null;

        public static string AsKey => "tetris";
        public override string Key => AsKey;
        public override bool DestroyOnClose => true;
    }
}
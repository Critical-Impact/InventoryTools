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

        public override void Draw()
        {
            var tetrisGame = Misc.TetrisGame.Instance.Game;
            ImGui.TextWrapped("Welcome to Tetris. ");
            ImGui.Text("Please turn on the tetris overlay, this will overwrite the contents of your main inventory window.");
            ImGui.Text("Please make sure your inventory is set to 'Open All'. While the overlay is active, you will not be able to access your inventory.");

            if (ImGui.Button(TetrisGame.TetrisEnabled ? "Disable Tetris Overlay" : "Enable Tetris Overlay"))
            {
                TetrisGame.ToggleTetris();
            }
            
            ImGui.Text("Overlay: " + (TetrisGame.TetrisEnabled ? "Enabled" : "Disabled"));
            ImGui.Text("Current Status: " + tetrisGame.Status.ToString());
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
            if ((tetrisGame.Status == Game.GameStatus.InProgress || tetrisGame.Status == Game.GameStatus.Paused) && ImGui.Button("End Game"))
            {
                tetrisGame.GameOver();
            }
        }

        public override Vector2 Size => new Vector2(800, 300);
        public override Vector2 MaxSize => new Vector2(5000, 5000);
        public override Vector2 MinSize => new Vector2(300, 300);

        public override void Invalidate()
        {
            
        }
        public override FilterConfiguration? SelectedConfiguration => null;

        public static string AsKey => "tetris";
        public override string Name { get; } = "Allagan Tools - Tetris";
        public override string Key => AsKey;
        public override bool DestroyOnClose => true;
    }
}
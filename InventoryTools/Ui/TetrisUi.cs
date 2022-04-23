using ImGuiNET;
using InventoryTools.Misc;
using Tetris.GameEngine;

namespace InventoryTools
{
    public partial class InventoryToolsUi
    {
        private unsafe void DrawTetrisTab()
        {
            var tetrisGame = Misc.TetrisGame.Instance.Game;
            ImGui.Text("Welcome to tetris. Please make sure your inventory is set to open all and visible then hit start. Use left,right,down,up to control.");
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
    }
}
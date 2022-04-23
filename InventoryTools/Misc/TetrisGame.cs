using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using InventoryTools.GameUi;
using Tetris.GameEngine;

namespace InventoryTools.Misc
{
    public class TetrisGame : IDisposable
    {
        private static readonly int[,] clearBlock = {
            {0,0,0,0,0},
            {0,0,0,0,0},
            {0,0,0,0,0},
            {0,0,0,0,0}
        };

        public Dictionary<int, Vector4> BlockColors = new()
        {
            {1, ImGuiColors.DalamudWhite},
            {2, ImGuiColors.ParsedPurple},
            {3, ImGuiColors.ParsedBlue},
            {4, ImGuiColors.HealerGreen},
            {5, ImGuiColors.DalamudYellow},
            {6, ImGuiColors.DalamudRed},
            {7, ImGuiColors.TankBlue},
            {8, ImGuiColors.DalamudGrey},
        };
        public Vector4 BackgroundColour = new Vector4(0,0,0,0.4f);
        

        public int CursorX = 0;
        public int CursorY = 0;
        private static int _timerCounter = 0;
        private static readonly int _timerStep = 10;

        public Game Game;
        private static  Timer? _gameTimer;

        public TetrisGame()
        {
            Game = new Game();
            _gameTimer = new System.Timers.Timer(800);
            _gameTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            Service.Framework.Update += FrameworkOnOnUpdateEvent;
            _gameTimer.Start();
        }
        
        private bool isKeyPressed(VirtualKey[] keys) {
            foreach (var vk in Service.KeyState.GetValidVirtualKeys()) {
                if (keys.Contains(vk)) {
                    if (!Service.KeyState[vk]) return false;
                } else {
                    if (Service.KeyState[vk]) return false;
                }
            }
            return true;
        }

        private DateTime? lastMoveTime = null;
        
        private void FrameworkOnOnUpdateEvent(Framework framework) {
        try {
            if (Game.Status != Game.GameStatus.InProgress) return;

            if (lastMoveTime != null && lastMoveTime.Value.AddMilliseconds(300) >= DateTime.Now)
            {
                return;
            }                

            if (isKeyPressed(new[]{VirtualKey.UP})) {
                Game.Rotate();
                lastMoveTime = DateTime.Now;
            }

            if (isKeyPressed(new[]{VirtualKey.DOWN})) {
                Game.MoveDown();
                lastMoveTime = DateTime.Now;
            }

            if (isKeyPressed(new[]{VirtualKey.LEFT})) {
                Game.MoveLeft();
                lastMoveTime = DateTime.Now;
            }

            if (isKeyPressed(new[]{VirtualKey.RIGHT})) {
                Game.MoveRight();
                lastMoveTime = DateTime.Now;
            }

        } catch (Exception) {
            
        }
        }
        
        public static void EnableTetris()
        {
            PluginService.FilterManager.RemoveOverlay(WindowName.InventoryExpansion);
            PluginService.FilterManager.AddOverlay(new TetrisOverlay());
        }

        public static void DisableTetris()
        {
            PluginService.FilterManager.RemoveOverlay(WindowName.InventoryExpansion);
            PluginService.FilterManager.AddOverlay(new InventoryExpansionOverlay());
        }
        
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (Game.Status != Game.GameStatus.Finished)
            {
                if (Game.Status != Game.GameStatus.Paused)
                {
                    _timerCounter += _timerStep;
                    Game.MoveDown();
                    if (Game.Status == Game.GameStatus.Finished)
                    {
                        _gameTimer?.Stop();
                    }
                    else
                    {
                        if ( _timerCounter >= ( 1000 - (Game.Lines * 10) ) )
                        {
                            if (_gameTimer != null)
                            {
                                _gameTimer.Interval -= 50;
                            }
                            _timerCounter = 0;
                        }
                    }
                }
            }
        }

        public static void Restart()
        {
            _tetrisGame = new TetrisGame();
        }

        private static TetrisGame? _tetrisGame;

        public static TetrisGame Instance
        {
            get
            {
                if (_tetrisGame == null)
                {
                    _tetrisGame = new TetrisGame();
                }

                return _tetrisGame;
            }
        }

        public static bool HasInstance => _tetrisGame != null;


        public Dictionary<InventoryType, Dictionary<Vector2, Vector4?>> DrawScene()
        {
            Dictionary<Vector2, Vector4> positions = new();

            //Draw PlayField


            var gameBoard = Game.ActualBoard.ToArray();

            for (int x = 0; x < gameBoard.GetLength(0); x += 1) {
                for (int y = 0; y < gameBoard.GetLength(1); y += 1)
                {
                    var position = new Vector2(y, x);
                    var block = gameBoard[x, y];
                    if (!positions.ContainsKey(position))
                    {
                        var blockColor = BlockColors.ContainsKey(block) ? BlockColors[block] : BackgroundColour;
                        positions.Add(position, blockColor);
                    }
                }
            }

            Dictionary<InventoryType, Dictionary<Vector2, Vector4?>> finalPositions = new();
            finalPositions.Add(InventoryType.Bag0, new Dictionary<Vector2, Vector4?>());
            finalPositions.Add(InventoryType.Bag1, new Dictionary<Vector2, Vector4?>());
            finalPositions.Add(InventoryType.Bag2, new Dictionary<Vector2, Vector4?>());
            finalPositions.Add(InventoryType.Bag3, new Dictionary<Vector2, Vector4?>());
            foreach (var positionColor in positions)
            {
                var position = positionColor.Key;
                var color = positionColor.Value;
                InventoryType correctBag;
                var x = position.X;
                var y = position.Y;
                //Bag 0 or 2
                if ((int)x / 5 == 0)
                {
                    //Bag 0
                    if ((int)y / 7 == 0)
                    {
                        correctBag = InventoryType.Bag0;
                    }
                    else
                    {
                        y -= 7;
                        correctBag = InventoryType.Bag2;
                    }
                }
                //Bag 1 or 3
                else
                {
                    x -= 5;
                    //Bag 1
                    if ((int)y / 7 == 0)
                    {
                        correctBag = InventoryType.Bag1;
                    }
                    else
                    {
                        y -= 7;
                        correctBag = InventoryType.Bag3;
                    }
                }
                var actualPosition = new Vector2(x, y);
                if (!finalPositions[correctBag].ContainsKey(actualPosition))
                {
                    finalPositions[correctBag].Add(actualPosition, color);
                }
                
            }

            foreach (var finalPosition in finalPositions)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 7; y++)
                    {
                        if (!finalPosition.Value.ContainsKey(new Vector2(x, y)))
                        {
                            finalPosition.Value.Add(new Vector2(x,y ), BackgroundColour);
                        }
                    }
                }
            }

            return finalPositions;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _gameTimer?.Dispose();
                Service.Framework.Update -= FrameworkOnOnUpdateEvent;

            }
        }
    }
}
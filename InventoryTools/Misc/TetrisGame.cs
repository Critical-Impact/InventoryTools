using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.ClientState.Keys;
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
            {1, new Vector4(0,1,1,1)}, //Straight Line
            {2, new Vector4(1,1,0,1)}, //Block
            {3, new Vector4(1,0.498f,0,1)}, //L
            {4, new Vector4(0.0784f,0,1,1)}, //Reverse L
            {5, new Vector4(0,1,0,1)},//S block
            {6, new Vector4(1,0,0,1)},//Z block
            {7, new Vector4(0.501f,0,0.501f,1)},//T block
            {8, new Vector4(1,1,1,0.5f)}, //Shadow Piece
        };
        public Vector4 BackgroundColour = new Vector4(0,0,0,0.4f);
        

        public int CursorX = 0;
        public int CursorY = 0;
        private static int _timerCounter = 0;
        private static readonly int _timerStep = 10;

        public Game Game;
        private static Timer? _gameTimer;

        public TetrisGame()
        {
            Game = new Game();
            _gameTimer = new System.Timers.Timer(800);
            _gameTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            PluginService.FrameworkService.Update += FrameworkOnOnUpdateEvent;
            _gameTimer.Start();
        }
        
        private bool isKeyPressed(VirtualKey[] keys) {
            foreach (var vk in PluginService.KeyStateService.GetValidVirtualKeys()) {
                if (keys.Contains(vk)) {
                    if (!PluginService.KeyStateService[vk]) return false;
                } else {
                    if (PluginService.KeyStateService[vk]) return false;
                }
            }
            return true;
        }

        private DateTime? lastMoveTime = null;
        
        private void FrameworkOnOnUpdateEvent(IFrameworkService framework) {
        try {
            if (Game.Status != Game.GameStatus.InProgress) return;

            if (lastMoveTime != null && lastMoveTime.Value.AddMilliseconds(100) >= DateTime.Now)
            {
                PluginService.KeyStateService.ClearAll();
                return;
            }                

            if (isKeyPressed(new[]{VirtualKey.Z})) {
                Game.RotateLeft();
                lastMoveTime = DateTime.Now;
            }                 

            if (isKeyPressed(new[]{VirtualKey.X})) {
                Game.RotateRight();
                lastMoveTime = DateTime.Now;
            }          

            if (isKeyPressed(new[]{VirtualKey.UP})) {
                Game.SmashDown();
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
            PluginService.OverlayService.RefreshOverlayStates();
            PluginService.KeyStateService.ClearAll();
            

        } catch (Exception) {
            
        }
        }

        public static bool TetrisEnabled { get; set; }

        public static void ToggleTetris()
        {
            if (TetrisEnabled)
            {
                DisableTetris();
            }
            else
            {
                EnableTetris();
            }
        }

        public static void EnableTetris()
        {
            TetrisEnabled = true;
            PluginService.OverlayService.RemoveOverlay(WindowName.InventoryExpansion);
            PluginService.OverlayService.AddOverlay(new TetrisOverlay());
            PluginService.OverlayService.RefreshOverlayStates();
        }

        public static void DisableTetris()
        {
            TetrisEnabled = false;
            PluginService.OverlayService.RemoveOverlay(WindowName.InventoryExpansion);
            PluginService.OverlayService.AddOverlay(new InventoryExpansionOverlay());
            PluginService.OverlayService.RefreshOverlayStates();
        }
        
        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            if (Game.Status != Game.GameStatus.Finished)
            {
                if (Game.Status != Game.GameStatus.Paused)
                {
                    _timerCounter += _timerStep;
                    Game.MoveDown();
                    PluginService.OverlayService.RefreshOverlayStates();
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
                        //Shadow Block
                        if (block == 8)
                        {
                            var currentBlock = Game.CurrPiece.PieceType;
                            var currentBlockColour = BlockColors.ContainsKey(currentBlock) ? BlockColors[currentBlock] : BackgroundColour;
                            var blockColor = BlockColors.ContainsKey(block) ? BlockColors[block] * currentBlockColour : BackgroundColour;
                            positions.Add(position, blockColor);
                        }
                        else
                        {
                            var blockColor = BlockColors.ContainsKey(block) ? BlockColors[block] : BackgroundColour;
                            positions.Add(position, blockColor);   
                        }
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
        
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                _gameTimer?.Dispose();
                PluginService.FrameworkService.Update -= FrameworkOnOnUpdateEvent;
            }
            _disposed = true;         
        }
        
        ~TetrisGame()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( _disposed == false )
            {
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}
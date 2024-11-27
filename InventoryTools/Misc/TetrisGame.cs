using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using Microsoft.Extensions.Logging;
using Tetris.GameEngine;

namespace InventoryTools.Misc
{
    public class TetrisGame : MediatorSubscriberBase
    {
        public TetrisGame(ILogger<TetrisGame> logger, MediatorService mediatorService) : base(logger, mediatorService)
        {
        }

        private readonly int[,] clearBlock = {
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
        private int _timerCounter = 0;
        private readonly int _timerStep = 10;

        public Game? Game;
        private Timer? _gameTimer;



        private void Initialize()
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

        private void FrameworkOnOnUpdateEvent(IFramework framework) {
        try {
            if (Game != null && Game.Status != Game.GameStatus.InProgress) return;

            if (lastMoveTime != null && lastMoveTime.Value.AddMilliseconds(100) >= DateTime.Now)
            {
                Service.KeyState.ClearAll();
                return;
            }

            if (Game != null)
            {
                if (isKeyPressed(new[] { VirtualKey.Z }))
                {
                    Game.RotateLeft();
                    lastMoveTime = DateTime.Now;
                }

                if (isKeyPressed(new[] { VirtualKey.X }))
                {
                    Game.RotateRight();
                    lastMoveTime = DateTime.Now;
                }

                if (isKeyPressed(new[] { VirtualKey.UP }))
                {
                    Game.SmashDown();
                    lastMoveTime = DateTime.Now;
                }

                if (isKeyPressed(new[] { VirtualKey.DOWN }))
                {
                    Game.MoveDown();
                    lastMoveTime = DateTime.Now;
                }

                if (isKeyPressed(new[] { VirtualKey.LEFT }))
                {
                    Game.MoveLeft();
                    lastMoveTime = DateTime.Now;
                }

                if (isKeyPressed(new[] { VirtualKey.RIGHT }))
                {
                    Game.MoveRight();
                    lastMoveTime = DateTime.Now;
                }
            }

            MediatorService.Publish(new OverlaysRequestRefreshMessage());
            Service.KeyState.ClearAll();


        } catch (Exception) {

        }
        }

        public bool TetrisEnabled { get; set; }

        public void ToggleTetris()
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

        public void EnableTetris()
        {
            TetrisEnabled = true;
            //PluginService.OverlayService.RemoveOverlay(WindowName.InventoryExpansion);
            //PluginService.OverlayService.AddOverlay(new TetrisOverlay());
            MediatorService.Publish(new OverlaysRequestRefreshMessage());
        }

        public void DisableTetris()
        {
            TetrisEnabled = false;
            //PluginService.OverlayService.RemoveOverlay(WindowName.InventoryExpansion);
            //PluginService.OverlayService.AddOverlay(new InventoryExpansionOverlay());
            MediatorService.Publish(new OverlaysRequestRefreshMessage());
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            if (Game == null)
            {
                return;
            }

            if (Game.Status != Game.GameStatus.Finished)
            {
                if (Game.Status != Game.GameStatus.Paused)
                {
                    _timerCounter += _timerStep;
                    Game.MoveDown();
                    MediatorService.Publish(new OverlaysRequestRefreshMessage());
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

        public void Restart()
        {
            Initialize();
        }

        public Dictionary<InventoryType, Dictionary<Vector2, Vector4?>> DrawScene()
        {
            Dictionary<Vector2, Vector4> positions = new();

            //Draw PlayField

            var gameBoard = Game!.ActualBoard.ToArray();

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
                Service.Framework.Update -= FrameworkOnOnUpdateEvent;
            }
            _disposed = true;
        }
    }
}
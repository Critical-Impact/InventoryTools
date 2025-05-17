using System;
using Tetris.Interfaces;

namespace Tetris.GameEngine
{
    public class Game: IMovable, IMode
    {
        private const int _default_board_width = 10;
        private const int _default_board_height = 14;

        public enum GameStatus
        {
            ReadyToStart,
            InProgress,
            Paused,
            Finished
        }

        #region Private Fields

        /// <summary>
        /// The Playfield
        /// </summary>
        private Board _gameBoard;
        private GameStatus _status;
        private Piece _currPiece;
        private Piece? _nextPiece;
        private Random _rnd;
        private int _posX;
        private int _posY;
        private int _lines;
        private int _score;

        #endregion

        #region Constructors

        public Game()
        {
            _gameBoard = new Board(_default_board_width, _default_board_height);
            
            _currPiece = null!;
            _nextPiece = null;
            _status = GameStatus.ReadyToStart;
            ShadowPieceMode = true;
            NextPieceMode = true;
            _rnd = new Random();
            _posX = _posY = 0;
            _lines = 0;
            _score = 0;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            if (this._status != GameStatus.ReadyToStart)
            {
                throw new InvalidOperationException("Only game with status 'ReadyToStart' can be started");
            }
            this._status = GameStatus.InProgress;
            DropNewPiece();
        }

        public void Pause()
        {
            if (this._status == GameStatus.InProgress)
            {
                this._status = GameStatus.Paused;
            }
            else if (this._status == GameStatus.Paused)
            {
                this._status = GameStatus.InProgress;
            }
            else 
            {
                return;
            }
        }

        public void GameOver()
        {
            if ( (this._status != GameStatus.InProgress) && (this._status != GameStatus.Paused) )
            {
                throw new InvalidOperationException("Only game with status 'InProgress' or 'Pause'  can be finished");
            }
            _status = GameStatus.Finished;
        }

        #endregion

        #region Public Properties

        public int PosX
        {
            get 
            {
                return this._posX;
            }
        }

        public int PosY
        {
            get
            {
                return this._posY;
            }
        }

        public Board ActualBoard
        {
            get
            {
                if (this.Status == GameStatus.ReadyToStart)
                {
                    return this._gameBoard;
                }
                Board tmp_board = (Board)_gameBoard.Clone();
                Piece tmp_piece = (Piece)_currPiece.Clone();
                
                if (ShadowPieceMode == true)
                {
                    Piece shadow_piece = (Piece)_currPiece.Clone();
                    tmp_board.FixShadowPiece(shadow_piece, _posX, _posY);
                }
                tmp_board.FixPiece(tmp_piece, _posX, _posY);
                return tmp_board;
            }
        }

        public Piece? NextPiece
        {
            get 
            { 
                return _nextPiece; 
            }
        }

        public Piece CurrPiece
        {
            get
            {
                return _currPiece;
            }
        }

        public GameStatus Status
        {
            get 
            {
                return this._status; 
            }
        }

        public int Lines
        {
            get 
            { 
                return _lines; 
            }
        }

        public int Score
        {
            get 
            { 
                return _score; 
            }
        }

        #endregion

        #region Private Methods

        private void Step()
        {
            if (this.Status == GameStatus.InProgress)
            {
                if (_gameBoard.CanPosAt(_currPiece, _posX, _posY + 1))
                {
                    _posY++;
                }
                else
                {
                    _gameBoard.FixPiece(_currPiece, _posX, _posY);
                    int currLinesMade = _gameBoard.CheckLines();
                    _lines += currLinesMade;
                    switch (currLinesMade)
                    {
                        case 1:
                            _score += 40;
                            break;
                        case 2:
                            _score += 100;
                            break;
                        case 3:
                            _score += 300;
                            break;
                        case 4:
                            _score += 1200;
                            break;
                    }

                    if (_gameBoard.IsTopReached())
                    {
                        GameOver();
                    }
                    else
                    {
                        DropNewPiece();
                    }
                }
            }
        }

        private void DropNewPiece()
        {
            _rnd = new Random(DateTime.Now.Millisecond);
            var nextPiece = _nextPiece ?? PieceFactory.GetRandomPiece(_rnd);
            if (nextPiece != null)
            {
                _currPiece = nextPiece;
                _posY = _currPiece.InitPosY;
                _posX = ((_gameBoard.Width - 1) / 2) + _currPiece.InitPosX;
                _nextPiece = PieceFactory.GetRandomPiece(_rnd);
            }
        }

        #endregion

        #region IMovable Implementation

        public void MoveRight()
        {
            if (_gameBoard.CanPosAt(_currPiece, _posX + 1, _posY))
            {
                _posX++;
            }
        }

        public void MoveLeft()
        {
            if (_gameBoard.CanPosAt(_currPiece, _posX - 1, _posY))
            {
                _posX--;
            }
        }

        public void MoveDown()
        {
            Step();
        }

        public void SmashDown()
        {
            while (_gameBoard.CanPosAt(_currPiece, _posX, _posY + 1))
            {
                Step();
            }
            MoveDown();
        }

        public void RotateLeft()
        {
            Piece tmp_piece = _currPiece.RotateLeft();
            if (_gameBoard.CanPosAt(tmp_piece, _posX, _posY))
            {
                _currPiece = tmp_piece;
            }
        }

        public void RotateRight()
        {
            Piece tmp_piece = _currPiece.RotateRight();
            if (_gameBoard.CanPosAt(tmp_piece, _posX, _posY))
            {
                _currPiece = tmp_piece;
            }
        }

        #endregion

        #region IMode Implementation

        public bool NextPieceMode
        {
            get; set;
        }

        public bool ShadowPieceMode
        {
            get; set;
        }

        #endregion
    }
}

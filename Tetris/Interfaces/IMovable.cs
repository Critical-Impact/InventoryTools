using System;

namespace Tetris.Interfaces
{
    public interface IMovable
    {
        void MoveRight();
        void MoveLeft();
        void MoveDown();
        void SmashDown();
        void Rotate();
    }
}

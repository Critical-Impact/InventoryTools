namespace Tetris.Interfaces
{
    public interface IMovable
    {
        void MoveRight();
        void MoveLeft();
        void MoveDown();
        void SmashDown();
        void RotateLeft();
        void RotateRight();
    }
}

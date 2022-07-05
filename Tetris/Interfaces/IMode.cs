namespace Tetris.Interfaces
{
    public interface IMode
    {
        bool NextPieceMode { get; set; }
        bool ShadowPieceMode { get; set; }
    }
}

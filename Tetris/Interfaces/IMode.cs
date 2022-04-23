using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Interfaces
{
    public interface IMode
    {
        bool NextPieceMode { get; set; }
        bool ShadowPieceMode { get; set; }
    }
}

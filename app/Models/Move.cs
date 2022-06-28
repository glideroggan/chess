using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Models
{
    public record Move(Position From, Position To)
    {
    }

    public record PieceMove(Position From, Position To, bool MoveAllowed=true, bool CaptureAllowed=true)
    {
    }
}

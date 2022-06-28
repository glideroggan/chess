using System.Linq;
using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace app.Components;

public class PieceState
{
    public bool PlayerMoved { get; set; }
    public bool Hovering { get; set; }
}
public class BoardCellBase : ComponentBase
{
    [Inject] private IManager Manager { get; set; } = null!;
    [CascadingParameter] public UIState State { get; set; } = null!;
    [Parameter] public bool IsWhite { get; set; }
    [Parameter] public Position Position { get; set; } = null!;
    [Parameter] public EventCallback<PieceState> UpdateBoardCallback { get; set; }


    protected bool Tinted { get; set; }
    protected bool MovedTo { get; set; }

    protected char? _piece;
    protected string _checked = "";

    protected async Task OnMove()
    {
        State.StartPlace = Position;
        if (!Manager.MyTurn(Position)) return;
        State.LegalMoves = Manager.GetLegalMoves(from: Position);
        await UpdateBoardCallback.InvokeAsync(new PieceState { PlayerMoved = false, Hovering = true });
    }

    protected override Task OnParametersSetAsync()
    {
        _piece = Manager.GetPiece(Position);
        _checked = _piece?.ToEnum() == PieceEnum.King && Manager.CanBeCaptured(Position) ? "checked" : "";
        Tinted = State.LegalMoves.Any(x => x.To == Position);
        // Tinted = Manager.ValidMove(Position);
        MovedTo = Manager.GetHistory().LastOrDefault()?.To == Position;

        return Task.CompletedTask;
    }

    protected async Task<bool> Drop(DragEventArgs e)
    {
        if (!Manager.Move(State.StartPlace, Position))
        {
            State.StartPlace = Position.Empty;
            return false;
        }

        State.StartPlace = Position.Empty;
        State.LegalMoves.Clear();

        await UpdateBoardCallback.InvokeAsync(new PieceState { PlayerMoved = true, Hovering = false});
        return false;
    }
}
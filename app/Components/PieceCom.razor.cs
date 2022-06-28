using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Components;

namespace app.Components;

public class PieceComBase : ComponentBase
{
    [Inject] private IManager Manager { get; set; }
    [CascadingParameter] public UIState State { get; set; }
    [Parameter] public char Val { get; set; }
    [Parameter] public EventCallback OnMove { get; set; }

    protected int x;
    protected int y;
    protected char pieceValue;
    protected string myTurn;

    protected override async Task OnParametersSetAsync()
    {
        pieceValue = Val;
        myTurn = Manager.GetTurn() == Val.GetColor() ? "true" : "false";
    }

    protected async Task DragStart()
    {
        await OnMove.InvokeAsync();
    }
}
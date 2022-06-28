using System.Collections.Generic;
using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Components;

namespace app.Components.Modals;

public class PromotionBase : ComponentBase
{
    [Inject] private IManager Manager { get; set; }
    [Parameter] public PieceInfo PieceInfo { get; set; }
    [Parameter] public EventCallback<PieceInfo> OnDone { get; set; }
    [Parameter] public EventCallback Back { get; set; }

    protected List<PieceEnum> choices = new();

    protected async Task Select(char choice) => await OnDone.InvokeAsync(new(choice, PieceInfo.Pos));

    protected override async Task OnParametersSetAsync()
    {
        choices.Clear();
        // TODO: add possibility for black?
        if (PieceInfo.Piece.ToEnum() != PieceEnum.Pawn) return;
        
        var possibleResurrections = new[] {PieceEnum.Queen, PieceEnum.Bishop, PieceEnum.Knight, PieceEnum.Rook };
        var captures = Manager.GetCaptures(ColorEnum.White);
        
        foreach (var res in possibleResurrections)
        {
            if (captures.Contains(res.ToChar()))
                choices.Add(res);
        }
    }
}
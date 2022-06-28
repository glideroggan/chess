using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using app.Models;
using app.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using app.Components;

namespace app.Pages;


public class IndexBase : ComponentBase
{
    [Inject] private IManager Manager { get; set; }
    [Inject] private IAiService? Ai { get; set; }
    [Inject] private ILogger<IndexBase> logger { get; set; }
    [Inject] private IJSRuntime JS { get; set; }
    [CascadingParameter] protected UIState State { get; set; } = null!;

    protected bool _white;
    private bool _playSound;
    protected bool gameOver;
    protected string winner;
    private bool resetHistory;



    protected bool promotionModal;
    protected PieceInfo promotionPosition;
    protected bool aiOn;
    protected List<char> whiteCaptures = new();
    protected List<char> blackCaptures = new();

    protected void Switch() => _white = !_white;

    private async Task PlaySound()
    {
        await JS.InvokeVoidAsync("play");
    }

    protected void GameOver()
    {
        Manager.Reset();
        resetHistory = true;
        gameOver = false;
        StateHasChanged();
    }

    protected void ToggleAi()
    {
        aiOn = !aiOn;
        
    }

    protected override Task OnInitializedAsync()
    {
        State = new UIState();
        var timer = new Timer {Interval = 1000};
        timer.Elapsed += async (s, ea) =>
        {
            await Tick();
        };
        timer.AutoReset = true;
        timer.Start();
        
        return Task.CompletedTask;
    }

    protected Task PromotionDone(PieceInfo info)
    {
        Manager.Promote(info);
        promotionModal = false;
        
        // TODO: update captures

        return Task.CompletedTask;
    }

    protected async Task UpdateStatuses(PieceState info)
    {
        if (info.PlayerMoved)
        {
            await PlaySound();
        }
        
        UpdateCaptures();

        var promotion = Manager.CanPromote();
        if (promotion != null)
        {
            logger.LogWarning("have promotion!");
            promotionPosition = promotion;
            // There is a chance the tick goes before this, and then tick will execute
            // might be better to check HavePromotions in the tick method
            promotionModal = true;
        }

        StateHasChanged();
    }

    private void UpdateCaptures()
    {
        blackCaptures = Manager.GetCaptures(ColorEnum.White);
        whiteCaptures = Manager.GetCaptures(ColorEnum.Black);
    }

    private async Task Tick()
    {
        if (Manager.GetTurn() == ColorEnum.White || Ai.Working || promotionModal || gameOver) return;

        if (!aiOn) return;
        Ai.Working = true;
        try
        {
            // logger.LogWarning("Ticking");
            // TODO: ugly?
            var status = Manager.CheckStatus(ColorEnum.Black);
            if (status != null)
            {
                winner = "White";
                gameOver = true;
            }
            else
            {
                // logger.LogInformation($"FEN: {Manager.Fen}");
                await Ai.MakeMove();
                await PlaySound();

                await InvokeAsync(UpdateCaptures);
                
                
                status = Manager.CheckStatus(ColorEnum.White);
                if (status != null)
                {
                    winner = "Black";
                    gameOver = true;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Ai.Working = false;
            StateHasChanged();                
        }
    }
}
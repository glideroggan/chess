using System.Collections.Generic;
using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.AspNetCore.Components;

namespace app.Components
{
    public partial class History
    {
        [Inject] private IManager Manager { get; set; }
        
        [Parameter] public bool ResetHistory { get; set; }
        [Parameter] public EventCallback<bool> ResetHistoryChanged { get; set; }

        private List<Move> Moves { get; set; } = new();

        protected override async Task OnParametersSetAsync()
        {
            if (ResetHistory)
            {
                Moves.Clear();
                ResetHistory = false;
                await ResetHistoryChanged.InvokeAsync(ResetHistory);
            }
        }

        protected override Task OnInitializedAsync()
        {
            Manager.OnMove += (log) =>
            {
                Moves.Add(log);
                StateHasChanged();
            };

            return Task.CompletedTask;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
using Microsoft.AspNetCore.Components;

namespace app.Components
{
    public partial class CapturesRazorComponent : ComponentBase
    {
        [CascadingParameter] public UIState State { get; set; }
        [Parameter] public List<char> Caps { get; set; } = new();

        private List<char> _captures = new();

        public override Task SetParametersAsync(ParameterView parameters)
        {
            _captures = Caps.ToList();
            return base.SetParametersAsync(parameters);
        }

        protected override Task OnInitializedAsync()
        {
            _captures = Caps.ToList();
            return base.OnInitializedAsync();
        }
    }
}
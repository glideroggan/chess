using Microsoft.AspNetCore.Components;

namespace app.Components;

public class ModalBase : ComponentBase
{
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }
}

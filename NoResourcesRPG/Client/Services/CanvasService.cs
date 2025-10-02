using Microsoft.JSInterop;
using NoResourcesRPG.Client.Components;
using NoResourcesRPG.Shared.DTOs;
using NoResourcesRPG.Shared.Models;

namespace NoResourcesRPG.Client.Services;
public class CanvasService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<Pages.Game>? _dotNetRef;
    public CanvasService(IJSRuntime js) => _js = js;
    public ValueTask DrawWorldAsync(string canvasId, MapUpdateDto map, int recSize) =>
        _js.InvokeVoidAsync("canvasHelper.drawWorld", canvasId, map.Resources, map.Players.Values, map.Player, recSize);
    public ValueTask StartRenderLoop(Pages.Game page)
    {
        _dotNetRef = DotNetObjectReference.Create(page);
        return _js.InvokeVoidAsync("canvasHelper.startRenderLoop", _dotNetRef);
    }
    public async ValueTask DisposeAsync()
    {
        await _js.InvokeVoidAsync("canvasHelper.stopRenderLoop");
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}

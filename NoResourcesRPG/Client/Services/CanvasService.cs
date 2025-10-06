using Microsoft.JSInterop;
using NoResourcesRPG.Client.Components;
using NoResourcesRPG.Shared.DTOs;
using NoResourcesRPG.Shared.Models;

namespace NoResourcesRPG.Client.Services;
public class CanvasService(IJSRuntime js) : IAsyncDisposable
{
    private DotNetObjectReference<Pages.Game>? _dotNetRef;

    public ValueTask DrawWorldAsync(string canvasId, MapUpdateDto map, int recSize) =>
        js.InvokeVoidAsync("canvasHelper.drawWorld", canvasId, map.Resources, map.Players.Values, map.Player, recSize);
    public ValueTask StartRenderLoop(Pages.Game page)
    {
        _dotNetRef = DotNetObjectReference.Create(page);
        return js.InvokeVoidAsync("canvasHelper.startRenderLoop", _dotNetRef);
    }
    public async Task<Size> GetCanvasSize(string canvasId)
    {
        var size = await js.InvokeAsync<Size>("canvasHelper.getCanvasSize", canvasId);
        return size ?? new();
    }
    public async ValueTask DisposeAsync()
    {
        await js.InvokeVoidAsync("canvasHelper.stopRenderLoop");
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}

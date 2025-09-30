using Microsoft.JSInterop;
using NoResourcesRPG.Client.Components;
using NoResourcesRPG.Shared.Models;

namespace NoResourcesRPG.Client.Services;
public class CanvasService
{
    private readonly IJSRuntime _js;
    public CanvasService(IJSRuntime js) => _js = js;

    public ValueTask ClearAsync(string canvasId) =>
        _js.InvokeVoidAsync("canvasHelper.clear", canvasId);

    public ValueTask DrawRectAsync(string canvasId, int x, int y, int w, int h, string color) =>
        _js.InvokeVoidAsync("canvasHelper.drawRect", canvasId, x, y, w, h, color);

    public ValueTask DrawResourcesAsync(string canvasId, IEnumerable<Resource> resources, int recSize) =>
        _js.InvokeVoidAsync("canvasHelper.drawResources", canvasId, resources, recSize);

    public ValueTask DrawPlayersAsync(string canvasId, IEnumerable<Character> players, int recSize) =>
        _js.InvokeVoidAsync("canvasHelper.drawPlayers", canvasId, players, recSize);

    public ValueTask DrawSelfAsync(string canvasId, Character player, int recSize) =>
        _js.InvokeVoidAsync("canvasHelper.drawSelf", canvasId, player, recSize);
}

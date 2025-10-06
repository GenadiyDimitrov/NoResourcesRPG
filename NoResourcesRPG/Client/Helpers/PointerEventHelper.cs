
using Microsoft.AspNetCore.Components.Web;
using NoResourcesRPG.Client.Components;
using NoResourcesRPG.Client.Pages;
using NoResourcesRPG.Client.Services;
using NoResourcesRPG.Shared.DTOs;
using System.Numerics;

namespace NoResourcesRPG.Client.Helpers;

public class PointerEventHelper
{
    public delegate void SizeDelegate(Size size);
    public delegate void PointDelegate(double x, double y);
    public SizeDelegate OnClick;
    public PointDelegate OnDrag;
    private const double dragThreshold = 2; // px
    private const int clickCooldownMs = 200;
    private const double clickTimeThresholdMs = 200;
    private const double clickDistanceThreshold = 5;

    private Size _lastSize = new();
    private bool _isDragging = false;
    private bool _dragStarted = false;
    private (double X, double Y) _start = (0, 0);
    private DateTime _lastPointerDown;
    private DateTime _lastClickTime;
    private bool CanClick()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastClickTime).TotalMilliseconds < clickCooldownMs)
            return false;

        _lastClickTime = now;
        return true;
    }
    public async void OnPointerDown(PointerEventArgs e, CanvasService canvasService, string canvasId)
    {
        _lastSize = await canvasService.GetCanvasSize(canvasId);

        //set position
        _start.X = e.ClientX;
        _start.Y = e.ClientY;
        //set start click time
        _lastPointerDown = DateTime.UtcNow;

        // Only activate drag if starting in bottom-left quadrant
        if (_start.X < _lastSize.Width / 2 && _start.Y > _lastSize.Height / 2)
        {
            Console.WriteLine("Drag started");
            _isDragging = true;
            _dragStarted = false;
        }
    }

    public async void OnPointerUp(PointerEventArgs e, CanvasService canvasService, string canvasId)
    {
        var dt = (DateTime.UtcNow - _lastPointerDown).TotalMilliseconds;
        var dx = Math.Abs(e.ClientX - _start.X);
        var dy = Math.Abs(e.ClientY - _start.Y);
        if (dt < clickTimeThresholdMs && dx < clickDistanceThreshold && dy < clickDistanceThreshold)
        {
            if (CanClick())
            {
                var x = (e.ClientX - _lastSize.Left) * _lastSize.ScaleX;
                var y = (e.ClientY - _lastSize.Top) * _lastSize.ScaleY;
                Console.WriteLine($"Click detected! {x}x{y}");
                OnClick?.Invoke(new(_lastSize.Width, _lastSize.Height, x, y, 0, 0));
            }
            else
            {
                Console.WriteLine("Click ignored (too soon)");
            }
        }
        else if (_dragStarted)
        {
            Console.WriteLine("Drag ended");
        }

        _isDragging = false;
        _dragStarted = false;
    }

    public void OnPointerMove(PointerEventArgs e, CanvasService canvasService, string canvasId)
    {
        if (_isDragging)
        {
            var dx = e.ClientX - _start.X;
            var dy = e.ClientY - _start.Y;
            // Drag threshold

            if (!_dragStarted && (Math.Abs(dx) > dragThreshold || Math.Abs(dy) > dragThreshold))
            {
                _dragStarted = true;
                Console.WriteLine("Dragging started");
            }

            if (_dragStarted)
            {
                OnDrag?.Invoke(dx, dy);
                // Actual dragging logic
                Console.WriteLine($"Dragging {dx},{dy}");
            }
        }
    }
}



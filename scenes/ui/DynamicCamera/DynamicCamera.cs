using Game;
using Godot;
using System.Linq;

namespace UI
{

  public partial class DynamicCamera : Camera2D
  {
    private Level level;
    private BoxContainer grid;

    private InputEventScreenDrag[] dragEvents = new InputEventScreenDrag[2];
    private Vector2 lastDragDistance = Vector2.Zero;
    private float zoomIncrement = 0.25f;
    private int zoomSensitivity = 10;
    private Vector2 initialZoom;
    private Vector2 targetZoom;
    private Vector2 minZoom = new(0.5f, 0.5f);
    private Vector2 maxZoom = new(1.5f, 1.5f);
    private bool isZooming = false;

    public override void _Ready()
    {
      level = GetParent<Level>();
      grid = level.GetNode<BoxContainer>("Grid");

      SetupCamera();
    }

    public override void _Process(double delta)
    {
      if (!level.IsInputAllowed()) return;
      HandleInput();

      if (isZooming)
      {
        if (Zoom.IsEqualApprox(targetZoom))
        {
          isZooming = false;
          Zoom = targetZoom;
        }
        else
        {
          Zoom = Zoom.Lerp(targetZoom, 0.5f);
        }
      }
    }

    /**
     * Inspired from https://kidscancode.org/godot_recipes/3.x/2d/touchscreen_camera/index.html
     */
    public override void _Input(InputEvent @event)
    {
      if (!level.IsInputAllowed()) return;

      if (@event is InputEventScreenDrag dragEvent)
      {
        dragEvents[dragEvent.Index] = dragEvent;

        var activeEvents = dragEvents.Where(e => e != null).ToArray();

        if (activeEvents.Length == dragEvents.Length)
        {
          var dragDistance = activeEvents[0].Position.DirectionTo(activeEvents[1].Position);

          if (dragDistance < lastDragDistance)
          {
            DecreaseZoom();
          }
          else if (dragDistance > lastDragDistance)
          {
            IncreaseZoom();
          }

          lastDragDistance = dragDistance;
          dragEvents = new InputEventScreenDrag[2];
        }
      }
    }

    private void SetupCamera()
    {
      var gridRect = grid.GetRect();
      var cameraPosition = gridRect.Position + grid.Size / 2;

      GlobalPosition = cameraPosition;
      targetZoom = Zoom;
      initialZoom = Zoom;
    }

    private void HandleInput()
    {
      if (Input.IsActionJustPressed("scroll_down"))
      {
        DecreaseZoom();
      }
      else if (Input.IsActionJustPressed("scroll_up"))
      {
        IncreaseZoom();
      }
    }

    private void IncreaseZoom()
    {
      if (isZooming || Zoom.IsEqualApprox(maxZoom)) return;
      targetZoom = new Vector2(Zoom.X + zoomIncrement, Zoom.Y + zoomIncrement);

      isZooming = true;
    }

    private void DecreaseZoom()
    {
      if (isZooming || Zoom.IsEqualApprox(minZoom)) return;
      targetZoom = new Vector2(Zoom.X - zoomIncrement, Zoom.Y - zoomIncrement);

      isZooming = true;
    }
  }
}

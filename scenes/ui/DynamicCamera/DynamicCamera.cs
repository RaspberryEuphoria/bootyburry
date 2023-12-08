using Game;
using Godot;

namespace UI
{

  public partial class DynamicCamera : Camera2D
  {
    private ConfigFile config = new();
    private Level level;
    private BoxContainer grid;

    private InputEventScreenDrag[] dragEvents = new InputEventScreenDrag[2];
    private Vector2 lastDragDistance = Vector2.Zero;
    private float zoomIncrement = 0.25f;
    private float initialZoomPercentage;
    public Vector2 TargetZoom { get; private set; }
    private Vector2 minZoom = new(0.5f, 0.5f);
    private Vector2 maxZoom = new(1.5f, 1.5f);
    private bool isZooming = false;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      level = GetParent<Level>();
      grid = level.GetNode<BoxContainer>("Grid");

      var levelUI = level.GetNode<LevelUI>("LevelUI");
      levelUI.IncrementZoom += IncreaseZoom;
      levelUI.DecrementZoom += DecreaseZoom;

      SetupCamera();
    }

    public override void _Process(double delta)
    {
      if (isZooming)
      {
        if (Zoom.IsEqualApprox(TargetZoom))
        {
          isZooming = false;
          Zoom = TargetZoom;
        }
        else
        {
          Zoom = Zoom.Lerp(TargetZoom, 0.5f);
        }
      }
    }

    private void SetupCamera()
    {
      var gridRect = grid.GetRect();
      var cameraPosition = gridRect.Position + grid.Size / 2;

      GlobalPosition = cameraPosition;
      TargetZoom = Zoom;
    }

    private bool IsGridFullyVisible()
    {
      var gridRect = grid.GetRect();
      var cameraRect = GetViewportRect();

      return cameraRect.HasPoint(gridRect.Position) && cameraRect.HasPoint(gridRect.End);
    }

    private void IncreaseZoom()
    {
      if (isZooming || Zoom.IsEqualApprox(maxZoom)) return;
      TargetZoom = new Vector2(Zoom.X + zoomIncrement, Zoom.Y + zoomIncrement);

      isZooming = true;
    }

    private void DecreaseZoom()
    {
      if (isZooming || Zoom.IsEqualApprox(minZoom)) return;
      TargetZoom = new Vector2(Zoom.X - zoomIncrement, Zoom.Y - zoomIncrement);

      isZooming = true;
    }
  }
}

using Game;
using Godot;

namespace UI
{

  public partial class DynamicCamera : Camera2D
  {
    private ConfigFile config = new();
    private Level level;
    private BoxContainer grid;
    private Label currentZoomLabel;
    private Button increaseZoomButton;
    private Button decreaseZoomButton;

    private InputEventScreenDrag[] dragEvents = new InputEventScreenDrag[2];
    private Vector2 lastDragDistance = Vector2.Zero;
    private float zoomIncrement = 0.25f;
    private float initialZoomPercentage;
    private Vector2 targetZoom;
    private Vector2 minZoom = new(0.5f, 0.5f);
    private Vector2 maxZoom = new(1.5f, 1.5f);
    private bool isZooming = false;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      level = GetParent<Level>();
      grid = level.GetNode<BoxContainer>("Grid");

      currentZoomLabel = GetNode<Label>("%CurrentZoomLabel");
      increaseZoomButton = GetNode<Button>("%IncreaseZoomButton");
      decreaseZoomButton = GetNode<Button>("%DecreaseZoomButton");

      increaseZoomButton.ButtonUp += IncreaseZoom;
      decreaseZoomButton.ButtonUp += DecreaseZoom;

      SetupCamera();

      var uiScale = (float)config.GetValue("settings", "ui_scale");

      if (uiScale != 1)
      {
        var zoomLabelFontSize = currentZoomLabel.Get("theme_override_font_sizes/font_size");
        var newFontSize = (int)zoomLabelFontSize * uiScale;

        currentZoomLabel.Set("theme_override_font_sizes/font_size", newFontSize);
        increaseZoomButton.Set("theme_override_font_sizes/font_size", newFontSize);
        decreaseZoomButton.Set("theme_override_font_sizes/font_size", newFontSize);
      }
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

    private void SetupCamera()
    {
      var gridRect = grid.GetRect();
      var cameraPosition = gridRect.Position + grid.Size / 2;

      GlobalPosition = cameraPosition;
      targetZoom = Zoom;

      var zoomPercentage = Zoom.X * 100;
      currentZoomLabel.Text = $"{zoomPercentage}%";
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

      var zoomPercentage = targetZoom.X * 100;
      currentZoomLabel.Text = $"{zoomPercentage}%";
    }

    private void DecreaseZoom()
    {
      if (isZooming || Zoom.IsEqualApprox(minZoom)) return;
      targetZoom = new Vector2(Zoom.X - zoomIncrement, Zoom.Y - zoomIncrement);

      isZooming = true;

      var zoomPercentage = targetZoom.X * 100;
      currentZoomLabel.Text = $"{zoomPercentage}%";
    }
  }
}

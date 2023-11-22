using Game;
using Godot;
using System.Linq;

namespace UI
{

  public partial class DynamicCamera : Camera2D
  {
    private Level level;
    private BoxContainer grid;
    private float zoomIncrement = 1.5f;

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
    }

    private void SetupCamera()
    {
      var gridRect = grid.GetRect();
      var topLeftCornerPosition = new Vector2(0, 0);
      var bottomRightCornerPosition = new Vector2(gridRect.Size.X, gridRect.Size.Y);

      // Set the camera so that the grid is at the center of the screen
      var cameraPosition = gridRect.Position + grid.Size / 2;

      GlobalPosition = cameraPosition;
    }

    private void HandleInput()
    {
      if (Input.IsActionJustPressed("scroll_down"))
      {
        Zoom /= zoomIncrement;
      }
      else if (Input.IsActionJustPressed("scroll_up"))
      {
        Zoom *= zoomIncrement;
      }
    }
  }
}

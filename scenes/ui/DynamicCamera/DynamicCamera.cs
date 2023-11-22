using Game;
using Godot;
using System.Linq;

namespace UI
{

  public partial class DynamicCamera : Camera2D
  {
    private Level level;
    private Tile firstTile;
    private Tile lastTile;

    public override void _Ready()
    {
      level = GetParent<Level>();

      var tiles = level.GetChildren().OfType<Tile>();

      firstTile = tiles.First();
      lastTile = tiles.Last();

      SetupCamera();
    }

    private void SetupCamera()
    {
      var firstTilePosition = firstTile.GlobalPosition;
      var lastTilePosition = lastTile.GlobalPosition;

      var cameraPosition = new Vector2(
        (firstTilePosition.X + lastTilePosition.X) / 2,
        (firstTilePosition.Y + lastTilePosition.Y) / 2
      );

      GlobalPosition = cameraPosition;
    }
  }
}

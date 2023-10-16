using Godot;

namespace Game
{

  public partial class Selector : Node2D
  {
    [Export]
    public Direction direction;

    public override void _Ready()
    {
      Visible = false;
    }

    public void OnTileSelected(Tile tile)
    {
      if (tile.IsOnBorder(direction)) return;
      if (tile.HasHazardTerrain()) return;

      var tileWithTreasureInDirection = tile.GetNavigableTileInDirection(direction);
      var hasTileWithHazardInDirection = tileWithTreasureInDirection != null && tileWithTreasureInDirection != tile;
      if (!hasTileWithHazardInDirection) return;

      Visible = true;
    }

    public void OnTileUnselected(Tile _tile)
    {
      Visible = false;
    }
  }
}
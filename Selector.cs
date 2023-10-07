using Godot;

namespace Game
{

  public partial class Selector : StaticBody2D
  {
    [Export]
    public Direction direction;

    private Tile tile;

    public override void _Ready()
    {
      Visible = false;

      tile = GetParent<Tile>();

      tile.TileSelected += OnTileSelected;
      tile.TileUnselected += OnTileUnselected;
    }

    public void OnTileSelected(Tile _tile)
    {
      if (tile.IsOnBorder(direction)) return;
      if (!tile.HasAdjacentTileWithStarInDirection(direction)) return;
      Visible = true;
    }

    public void OnTileUnselected(Tile _tile)
    {
      Visible = false;
    }
  }
}
using Godot;

namespace Game
{
  public partial class Firewall : Node2D
  {
    private Level level;
    private Tile tile;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      level = tile.GetParent<Level>();

      tile.TileSelected += OnTileSelected;
    }

    public override void _ExitTree()
    {
      tile.TileSelected -= OnTileSelected;
    }

    public bool CanBeDockedFromDirection(Direction direction)
    {
      return false;
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return false;
    }

    public void Sunk()
    {
    }

    public void OnTileSelected(Tile tile)
    {
      Sunk();
    }
  }
}
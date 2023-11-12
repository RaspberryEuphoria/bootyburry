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
    }

    public bool CanBeDockedFromDirection(Direction _direction)
    {
      return false;
    }

    public bool CanUndockInDirection(Direction _direction)
    {
      return false;
    }

    public void Sunk()
    {
    }
  }
}
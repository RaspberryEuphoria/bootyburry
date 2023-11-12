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

    public bool IsBlockedFromDirection(Direction _direction)
    {
      return true;
    }

    public bool IsSelectableFromDirection(Direction _direction)
    {
      return false;
    }

    public bool CanUndockInDirection(Direction _direction)
    {
      return false;
    }

    public Direction? GetForcedDirection()
    {
      return null;
    }

    public void Sunk()
    {
    }
  }
}
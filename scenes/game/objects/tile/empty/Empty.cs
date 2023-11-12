using Godot;

namespace Game
{
  public partial class Empty : Node2D
  {
    public bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
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
  }
}
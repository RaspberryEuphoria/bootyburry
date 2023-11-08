using Godot;

namespace Game
{
  public partial class Empty : Node2D
  {
    public bool CanBeDockedFromDirection(Direction direction)
    {
      return false;
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return false;
    }
  }
}
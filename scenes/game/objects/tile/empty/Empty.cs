using Godot;

namespace Game
{
  public partial class Empty : TileTerrain
  {

    private Tile _rootTile;
    public override Tile RootTile
    {
      get => _rootTile;
      set
      {
        _rootTile = value;
      }
    }
    public override bool IsPlayerControlled { get => false; }
    public override bool ExpandPreviousPath { get => false; }

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
    }

    public override bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool IsSelectableFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool CanUndockInDirection(Direction _direction)
    {
      return false;
    }
  }
}
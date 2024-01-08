using Godot;

namespace Game
{
  public abstract partial class TileTerrain : Node2D
  {
    public abstract Tile RootTile
    {
      get;
      set;
    }
    public abstract bool IsPlayerControlled { get; }
    public abstract bool ExpandPreviousPath { get; }

    public virtual Tile GetNextSelectableTileInDirection(Direction direction)
    {
      return RootTile.GetNextSelectableTileInDirection(direction);
    }

    public virtual Tile GetNextCoreTileInDirection(Direction direction)
    {
      return RootTile.GetNextCoreTileInDirection(direction);
    }

    public virtual Tile GetAdjacentTile(Direction direction)
    {
      return RootTile.GetAdjacentTile(direction);
    }

    public virtual void Toggle()
    {

    }

    public virtual void Init()
    {

    }

    public abstract bool IsBlockedFromDirection(Direction direction);
    public abstract bool IsSelectableFromDirection(Direction direction);
    public abstract bool CanUndockInDirection(Direction direction);
  }
}
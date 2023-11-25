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

    public Tile DefaultGetNextSelectableTileInDirection(Direction direction)
    {
      return RootTile.GetNextSelectableTileInDirection(direction);
    }

    public Tile DefaultGetNextCoreTileInDirection(Direction direction)
    {
      return RootTile.GetNextCoreTileInDirection(direction);
    }

    public abstract Tile GetNextSelectableTileInDirection(Direction direction);
    public abstract Tile GetNextCoreTileInDirection(Direction direction);
    public abstract bool IsBlockedFromDirection(Direction direction);
    public abstract bool IsSelectableFromDirection(Direction direction);
    public abstract bool CanUndockInDirection(Direction direction);
  }
}
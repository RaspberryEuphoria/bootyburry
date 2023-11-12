using Godot;

namespace Game
{
  public partial class Proxy : Node2D
  {
    [Export]
    private Tile exitTile;

    private Level level;
    private Tile tile;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      level = tile.GetParent<Level>();

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      if (exitTile == null)
      {
        GD.PrintErr($"Proxy on tile {tile.Name} doesn't have an exit.");
      }
    }

    public bool Dock()
    {
      return true;
    }

    public bool CanBeDockedFromDirection(Direction direction)
    {
      return true;
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return true;
    }

    public void OnCurrentTileUpdated(Tile _tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null) return;

      // We don't want to trigger an infinite recursion by going back and forth between two proxies;
      // The proxy should only be triggered if the tile we're coming from is not a proxy itself.
      if (previousTile.Terrain is Proxy)
      {
        Level.TriggerInputInDirection(direction);
        return;
      }

      exitTile.Select(tile, direction);
    }
  }
}
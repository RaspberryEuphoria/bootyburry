using Godot;

namespace Game
{
  /**
   * A Proxy is a special type of Tile that is used to connect two Tiles.
   * 1. The player moves to the tile with Proxy A
   * 2. The Proxy A automatically selects Proxy B (through the "ExitTile" property)
   * 3. The Proxy B triggers an input with the same direction the player used
     when moving to Proxy A
   * 4. Finally, the player is stopped at the next selectable tile.
   */
  public partial class Proxy : Node2D
  {
    [Export]
    public Tile ExitTile;

    public static readonly bool IsPlayerControlled = false;
    public static readonly bool ExpandPreviousPath = false;

    private Level level;
    private Tile rootTile;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
      rootTile = GetParent<Tile>();
      level = rootTile.GetParent<Level>();
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      if (ExitTile == null)
      {
        GD.PrintErr($"Proxy on tile {rootTile.Name} doesn't have an exit.");
      }
    }

    public bool Dock()
    {
      return true;
    }

    public bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public bool IsSelectableFromDirection(Direction _direction)
    {
      return true;
    }

    public bool CanUndockInDirection(Direction _direction)
    {
      return true;
    }

    public Direction? GetForcedDirection()
    {
      return null;
    }

    public void OnCurrentTileUpdated(Tile tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null || tile != rootTile) return;

      animationPlayer.Play("shake");

      // Case 1: this Proxy was selected by the player
      if (previousTile.Terrain is not Proxy)
      {
        ExitTile.Select(rootTile, direction);
        return;
      }

      // Case 2: this Proxy was selected by its twin
      level.TriggerInputInDirection(direction);
    }
  }
}
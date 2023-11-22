using System.Linq;
using Godot;

namespace Game
{
  public partial class Core : Node2D
  {
    [Export]
    private bool isCoreEnabledOnStart = false;

    public static readonly bool IsPlayerControlled = true;
    public static readonly bool ExpandPreviousPath = false;

    private AnimationPlayer animationPlayer;
    private InnerCore innerCore;
    private Tile tile;
    private Selector[] selectors;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      selectors = GetChildren().OfType<Selector>().ToArray();
      innerCore = GetNode<InnerCore>("InnerCore");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      tile.TileSelected += OnTileSelected;
      tile.TileUnselected += OnTileUnselected;

      for (int i = 0; i < selectors.Length; i++)
      {
        tile.TileSelected += selectors[i].OnTileSelected;
        tile.TileUnselected += selectors[i].OnTileUnselected;
      }

      if (isCoreEnabledOnStart)
      {
        innerCore.Toggle();
        animationPlayer.Play("enable");
      }
    }

    public bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public bool IsSelectableFromDirection(Direction direction)
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

    public void Dock()
    {
      innerCore.Toggle();

      if (innerCore.IsEnabled())
      {
        animationPlayer.Play("enable");
      }
      else
      {
        animationPlayer.Play("disable");
      }
    }

    public void Undock()
    {
    }

    public bool IsEnabled()
    {
      return innerCore.IsEnabled();
    }

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      Dock();
    }

    public void OnTileUnselected(Tile tile)
    {
      Undock();
    }
  }
}
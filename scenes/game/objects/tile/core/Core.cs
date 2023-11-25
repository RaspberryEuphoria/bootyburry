using System;
using System.Linq;
using Godot;

namespace Game
{
  public partial class Core : TileTerrain
  {
    [Export]
    private bool isCoreEnabledOnStart = false;

    private Tile _rootTile;
    public override Tile RootTile
    {
      get => _rootTile;
      set
      {
        _rootTile = value;
      }
    }
    public override bool IsPlayerControlled { get => true; }
    public override bool ExpandPreviousPath { get => false; }

    private AnimationPlayer animationPlayer;
    private InnerCore innerCore;
    private Selector[] selectors;

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      selectors = GetChildren().OfType<Selector>().ToArray();
      innerCore = GetNode<InnerCore>("InnerCore");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      RootTile.TileSelected += OnTileSelected;
      RootTile.TileUnselected += OnTileUnselected;

      for (int i = 0; i < selectors.Length; i++)
      {
        RootTile.TileSelected += selectors[i].OnTileSelected;
        RootTile.TileUnselected += selectors[i].OnTileUnselected;
      }

      if (isCoreEnabledOnStart)
      {
        innerCore.Toggle();
        animationPlayer.Play("enable");
      }
    }

    public override Tile GetNextSelectableTileInDirection(Direction direction)
    {
      return DefaultGetNextSelectableTileInDirection(direction);
    }

    public override Tile GetNextCoreTileInDirection(Direction direction)
    {
      return DefaultGetNextCoreTileInDirection(direction);
    }

    public override bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool IsSelectableFromDirection(Direction direction)
    {
      return true;
    }

    public override bool CanUndockInDirection(Direction _direction)
    {
      return true;
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
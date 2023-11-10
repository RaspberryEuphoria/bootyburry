using System;
using System.Linq;
using Godot;

namespace Game
{
  public partial class Core : Node2D
  {
    [Export]
    private bool isCoreEnabledOnStart = false;

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

    public override void _ExitTree()
    {
      tile.TileSelected -= OnTileSelected;
      tile.TileUnselected -= OnTileUnselected;

      for (int i = 0; i < selectors.Length; i++)
      {
        tile.TileSelected -= selectors[i].OnTileSelected;
        tile.TileUnselected -= selectors[i].OnTileUnselected;
      }
    }

    public bool CanBeDockedFromDirection(Direction direction)
    {
      return true;
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return true;
    }

    public void Dock()
    {
      SurveyHazards();

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
      SurveyHazards();
    }

    public void SurveyHazards()
    {
      foreach (Direction direction in Enum.GetValues(typeof(Direction)))
      {
        var nextCoreTile = tile.GetNavigableTileInDirection(direction);
        if (nextCoreTile == null) continue;

        var nextHazardTile = tile.GetHazardTileInPath(direction, nextCoreTile);
        if (nextHazardTile == null) continue;
      }
    }

    public bool HasBurriedTreasure()
    {
      return innerCore.IsEnabled();
    }

    public void OnTileSelected(Tile tile)
    {
      Dock();
    }

    public void OnTileUnselected(Tile tile)
    {
      Undock();
    }
  }
}
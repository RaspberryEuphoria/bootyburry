using System;
using Godot;

namespace Game
{

  public partial class Selector : Node2D
  {
    [Export]
    public Direction direction;

    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
      Visible = false;
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void OnTileSelected(Tile tile)
    {
      if (tile.IsOnBorder(direction)) return;
      if (tile.HasHazardTerrain()) return;

      var tileWithTreasureInDirection = tile.GetNavigableTileInDirection(direction);
      var hasTileWithHazardInDirection = tileWithTreasureInDirection != null && tileWithTreasureInDirection != tile;
      if (!hasTileWithHazardInDirection) return;

      Visible = true;

      var hasTileCore = tileWithTreasureInDirection.HasNode("Core");
      if (!hasTileCore) return;

      var tileCore = tileWithTreasureInDirection.GetNode<Core>("Core");

      var hasBurriedTreasure = tileCore.HasBurriedTreasure();
      if (hasBurriedTreasure)
      {
        animationPlayer.Play("disable");
      }
      else
      {
        animationPlayer.Play("enable");
      }
    }

    public void OnTileUnselected(Tile _tile)
    {
      Visible = false;
    }
  }
}
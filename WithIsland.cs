using System;
using System.Linq;
using Godot;

namespace Game
{
  public partial class WithIsland : Node2D
  {
    [Export]
    private bool isTreasureBurriedOnStart = false;

    private Treasure treasure;
    private Tile tile;

    private Sprite2D boat;
    private Selector[] selectors;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      selectors = GetChildren().OfType<Selector>().ToArray();
      boat = GetNode<Sprite2D>("Boat");
      treasure = GetNode<Treasure>("Treasure");

      tile.TileSelected += OnTileSelected;
      tile.TileUnselected += OnTileUnselected;

      for (int i = 0; i < selectors.Length; i++)
      {
        tile.TileSelected += selectors[i].OnTileSelected;
        tile.TileUnselected += selectors[i].OnTileUnselected;
      }

      if (isTreasureBurriedOnStart)
      {
        treasure.Burry();
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

    public void Dock()
    {
      SurveyHazards();

      treasure.Toggle();
      boat.Visible = true;
    }

    public void Undock()
    {
      SurveyHazards();

      boat.Visible = false;
    }

    public void SurveyHazards()
    {
      foreach (Direction direction in Enum.GetValues(typeof(Direction)))
      {
        var nextIslandTile = tile.GetNavigableTileInDirection(direction);
        if (nextIslandTile == null) continue;

        var nextHazardTile = tile.GetHazardTileInPath(direction, nextIslandTile);
        if (nextHazardTile == null) continue;

        (nextHazardTile.Terrain as WithWreck).ToggleDangerVisibility();
      }
    }

    public bool HasBurriedTreasure()
    {
      return treasure.IsBurried();
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
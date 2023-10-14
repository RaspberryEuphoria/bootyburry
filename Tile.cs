using System;
using System.Linq;
using Godot;

namespace Game
{
  public enum TileState { Selected, Unselected }
  public enum TileType { Island, Water, Wreck }

  public partial class TileWithIsland : Tile
  {
    public new WithIsland Terrain { get; private set; }
  }

  public partial class TileWithWreck : Tile
  {
    public new WithWreck Terrain { get; private set; }
  }

  [Tool]
  public partial class Tile : Node2D
  {
    [Signal]
    public delegate void TileSelectedEventHandler(Tile tile);
    [Signal]
    public delegate void TileUnselectedEventHandler(Tile tile);

    [Export]
    public TileType Type
    {
      get => type;
      set
      {
        type = value;

        if (!Engine.IsEditorHint()) return;
        GenerateTerrain();
      }
    }
    }
    public Node Terrain { get; private set; }
    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    static public readonly int Size = 64;

    private Board board;
    private Sprite2D boat;
    private Selector[] selectors;
    private TileState state = TileState.Unselected;
    private TileType type = TileType.Water;

    public override void _Ready()
    {
      if (Engine.IsEditorHint()) return;

      try
      {
        board = GetParent<Board>();
      }
      catch (Exception)
      {
        GD.PrintErr("Tile must be a child of Board to be initialized.");
        return;
      }
    }

    public override void _EnterTree()
    {
      if (!Engine.IsEditorHint()) return;
      GenerateTerrain();
    }

    public void Init(int column, int row, int id)
    {
      Id = id;
      Row = row;
      Column = column;

      GenerateTerrain();
    }

    private void GenerateTerrain()
    {
      if (!IsInsideTree()) return;

      /**
        * DON'T GENERATE A TERRAIN WHEN VIEWING THE TILE DIRECTLY IN THE EDITOR!
        * This will cause issues when working on a level, since the tiles will have
        * a hidden terrain that doesn't appear in the tree.
        */
      if (GetOwner<Node>() == null) return;

      var childCount = GetChildCount();
      if (childCount == 1)
      {
        var child = GetChild(0);
        var isChildRightType = type switch
        {
          TileType.Island => child.IsInGroup("island"),
          TileType.Wreck => child.IsInGroup("wreck"),
          TileType.Water => child.IsInGroup("water"),
          _ => throw new Exception($"Invalid tile type {type}!")
        };

        if (isChildRightType)
        {
          Terrain = child;
          return;
        }
      }

      GD.Print($"Generating a new terrain for {Name}.");

      var children = GetChildren();
      foreach (var child in children)
      {
        var child = GetChild(i);
        child.Free();
      }

      var newTerrainPath = Type switch
      {
        TileType.Island => "WithIsland",
        TileType.Wreck => "WithWreck",
        _ => "WithWater"
      };

      var newTerrain = ResourceLoader.Load<PackedScene>($"res://{newTerrainPath}.tscn").Instantiate();
      newTerrain.Name = newTerrainPath;

      AddChild(newTerrain);

      if (Engine.IsEditorHint())
      {
        newTerrain.Owner = GetTree().EditedSceneRoot;
      }

      Terrain = newTerrain;
    }

    public Tile GetAdjacentTile(Direction direction)
    {
      if (IsOnBorder(direction)) return null;

      var tiles = board.GetTiles();

      try
      {
        if (direction == Direction.Up) return tiles.ElementAt(Id - board.GetColumns());
        if (direction == Direction.Down) return tiles.ElementAt(Id + board.GetColumns());
        if (direction == Direction.Left) return tiles.ElementAt(Id - 1);
        if (direction == Direction.Right) return tiles.ElementAt(Id + 1);
      }
      catch (Exception)
      {
        return null;
      }

      return null;
    }

    public Tile GetTileWithIslandInDirection(Direction direction)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (board.useBot && adjacentTile.HasHazardTerrain()) return null;
      if (!adjacentTile.HasIslandTerrain()) return adjacentTile.GetTileWithIslandInDirection(direction);

      return adjacentTile;
    }

    public bool HasTileWithTreasureInDirection(Direction direction)
    {
      return GetTileWithIslandInDirection(direction) != null;
    }
    public Tile GetHazardTileInPath(Direction direction, Tile goalTile)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (adjacentTile == goalTile) return null;
      if (!adjacentTile.HasHazardTerrain()) return adjacentTile.GetHazardTileInPath(direction, goalTile);

      return adjacentTile;
    }

    public bool HasTileWithHazardInDirection(Direction direction)
    {
      return GetHazardTileInPath(direction, null) != null;
    }

    public bool IsAdjacentTo(Tile tile)
    {
      var isAdjacent = false;

      if (tile.Row == Row && tile.Column == Column - 1) isAdjacent = true;
      if (tile.Row == Row && tile.Column == Column + 1) isAdjacent = true;
      if (tile.Column == Column && tile.Row == Row - 1) isAdjacent = true;
      if (tile.Column == Column && tile.Row == Row + 1) isAdjacent = true;

      return isAdjacent;
    }

    public bool IsOnBorder(Direction direction)
    {
      var board = GetParent<Board>();
      var rowsCount = board.GetRows();
      var columnsCount = board.GetColumns();
      var isOnBorder = false;

      if (direction == Direction.Up && Row == 0) isOnBorder = true;
      if (direction == Direction.Down && Row == rowsCount - 1) isOnBorder = true;
      if (direction == Direction.Left && Column == 0) isOnBorder = true;
      if (direction == Direction.Right && Column == columnsCount - 1) isOnBorder = true;

      return isOnBorder;
    }

    public bool HasHazardTerrain()
    {
      return Terrain is WithWreck;
    }

    public bool HasIslandTerrain()
    {
      return Terrain is WithIsland;
    }

    public bool IsIsland()
    {
      return Type == TileType.Island;
    }

    public void Select()
    {
      state = TileState.Selected;
      EmitSignal(SignalName.TileSelected, this);
    }

    public void Unselect()
    {
      state = TileState.Unselected;
      EmitSignal(SignalName.TileUnselected, this);
    }
  }
}
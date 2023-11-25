using System;
using System.Linq;
using Godot;

namespace Game
{
  public enum TileState { Selected, Unselected }
  public enum TileType { Core, Empty, Firewall, Router, Proxy }

  [Tool]
  public partial class Tile : BoxContainer
  {
    [Signal]
    public delegate void TileSelectedEventHandler(Tile tile, Tile previousTile, Direction direction);
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
    public TileTerrain Terrain { get; private set; }
    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    private Level level;
    private Selector[] selectors;
    private TileState state = TileState.Unselected;
    private TileType type = TileType.Empty;

    public override void _Ready()
    {
      if (Engine.IsEditorHint()) return;

      try
      {
        level = GetTree().Root.GetNode<Level>("Level");
      }
      catch (Exception)
      {
        GD.PrintErr("Tile must be contained inside of a Level to be initialized.");
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
        // We cant do GetChild<TileTerrain>(0) because Godot doesn't expect an abstract class at this point
        var child = GetChild(0);
        var isChildRightType = type switch
        {
          TileType.Core => child.IsInGroup("core"),
          TileType.Firewall => child.IsInGroup("firewall"),
          TileType.Empty => child.IsInGroup("empty"),
          TileType.Router => child.IsInGroup("router"),
          TileType.Proxy => child.IsInGroup("proxy"),
          _ => throw new Exception($"Invalid tile type {type} on tile {Name}!")
        };

        if (isChildRightType)
        {
          Terrain = child as TileTerrain;
          return;
        }
      }

      GD.Print($"Generating a new terrain for {Name}.");

      var children = GetChildren();
      foreach (var child in children)
      {
        child.Free();
      }

      var newTerrainPath = Type switch
      {
        TileType.Core => "core/Core",
        TileType.Firewall => "firewall/Firewall",
        TileType.Empty => "empty/Empty",
        TileType.Router => "router/Router",
        TileType.Proxy => "proxy/Proxy",
        _ => "Empty"
      };

      var newTerrain = ResourceLoader.Load<PackedScene>($"res://scenes/game/objects/tile/{newTerrainPath}.tscn").Instantiate<TileTerrain>();
      newTerrain.Name = Type.ToString();

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

      // @todo: maybe this is where we should put the Terrain switch instead of specific methods?

      var tiles = level.GetTiles();

      try
      {
        if (direction == Direction.Up) return tiles.ElementAt(Id - level.ColumnsCount);
        if (direction == Direction.Down) return tiles.ElementAt(Id + level.ColumnsCount);
        if (direction == Direction.Left) return tiles.ElementAt(Id - 1);
        if (direction == Direction.Right) return tiles.ElementAt(Id + 1);
      }
      catch (Exception)
      {
        return null;
      }

      return null;
    }

    public Tile GetNextSelectableTileInDirection(Direction direction)
    {
      if (IsBlockedFromDirection(direction)) return null;

      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (adjacentTile.IsBlockedFromDirection(direction)) return null;
      if (adjacentTile.IsSelectableFromDirection(direction)) return adjacentTile;

      return adjacentTile.Terrain.GetNextSelectableTileInDirection(direction);
    }

    public Tile GetNextCoreTileInDirection(Direction direction)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (adjacentTile.IsCore()) return adjacentTile;

      return adjacentTile.Terrain.GetNextCoreTileInDirection(direction);
    }

    public Tile GetBlockerInPathToTile(Direction direction, Tile goalTile)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (adjacentTile == goalTile) return null;
      if (adjacentTile.IsBlockedFromDirection(direction)) return adjacentTile;

      return adjacentTile.Terrain switch
      {
        Router router => adjacentTile.GetBlockerInPathToTile(router.Direction, goalTile),
        Proxy proxy => proxy.ExitTile.GetBlockerInPathToTile(direction, goalTile),
        _ => adjacentTile.GetBlockerInPathToTile(direction, goalTile)
      };
    }

    public bool HasBlockerInPathToTile(Direction direction, Tile goalTile)
    {
      return GetBlockerInPathToTile(direction, goalTile) != null;
    }

    public bool IsOnBorder(Direction direction)
    {
      var rowsCount = level.ColumnsCount;
      var columnsCount = level.ColumnsCount;
      var isOnBorder = false;

      if (direction == Direction.Up && Row == 0) isOnBorder = true;
      if (direction == Direction.Down && Row == rowsCount - 1) isOnBorder = true;
      if (direction == Direction.Left && Column == 0) isOnBorder = true;
      if (direction == Direction.Right && Column == columnsCount - 1) isOnBorder = true;

      return isOnBorder;
    }

    public bool IsBlockedFromDirection(Direction direction)
    {
      return Terrain.IsBlockedFromDirection(direction);
    }

    public bool IsSelectableFromDirection(Direction direction)
    {
      return Terrain.IsSelectableFromDirection(direction);
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return Terrain.CanUndockInDirection(direction);
    }

    public bool ExpandPreviousPath()
    {
      return Terrain.ExpandPreviousPath;
    }

    public bool IsPlayerControlled()
    {
      return Terrain.IsPlayerControlled;
    }

    private bool IsBlockedByCurrent(Direction direction)
    {
      if (Terrain is not Router) return false;

      var terrain = Terrain as Router;
      return terrain.Direction == Level.GetOpposedDirection(direction);
    }

    public bool IsCore()
    {
      return Type == TileType.Core;
    }

    public bool IsProxy()
    {
      return Type == TileType.Proxy;
    }

    public void Select(Tile previousTile, Direction direction)
    {
      state = TileState.Selected;
      EmitSignal(SignalName.TileSelected, this, previousTile, (int)direction);
    }

    public void Unselect()
    {
      state = TileState.Unselected;
      EmitSignal(SignalName.TileUnselected, this);
    }

    public static Vector2 GetCenterPoint(Tile tile)
    {
      var tilePosition = tile.GlobalPosition;
      var tileDimensions = tile.Size;

      return new Vector2(
        tilePosition.X + tileDimensions.X / 2,
        tilePosition.Y + tileDimensions.Y / 2
      );
    }
  }
}
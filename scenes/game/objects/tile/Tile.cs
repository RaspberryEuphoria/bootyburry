using System;
using System.Linq;
using Godot;

namespace Game
{
  public enum TileState { Selected, Unselected }
  public enum TileType { Core, Empty, Firewall, Router }

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
    public Node Terrain { get; private set; }
    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    static public readonly int Size = 64;

    private Level level;
    private Selector[] selectors;
    private TileState state = TileState.Unselected;
    private TileType type = TileType.Empty;

    public override void _Ready()
    {
      if (Engine.IsEditorHint()) return;

      try
      {
        level = GetParent<Level>();
      }
      catch (Exception)
      {
        GD.PrintErr("Tile must be a child of Level to be initialized.");
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
          TileType.Core => child.IsInGroup("core"),
          TileType.Firewall => child.IsInGroup("firewall"),
          TileType.Empty => child.IsInGroup("empty"),
          TileType.Router => child.IsInGroup("router"),
          _ => throw new Exception($"Invalid tile type {type} on tile {Name}!")
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
        child.Free();
      }

      var newTerrainPath = Type switch
      {
        TileType.Core => "core/Core",
        TileType.Firewall => "firewall/Firewall",
        TileType.Empty => "empty/Empty",
        TileType.Router => "router/Router",
        _ => "Empty"
      };

      var newTerrain = ResourceLoader.Load<PackedScene>($"res://scenes/game/objects/tile/{newTerrainPath}.tscn").Instantiate();
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

      var tiles = level.GetTiles();

      try
      {
        if (direction == Direction.Up) return tiles.ElementAt(Id - level.GetColumns());
        if (direction == Direction.Down) return tiles.ElementAt(Id + level.GetColumns());
        if (direction == Direction.Left) return tiles.ElementAt(Id - 1);
        if (direction == Direction.Right) return tiles.ElementAt(Id + 1);
      }
      catch (Exception)
      {
        return null;
      }

      return null;
    }

    public Tile GetNavigableTileInDirection(Direction direction)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (IsBlockedByCurrent(direction)) return null;

      if (adjacentTile.CanBeDockedFromDirection(direction)) return adjacentTile;

      return adjacentTile.GetNavigableTileInDirection(direction);
    }

    public bool HasNavigableTileInDirection(Direction direction)
    {
      return GetNavigableTileInDirection(direction) != null;
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

    public bool IsOnBorder(Direction direction)
    {
      var level = GetParent<Level>();
      var rowsCount = level.GetRows();
      var columnsCount = level.GetColumns();
      var isOnBorder = false;

      if (direction == Direction.Up && Row == 0) isOnBorder = true;
      if (direction == Direction.Down && Row == rowsCount - 1) isOnBorder = true;
      if (direction == Direction.Left && Column == 0) isOnBorder = true;
      if (direction == Direction.Right && Column == columnsCount - 1) isOnBorder = true;

      return isOnBorder;
    }

    public bool CanBeDockedFromDirection(Direction direction)
    {
      return Terrain switch
      {
        Core core => core.CanBeDockedFromDirection(direction),
        Router router => router.CanBeDockedFromDirection(direction),
        Firewall firewall => firewall.CanBeDockedFromDirection(direction),
        Empty empty => empty.CanBeDockedFromDirection(direction),
        _ => throw new Exception($"Invalid terrain type {Terrain.GetType()} on tile {Name}!"),
      };
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return Terrain switch
      {
        Core core => core.CanUndockInDirection(direction),
        Router router => router.CanUndockInDirection(direction),
        Firewall firewall => firewall.CanUndockInDirection(direction),
        Empty empty => empty.CanUndockInDirection(direction),
        _ => throw new Exception($"Invalid terrain type {Terrain.GetType()} on tile {Name}!"),
      };
    }

    public bool CanDockTo()
    {
      return Terrain.HasMethod("Dock");
    }

    private bool IsBlockedByCurrent(Direction direction)
    {
      if (Terrain is not Router) return false;

      var terrain = Terrain as Router;
      return terrain.Direction == Level.GetOpposedDirection(direction);
    }

    /** Hazards are deadly terrains */
    public bool HasHazardTerrain()
    {
      return Terrain is Firewall;
    }

    public bool IsCore()
    {
      return Type == TileType.Core;
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
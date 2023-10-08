using System;
using System.Linq;
using Godot;

namespace Game
{
  public enum TileState { Selected, Unselected }
  public enum TileType { Water, Island, Wreck }

  [Tool]
  public partial class Tile : StaticBody2D
  {
    [Signal]
    public delegate void TileSelectedEventHandler(Tile tile);
    [Signal]
    public delegate void TileUnselectedEventHandler(Tile tile);

    [Export]
    public bool isDisabled = false;

    private TileType type;

    [Export]
    public TileType Type
    {
      get { return type; }
      set
      {
        type = value;

        var texture = GetNode<TileTexture>("TileTexture");
        texture.Type = value;
      }
    }

    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    static public readonly int Size = 64;

    private Board board;
    private Treasure treasure;
    private Sprite2D boat;
    private Selector[] selectors;
    private TileState state = TileState.Unselected;

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

      selectors = GetChildren().OfType<Selector>().ToArray();
      boat = GetNode<Sprite2D>("Boat");

      var hasTreasure = HasNode("Treasure");

      if (type == TileType.Island)
      {
        if (hasTreasure)
        {
          treasure = GetNode<Treasure>("Treasure");
        }
        else
        {
          throw new Exception("Tile must have a Treasure child node.");
        }
      }
      else
      {
        if (hasTreasure)
        {
          GetNode("Treasure").QueueFree();
          GD.PrintErr("Tile has a Treasure child node but is not of type Island.");
        }
      }
    }

    public void Init(int column, int row, int id)
    {
      Id = id;
      Row = row;
      Column = column;
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

    public Tile GetAdjacentTileWithTreasureInDirection(Direction direction)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (adjacentTile.IsBlocker()) return null;
      if (!adjacentTile.HasTreasure()) return adjacentTile.GetAdjacentTileWithTreasureInDirection(direction);

      return adjacentTile;
    }

    public bool HasAdjacentTileWithTreasureInDirection(Direction direction)
    {
      return GetAdjacentTileWithTreasureInDirection(direction) != null;
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

    public bool HasTreasure()
    {
      return treasure != null;
    }

    public bool IsBlocker()
    {
      return Type == TileType.Wreck;
    }

    public bool HasActiveTreasure()
    {
      return treasure != null && treasure.IsActive();
    }

    public void Select()
    {
      state = TileState.Selected;
      treasure.Activate();
      boat.Visible = true;

      EmitSignal(SignalName.TileSelected, this);
    }

    public void Unselect()
    {
      state = TileState.Unselected;
      boat.Visible = false;

      EmitSignal(SignalName.TileUnselected, this);
    }

    public void DeactivateTreasure()
    {
      treasure.Deactivate();
    }
  }
}
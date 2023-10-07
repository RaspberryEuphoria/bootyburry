using System;
using System.Linq;
using Godot;

namespace Game
{
  public enum TileState { Selected, Unselected }

  public partial class Tile : StaticBody2D
  {
    [Signal]
    public delegate void TileSelectedEventHandler(Tile tile);
    [Signal]
    public delegate void TileUnselectedEventHandler(Tile tile);

    [Export]
    public bool isDisabled = false;

    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    static public readonly int Size = 32;

    private Board board;
    private Star star = null;
    private Selector[] selectors;
    private Sprite2D border;
    private Sprite2D background;
    private TileState state = TileState.Unselected;

    public override void _Ready()
    {
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
      border = GetNode<Sprite2D>("Border");
      background = GetNode<Sprite2D>("Background");

      if (HasNode("Star"))
      {
        if (isDisabled)
        {
          GD.PrintErr("It is not possible to have a star on a disabled tile, check tile " + Id + " on row " + Row + " and column " + Column + ".");
        }

        star = GetNode<Star>("Star");
      }

      if (isDisabled)
      {
        border.Visible = false;
        background.Modulate = new Color(0.5f, 0.5f, 0.5f);
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

    public Tile GetAdjacentTileWithStarInDirection(Direction direction)
    {
      var adjacentTile = GetAdjacentTile(direction);

      if (adjacentTile == null) return null;
      if (!adjacentTile.HasStar()) return adjacentTile.GetAdjacentTileWithStarInDirection(direction);

      return adjacentTile;
    }

    public bool HasAdjacentTileWithStarInDirection(Direction direction)
    {
      return GetAdjacentTileWithStarInDirection(direction) != null;
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

    public bool HasStar()
    {
      return star != null;
    }

    public bool HasActiveStar()
    {
      return star != null && star.IsActive();
    }

    public void Select()
    {
      state = TileState.Selected;
      star.Activate();

      EmitSignal(SignalName.TileSelected, this);
    }

    public void Unselect()
    {
      state = TileState.Unselected;

      EmitSignal(SignalName.TileUnselected, this);
    }

    public void DeactivateStar()
    {
      star.Deactivate();
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public enum Direction { Up, Down, Left, Right }

  public partial class Board : Node2D
  {
    [Export]
    private int columns;
    [Export]
    private int rows;
    [Export]
    public bool useBot = false;
    [Export]
    public int botMaxTries = 50;
    [Export]
    public float botDelay = 0.5f;

    public int HammerType { get; set; }

    public bool IsReady { get; private set; } = false;
    public bool IsWon { get; private set; } = false;
    public IEnumerable<Direction> Moves { get; private set; } = new List<Direction>();

    private Tile currentTile;
    private Tile previousTile;
    private IEnumerable<Tile> tiles;
    private IEnumerable<Tile> tilesWithTreasures;
    public readonly Dictionary<string, Direction> actionToDirection = new()
    {
        { "move_up", Direction.Up },
        { "move_left", Direction.Left },
        { "move_down", Direction.Down },
        { "move_right", Direction.Right }
    };

    public override void _Ready()
    {
      if (useBot) SetupBot();

      if (rows < 1 && columns < 1)
      {
        GD.PrintErr("Board rows and columns must be greater than 0 to be initialized.");
        return;
      }

      PrepareBoard();

      var tilesWithActiveTreasures = tilesWithTreasures.Where(t => t.HasActiveTreasure());
      foreach (var tile in tilesWithActiveTreasures)
      {
        tile.Select();
      }
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("reset"))
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      if (!IsReady || IsWon) return;
      HandleInput();
    }

    public void SetupBot()
    {
      var bot = ResourceLoader.Load<PackedScene>("res://Bot.tscn").Instantiate<Bot>();
      bot.maxTries = botMaxTries;
      bot.delay = botDelay;
      bot.disabled = false;
      AddChild(bot);
    }

    public void PrepareBoard()
    {
      tiles = GetChildren().OfType<Tile>();
      tilesWithTreasures = tiles.Where(t => t.HasTreasure());

      for (int y = 0; y < rows; y++)
      {
        for (int x = 0; x < columns; x++)
        {
          var index = y * columns + x;
          var tile = tiles.ElementAt(index);

          tile.TileSelected += OnTileSelected;
          tile.Init(x, y, index);
        }
      }

      IsReady = true;
    }

    public void HandleInput()
    {
      foreach (var itr in actionToDirection)
      {
        if (Input.IsActionJustPressed(itr.Key))
        {
          var nextTile = currentTile.GetAdjacentTileWithTreasureInDirection(itr.Value);
          if (nextTile == null) return;
          Moves = Moves.Append(itr.Value);

          // @warning: previously, it was previousTile.DeactivateTreasure(), but this doesn't work
          // when it's the first move of the game, because previousTile is null.
          // We'll have to verify that this doesn't break anything.
          if (nextTile.HasActiveTreasure()) currentTile.DeactivateTreasure();
          nextTile.Select();

          previousTile = currentTile;
        }
      }
    }

    public List<string> GetAvailableActions()
    {
      var availableActions = new List<string>();
      foreach (var itr in actionToDirection)
      {
        var nextTile = currentTile.GetAdjacentTileWithTreasureInDirection(itr.Value);
        if (nextTile == null) continue;
        availableActions.Add(itr.Key);
      }

      return availableActions;
    }

    public void CheckScore()
    {
      var tilesWithActiveTreasures = tilesWithTreasures.Where(t => t.HasActiveTreasure());
      if (tilesWithActiveTreasures.Count() == tilesWithTreasures.Count())
      {
        IsWon = true;

        GD.Print("You won!");
        string report = $"Moves ({Moves.Count()}): ";
        for (int i = 0; i < Moves.Count(); i++)
        {
          report += Moves.ElementAt(i).ToString() + " / ";
        }
        GD.Print(report);
      }
    }

    public int GetRows()
    {
      return rows;
    }

    public int GetColumns()
    {
      return columns;
    }

    public IEnumerable<Tile> GetTiles()
    {
      return tiles;
    }

    private void OnTileSelected(Tile tile)
    {
      currentTile?.Unselect();
      currentTile = tile;

      // We don't want to check the score the first time a tile is selected,
      // since the board may not be ready yet if the treasure is on an early tile.
      if (!IsReady) return;

      CheckScore();
    }

    public static Direction GetOpposedDirection(Direction direction)
    {
      if (direction == Direction.Up) return Direction.Down;
      if (direction == Direction.Down) return Direction.Up;
      if (direction == Direction.Left) return Direction.Right;
      if (direction == Direction.Right) return Direction.Left;

      return Direction.Up;
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public enum Direction { Up, Down, Left, Right }
  public enum GameState { Playing, Won, Lost }

  public partial class Board : Node2D
  {
    [Export]
    public Tile startingTile;
    [Export]
    public bool useBot = false;
    [Export]
    public int botMaxTries = 50;
    [Export]
    public float botDelay = 0.5f;

    public bool IsReady { get; private set; } = false;
    public GameState GameState { get; private set; } = GameState.Playing;
    public IEnumerable<Direction> Moves { get; private set; } = new List<Direction>();
    private int columns;
    private int rows;

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

      if (startingTile == null)
      {
        GD.PrintErr("Board must have a starting tile to be initialized.");
        return;
      }

      PrepareBoard();
      startingTile.Dock();
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("reset"))
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      if (!IsReady || GameState != GameState.Playing) return;
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

      columns = tiles.Select(t => t.Position[0]).Distinct().ToList().Count;
      rows = tiles.Select(t => t.Position[1]).Distinct().ToList().Count;

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
          var direction = itr.Value;
          var nextTile = currentTile.GetTileWithTreasureInDirection(direction);
          if (nextTile == null) return;

          Moves = Moves.Append(direction);

          var hazardTile = currentTile.GetHazardTileInPath(direction, nextTile);
          if (hazardTile != null)
          {
            hazardTile.Sunk();
            Lose();
            return;
          }

          nextTile.Dock();

          previousTile = currentTile;
        }
      }
    }

    public List<string> GetAvailableActions()
    {
      var availableActions = new List<string>();
      foreach (var itr in actionToDirection)
      {
        var nextTile = currentTile.GetTileWithTreasureInDirection(itr.Value);
        if (nextTile == null) continue;
        availableActions.Add(itr.Key);
      }

      return availableActions;
    }

    private void CheckScore()
    {
      var tilesWithActiveTreasures = tilesWithTreasures.Where(t => t.HasActiveTreasure());
      if (tilesWithActiveTreasures.Count() == tilesWithTreasures.Count())
      {
        Win();
      }
    }

    private void Win()
    {
      GameState = GameState.Won;

      GD.Print("You won!");
      string report = $"Moves ({Moves.Count()}): ";
      for (int i = 0; i < Moves.Count(); i++)
      {
        report += Moves.ElementAt(i).ToString() + " / ";
      }
      GD.Print(report);
    }

    private void Lose()
    {
      GameState = GameState.Lost;

      GD.Print("You lost!");
      string report = $"Moves ({Moves.Count()}): ";
      for (int i = 0; i < Moves.Count(); i++)
      {
        report += Moves.ElementAt(i).ToString() + " / ";
      }
      GD.Print(report);
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
      currentTile?.Undock();
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
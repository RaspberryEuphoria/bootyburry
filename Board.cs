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
    private IEnumerable<Tile> tilesWithStars;
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

      var tilesWithActiveStars = tilesWithStars.Where(t => t.HasActiveStar());
      foreach (var tile in tilesWithActiveStars)
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
      tilesWithStars = tiles.Where(t => t.HasStar());

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
          var nextTile = currentTile.GetAdjacentTileWithStarInDirection(itr.Value);
          if (nextTile == null) return;
          Moves = Moves.Append(itr.Value);

          GD.Print("Moves: " + Moves.Count());
          GD.Print("Last move: " + itr.Value);
          GD.Print("---");

          if (nextTile.HasActiveStar()) previousTile.DeactivateStar();
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
        var nextTile = currentTile.GetAdjacentTileWithStarInDirection(itr.Value);
        if (nextTile == null) continue;
        availableActions.Add(itr.Key);
      }

      return availableActions;
    }

    public void CheckScore()
    {

      var tilesWithActiveStars = tilesWithStars.Where(t => t.HasActiveStar());
      if (tilesWithActiveStars.Count() == tilesWithStars.Count())
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
      // since the board may not be ready yet if the star is on an early tile.
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
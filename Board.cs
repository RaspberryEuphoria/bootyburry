using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public enum Direction { Up, Down, Left, Right }
  public enum GameState { Playing, Won, Lost }

  public partial class Board : Node2D
  {
    [Signal]
    public delegate void GameStartEventHandler();
    [Signal]
    public delegate void GameWonEventHandler();
    [Signal]
    public delegate void GameLostEventHandler();

    [Export]
    public PackedScene nextLevel;
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
    private IEnumerable<Treasure> treasures;
    private IEnumerable<NavigationPath> navigationPaths = new List<NavigationPath>();
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

      if (!startingTile.IsIsland())
      {
        GD.PrintErr("Starting tile must be of Island type.");
        return;
      }

      PrepareBoard();
      EmitSignal(SignalName.GameStart);

      startingTile.Select();
    }

    public override void _ExitTree()
    {
      foreach (var tile in tiles)
      {
        tile.TileSelected -= OnTileSelected;
      }
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("reset"))
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      if (Input.IsActionJustPressed("next_level") && GameState == GameState.Won && nextLevel != null)
      {
        GetTree().ChangeSceneToPacked(nextLevel);
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

      treasures = tiles.Where(t => t.HasIslandTerrain()).Select(t => t.Terrain.GetNode<Treasure>("Treasure"));
      IsReady = true;
    }

    public void HandleInput()
    {
      foreach (var itr in actionToDirection)
      {
        if (Input.IsActionJustPressed(itr.Key))
        {
          var direction = itr.Value;
          var nextTile = currentTile.GetTileWithIslandInDirection(direction);
          if (nextTile == null) return;

          Moves = Moves.Append(direction);
          currentTile.Unselect();

          var navigationPath = GetNavigationPath(currentTile, nextTile);
          if (navigationPath != null)
          {
            navigationPath.QueueFree();
            navigationPaths = navigationPaths.Where(np => np != navigationPath);
          }

          HidePreviousNavigationPath();

          var hazardTile = currentTile.GetHazardTileInPath(direction, nextTile);
          if (hazardTile != null)
          {
            DrawNavigationPath(currentTile, hazardTile);
            Lose();

            hazardTile.Select();
            return;
          }

          DrawNavigationPath(currentTile, nextTile);
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
        var nextTile = currentTile.GetTileWithIslandInDirection(itr.Value);
        if (nextTile == null) continue;
        availableActions.Add(itr.Key);
      }

      return availableActions;
    }

    private void HidePreviousNavigationPath()
    {
      if (!navigationPaths.Any()) return;
      navigationPaths.Last().Fade();
    }

    private void ShowEveryNavigationPath()
    {
      var delayInSec = 0.25f;
      foreach (var navigationPath in navigationPaths)
      {
        navigationPath.ShowAfterDelay(delayInSec);
        delayInSec += 0.25f;
      }
    }

    private NavigationPath GetNavigationPath(Tile startingTile, Tile targetTile)
    {
      if (!navigationPaths.Any()) return null;

      try
      {
        return navigationPaths.First(np =>
          np.Points.SequenceEqual(new Vector2[] { startingTile.Position, targetTile.Position })
          || np.Points.SequenceEqual(new Vector2[] { targetTile.Position, startingTile.Position }
        ));
      }
      catch (Exception)
      {
        return null;
      }
    }

    private void DrawNavigationPath(Tile startingTile, Tile targetTile)
    {
      var navigationPath = ResourceLoader.Load<PackedScene>("res://NavigationPath.tscn").Instantiate<NavigationPath>();
      navigationPath.Init(startingTile, targetTile);
      AddChild(navigationPath);

      navigationPaths = navigationPaths.Append(navigationPath);
    }

    private void CheckScore()
    {
      var burriedTreasures = treasures.Where(t => t.IsBurried());
      if (burriedTreasures.Count() == treasures.Count())
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

      EmitSignal(SignalName.GameWon);

      var victoryScreen = ResourceLoader.Load<PackedScene>("res://VictoryScreen.tscn").Instantiate<VictoryScreen>();
      victoryScreen.Init(Moves, nextLevel);
      AddChild(victoryScreen);
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

      EmitSignal(SignalName.GameLost);

      var losingScreen = ResourceLoader.Load<PackedScene>("res://LosingScreen.tscn").Instantiate<LosingScreen>();
      losingScreen.Init(Moves, nextLevel);
      AddChild(losingScreen);
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
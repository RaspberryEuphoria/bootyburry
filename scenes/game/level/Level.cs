using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public enum Direction { Up, Down, Left, Right }
  public enum GameState { Playing, Won, Lost }

  public partial class Level : Node2D
  {
    [Signal]
    public delegate void GameStartEventHandler();
    [Signal]
    public delegate void GameWonEventHandler();
    [Signal]
    public delegate void GameLostEventHandler();
    [Signal]
    public delegate void PlayerMovedEventHandler(int score);

    [Export]
    public PackedScene nextLevel;
    [Export]
    public Tile startingTile;
    [Export]
    /** The values correspond to [gold, silver, bronze] */
    public int[] medals = new int[] { 0, 0, 0 };

    [ExportGroup("Bot Properties")]
    [Export]
    public bool useBot = false;
    [Export]
    public float botDelay = 0.5f;

    public static readonly Dictionary<string, Direction> actionToDirection = new()
    {
        { "move_up", Direction.Up },
        { "move_left", Direction.Left },
        { "move_down", Direction.Down },
        { "move_right", Direction.Right }
    };

    public bool IsReady { get; private set; } = false;
    public GameState GameState { get; private set; } = GameState.Playing;
    public IEnumerable<Direction> Moves { get; private set; } = new List<Direction>();
    public int score = 0;
    private int columns;
    private int rows;
    private Tile currentTile;
    private Tile previousTile;
    private IEnumerable<Tile> tiles;
    private IEnumerable<InnerCore> treasures;
    private IEnumerable<NavigationPath> navigationPaths = new List<NavigationPath>();

    public override void _Ready()
    {
      if (useBot) SetupBot();

      if (startingTile == null)
      {
        GD.PrintErr("Level must have a starting tile to be initialized.");
        return;
      }

      if (!startingTile.IsCore())
      {
        GD.PrintErr("Starting tile must be of Core type.");
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
      if (Input.IsActionJustPressed("retry"))
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
      bot.maxTries = medals[0];
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

      treasures = tiles.Where(t => t.IsCore()).Select(t => t.Terrain.GetNode<InnerCore>("InnerCore"));
      IsReady = true;
    }

    public void HandleInput()
    {
      foreach (var itr in actionToDirection)
      {
        if (Input.IsActionJustPressed(itr.Key))
        {
          var isMovePlayerControlled = NavigateInDirection(itr.Value);
          if (isMovePlayerControlled == true) AddMove(itr.Value);

          CheckScore();
        }
      }
    }

    private void AddMove(Direction direction)
    {
      score++;
      Moves = Moves.Append(direction);

      EmitSignal(SignalName.PlayerMoved, score);
    }

    public static void TriggerInputInDirection(Direction direction)
    {
      /*
       * When navigating through a Router tile, if the Router direction
       * is identical to the action just pressed, the input can't be
       * registered unless we release the action in the same frame!
       */
      Input.ActionRelease(actionToDirection.FirstOrDefault(itr => itr.Value == direction).Key);

      var action = actionToDirection.FirstOrDefault(itr => itr.Value == direction).Key;

      var press = new InputEventAction
      {
        Action = action,
        Pressed = true,
      };
      Input.ParseInputEvent(press);

      var release = new InputEventAction
      {
        Action = action,
        Pressed = false
      };
      Input.ParseInputEvent(release);
    }

    public List<string> GetAvailableActions()
    {
      var availableActions = new List<string>();
      foreach (var itr in actionToDirection)
      {
        var nextTile = currentTile.GetNavigableTileInDirection(itr.Value);
        if (nextTile == null) continue;
        availableActions.Add(itr.Key);
      }

      return availableActions;
    }

    /**
     * Attempts to navigate in given direction.
     * Returns true if the move was player controlled (.ie from a dockable tile),
     * false otherwise (.ie from a tile that forces movement),
     * null if the move is invalid.
     */
    private bool? NavigateInDirection(Direction direction)
    {
      if (!currentTile.CanUndockInDirection(direction)) return null;

      var nextTile = currentTile.GetNavigableTileInDirection(direction);
      if (nextTile == null || currentTile == nextTile) return null;

      var isMovePlayerControlled = currentTile.CanDockTo();

      currentTile.Unselect();

      var navigationPath = GetNavigationPath(currentTile, nextTile);
      if (navigationPath != null)
      {
        navigationPath.QueueFree();
        navigationPaths = navigationPaths.Where(np => np != navigationPath);
      }

      if (isMovePlayerControlled) HidePreviousNavigationPath();

      var hazardTile = currentTile.GetHazardTileInPath(direction, nextTile);
      if (hazardTile != null)
      {
        DrawNavigationPath(currentTile, hazardTile, expandPreviousPath: !isMovePlayerControlled);
        Lose();

        hazardTile.Select();
        return isMovePlayerControlled;
      }

      DrawNavigationPath(currentTile, nextTile, expandPreviousPath: !isMovePlayerControlled);

      nextTile.Select();
      previousTile = currentTile;

      return isMovePlayerControlled;
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
           np.Points.Contains(startingTile.Position) && np.Points.Contains(targetTile.Position)
        );
      }
      catch (Exception)
      {
        return null;
      }
    }

    private void DrawNavigationPath(Tile startingTile, Tile targetTile, bool expandPreviousPath)
    {
      var navigationPath = expandPreviousPath
        ? navigationPaths.Last()
        : ResourceLoader.Load<PackedScene>("res://NavigationPath.tscn").Instantiate<NavigationPath>();

      if (expandPreviousPath)
      {
        navigationPath.AddTileToPoints(targetTile);
        return;
      }

      navigationPath.Init(startingTile, targetTile);
      AddChild(navigationPath);

      navigationPaths = navigationPaths.Append(navigationPath);
    }

    private void CheckScore()
    {
      var burriedTreasures = treasures.Where(t => t.IsEnabled());
      if (burriedTreasures.Count() == treasures.Count()) Win();
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

      var victoryUI = ResourceLoader.Load<PackedScene>("res://scenes/ui/VictoryUI.tscn").Instantiate<UI.VictoryUI>();
      victoryUI.Init(nextLevel, Moves.Count());
      AddChild(victoryUI);
    }

    private void Lose()
    {
      GameState = GameState.Lost;

      if (useBot) return;

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
      // since the level may not be ready yet if the innerCore is on an early tile.
      if (!IsReady) return;
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
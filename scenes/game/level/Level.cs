using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using InputHandler;
using UI;

namespace Game
{
  public enum Direction { Up, Down, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight }
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
    [Signal]
    public delegate void CurrentTileUpdatedEventHandler(Tile currentTile, Tile previousTile, Direction direction);

    [Export]
    public PackedScene nextLevel;
    [Export]
    public Tile startingTile;
    [Export]
    public int OptimalScore { get; private set; } = 0;
    [Export]
    public string LevelTitle;
    [Export]
    public string LevelSubtitle;

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
    public int Score = 0;

    public int ColumnsCount { get; private set; }
    public int RowsCount { get; private set; }
    private Tile currentTile;
    private Tile previousTile;
    private IEnumerable<Tile> tiles = new List<Tile>();
    private IEnumerable<InnerCore> cores;
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

      var touchScreenHandler = ResourceLoader.Load<PackedScene>("res://scenes/input/TouchScreenHandler.tscn").Instantiate<TouchScreenHandler>();
      AddChild(touchScreenHandler);

      var dynamicCamera = ResourceLoader.Load<PackedScene>("res://scenes/ui/DynamicCamera/DynamicCamera.tscn").Instantiate<DynamicCamera>();
      AddChild(dynamicCamera);

      PrepareBoard();
      EmitSignal(SignalName.GameStart);

      startingTile.Select(null, Direction.Up);
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("retry"))
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      if (!IsInputAllowed()) return;
      HandleInput();
    }

    public void SetupBot()
    {
      var bot = ResourceLoader.Load<PackedScene>("res://Bot.tscn").Instantiate<Bot>();
      bot.maxTries = OptimalScore;
      bot.delay = botDelay;
      bot.disabled = false;
      AddChild(bot);
    }

    public void PrepareBoard()
    {
      var grid = GetNode<BoxContainer>("Grid");
      var rows = grid.GetChildren().OfType<BoxContainer>();
      RowsCount = rows.Count();
      ColumnsCount = rows.First().GetChildren().OfType<Tile>().Count();

      for (int y = 0; y < RowsCount; y++)
      {
        for (int x = 0; x < ColumnsCount; x++)
        {
          var index = y * ColumnsCount + x;
          var tile = rows.ElementAt(y).GetChildren().OfType<Tile>().ElementAt(x);

          tile.TileSelected += OnTileSelected;
          tile.Init(x, y, index);

          tiles = tiles.Append(tile);
        }
      }

      cores = tiles.Where(t => t.IsCore()).Select(t => t.Terrain.GetNode<InnerCore>("InnerCore"));
      IsReady = true;
    }

    public bool IsInputAllowed()
    {
      return IsReady && GameState == GameState.Playing;
    }

    public void HandleInput()
    {
      if (!currentTile.IsPlayerControlled()) return;

      foreach (var itr in actionToDirection)
      {
        if (Input.IsActionJustPressed(itr.Key)) NavigateInDirection(itr.Value);
      }
    }

    private void AddMove(Direction direction)
    {
      Score++;
      Moves = Moves.Append(direction);

      EmitSignal(SignalName.PlayerMoved, Score);
    }

    public void TriggerInputInDirection(Direction direction)
    {
      NavigateInDirection(direction);
    }

    public List<string> GetAvailableActions()
    {
      var availableActions = new List<string>();
      foreach (var itr in actionToDirection)
      {
        var nextTile = currentTile.GetNextSelectableTileInDirection(itr.Value);
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
      var nextTile = currentTile.GetNextSelectableTileInDirection(direction);
      if (nextTile == null || currentTile == nextTile) return null;

      var nextCoreTile = currentTile.GetNextCoreTileInDirection(direction);
      if (nextCoreTile == null || currentTile == nextCoreTile) return null;

      var expandPreviousPath = currentTile.ExpandPreviousPath();
      var shouldAddMove = currentTile.IsPlayerControlled();

      currentTile.Unselect();

      var navigationPath = GetNavigationPath(currentTile, nextTile);
      if (navigationPath != null)
      {
        navigationPath.QueueFree();
        navigationPaths = navigationPaths.Where(np => np != navigationPath);
      }

      if (!expandPreviousPath) HidePreviousNavigationPath();
      DrawNavigationPath(currentTile, nextTile, expandPreviousPath);

      nextTile.Select(currentTile, direction);
      previousTile = currentTile;

      return shouldAddMove;
    }

    private void HidePreviousNavigationPath()
    {
      if (!navigationPaths.Any()) return;
      navigationPaths.Last().Fade();
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
      // @todo: reproduce a bug where moving immediatly to a Router tile will crash the level
      // for some reason, the navigationPaths list is empty when it shouldn't be

      try
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
      catch (Exception e)
      {
        GD.Print("There was an error while drawing the navigation path: " + e.Message);
      }
    }

    public int GetCoresCount()
    {
      if (cores == null) return 0;
      return cores.Count();
    }

    public int GetActiveCoresCount()
    {
      if (cores == null) return 0;
      return cores.Where(t => t.IsEnabled()).Count();
    }

    public Direction GetLastDirection()
    {
      return Moves.Last();
    }

    private void CheckScore()
    {
      if (GetActiveCoresCount() == GetCoresCount()) Win();
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

      if (nextLevel == null)
      {
        var ending = ResourceLoader.Load<PackedScene>("res://scenes/ui/EndScreen.tscn").Instantiate<Node>();
        AddChild(ending);
        return;
      }

      var victoryUI = ResourceLoader.Load<PackedScene>("res://scenes/ui/VictoryUI.tscn").Instantiate<VictoryUI>();
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
    }

    public IEnumerable<Tile> GetTiles()
    {
      return tiles;
    }

    private void OnTileSelected(Tile tile, Tile previousTile, Direction direction)
    {
      currentTile = tile;

      if (previousTile != null && previousTile.IsPlayerControlled()) AddMove(direction);

      CheckScore();
      EmitSignal(SignalName.CurrentTileUpdated, currentTile, previousTile, (int)direction);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Helpers;
using UI;

namespace Game
{
  public enum Direction { Up, Down, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight }
  public enum GameState { Initializing, Playing, Won, Lost }

  public partial class Level : Node2D
  {
    [Signal]
    public delegate void GameStartEventHandler();
    [Signal]
    public delegate void GameWonEventHandler();
    [Signal]
    public delegate void GameLostEventHandler();
    [Signal]
    public delegate void PlayerMovedEventHandler(int scoren, Direction direction);
    [Signal]
    public delegate void CurrentTileUpdatedEventHandler(Tile currentTile, Tile previousTile, Direction direction);
    [Signal]
    public delegate void BoardTilesUpdatedEventHandler();

    [Export]
    public int Id;
    [Export]
    public Tile startingTile;
    [Export]
    public int OptimalScore { get; private set; } = 0;

    [ExportGroup("Bot Properties")]
    [Export]
    public bool useBot = false;

    public readonly Dictionary<string, Direction> ActionToDirection = new()
    {
        { "move_up", Direction.Up },
        { "move_left", Direction.Left },
        { "move_down", Direction.Down },
        { "move_right", Direction.Right }
    };

    public bool IsReady { get; private set; } = false;
    public GameState GameState { get; private set; } = GameState.Initializing;
    public IEnumerable<Direction> Moves { get; private set; } = new List<Direction>();
    public int Score = 0;
    public int ColumnsCount { get; private set; }
    public int RowsCount { get; private set; }
    public bool HasRotated { get; private set; } = false;

    private VBoxContainer grid;
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

      var touchScreenHandler = ResourceLoader.Load<PackedScene>("res://scenes/input/TouchScreenHandler.tscn").Instantiate();
      AddChild(touchScreenHandler);

      var worldEnvironment = ResourceLoader.Load<PackedScene>("res://scenes/game/level/WorldEnvironment.tscn").Instantiate();
      AddChild(worldEnvironment);

      PrepareBoard();
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("retry"))
      {
        Retry();
      }

      if (Input.IsActionJustPressed("go_to_menu"))
      {
        var mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/game/menus/MainMenu.tscn");
        GetTree().ChangeSceneToPacked(mainMenu);
      }

      if (!IsInputAllowed()) return;
      HandleInput();
    }

    public void StartGame()
    {
      EmitSignal(SignalName.GameStart);
      GameState = GameState.Playing;
    }

    public void Retry()
    {
      GetTree().ReloadCurrentScene();
    }

    public void SetupBot()
    {
      var bot = ResourceLoader.Load<PackedScene>("res://Bot.tscn").Instantiate<Bot>();
      bot.maxTries = OptimalScore;
      bot.disabled = false;
      AddChild(bot);
    }

    private async void PrepareBoard()
    {
      grid = GetNode<VBoxContainer>("LevelUI/LevelRoot/GridContainer/%Grid");
      grid.Visible = false;

      var rows = grid.GetChildren().OfType<HBoxContainer>();
      RowsCount = rows.Count();
      ColumnsCount = rows.First().GetChildren().OfType<Tile>().Count();

      /**
       * On desktop screens, we want the grid to be in landscape mode instead of portrait.
       */
      if (!Device.IsMobile() && RowsCount > ColumnsCount && !HasRotated)
      {
        await RotateBoard();
        return;
      }

      for (int y = 0; y < RowsCount; y++)
      {
        for (int x = 0; x < ColumnsCount; x++)
        {
          var index = y * ColumnsCount + x;
          var tile = rows.ElementAt(y).GetChildren().OfType<Tile>().ElementAt(x);

          tiles = tiles.Append(tile);

          tile.TileSelected += OnTileSelected;
          tile.TerrainReplaced += OnTerrainReplaced;

          tile.Init(x, y, index);
        }
      }

      cores = tiles.Where(t => t.IsCore()).Select(t => t.Terrain.GetNode<InnerCore>("InnerCore"));
      IsReady = true;
      grid.Visible = true;

      startingTile.Select(null, Direction.Up);
      StartGame();
    }

    private async Task RotateBoard()
    {
      var rows = grid.GetChildren().OfType<HBoxContainer>();
      RowsCount = rows.Count();
      ColumnsCount = rows.First().GetChildren().OfType<Tile>().Count();

      if (RowsCount == ColumnsCount) return;

      var rowTheme = rows.First().Theme;
      var newTiles = new List<Tile>();
      var startingTileName = startingTile.Name;

      // We want to moves tiles from the bottom to the top, and from the right to the left.
      for (int x = ColumnsCount - 1; x >= 0; x--)
      {
        for (int y = 0; y < RowsCount; y++)
        {
          var originalTile = rows.ElementAt(y).GetChildren().OfType<Tile>().ElementAt(x);
          var newTile = originalTile.Duplicate() as Tile;
          newTiles.Add(newTile);

          if (startingTileName == originalTile.Name) startingTile = newTile;
        }
      }

      foreach (var child in grid.GetChildren())
      {
        child.QueueFree();
        await ToSignal(child, SignalName.TreeExited);
      }

      int i = 0;
      for (int x = 0; x < ColumnsCount; x++)
      {
        var row = new HBoxContainer
        {
          Name = $"Row_{x}",
          Theme = rowTheme,
        };
        grid.AddChild(row);

        for (int y = 0; y < RowsCount; y++)
        {
          var newTile = newTiles.ElementAt(i);
          row.AddChild(newTile);
          i++;
        }
      }

      HasRotated = true;

      PrepareBoard();
    }

    public bool IsInputAllowed()
    {
      return IsReady && GameState == GameState.Playing;
    }

    public void HandleInput()
    {
      if (!currentTile.IsPlayerControlled()) return;

      foreach (var itr in ActionToDirection)
      {
        if (Input.IsActionJustPressed(itr.Key)) NavigateInDirection(itr.Value);
      }
    }

    private void AddMove(Direction direction)
    {
      Score++;
      Moves = Moves.Append(direction);

      EmitSignal(SignalName.PlayerMoved, Score, (int)direction);
    }

    public void TriggerInputInDirection(Direction direction)
    {
      NavigateInDirection(direction);
    }

    public List<string> GetAvailableActions()
    {
      var availableActions = new List<string>();
      foreach (var itr in ActionToDirection)
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
      if (currentTile.HasBlockerInPathToTile(direction, nextCoreTile)) return null;

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

    public void DrawNavigationPath(Tile startingTile, Tile targetTile, bool expandPreviousPath)
    {
      // @todo: reproduce a bug where moving immediatly to a Router tile will crash the level
      // for some reason, the navigationPaths list is empty when it shouldn't be

      // try
      // {
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
      // }
      // catch (Exception e)
      // {
      //   GD.Print("There was an error while drawing the navigation path: " + e.Message);
      // }
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

      var nextLevelId = Id + 1;
      var nextLevel = GD.Load<PackedScene>($"res://scenes/game/level/Level_{nextLevelId}.tscn");

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

    private void OnTerrainReplaced()
    {
      EmitSignal(SignalName.BoardTilesUpdated);
    }

    public int GetCurrentTileId()
    {
      return currentTile.Id;
    }

    public static Direction GetOpposedDirection(Direction direction)
    {
      if (direction == Direction.Up) return Direction.Down;
      if (direction == Direction.Down) return Direction.Up;
      if (direction == Direction.Left) return Direction.Right;
      if (direction == Direction.Right) return Direction.Left;
      if (direction == Direction.TopRight) return Direction.BottomLeft;
      if (direction == Direction.BottomRight) return Direction.TopLeft;
      if (direction == Direction.TopLeft) return Direction.BottomRight;
      if (direction == Direction.BottomLeft) return Direction.TopRight;

      return Direction.Up;
    }

    public static Direction GetRotatedDirection(Direction direction)
    {
      if (direction == Direction.Up) return Direction.Left;
      if (direction == Direction.Down) return Direction.Right;
      if (direction == Direction.Left) return Direction.Down;
      if (direction == Direction.Right) return Direction.Up;
      if (direction == Direction.TopRight) return Direction.TopLeft;
      if (direction == Direction.BottomRight) return Direction.TopRight;
      if (direction == Direction.TopLeft) return Direction.BottomLeft;
      if (direction == Direction.BottomLeft) return Direction.BottomRight;

      return Direction.Up;
    }
  }
}
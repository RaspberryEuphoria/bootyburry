using Godot;

namespace Game
{

  public partial class Selector : Node2D
  {
    [Export]
    private Direction direction;

    private Level level;
    private Tile rootTile;
    private Area2D area2D;
    private AnimationPlayer animationPlayer;
    private StringName idleAnimation = "idle";

    public override void _Ready()
    {
      Hide();

      level = GetTree().Root.GetNode<Level>("Level");
      level.GameWon += OnGameWon;
      level.BoardTilesUpdated += OnBoardTilesUpdated;

      rootTile = GetParent<Core>().GetParent<Tile>();
      rootTile.TileSelected += OnTileSelected;
      rootTile.TileUnselected += OnTileUnselected;

      area2D = GetNode<Area2D>("Area2D");
      area2D.InputEvent += OnInput;

      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
      animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public override void _ExitTree()
    {
      level.GameWon -= OnGameWon;
      level.BoardTilesUpdated -= OnBoardTilesUpdated;

      rootTile.TileSelected -= OnTileSelected;
      rootTile.TileUnselected -= OnTileUnselected;

      area2D.InputEvent -= OnInput;

      animationPlayer.AnimationFinished -= OnAnimationFinished;
    }

    private void OnInput(Node viewport, InputEvent @event, long shapeIdx)
    {
      if (@event is not InputEventMouseButton mouseButton) return;
      if (!mouseButton.Pressed) return;

      level.TriggerInputInDirection(direction);
    }

    private void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      ShowIfDirectionIsAvailable();
    }

    private void OnTileUnselected(Tile _tile)
    {
      Hide();
    }

    private void OnBoardTilesUpdated()
    {
      if (rootTile.Id != level.GetCurrentTileId()) return;

      ShowIfDirectionIsAvailable();
    }

    private void ShowIfDirectionIsAvailable()
    {
      if (rootTile.IsOnBorder(direction)) return;
      if (rootTile.IsBlockedFromDirection(direction)) return;

      var nextTileWithCore = rootTile.GetNextCoreTileInDirection(direction);
      if (nextTileWithCore == null || nextTileWithCore == rootTile) return;
      if (rootTile.HasBlockerInPathToTile(direction, nextTileWithCore)) return;

      var tileCore = nextTileWithCore.GetChild<Core>(0);
      var animationToPlay = tileCore.IsEnabled() ? "disable" : "enable";
      animationPlayer.Play(animationToPlay);

      Show();
    }

    private void OnGameWon()
    {
      Hide();
    }

    private void OnAnimationFinished(StringName animation)
    {
      if (animation != idleAnimation) animationPlayer.Play(idleAnimation);
    }
  }
}
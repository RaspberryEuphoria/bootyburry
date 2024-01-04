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
      Visible = false;

      area2D = GetNode<Area2D>("Area2D");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      level = GetTree().Root.GetNode<Level>("Level");
      level.GameWon += OnGameWon;

      rootTile = GetParent<Core>().GetParent<Tile>();
      rootTile.TileSelected += OnTileSelected;
      rootTile.TileUnselected += OnTileUnselected;

      area2D.InputEvent += OnInput;

      animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public override void _ExitTree()
    {
      level.GameWon -= OnGameWon;
      rootTile.TileSelected -= OnTileSelected;
      rootTile.TileUnselected -= OnTileUnselected;
    }

    private void OnInput(Node viewport, InputEvent @event, long shapeIdx)
    {
      if (@event is not InputEventMouseButton mouseButton) return;
      if (!mouseButton.Pressed) return;

      level.TriggerInputInDirection(direction);
    }

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      if (rootTile.IsOnBorder(direction)) return;
      if (rootTile.IsBlockedFromDirection(direction)) return;

      var nextTileWithCore = rootTile.GetNextCoreTileInDirection(direction);
      if (nextTileWithCore == null || nextTileWithCore == rootTile) return;
      if (rootTile.HasBlockerInPathToTile(direction, nextTileWithCore)) return;

      var tileCore = nextTileWithCore.GetChild<Core>(0);
      var animationToPlay = tileCore.IsEnabled() ? "disable" : "enable";
      animationPlayer.Play(animationToPlay);

      Visible = true;
    }

    public void OnTileUnselected(Tile _tile)
    {
      Visible = false;
    }

    public void OnGameWon()
    {
      Visible = false;
    }

    private void OnAnimationFinished(StringName animation)
    {
      if (animation != idleAnimation) animationPlayer.Play(idleAnimation);
    }
  }
}
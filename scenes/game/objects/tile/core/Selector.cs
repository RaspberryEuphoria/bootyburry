using Godot;

namespace Game
{

  public partial class Selector : Node2D
  {
    [Export]
    private Direction direction;

    private Tile rootTile;
    private AnimationPlayer animationPlayer;
    private StringName idleAnimation = "idle";

    public override void _Ready()
    {
      Visible = false;

      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      var level = GetTree().Root.GetNode<Level>("Level");
      level.GameWon += OnGameWon;

      rootTile = GetParent<Core>().GetParent<Tile>();
      rootTile.TileSelected += OnTileSelected;
      rootTile.TileUnselected += OnTileUnselected;

      animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      if (rootTile.IsOnBorder(direction)) return;
      if (rootTile.IsBlockedFromDirection(direction)) return;

      var nextTileWithCore = rootTile.GetNextCoreTileInDirection(direction);
      if (nextTileWithCore == null) return;
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
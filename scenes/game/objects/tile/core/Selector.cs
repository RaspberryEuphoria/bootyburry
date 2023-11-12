using Godot;

namespace Game
{

  public partial class Selector : Node2D
  {
    [Export]
    private Direction direction;

    private AnimationPlayer animationPlayer;
    private Tile parentTile;

    public override void _Ready()
    {
      Visible = false;

      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      parentTile = GetParent<Core>().GetParent<Tile>();
      var level = parentTile.GetParent<Level>();

      level.GameWon += OnGameWon;
      parentTile.TileSelected += OnTileSelected;
      parentTile.TileUnselected += OnTileUnselected;
    }

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      if (parentTile.IsOnBorder(direction)) return;
      if (parentTile.IsBlockedFromDirection(direction)) return;

      var nextTileWithCore = parentTile.GetNextCoreTileInDirection(direction);
      if (nextTileWithCore == null) return;
      if (parentTile.HasBlockerInPathToTile(direction, nextTileWithCore)) return;

      var tileCore = nextTileWithCore.GetNode<Core>("Core");
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
  }
}
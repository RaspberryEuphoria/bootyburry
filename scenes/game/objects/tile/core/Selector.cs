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
      if (parentTile.IsFirewall()) return;

      var tileWithTreasureInDirection = parentTile.GetNavigableTileInDirection(direction);
      var hasTileWithHazardInDirection = tileWithTreasureInDirection != null && tileWithTreasureInDirection != parentTile;
      if (!hasTileWithHazardInDirection) return;

      Visible = true;

      var hasTileCore = tileWithTreasureInDirection.HasNode("Core");
      if (!hasTileCore) return;

      var tileCore = tileWithTreasureInDirection.GetNode<Core>("Core");

      var hasBurriedTreasure = tileCore.HasBurriedTreasure();
      if (hasBurriedTreasure)
      {
        animationPlayer.Play("disable");
      }
      else
      {
        animationPlayer.Play("enable");
      }
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
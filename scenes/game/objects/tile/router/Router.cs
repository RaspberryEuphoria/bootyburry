using Godot;

namespace Game
{
  public partial class Router : Node2D
  {
    [Export]
    public Direction Direction { get; private set; } = Direction.Up;

    public static readonly bool IsPlayerControlled = false;
    public static readonly bool ExpandPreviousPath = true;

    private Level level;
    private Tile rootTile;
    private Sprite2D arrows;

    public override void _Ready()
    {
      rootTile = GetParent<Tile>();
      level = GetTree().Root.GetNode<Level>("Level");
      arrows = GetNode<Sprite2D>("Arrows");

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      SetupRotation();
    }

    public bool IsBlockedFromDirection(Direction direction)
    {
      return Direction == Level.GetOpposedDirection(direction);
    }

    public bool IsSelectableFromDirection(Direction direction)
    {
      return Direction != Level.GetOpposedDirection(direction);
    }

    public bool CanUndockInDirection(Direction direction)
    {
      return Direction == direction;
    }

    public Direction GetForcedDirection()
    {
      return Direction;
    }

    public void SetupRotation()
    {
      arrows.RotationDegrees = Direction switch
      {
        Direction.Up => 90,
        Direction.Down => -90,
        Direction.Left => 0, // This is how the sprite was drawn
        Direction.Right => -180,
        _ => 90,
      };
    }

    public void OnCurrentTileUpdated(Tile tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null || tile != rootTile) return;

      level.TriggerInputInDirection(Direction);
    }
  }
}
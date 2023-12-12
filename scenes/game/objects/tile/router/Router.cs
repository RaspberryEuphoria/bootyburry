using Godot;

namespace Game
{
  public partial class Router : TileTerrain
  {
    [Export]
    public Direction Direction { get; private set; } = Direction.Up;

    private Tile _rootTile;
    public override Tile RootTile
    {
      get => _rootTile;
      set
      {
        _rootTile = value;
      }
    }
    public override bool IsPlayerControlled { get => false; }
    public override bool ExpandPreviousPath { get => false; }

    private Level level;
    private Sprite2D arrows;

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      level = GetTree().Root.GetNode<Level>("Level");
      arrows = GetNode<Sprite2D>("Arrows");

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      SetupRotation();
    }

    public override void _ExitTree()
    {
      level.CurrentTileUpdated -= OnCurrentTileUpdated;
    }

    public override Tile GetNextSelectableTileInDirection(Direction direction)
    {
      return RootTile.GetNextSelectableTileInDirection(Direction);
    }

    public override Tile GetNextCoreTileInDirection(Direction direction)
    {
      return RootTile.GetNextCoreTileInDirection(Direction);
    }

    public override bool IsBlockedFromDirection(Direction direction)
    {
      return Direction == Level.GetOpposedDirection(direction);
    }

    public override bool IsSelectableFromDirection(Direction direction)
    {
      return Direction != Level.GetOpposedDirection(direction);
    }

    public override bool CanUndockInDirection(Direction direction)
    {
      return Direction == direction;
    }

    public void SetupRotation()
    {
      arrows.RotationDegrees = Direction switch
      {
        Direction.Up => 90,
        Direction.Down => -90,
        Direction.Left => 0, // This is how the sprite was drawn
        Direction.Right => -180,
        Direction.TopLeft => 45,
        Direction.BottomLeft => -45,
        Direction.TopRight => 135,
        Direction.BottomRight => -135,
        _ => 90,
      };
    }

    public void OnCurrentTileUpdated(Tile tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null || tile != RootTile) return;

      level.TriggerInputInDirection(Direction);
    }
  }
}
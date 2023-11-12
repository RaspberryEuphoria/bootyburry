using Godot;

namespace Game
{
  public partial class Router : Node2D
  {
    [Export]
    public Direction Direction { get; private set; } = Direction.Up;

    private Tile tile;
    private Sprite2D arrows;

    public override void _Ready()
    {
      arrows = GetNode<Sprite2D>("Arrows");

      tile = GetParent<Tile>();
      tile.TileSelected += OnTileSelected;

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

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      GD.Print($"Router {tile.Name} selected from {_previousTile.Name} in direction {_direction}");
      Level.TriggerInputInDirection(Direction);
    }
  }
}
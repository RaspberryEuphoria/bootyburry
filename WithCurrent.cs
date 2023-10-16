using Godot;

namespace Game
{
  [Tool]
  public partial class WithCurrent : Node2D
  {
    [Export]
    public Direction Direction { get; private set; } = Direction.Up;
    [Export]
    private float moveSpeed = 0.5f;

    private Tile tile;
    private Sprite2D currentTexture;
    private Sprite2D foamTexture;

    public override void _Ready()
    {
      var waterTexture = GetNode<Sprite2D>("WaterTexture");
      currentTexture = waterTexture.GetNode<Sprite2D>("CurrentTexture");
      foamTexture = waterTexture.GetNode<Sprite2D>("FoamTexture");

      tile = GetParent<Tile>();

      tile.TileSelected += OnTileSelected;

      SetupRotation();
    }

    public override void _ExitTree()
    {
      tile.TileSelected -= OnTileSelected;
    }

    public override void _PhysicsProcess(double delta)
    {
      AnimateSprites();
    }

    public bool CanNavigateTo(Direction direction)
    {
      return Direction != Board.GetOpposedDirection(direction);
    }

    public void SetupRotation()
    {
      currentTexture.RotationDegrees = Direction switch
      {
        Direction.Up => 90,
        Direction.Down => -90,
        Direction.Left => 0, // This is how the sprite was drawn
        Direction.Right => -180,
        _ => 90,
      };
      foamTexture.RotationDegrees = currentTexture.RotationDegrees;
    }

    public void AnimateSprites()
    {
      currentTexture.RegionRect = new Rect2(currentTexture.RegionRect.Position + new Vector2(moveSpeed, 0f), currentTexture.RegionRect.Size);
      foamTexture.RegionRect = new Rect2(foamTexture.RegionRect.Position + new Vector2(moveSpeed * 0.5f, 0f), foamTexture.RegionRect.Size);
    }

    public void OnTileSelected(Tile tile)
    {
      Board.TriggerInputInDirection(Direction);
    }
  }
}
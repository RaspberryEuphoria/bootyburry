using Godot;

namespace Game
{
  [Tool]
  public partial class WithCurrent : Node2D
  {
    [Export]
    private Direction direction = Direction.Up;

    private Sprite2D currentTexture;

    public override void _Ready()
    {
      currentTexture = GetNode<Sprite2D>("CurrentTexture");
      currentTexture.RotationDegrees = direction switch
      {
        Direction.Up => 90,
        Direction.Down => -90,
        Direction.Left => 0, // This is how the sprite was drawn
        Direction.Right => -180,
        _ => 90,
      };
    }

    public override void _PhysicsProcess(double delta)
    {

      currentTexture.RegionRect = new Rect2(currentTexture.RegionRect.Position + new Vector2(1f, 0f), currentTexture.RegionRect.Size);
    }
  }
}
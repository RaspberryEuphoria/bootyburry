using Godot;

namespace Game
{
  public enum StarState { Active, Inactive }

  public partial class Star : StaticBody2D
  {
    [Export]
    public StarState state = StarState.Inactive;

    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
      sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
      sprite.Play(state == StarState.Active ? "active" : "inactive");

      Position = new Vector2(Tile.Size / 2, Tile.Size / 2);
    }

    public void Activate()
    {
      state = StarState.Active;
      sprite.Play("active");
    }

    public void Deactivate()
    {
      state = StarState.Inactive;
      sprite.Play("inactive");
    }

    public bool IsActive()
    {
      return state == StarState.Active;
    }
  }
}
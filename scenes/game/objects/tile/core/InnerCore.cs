using Godot;

namespace Game
{
  public enum CoreState { Enabled, Disabled }

  public partial class InnerCore : Node2D
  {
    [Export]
    private CoreState state = CoreState.Disabled;

    private Sprite2D sprite;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
      sprite = GetNode<Sprite2D>("Sprite2D");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Toggle()
    {
      if (state == CoreState.Enabled)
      {
        Disable();
      }
      else
      {
        Enable();
      }
    }

    private void Enable()
    {
      state = CoreState.Enabled;
      animationPlayer.Play("enable");
    }

    private void Disable()
    {
      animationPlayer.Play("disable");
      state = CoreState.Disabled;
    }

    private void CleanUp()
    {
      sprite.Visible = false;
    }

    public bool IsEnabled()
    {
      return state == CoreState.Enabled;
    }
  }
}
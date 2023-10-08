using Godot;

namespace Game
{
  public enum TreasureState { Active, Inactive }

  [Tool]
  public partial class Treasure : StaticBody2D
  {
    [Export]
    public TreasureState State
    {
      get { return state; }
      set
      {
        state = value;

        if (value == TreasureState.Active) Activate();
        else Deactivate();
      }
    }

    private Sprite2D sprite;
    private TreasureState state = TreasureState.Inactive;

    public override void _EnterTree()
    {
      sprite = GetNode<Sprite2D>("Sprite2D");

      if (state == TreasureState.Inactive) { Deactivate(); }
      else { Activate(); }
    }

    public override void _Ready()
    {
      sprite = GetNode<Sprite2D>("Sprite2D");

      if (state == TreasureState.Inactive) { Deactivate(); }
      else { Activate(); }
    }

    public void Activate()
    {
      state = TreasureState.Active;

      if (Engine.IsEditorHint())
      {
        sprite.Modulate = new Color(1, 1, 1, 1f);
        return;
      }

      Visible = true;
    }

    public void Deactivate()
    {
      state = TreasureState.Inactive;

      if (Engine.IsEditorHint())
      {
        sprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        return;
      }

      Visible = false;
    }

    public bool IsActive()
    {
      return state == TreasureState.Active;
    }
  }
}
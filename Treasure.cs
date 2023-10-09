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

        if (value == TreasureState.Active) Burry();
        else Dig();
      }
    }
    [Export]
    public Sprite2D burriedSprite;
    [Export]
    public Sprite2D diggedSprite;

    private TreasureState state = TreasureState.Inactive;

    public override void _EnterTree()
    {
      if (state == TreasureState.Active)
      {
        Burry();
      }
      else
      {
        CleanUp();
      }
    }

    public override void _Ready()
    {
      if (state == TreasureState.Active)
      {
        Burry();
      }
      else
      {
        CleanUp();
      }
    }

    public void Toggle()
    {
      if (state == TreasureState.Active) { Dig(); }
      else { Burry(); }
    }

    public void Burry()
    {
      state = TreasureState.Active;
      burriedSprite.Visible = true;
      diggedSprite.Visible = false;

      burriedSprite.Modulate = new Color(1, 1, 1, 1f);
    }

    public void Dig()
    {
      state = TreasureState.Inactive;

      if (Engine.IsEditorHint())
      {
        burriedSprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        return;
      }

      burriedSprite.Visible = false;
      diggedSprite.Visible = true;
    }

    public void CleanUp()
    {
      diggedSprite.Visible = false;

      if (Engine.IsEditorHint())
      {
        burriedSprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        return;
      }

      burriedSprite.Visible = false;
    }

    public bool IsActive()
    {
      return state == TreasureState.Active;
    }
  }
}
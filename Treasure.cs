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

    private Sprite2D burriedSprite;
    private Sprite2D diggedSprite;
    private TreasureState state = TreasureState.Inactive;

    public override void _EnterTree()
    {
      burriedSprite = GetNode<Sprite2D>("Burried");
      diggedSprite = GetNode<Sprite2D>("Digged");

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
      burriedSprite = GetNode<Sprite2D>("Burried");
      diggedSprite = GetNode<Sprite2D>("Digged");

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

      if (Engine.IsEditorHint() && burriedSprite != null)
      {
        burriedSprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        return;
      }

      burriedSprite.Visible = false;
      diggedSprite.Visible = true;
    }

    public void CleanUp()
    {
      burriedSprite.Visible = false;
      diggedSprite.Visible = false;
    }

    public bool IsActive()
    {
      return state == TreasureState.Active;
    }
  }
}
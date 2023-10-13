using Godot;

namespace Game
{
  public enum TreasureState { Burried, Digged }

  public partial class Treasure : Node2D
  {
    [Export]
    public TreasureState state = TreasureState.Digged;
    [Export]
    public Sprite2D burriedSprite;
    [Export]
    public Sprite2D diggedSprite;

    public void Toggle()
    {
      if (state == TreasureState.Burried)
      {
        Dig();
      }
      else
      {
        Burry();
      }
    }

    public void Burry()
    {
      state = TreasureState.Burried;
      burriedSprite.Visible = true;
      diggedSprite.Visible = false;

      burriedSprite.Modulate = new Color(1, 1, 1, 1f);
    }

    public void Dig()
    {
      state = TreasureState.Digged;
      burriedSprite.Visible = false;
      diggedSprite.Visible = true;
    }

    public void CleanUp()
    {
      diggedSprite.Visible = false;
      burriedSprite.Visible = false;
    }

    public bool IsBurried()
    {
      return state == TreasureState.Burried;
    }
  }
}
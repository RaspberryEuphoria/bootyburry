using Godot;

namespace Game
{
  public enum TreasureState { Burried, Digged }

  public partial class Treasure : Node2D
  {
    [Export]
    public TreasureState State { get; private set; } = TreasureState.Digged;
    private Sprite2D burriedSprite;
    private Sprite2D diggedSprite;

    public override void _Ready()
    {
      burriedSprite = GetNode<Sprite2D>("%Burried");
      diggedSprite = GetNode<Sprite2D>("%Digged");
    }

    public void Toggle()
    {
      if (State == TreasureState.Burried)
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
      State = TreasureState.Burried;
      burriedSprite.Visible = true;
      diggedSprite.Visible = false;

      burriedSprite.Modulate = new Color(1, 1, 1, 1f);
    }

    public void Dig()
    {
      State = TreasureState.Digged;
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
      return State == TreasureState.Burried;
    }
  }
}
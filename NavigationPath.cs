using Godot;

namespace Game
{
  public partial class NavigationPath : Line2D
  {
    public void Init(Tile startingTile, Tile endingTile)
    {
      Points = new Vector2[] { startingTile.Position, endingTile.Position };
    }

    public void Fade()
    {
      Modulate = new Color(1, 1, 1, 0.25f);
    }

    public void Unfade()
    {
      Modulate = new Color(1, 1, 1, 1);
    }

    public async void ShowAfterDelay(float delay)
    {
      await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
      Show();
    }
  }
}
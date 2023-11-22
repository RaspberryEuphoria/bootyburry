using Godot;

namespace Game
{
  public partial class NavigationPath : Line2D
  {
    public void Init(Tile startingTile, Tile endingTile)
    {
      Points = new Vector2[] { Tile.GetCenterPoint(startingTile), Tile.GetCenterPoint(endingTile) };
    }

    public void AddTileToPoints(Tile newTile)
    {
      var newPoints = new Vector2[Points.Length + 1];
      Points.CopyTo(newPoints, 0);
      newPoints[^1] = Tile.GetCenterPoint(newTile);
      Points = newPoints;
    }

    public void Fade()
    {
      Modulate = new Color(1, 1, 1, 0.25f);
    }

    public void Unfade()
    {
      Modulate = new Color(1, 1, 1, 1);
    }
  }
}
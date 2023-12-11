using Godot;

namespace Game
{
  public partial class NavigationPath : Line2D
  {
    public override void _Ready()
    {
      // Because the tiles are in a Canvas layer, we can't just create the Points
      // with the tiles position.
      // We have to offset the points by half the viewport size.
      var viewport = GetViewportRect().End;
      Position = new Vector2(-viewport.X / 2, -viewport.Y / 2);
    }

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
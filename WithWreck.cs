using Godot;

namespace Game
{
  public partial class WithWreck : Node2D
  {
    private Tile tile;
    private Sprite2D boat;
    private Sprite2D danger;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      boat = GetNode<Sprite2D>("Boat");
      danger = GetNode<Sprite2D>("Danger");

      tile.TileSelected += OnTileSelected;
    }

    public void Sunk()
    {
      boat.Visible = true;
      tile.Select();
    }

    public void ToggleDangerVisibility()
    {
      danger.Visible = !danger.Visible;
    }

    public void OnTileSelected(Tile tile)
    {
      danger.Visible = false;
      Sunk();
    }
  }
}
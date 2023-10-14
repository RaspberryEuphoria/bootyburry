using Godot;

namespace Game
{
  public partial class WithWreck : Node2D
  {
    private Board board;
    private Tile tile;
    private Sprite2D boat;
    private Sprite2D danger;

    public override void _Ready()
    {
      tile = GetParent<Tile>();
      board = tile.GetParent<Board>();
      boat = GetNode<Sprite2D>("Boat");
      danger = GetNode<Sprite2D>("Danger");

      tile.TileSelected += OnTileSelected;
      board.GameWon += HideDanger;
      board.GameLost += HideDanger;
    }

    public override void _ExitTree()
    {
      tile.TileSelected -= OnTileSelected;
    }

    public void Sunk()
    {
      boat.Visible = true;
    }

    public void ToggleDangerVisibility()
    {
      danger.Visible = !danger.Visible;
    }

    public void HideDanger()
    {
      danger.Visible = false;
    }

    public void OnTileSelected(Tile tile)
    {
      danger.Visible = false;
      Sunk();
    }
  }
}
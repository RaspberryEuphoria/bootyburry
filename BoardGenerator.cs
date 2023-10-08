using Godot;

namespace Game
{
  public partial class BoardGenerator : Node2D
  {
    [Export]
    private int columns;
    [Export]
    private int rows;
    [Export]
    private int gap = 64 + 32;

    public override void _Ready()
    {
      if (rows < 1 && columns < 1)
      {
        GD.PrintErr("Board rows and columns must be greater than 0 to be initialized.");
        return;
      }

      PrepareBoard();
    }

    public void PrepareBoard()
    {
      for (int y = 0; y < rows; y++)
      {
        for (int x = 0; x < columns; x++)
        {
          var tile = ResourceLoader.Load<PackedScene>("res://Tile.tscn").Instantiate<Tile>();

          tile.SetProcess(false);
          tile.Position = new Vector2(gap * x, gap * y);
          tile.Name = $"Tile_{y}_{x}";

          AddChild(tile);
        }
      }

      Name = "Board";
    }
  }
}
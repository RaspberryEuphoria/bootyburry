using System;
using Godot;
using UI;

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

    private Level level;

    public override void _Ready()
    {
      level = new Level
      {
        Name = "Level"
      };

      if (rows < 1 && columns < 1)
      {
        GD.PrintErr("Level rows and columns must be greater than 0 to be initialized.");
        return;
      }

      PrepareBoard();
    }

    public void PrepareBoard()
    {
      var background = ResourceLoader.Load<PackedScene>("res://scenes/game/level/LevelBackground.tscn").Instantiate<Sprite2D>();
      var levelUI = ResourceLoader.Load<PackedScene>("res://scenes/ui/LevelUI.tscn").Instantiate<LevelUI>();

      level.AddChild(background);
      level.AddChild(levelUI);

      for (int y = 0; y < rows; y++)
      {
        for (int x = 0; x < columns; x++)
        {
          var tile = ResourceLoader.Load<PackedScene>("res://scenes/game/objects/tile/Tile.tscn").Instantiate<Tile>();

          if (y == 0 && x == 0)
          {
            level.startingTile = tile;
            tile.Type = TileType.Core;
          }

          tile.SetProcess(false);
          tile.Position = new Vector2(gap * x, gap * y);
          tile.CallDeferred("set_name", $"Tile_{y}_{x}");
          level.AddChild(tile);
        }
      }

      AddChild(level);
    }
  }
}
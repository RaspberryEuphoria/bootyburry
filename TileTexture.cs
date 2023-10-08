using System;
using Godot;

namespace Game
{
  [Tool]
  public partial class TileTexture : Sprite2D
  {

    [Export]
    public TileType Type
    {
      get { return type; }
      set
      {
        type = value;
        SetTexture(value);
      }
    }

    private TileType type;

    private void SetTexture(TileType type)
    {
      var texture = type switch
      {
        TileType.Water => GD.Load<Texture2D>("res://assets/gameplay/tile_water_type_0.png"),
        TileType.Island => GD.Load<Texture2D>("res://assets/gameplay/tile_island_type_0.png"),
        TileType.Wreck => GD.Load<Texture2D>("res://assets/gameplay/tile_wreck_type_0.png"),
        _ => throw new NotImplementedException()
      };

      Texture = texture;
    }
  }
}
using Godot;

namespace Game
{
  public partial class WithWater : Node2D
  {
    private Sprite2D sprite;
    private ShaderMaterial shaderMaterial;

    public override void _Ready()
    {
      sprite = GetNode<Sprite2D>("Texture");
      shaderMaterial = sprite.Material as ShaderMaterial;

      var rand = GD.RandRange(0, 1);
      // if (rand == 1) use alternative texture

      shaderMaterial.SetShaderParameter("speed", GD.RandRange(1, 1.2));
      shaderMaterial.SetShaderParameter("min_strength", GD.RandRange(0.05, 0.7));
      shaderMaterial.SetShaderParameter("max_strength", GD.RandRange(0.10, 0.12));
      shaderMaterial.SetShaderParameter("interval", GD.RandRange(3, 3.2));
    }

    public bool CanNavigateTo()
    {
      return false;
    }
  }
}
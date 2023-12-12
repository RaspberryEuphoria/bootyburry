using Godot;

namespace Helpers
{
  public class KamiColors
  {
    public enum ColorType { Primary, Secondary, Tertiary, Error, Disabled, Dark, Light }

    private static readonly Color black = new(0.002f, 0.021f, 0.073f);
    private static readonly Color blue = new(0.144f, 0.476f, 0.929f);
    private static readonly Color green = new(0.565f, 0.984f, 0.424f);
    private static readonly Color grey = new(0.498f, 0.498f, 0.498f);
    private static readonly Color pastelGreen = new(0.467f, 0.867f, 0.467f);
    private static readonly Color white = new(0.895f, 0.895f, 0.895f);
    private static readonly Color pink = new(0.635f, 0.282f, 0.467f);
    private static readonly Color red = new(0.632f, 0.082f, 0.123f);

    public static Color GetPrimary()
    {
      return pastelGreen;
    }

    public static Color GetSecondary()
    {
      return pink;
    }

    public static Color GetTertiary()
    {
      return blue;
    }

    public static Color GetError()
    {
      return red;
    }

    public static Color GetDisabled()
    {
      return grey;
    }

    public static Color GetDark()
    {
      return black;
    }

    public static Color GetLight()
    {
      return white;
    }

    public static Color GetColorByType(ColorType colorType)
    {
      return colorType switch
      {
        ColorType.Primary => GetPrimary(),
        ColorType.Secondary => GetSecondary(),
        ColorType.Tertiary => GetTertiary(),
        ColorType.Error => GetError(),
        ColorType.Disabled => GetDisabled(),
        ColorType.Dark => GetDark(),
        ColorType.Light => GetLight(),
        _ => throw new System.NotImplementedException(),
      };
    }
  }
}